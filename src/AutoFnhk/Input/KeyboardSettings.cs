namespace AutoFnhk.Input;

public sealed class KeyboardSettings
{
    public bool Enabled { get; set; } = true;
    public int DefaultPressDurationMs { get; set; } = 60;
    public int DefaultReleaseDelayMs { get; set; } = 40;

    // Default Fortnite-style test bindings. These are configurable in the UI.
    public Dictionary<string, string> Bindings { get; } = new(StringComparer.OrdinalIgnoreCase)
    {
        ["MoveForward"] = "W",
        ["MoveBackward"] = "S",
        ["MoveLeft"] = "A",
        ["MoveRight"] = "D",
        ["Jump"] = "Space",
        ["Crouch"] = "C",
        ["Sprint"] = "LeftShift",
        ["Use"] = "E",
        ["Reload"] = "R",
        ["BuildMode"] = "Q",
        ["Edit"] = "F",
        ["Pickaxe"] = "1",
        ["WeaponSlot2"] = "2",
        ["WeaponSlot3"] = "3",
        ["WeaponSlot4"] = "4",
        ["WeaponSlot5"] = "5"
    };
}
