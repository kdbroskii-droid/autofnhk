using System.Numerics;

namespace AutoFnhk.Aim;

/// <summary>
/// Converts game-provided observations into Fortnoob test targets.
/// The game integration supplies observations; this class does not inspect or
/// modify other games or bypass security systems.
/// </summary>
public sealed class TargetDetector
{
    public IReadOnlyList<AimTarget> Detect(IEnumerable<TargetObservation> observations, AimSettings settings)
    {
        var result = new List<AimTarget>();

        foreach (var observation in observations)
        {
            if (!observation.IsFortnoobTestObject)
                continue;
            if (observation.Distance < 0 || observation.Distance > settings.MaximumTargetDistance)
                continue;
            if (settings.RequireConfidence && observation.Confidence < settings.MinimumConfidence)
                continue;

            result.Add(new AimTarget(
                observation.Head,
                observation.Torso,
                observation.Legs,
                observation.Confidence,
                observation.Distance,
                observation.ClassName));
        }

        return result;
    }
}

public readonly record struct TargetObservation(
    Vector2 Head,
    Vector2 Torso,
    Vector2 Legs,
    float Confidence,
    float Distance,
    string ClassName,
    bool IsFortnoobTestObject);
