namespace AutoFnhk.AI;

public enum AiCommandMode
{
    PlanOnly,
    ExecuteLater
}

public enum AiSessionState
{
    Idle,
    PromptOpen,
    Generating,
    WaitingToBegin,
    Running,
    Stopped,
    Error
}

public sealed record AiCommandRequest(
    string UserPrompt,
    int SkillLevel = 3,
    AiCommandMode Mode = AiCommandMode.PlanOnly);

public sealed record AiObjective(
    string Name,
    string Description,
    int Priority,
    string[] SuccessCriteria);

public sealed record AiPlan(
    string Summary,
    string[] Assumptions,
    AiObjective[] Objectives,
    string[] AlwaysOnBehaviors,
    string[] Conditions,
    string[] EvaluationMetrics,
    string[] NextSteps);

public sealed record AiResponse(
    bool Success,
    string Message,
    AiPlan? Plan = null,
    string? Error = null);
