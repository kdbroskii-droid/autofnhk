namespace AutoFnhk.Input;

public sealed class KeyboardSettings
{
    public bool Enabled { get; set; } = true;
    public int DefaultPressDurationMs { get; set; } = 60;
    public int DefaultReleaseDelayMs { get; set; } = 40;

    // Fortnite PC-style default baseline for Fortnoob's private test setup.
    // Epic confirms keyboard/mouse controls are configurable in-game;
    // the complete live default table is maintained inside Fortnite itself.
    public Dictionary<string, string> Bindings { get; } = new(StringComparer.OrdinalIgnoreCase)
    {
        // Movement
        ["MoveForward"] = "W",
        ["MoveBackward"] = "S",
        ["MoveLeft"] = "A",
        ["MoveRight"] = "D",
        ["Jump"] = "Space",
        ["Sprint"] = "LeftShift",
        ["Crouch"] = "LeftCtrl",

        // General gameplay
        ["Use"] = "E",
        ["Inventory"] = "Tab",
        ["Map"] = "M",
        ["Emote"] = "B",
        ["PushToTalk"] = "Y",

        // Building / editing
        ["Wall"] = "Q",
        ["Floor"] = "C",
        ["Stairs"] = "V",
        ["Roof"] = "F",
        ["Trap"] = "T",
        ["RotateBuilding"] = "R",
        ["Edit"] = "G",
        ["Repair"] = "H"
    };
}
