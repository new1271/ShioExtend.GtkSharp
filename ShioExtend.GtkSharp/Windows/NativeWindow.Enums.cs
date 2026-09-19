using System;

namespace ShioExtend.GtkSharp.Windows;

partial class NativeWindow
{
    [Flags]
    private enum WindowRuntimeFlags : uint
    {
        None = 0b000,
        Initialized = 0b001,
        Loaded = 0b010,
        Focused = 0b100,
        Destroyed = unchecked((uint)-1),
    }
}

public enum CloseReason : uint
{
    Unknown = 0,
    Programmically,
    UserClicked,
}

public enum DialogResult : uint
{
    Invalid = 0,
    Ok = 1,
    Cancel = 2,
    Abort = 3,
    Retry = 4,
    Ignore = 5,
    Yes = 6,
    No = 7,
    Close = 8,
    Help = 9,
    TryAgain = 10,
    Continue = 11,
    Timeout = 32000
}