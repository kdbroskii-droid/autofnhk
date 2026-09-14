using System.Runtime.InteropServices;
using AutoFnhk.Aim;

namespace AutoFnhk.Trigger;

/// <summary>
/// Requests a normal Windows left-mouse click when a valid Fortnoob test target
/// is aim-locked. This is intended for the user's own/private game test setup.
/// </summary>
public sealed class TriggerController
{
    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint inputCount, INPUT[] inputs, int inputSize);

    private const uint InputMouse = 0;
    private const uint MouseEventLeftDown = 0x0002;
    private const uint MouseEventLeftUp = 0x0004;

    public TriggerSettings Settings { get; }

    public TriggerController(TriggerSettings? settings = null)
    {
        Settings = settings ?? new TriggerSettings();
    }

    public bool ShouldFire(AimTarget? target, bool aimLocked)
    {
        if (!Settings.Enabled || !aimLocked || target is null)
            return false;

        return target.Value.Confidence >= Settings.MinimumTargetConfidence;
    }

    public async Task<bool> FireLeftMouseAsync(CancellationToken cancellationToken = default)
    {
        if (!Settings.Enabled)
            return false;

        if (Settings.FireDelayMilliseconds > 0)
            await Task.Delay(Settings.FireDelayMilliseconds, cancellationToken);

        var down = new INPUT
        {
            type = InputMouse,
            mouseInput = new MOUSEINPUT { dwFlags = MouseEventLeftDown }
        };

        var up = new INPUT
        {
            type = InputMouse,
            mouseInput = new MOUSEINPUT { dwFlags = MouseEventLeftUp }
        };

        if (SendInput(1, new[] { down }, Marshal.SizeOf<INPUT>()) != 1)
            return false;

        if (Settings.ReleaseDelayMilliseconds > 0)
            await Task.Delay(Settings.ReleaseDelayMilliseconds, cancellationToken);

        return SendInput(1, new[] { up }, Marshal.SizeOf<INPUT>()) == 1;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct INPUT
    {
        public uint type;
        public MOUSEINPUT mouseInput;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MOUSEINPUT
    {
        public int dx;
        public int dy;
        public uint mouseData;
        public uint dwFlags;
        public uint time;
        public nint dwExtraInfo;
    }
}
