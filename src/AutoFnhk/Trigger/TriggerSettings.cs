namespace AutoFnhk.Trigger;

public sealed class TriggerSettings
{
    public bool Enabled { get; set; } = true;
    public int FireDelayMilliseconds { get; set; } = 80;
    public int ReleaseDelayMilliseconds { get; set; } = 50;
    public float MinimumTargetConfidence { get; set; } = 0.70f;
}
