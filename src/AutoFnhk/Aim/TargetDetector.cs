using System.Numerics;

namespace AutoFnhk.Aim;

/// <summary>
/// Converts observations supplied by the Fortnoob test integration into valid
/// aim targets. This detector only accepts objects explicitly marked as
/// Fortnoob test objects; it does not inspect other games or bypass security.
/// </summary>
public sealed class TargetDetector
{
    public IReadOnlyList<AimTarget> Detect(
        IEnumerable<TargetObservation> observations,
        AimSettings settings)
    {
        ArgumentNullException.ThrowIfNull(observations);
        ArgumentNullException.ThrowIfNull(settings);

        var result = new List<AimTarget>();

        foreach (var observation in observations)
        {
            if (!IsValidObservation(observation, settings))
                continue;

            result.Add(new AimTarget(
                observation.Head,
                observation.Torso,
                observation.Legs,
                Math.Clamp(observation.Confidence, 0f, 1f),
                observation.Distance,
                observation.ClassName.Trim()));
        }

        return result;
    }

    private static bool IsValidObservation(
        TargetObservation observation,
        AimSettings settings)
    {
        if (!observation.IsFortnoobTestObject)
            return false;

        if (string.IsNullOrWhiteSpace(observation.ClassName))
            return false;

        if (!IsFinite(observation.Head) ||
            !IsFinite(observation.Torso) ||
            !IsFinite(observation.Legs))
            return false;

        if (!float.IsFinite(observation.Distance) ||
            observation.Distance < 0f ||
            observation.Distance > settings.MaximumTargetDistance)
            return false;

        if (!float.IsFinite(observation.Confidence))
            return false;

        if (settings.RequireConfidence &&
            observation.Confidence < settings.MinimumConfidence)
            return false;

        return true;
    }

    private static bool IsFinite(Vector2 value) =>
        float.IsFinite(value.X) && float.IsFinite(value.Y);
}

/// <summary>
/// One observation produced by the Fortnoob test integration.
/// Coordinates are screen-space positions for the head, torso and legs.
/// Confidence is expected to be 0..1, and Distance uses the game's test-space
/// distance units.
/// </summary>
public readonly record struct TargetObservation(
    Vector2 Head,
    Vector2 Torso,
    Vector2 Legs,
    float Confidence,
    float Distance,
    string ClassName,
    bool IsFortnoobTestObject);