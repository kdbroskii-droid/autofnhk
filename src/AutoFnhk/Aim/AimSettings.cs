namespace AutoFnhk.Aim;

public sealed class AimSettings
{
    public bool Enabled { get; set; } = true;
    public bool HeadPriority { get; set; } = true;
    public bool AllowTorsoFallback { get; set; } = true;
    public bool RequireConfidence { get; set; } = true;
    public float MinimumConfidence { get; set; } = 0.70f;
    public float CloseSmoothing { get; set; } = 18f;
    public float FarSmoothing { get; set; } = 5f;
    public float SnapDistance { get; set; } = 1.5f;
    public float ReferenceDistance { get; set; } = 1000f;
    public float MaximumTargetDistance { get; set; } = 2500f;
    public float TargetSwitchHysteresis { get; set; } = 0.15f;
}
