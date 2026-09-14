using System.Numerics;

namespace AutoFnhk.Aim;

/// <summary>
/// Calculates a screen-space aim point for Fortnoob's own test targets.
/// </summary>
public sealed class AimController
{
    public AimSettings Settings { get; }

    public AimController(AimSettings? settings = null)
    {
        Settings = settings ?? new AimSettings();
    }

    /// <summary>
    /// True when the current aim is not yet close enough to the selected target.
    /// </summary>
    public bool IsAimNeeded(Vector2 currentAim, AimTarget target)
    {
        if (!Settings.Enabled)
            return false;

        if (Settings.RequireConfidence && target.Confidence < Settings.MinimumConfidence)
            return false;

        var targetPoint = GetTargetPoint(target);
        return Vector2.Distance(currentAim, targetPoint) > Settings.SnapDistance;
    }

    /// <summary>
    /// True when the aim point is within the configured snap distance of the target.
    /// </summary>
    public bool IsAimLocked(Vector2 currentAim, AimTarget target)
    {
        if (!Settings.Enabled)
            return false;

        if (Settings.RequireConfidence && target.Confidence < Settings.MinimumConfidence)
            return false;

        return Vector2.Distance(currentAim, GetTargetPoint(target)) <= Settings.SnapDistance;
    }

    public Vector2 CalculateAimPoint(Vector2 currentAim, AimTarget target, float deltaSeconds)
    {
        if (!Settings.Enabled || deltaSeconds <= 0f)
            return currentAim;

        var targetPoint = GetTargetPoint(target);
        var offset = targetPoint - currentAim;
        var distanceToTarget = offset.Length();

        if (distanceToTarget <= Settings.SnapDistance)
            return targetPoint;

        var distanceFactor = Math.Clamp(
            target.Distance / Math.Max(Settings.ReferenceDistance, 1f), 0f, 1f);
        var smoothing = Lerp(Settings.CloseSmoothing, Settings.FarSmoothing, distanceFactor);
        var step = 1f - MathF.Exp(-smoothing * deltaSeconds);

        return currentAim + offset * Math.Clamp(step, 0f, 1f);
    }

    private Vector2 GetTargetPoint(AimTarget target) =>
        Settings.HeadPriority || !Settings.AllowTorsoFallback
            ? target.Head
            : target.Torso;

    private static float Lerp(float a, float b, float t) => a + (b - a) * t;
}
