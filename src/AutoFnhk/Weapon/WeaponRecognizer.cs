namespace AutoFnhk.Weapon;

/// <summary>
/// Classifies weapon observations supplied by the Fortnoob test integration.
/// The integration supplies the weapon name; this class normalizes common
/// aliases rather than inspecting or modifying another game.
/// </summary>
public sealed class WeaponRecognizer
{
    public WeaponResult Recognize(WeaponObservation observation)
    {
        var name = observation.Name?.Trim() ?? string.Empty;
        var confidence = Math.Clamp(observation.Confidence, 0f, 1f);

        if (string.IsNullOrWhiteSpace(name))
            return new WeaponResult(WeaponType.Unknown, confidence, null, "Unknown weapon");

        var normalized = name.ToLowerInvariant();
        var type = normalized switch
        {
            _ when normalized.Contains("shotgun") || normalized.Contains("pump") || normalized.Contains("tactical") => WeaponType.Shotgun,
            _ when normalized.Contains("sniper") || normalized.Contains("marksman") || normalized.Contains("bolt") => WeaponType.Sniper,
            _ when normalized.Contains("assault") || normalized.Contains("ar") || normalized.Contains("rifle") => WeaponType.AssaultRifle,
            _ => WeaponType.Unknown
        };

        return new WeaponResult(type, confidence, observation.EffectiveRange, name);
    }
}

public readonly record struct WeaponResult(
    WeaponType Type,
    float Confidence,
    float? EffectiveRange,
    string DisplayName);
