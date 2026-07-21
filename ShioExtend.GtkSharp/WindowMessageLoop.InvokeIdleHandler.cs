using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

using RiceTea.Core;
using RiceTea.Core.Helpers;
using RiceTea.Core.Threading;

#if NET472_OR_GREATER
using RiceTea.Core.Extensions;
#endif

namespace ShioExtend.GtkSharp;

partial class WindowMessageLoop
{
    private static class InvokeIdleHandler
    {
        private static readonly Swapable<Queue<IInvokeClosure>> _invokeClosureQueue = Swapable.CreateQueue<IInvokeClosure>(optimistic: true);
        public static readonly GLib.IdleHandler HandlerDelegate = delegate ()
        {
            ProcessAllInvoke();
            return false;
        };

        [ThreadStatic]
        private static Queue<IInvokeClosure>? _currentProcessingQueue;

        private static int _readBarrier;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddInvoke(IInvokeClosure closure)
        {
            Queue<IInvokeClosure> queue = _invokeClosureQueue.Value;
            lock (queue)
                queue.Enqueue(closure);
        }

        public static void ProcessAllInvoke()
        {
            if (InterlockedHelper.CompareExchange(ref _readBarrier, Booleans.TrueInt, Booleans.FalseInt) != Booleans.FalseInt)
            {
                ProcessAllInvoke_InInvokeCall();
                return;
            }

            Queue<IInvokeClosure> queue = _invokeClosureQueue.Swap();
            _currentProcessingQueue = queue;
            Monitor.Enter(queue);
            try
            {
                while (queue.TryDequeue(out IInvokeClosure? closure))
                {
                    if (closure is not null)
                    {
                        DoInvoke(closure);
                        queue = _currentProcessingQueue;
                    }
                }
            }
            finally
            {
                _currentProcessingQueue = null;
                Monitor.Exit(queue);
                Interlocked.Exchange(ref _readBarrier, Booleans.FalseInt);
            }
        }

        private static void ProcessAllInvoke_InInvokeCall()
        {
            Queue<IInvokeClosure>? queue = _currentProcessingQueue;
            if (queue is null)
                return;
            try
            {
                while (queue.TryDequeue(out IInvokeClosure? closure))
                {
                    if (closure is not null)
                        DoInvoke(closure);
                }
            }
            finally
            {
                Monitor.Exit(queue);
            }
            queue = _invokeClosureQueue.Swap();
            Monitor.Enter(queue);
            _currentProcessingQueue = queue;
        }

        private static void DoInvoke(IInvokeClosure closure) => closure.Invoke();
    }
}
