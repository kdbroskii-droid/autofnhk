using AutoFnhk.Input;

namespace AutoFnhk.AI;

public enum AiDecisionType
{
    Wait,
    MoveForward,
    MoveLeft,
    MoveRight,
    MoveBackward,
    Jump,
    Sprint,
    BuildWall,
    BuildFloor,
    BuildStairs,
    BuildRoof,
    Edit
}

public sealed record AiDecision(
    AiDecisionType Type,
    string Reason,
    int DurationMs = 120);

/// <summary>
/// Deterministic decision layer used after the natural-language plan has been
/// created. It turns current Fortnoob test state into a small, explainable
/// action decision. Game-state acquisition is intentionally separate.
/// </summary>
public sealed class AiDecisionEngine
{
    public AiDecision Decide(AiGameState state, AiPlan? plan, int skillLevel)
    {
        ArgumentNullException.ThrowIfNull(state);
        skillLevel = Math.Clamp(skillLevel, 1, 5);

        if (!state.InMatch)
            return new AiDecision(AiDecisionType.Wait, "Waiting for the Fortnoob test match to start.", 100);

        if (state.IsUnderPressure && state.CanBuild)
            return new AiDecision(AiDecisionType.BuildWall, "Pressure detected; create immediate cover.", 100);

        if (state.InCombat && state.HasTarget)
        {
            if (state.TargetDistance > 1200f)
                return new AiDecision(AiDecisionType.MoveForward, "Target is distant; close the gap while maintaining movement.", 120);

            if (state.TargetDistance < 400f && state.CanBuild && skillLevel >= 3)
                return new AiDecision(AiDecisionType.BuildWall, "Close engagement; use a defensive build decision.", 100);

            return skillLevel >= 4
                ? new AiDecision(AiDecisionType.MoveLeft, "Vary movement during the engagement.", 100)
                : new AiDecision(AiDecisionType.MoveRight, "Maintain movement during the engagement.", 100);
        }

        if (state.SafeToRotate)
            return new AiDecision(AiDecisionType.Sprint, "No immediate engagement; rotate through a safe route.", 500);

        return new AiDecision(AiDecisionType.MoveForward, "Continue controlled movement while evaluating the next state.", 120);
    }

    public IReadOnlyList<KeyboardAction> ToKeyboardActions(AiDecision decision)
    {
        return decision.Type switch
        {
            AiDecisionType.MoveForward => KeyboardActionLibrary.MovementBurst("W", decision.DurationMs),
            AiDecisionType.MoveLeft => KeyboardActionLibrary.MovementBurst("A", decision.DurationMs),
            AiDecisionType.MoveRight => KeyboardActionLibrary.MovementBurst("D", decision.DurationMs),
            AiDecisionType.MoveBackward => KeyboardActionLibrary.MovementBurst("S", decision.DurationMs),
            AiDecisionType.Jump => KeyboardActionLibrary.Jump(),
            AiDecisionType.Sprint => KeyboardActionLibrary.Sprint(decision.DurationMs),
            AiDecisionType.BuildWall => KeyboardActionLibrary.Build("wall"),
            AiDecisionType.BuildFloor => KeyboardActionLibrary.Build("floor"),
            AiDecisionType.BuildStairs => KeyboardActionLibrary.Build("stairs"),
            AiDecisionType.BuildRoof => KeyboardActionLibrary.Build("roof"),
            AiDecisionType.Edit => KeyboardActionLibrary.Edit(),
            _ => []
        };
    }
}
