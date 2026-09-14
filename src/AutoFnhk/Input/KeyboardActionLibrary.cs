namespace AutoFnhk.Input;

/// <summary>
/// Named low-level action sequences for the Fortnoob private test harness.
/// These are building blocks for the later AI planner, not game-specific macros.
/// </summary>
public static class KeyboardActionLibrary
{
    public static IReadOnlyList<KeyboardAction> MovementBurst(string direction, int durationMs = 120) =>
        direction.Trim().ToUpperInvariant() switch
        {
            "W" or "FORWARD" => [new KeyboardAction("Move Forward", "W", KeyboardActionType.Hold, durationMs)],
            "S" or "BACK" or "BACKWARD" => [new KeyboardAction("Move Backward", "S", KeyboardActionType.Hold, durationMs)],
            "A" or "LEFT" => [new KeyboardAction("Move Left", "A", KeyboardActionType.Hold, durationMs)],
            "D" or "RIGHT" => [new KeyboardAction("Move Right", "D", KeyboardActionType.Hold, durationMs)],
            _ => []
        };

    public static IReadOnlyList<KeyboardAction> Jump() =>
        [new KeyboardAction("Jump", "Space")];

    public static IReadOnlyList<KeyboardAction> Sprint(int durationMs = 500) =>
        [new KeyboardAction("Sprint", "LeftShift", KeyboardActionType.Hold, durationMs)];

    public static IReadOnlyList<KeyboardAction> Edit() =>
        [new KeyboardAction("Edit", "G")];

    public static IReadOnlyList<KeyboardAction> Build(string piece) =>
        piece.Trim().ToUpperInvariant() switch
        {
            "WALL" => [new KeyboardAction("Wall", "Q")],
            "FLOOR" => [new KeyboardAction("Floor", "C")],
            "STAIRS" or "STAIR" => [new KeyboardAction("Stairs", "V")],
            "ROOF" or "CONE" => [new KeyboardAction("Roof", "F")],
            _ => []
        };
}
