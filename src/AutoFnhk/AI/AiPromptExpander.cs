namespace AutoFnhk.AI;

/// <summary>
/// Local, deterministic prompt understanding. No API key is required here.
/// Gemini can enrich the plan later, but the app always has a local fallback.
/// </summary>
public sealed class AiPromptExpander
{
    private static readonly string[] ObjectiveVocabulary =
    [
        "1v1", "box fight", "build fight", "free build", "edit course",
        "quad edit", "triple edit", "piece control", "prefire",
        "copy style", "practice", "survive", "get eliminations"
    ];

    private static readonly string[] AlwaysOnBehaviors =
    [
        "Aggression", "Passive/safe decision making", "Third-party awareness",
        "Rotation", "Survival", "Elimination objective", "Adaptation",
        "Combo/sequence handling", "Conditional decision making",
        "Difficulty scaling", "Goal priorities"
    ];

    public AiPromptPlan Expand(AiPromptRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var text = request.Prompt.Trim();
        if (text.Length == 0)
            throw new ArgumentException("The AI prompt cannot be empty.", nameof(request));

        var objectives = ObjectiveVocabulary
            .Where(x => text.Contains(x, StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (objectives.Count == 0)
            objectives.Add("general gameplay objective");

        var behaviors = new List<string>(AlwaysOnBehaviors)
        {
            $"Use skill level {Math.Clamp(request.SkillLevel, 1, 5)}.",
            "Observe game state before choosing an action.",
            "Re-evaluate after meaningful state changes.",
            "Prefer actions that advance the highest-priority current goal."
        };

        var conditions = new List<string>
        {
            "If the primary objective is complete, move to the next requested objective.",
            "If the game state changes significantly, reconsider the current plan.",
            "If an action fails, record the failure and choose a valid alternative.",
            "If survival and the current objective conflict, use the configured goal priorities."
        };

        var evaluation = new List<string>
        {
            "Objective progress",
            "Successful and unsuccessful actions",
            "Fight outcomes and survival",
            "Decision quality and adaptation",
            "Performance compared with the selected difficulty"
        };

        if (text.Contains("aggressive", StringComparison.OrdinalIgnoreCase))
            conditions.Add("Aggressive modifier requested: favor proactive opportunities when risk is acceptable.");
        if (text.Contains("passive", StringComparison.OrdinalIgnoreCase))
            conditions.Add("Passive modifier requested: favor safer positions and avoid unnecessary fights.");
        if (text.Contains("adapt", StringComparison.OrdinalIgnoreCase))
            conditions.Add("Adaptation requested: change strategy when evidence shows the current approach is ineffective.");

        return new AiPromptPlan(
            Summary: text,
            Objectives: objectives,
            Behaviors: behaviors,
            Conditions: conditions,
            EvaluationRules: evaluation);
    }

    public static AiCommandRequest ToCommandRequest(AiPromptRequest request) =>
        new(request.Prompt.Trim(), Math.Clamp(request.SkillLevel, 1, 5), AiCommandMode.PlanOnly);
}
