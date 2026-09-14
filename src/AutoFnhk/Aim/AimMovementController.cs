using System.Numerics;
using System.Runtime.InteropServices;

namespace AutoFnhk.Aim;

/// <summary>
/// Applies the aim controller's calculated screen-space movement to the normal
/// Windows mouse input stream for the user's Fortnoob private test environment.
/// </summary>
public sealed class AimMovementController
{
    private readonly AimController _aimController;

    public AimMovementController(AimController? aimController = null)
    {
        _aimController = aimController ?? new AimController();
    }

    /// <summary>
    /// Gets the current Windows cursor position in screen coordinates.
    /// </summary>
    public Vector2 GetCursorPosition()
    {
        if (!GetCursorPos(out var point))
            throw new InvalidOperationException("Windows could not provide the cursor position.");

        return new Vector2(point.X, point.Y);
    }

    /// <summary>
    /// Calculates the next screen-space aim position and moves the cursor by the
    /// corresponding relative amount. Target coordinates must come from the
    /// Fortnoob test integration.
    /// </summary>
    public AimMovementResult MoveTowardsTarget(AimTarget target, float deltaSeconds)
    {
        if (!_aimController.Settings.Enabled)
            return new AimMovementResult(false, false, GetCursorPosition(), GetCursorPosition(), Vector2.Zero);

        var current = GetCursorPosition();
        var next = _aimController.CalculateAimPoint(current, target, deltaSeconds);
        var delta = next - current;

        if (!_aimController.IsAimLocked(current, target))
            SendRelativeMouseMove(delta);

        var actual = GetCursorPosition();
        var locked = _aimController.IsAimLocked(actual, target);

        return new AimMovementResult(true, locked, current, actual, delta);
    }

    /// <summary>
    /// Performs one relative mouse movement. No clicks or keyboard input are sent.
    /// </summary>
    private static void SendRelativeMouseMove(Vector2 delta)
    {
        var dx = ClampToInt(delta.X);
        var dy = ClampToInt(delta.Y);

        if (dx == 0 && dy == 0)
            return;

        var input = new INPUT
        {
            Type = InputMouse,
            MouseInput = new MOUSEINPUT
            {
                Dx = dx,
                Dy = dy,
                Flags = MouseEventMove
            }
        };

        if (SendInput(1, new[] { input }, Marshal.SizeOf<INPUT>()) != 1)
            throw new InvalidOperationException("Windows rejected the mouse movement input.");
    }

    private static int ClampToInt(float value)
    {
        if (!float.IsFinite(value))
            return 0;

        return (int)Math.Clamp(MathF.Round(value), int.MinValue, int.MaxValue);
    }

    private const uint InputMouse = 0;
    private const uint MouseEventMove = 0x0001;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool GetCursorPos(out POINT point);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint inputCount, INPUT[] inputs, int inputSize);

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct INPUT
    {
        public uint Type;
        public MOUSEINPUT MouseInput;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MOUSEINPUT
    {
        public int Dx;
        public int Dy;
        public uint MouseData;
        public uint Flags;
        public uint Time;
        public nint ExtraInfo;
    }
}

public readonly record struct AimMovementResult(
    bool MovementAttempted,
    bool AimLocked,
    Vector2 PreviousPosition,
    Vector2 CurrentPosition,
    Vector2 RequestedDelta);
