using System.Numerics;

namespace AutoFnhk.Aim;

public readonly record struct AimTarget(
    Vector2 Head,
    Vector2 Torso,
    Vector2 Legs,
    float Confidence,
    float Distance,
    string ClassName);
