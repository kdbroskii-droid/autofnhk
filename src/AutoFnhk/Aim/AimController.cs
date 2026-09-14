using System.Numerics;

namespace AutoFnhk.Aim;

/// <summary>
/// Calculates the next screen-space aim position for Fortnoob test targets.
/// Input comes from the game's test-target detector; this class does not
/// inject mouse/controller input itself.
/// </summary>
public sealed class AimController
{
    public AimSettings Settings { get; }

    public AimController(AimSettings? settings = null)
    {
        Settings = settings ?? new AimSettings();
    }

    public Vector2 CalculateAimPoint(
        Vector2 currentAim,
        Vector2 targetPoint,
        float distance,
        float deltaSeconds)
    {
        if (deltaSeconds <= 0f)
            return currentAim;

        var offset = targetPoint - currentAim;
        var distanceToTarget = offset.Length();

        if (distanceToTarget <= Settings.SnapDistance)
            return targetPoint;

        // Close targets can be approached more quickly; distant targets stay smoother.
        var distanceFactor = Math.Clamp(distance / Math.Max(Settings.ReferenceDistance, 1f), 0f, 1f);
        var smoothing = Lerp(Settings.CloseSmoothing, Settings.FarSmoothing, distanceFactor);
        var step = 1f - MathF.Exp(-smoothing * deltaSeconds);

        return currentAim + offset * Math.Clamp(step, 0f, 1f);
    }

    private static float Lerp(float a, float b, float t) => a + (b - a) * t;
}

public sealed class AimSettings
{
    /// <summary>How quickly the aim approaches a close target.</summary>
    public float CloseSmoothing { get; init; } = 18f;

    /// <summary>How quickly the aim approaches a distant target.</summary>
    public float FarSmoothing { get; init; } = 5f;

    /// <summary>Distance at which the controller snaps exactly onto the target point.</summary>
    public float SnapDistance { get; init; } = 1.5f;

    /// <summary>Distance used to transition between close and far smoothing.</summary>
    public float ReferenceDistance { get; init; } = 1000f;
}

/// <summary>
/// A target supplied by the Fortnoob test detector. The detector is expected
/// to classify only the game's own test characters/objects as valid targets.
/// </summary>
public readonly record struct AimTarget(
    Vector2 Head,
    Vector2 Torso,
    Vector2 Legs,
    float Confidence,
    float Distance,
    string ClassName)
{
    public Vector2 PreferredPoint => Head;
}
