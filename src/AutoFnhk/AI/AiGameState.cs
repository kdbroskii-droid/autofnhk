namespace AutoFnhk.AI;

/// <summary>
/// Runtime observations supplied by the user's Fortnoob test integration.
/// No game memory, anti-cheat bypass, or external-game scanning is performed here.
/// </summary>
public sealed record AiGameState(
    bool InMatch,
    bool InCombat,
    bool HasTarget,
    float TargetDistance,
    float HealthPercent,
    float ShieldPercent,
    int Eliminations,
    int NearbyPlayers,
    bool SafeToRotate,
    bool CanBuild,
    bool CanEdit,
    bool IsUnderPressure,
    DateTimeOffset Timestamp)
{
    public static AiGameState Idle => new(
        false, false, false, 0f, 100f, 0f, 0, 0, true, true, true, false, DateTimeOffset.UtcNow);
}
