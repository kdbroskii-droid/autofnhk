using System.Runtime.InteropServices;

namespace AutoFnhk.Input;

/// <summary>
/// Sends ordinary keyboard input for the user's own Fortnoob private test harness.
/// It does not inspect or modify another game's process or bypass security software.
/// </summary>
public sealed class KeyboardInputService
{
    private const uint InputKeyboard = 1;
    private const uint KeyEventKeyUp = 0x0002;

    private readonly KeyboardSettings _settings;

    public KeyboardInputService(KeyboardSettings? settings = null)
    {
        _settings = settings ?? new KeyboardSettings();
    }

    public KeyboardSettings Settings => _settings;

    public async Task<bool> PressAsync(string keyName, CancellationToken cancellationToken = default)
    {
        if (!_settings.Enabled || !TryParseKey(keyName, out var key))
            return false;

        SendKey(key, false);
        try
        {
            await Task.Delay(Math.Max(1, _settings.DefaultPressDurationMs), cancellationToken);
        }
        finally
        {
            SendKey(key, true);
        }

        if (_settings.DefaultReleaseDelayMs > 0)
            await Task.Delay(_settings.DefaultReleaseDelayMs, cancellationToken);

        return true;
    }

    public async Task<bool> HoldAsync(string keyName, int durationMs, CancellationToken cancellationToken = default)
    {
        if (!_settings.Enabled || !TryParseKey(keyName, out var key))
            return false;

        SendKey(key, false);
        try
        {
            await Task.Delay(Math.Max(1, durationMs), cancellationToken);
        }
        finally
        {
            SendKey(key, true);
        }

        return true;
    }

    public async Task<bool> PressBindingAsync(string actionName, CancellationToken cancellationToken = default)
    {
        if (!_settings.Bindings.TryGetValue(actionName, out var key))
            return false;

        return await PressAsync(key, cancellationToken);
    }

    public async Task<bool> HoldBindingAsync(string actionName, int durationMs, CancellationToken cancellationToken = default)
    {
        if (!_settings.Bindings.TryGetValue(actionName, out var key))
            return false;

        return await HoldAsync(key, durationMs, cancellationToken);
    }

    private static void SendKey(ushort virtualKey, bool keyUp)
    {
        var input = new INPUT
        {
            type = InputKeyboard,
            U = new INPUTUNION
            {
                ki = new KEYBDINPUT
                {
                    wVk = virtualKey,
                    wScan = 0,
                    dwFlags = keyUp ? KeyEventKeyUp : 0,
                    time = 0,
                    dwExtraInfo = UIntPtr.Zero
                }
            }
        };

        if (SendInput(1, new[] { input }, Marshal.SizeOf<INPUT>()) != 1)
            throw new InvalidOperationException("Windows rejected the keyboard input event.");
    }

    private static bool TryParseKey(string value, out ushort virtualKey)
    {
        virtualKey = 0;
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var name = value.Trim();

        if (name.Length == 1)
        {
            var c = char.ToUpperInvariant(name[0]);
            if (c is >= 'A' and <= 'Z' || c is >= '0' and <= '9')
            {
                virtualKey = c;
                return true;
            }
        }

        virtualKey = name.ToUpperInvariant() switch
        {
            "SPACE" => 0x20,
            "ENTER" => 0x0D,
            "TAB" => 0x09,
            "ESC" or "ESCAPE" => 0x1B,
            "SHIFT" or "LEFTSHIFT" => 0xA0,
            "RIGHTSHIFT" => 0xA1,
            "CTRL" or "CONTROL" or "LEFTCTRL" => 0xA2,
            "RIGHTCTRL" => 0xA3,
            "ALT" or "LEFTALT" => 0xA4,
            "RIGHTALT" => 0xA5,
            "BACKSPACE" => 0x08,
            "CAPSLOCK" => 0x14,
            "LEFT" => 0x25,
            "UP" => 0x26,
            "RIGHT" => 0x27,
            "DOWN" => 0x28,
            "F1" => 0x70,
            "F2" => 0x71,
            "F3" => 0x72,
            "F4" => 0x73,
            "F5" => 0x74,
            "F6" => 0x75,
            "F7" => 0x76,
            "F8" => 0x77,
            "F9" => 0x78,
            "F10" => 0x79,
            "F11" => 0x7A,
            "F12" => 0x7B,
            _ => 0
        };

        return virtualKey != 0;
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

    [StructLayout(LayoutKind.Sequential)]
    private struct INPUT
    {
        public uint type;
        public INPUTUNION U;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct INPUTUNION
    {
        [FieldOffset(0)]
        public KEYBDINPUT ki;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KEYBDINPUT
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public UIntPtr dwExtraInfo;
    }
}
