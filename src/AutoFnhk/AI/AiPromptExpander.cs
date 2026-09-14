using System.Text;

namespace AutoFnhk.AI;

/// <summary>
/// Expands a user's short Fortnoob test instruction into a structured AI plan.
/// This is the local planning layer; an external language model can later replace
/// or enrich the parsing without changing the rest of AutoFnhk.
/// </summary>
public sealed class AiPromptExpander
{
    private static readonly string[] KnownObjectives =
    [
        "1v1", "box fight", "build fight", "free build", "edit course",
        "quad edit", "triple edit", "piece control", "prefire",
        "practice", "survive", "get eliminations"
    ];

    public AiPromptPlan Expand(AiPromptRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var text = request.Prompt.Trim();
        if (text.Length == 0)
            throw new ArgumentException("The AI prompt cannot be empty.", nameof(request));

        var objectives = KnownObjectives
            .Where(x => text.Contains(x, StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (objectives.Count == 0)
            objectives.Add("general gameplay objective");

        var behaviors = new List<string>
        {
            "Observe the current Fortnoob test-game state before acting.",
            "Choose actions that advance the requested objective.",
            "Handle normal movement, aiming, weapon management, building and editing as required.",
            "Continuously reassess the situation after each meaningful action.",
            $"Operate at skill level {Math.Clamp(request.SkillLevel, 1, 5)}."
        };

        if (text.Contains("aggressive", StringComparison.OrdinalIgnoreCase))
            behaviors.Add("Prefer proactive engagements when the game state supports them.");
        if (text.Contains("passive", StringComparison.OrdinalIgnoreCase))
            behaviors.Add("Prefer safer decisions and avoid unnecessary engagements.");
        if (text.Contains("adapt", StringComparison.OrdinalIgnoreCase))
            behaviors.Add("Adapt the strategy when the current approach produces poor results.");

        var conditions = new List<string>
        {
            "If the requested objective is complete, evaluate whether another requested objective remains.",
            "If the game state changes significantly, reconsider the next action.",
            "If an action fails, record the failure and select a viable alternative."
        };

        var evaluation = new List<string>
        {
            "Record objective progress.",
            "Record successful and unsuccessful actions.",
            "Use results to improve later decisions during the same test session."
        };

        return new AiPromptPlan(
            Summary: text,
            Objectives: objectives,
            Behaviors: behaviors,
            Conditions: conditions,
            EvaluationRules: evaluation);
    }

    public static string ToModelPrompt(AiPromptRequest request, AiPromptPlan plan)
    {
        var sb = new StringBuilder();
        sb.AppendLine("You are the planning AI for the user's private Fortnoob test environment.");
        sb.AppendLine("Interpret the user's instruction as a gameplay objective, not as a fixed macro.");
        sb.AppendLine("Return decisions that can be validated by the AutoFnhk action system.");
        sb.AppendLine($"Skill level: {Math.Clamp(request.SkillLevel, 1, 5)}");
        sb.AppendLine($"User instruction: {request.Prompt.Trim()}");
        sb.AppendLine("Objectives:");
        foreach (var item in plan.Objectives) sb.AppendLine($"- {item}");
        sb.AppendLine("Always-on behaviors:");
        foreach (var item in plan.Behaviors) sb.AppendLine($"- {item}");
        sb.AppendLine("Conditions:");
        foreach (var item in plan.Conditions) sb.AppendLine($"- {item}");
        sb.AppendLine("Evaluation:");
        foreach (var item in plan.EvaluationRules) sb.AppendLine($"- {item}");
        return sb.ToString();
    }
}
