namespace AutoFnhk.Aim;

public sealed class TargetSelector
{
    private AimTarget? _current;

    public AimTarget? Current => _current;

    public AimTarget? Select(IReadOnlyList<AimTarget> targets, AimSettings settings)
    {
        var valid = targets
            .Where(t => t.Confidence >= settings.MinimumConfidence)
            .OrderBy(Score)
            .ToList();

        if (valid.Count == 0)
        {
            _current = null;
            return null;
        }

        var best = valid[0];
        if (_current is { } current && valid.Contains(current))
        {
            var currentScore = Score(current);
            if (currentScore <= Score(best) * (1f + settings.TargetSwitchHysteresis))
                return current;
        }

        _current = best;
        return best;
    }

    public void Clear() => _current = null;

    private static float Score(AimTarget target)
        => target.Distance / MathF.Max(target.Confidence, 0.01f);
}
