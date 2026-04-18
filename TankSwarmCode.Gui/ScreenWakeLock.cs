using System.Runtime.InteropServices;

namespace TankSwarmCode.Gui;

/// <summary>
/// Prevents Windows from blanking the display or entering sleep while the
/// simulation is active — the same technique used by video players.
/// </summary>
internal static class ScreenWakeLock
{
    // https://learn.microsoft.com/windows/win32/api/winbase/nf-winbase-setthreadexecutionstate
    private const uint ES_CONTINUOUS       = 0x80000000u;
    private const uint ES_SYSTEM_REQUIRED  = 0x00000001u;
    private const uint ES_DISPLAY_REQUIRED = 0x00000002u;

    [DllImport("kernel32.dll", SetLastError = false)]
    private static extern uint SetThreadExecutionState(uint esFlags);

    /// <summary>
    /// Keeps the display and system awake. Call whenever the simulation starts running.
    /// </summary>
    internal static void Prevent()
        => SetThreadExecutionState(ES_CONTINUOUS | ES_DISPLAY_REQUIRED | ES_SYSTEM_REQUIRED);

    /// <summary>
    /// Restores normal power-management behaviour. Call when the simulation is paused,
    /// stopped, or the application exits.
    /// </summary>
    internal static void Allow()
        => SetThreadExecutionState(ES_CONTINUOUS);
}
