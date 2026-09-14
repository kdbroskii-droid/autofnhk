namespace AutoFnhk.Input;

/// <summary>
/// Describes a named keyboard action that the Fortnoob test planner can execute.
/// </summary>
public sealed record KeyboardAction(
    string Name,
    string Key,
    KeyboardActionType Type = KeyboardActionType.Press,
    int DurationMs = 60,
    string? Description = null);

public enum KeyboardActionType
{
    Press,
    Hold
}
