using AutoFnhk.Aim;

namespace AutoFnhk.Trigger;

/// <summary>
/// Decides when the Fortnoob private test harness should request an LMB action.
/// The controller does not inject global mouse input; the Fortnoob test adapter
/// is responsible for performing the in-game test action.
/// </summary>
public sealed class TriggerController
{
    public TriggerSettings Settings { get; }

    public TriggerController(TriggerSettings? settings = null)
    {
        Settings = settings ?? new TriggerSettings();
    }

    public bool ShouldFire(AimTarget? target, bool aimLocked)
    {
        if (!Settings.Enabled || !aimLocked || target is null)
            return false;

        var value = target.Value;
        if (value.Confidence < Settings.MinimumTargetConfidence)
            return false;

        return true;
    }
}
