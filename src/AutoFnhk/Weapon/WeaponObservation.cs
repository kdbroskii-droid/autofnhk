namespace AutoFnhk.Weapon;

public readonly record struct WeaponObservation(
    string Name,
    float Confidence,
    float? EffectiveRange = null);
