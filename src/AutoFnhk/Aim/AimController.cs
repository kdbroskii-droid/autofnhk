using System.Numerics;

namespace AutoFnhk.Aim;

/// <summary>
/// Calculates a screen-space aim point for Fortnoob's own test targets.
/// It returns coordinates only; the game test harness decides how to apply them.
/// </summary>
public sealed class AimController
{
    public AimSettings Settings { get; }

    public AimController(AimSettings? settings = null)
    {
        Settings = settings ?? new AimSettings();
    }

    public Vector2 CalculateAimPoint(Vector2 currentAim, AimTarget target, float deltaSeconds)
    {
        if (!Settings.Enabled || deltaSeconds <= 0f)
            return currentAim;

        var targetPoint = Settings.HeadPriority || !Settings.AllowTorsoFallback
            ? target.Head
            : target.Torso;

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

    private static float Lerp(float a, float b, float t) => a + (b - a) * t;
}
