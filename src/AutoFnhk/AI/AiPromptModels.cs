namespace AutoFnhk.AI;

public sealed record AiPromptRequest(string Prompt, int SkillLevel);

public sealed record AiPromptPlan(
    string Summary,
    IReadOnlyList<string> Objectives,
    IReadOnlyList<string> Behaviors,
    IReadOnlyList<string> Conditions,
    IReadOnlyList<string> EvaluationRules);
