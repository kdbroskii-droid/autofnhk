using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AutoFnhk.Ai;

/// <summary>
/// Uses Gemini only as the natural-language planning layer. It does not send
/// mouse/keyboard input or directly control the game.
/// </summary>
public sealed class GeminiPromptService
{
    private const string DefaultModel = "gemini-2.5-flash";
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;

    public GeminiPromptService(HttpClient? httpClient = null)
    {
        _httpClient = httpClient ?? new HttpClient();
        _apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY") ?? string.Empty;
        _model = Environment.GetEnvironmentVariable("GEMINI_MODEL") ?? DefaultModel;
    }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_apiKey);

    public async Task<AiResponse> CreatePlanAsync(
        AiCommandRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!IsConfigured)
        {
            return new AiResponse(
                false,
                "Gemini is not configured.",
                Error: "Set the GEMINI_API_KEY environment variable on the PC running AutoFnhk.");
        }

        if (string.IsNullOrWhiteSpace(request.UserPrompt))
            return new AiResponse(false, "Enter an AI instruction first.");

        var endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={Uri.EscapeDataString(_apiKey)}";

        var payload = new
        {
            system_instruction = new
            {
                parts = new[] { new { text = BuildSystemPrompt() } }
            },
            contents = new[]
            {
                new
                {
                    role = "user",
                    parts = new[]
                    {
                        new { text = BuildUserPrompt(request) }
                    }
                }
            },
            generationConfig = new
            {
                responseMimeType = "application/json",
                responseSchema = BuildSchema(),
                temperature = 0.2
            }
        };

        try
        {
            using var content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");

            using var response = await _httpClient.PostAsync(endpoint, content, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return new AiResponse(
                    false,
                    "Gemini request failed.",
                    Error: $"HTTP {(int)response.StatusCode}: {body}");
            }

            using var document = JsonDocument.Parse(body);
            var text = document.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            if (string.IsNullOrWhiteSpace(text))
                return new AiResponse(false, "Gemini returned an empty plan.");

            var plan = JsonSerializer.Deserialize<AiPlan>(text, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return plan is null
                ? new AiResponse(false, "Gemini returned an invalid plan.")
                : new AiResponse(true, plan.Summary, plan);
        }
        catch (OperationCanceledException)
        {
            return new AiResponse(false, "Gemini request cancelled.");
        }
        catch (Exception ex)
        {
            return new AiResponse(false, "Gemini request failed.", Error: ex.Message);
        }
    }

    private static string BuildSystemPrompt() => """
        You are the planning intelligence for AutoFnhk, a private test harness for the user's own game Fortnoob.
        Your job is to translate natural-language user instructions into a safe, precise, machine-readable gameplay plan.

        Do not invent unavailable game state. Do not claim an action happened. Produce a plan only.
        Keep normal gameplay mechanics as always-on behavior rather than requiring the user to request them.

        Always consider: positioning, movement, target selection, aiming, building/editing when supported,
        resource awareness, state changes, risk, survival, adaptation, and evaluation.

        Supported high-level concepts include: 1v1, box fight, build fight, free build, edit course,
        quad edit, triple edit, piece control, prefire, aggressive, passive, third party, rotate,
        survive, get eliminations, copy style, adapt, timed practice, command sequences, conditions,
        difficulty modifiers, and goal modifiers.

        The output must be a plan that AutoFnhk can validate before any future execution layer is allowed to act.
        """;

    private static string BuildUserPrompt(AiCommandRequest request) => $"""
        USER INSTRUCTION:
        {request.UserPrompt}

        SKILL LEVEL: {Math.Clamp(request.SkillLevel, 1, 5)}
        MODE: {request.Mode}

        Expand the instruction into concrete objectives, always-on behaviors, conditions,
        evaluation metrics, and immediate next steps. Resolve obvious synonyms, but do not
        add unrelated objectives.
        """;

    private static object BuildSchema() => new
    {
        type = "object",
        properties = new
        {
            summary = new { type = "string" },
            assumptions = new { type = "array", items = new { type = "string" } },
            objectives = new
            {
                type = "array",
                items = new
                {
                    type = "object",
                    properties = new
                    {
                        name = new { type = "string" },
                        description = new { type = "string" },
                        priority = new { type = "integer", minimum = 1, maximum = 100 },
                        successCriteria = new { type = "array", items = new { type = "string" } }
                    },
                    required = new[] { "name", "description", "priority", "successCriteria" }
                }
            },
            alwaysOnBehaviors = new { type = "array", items = new { type = "string" } },
            conditions = new { type = "array", items = new { type = "string" } },
            evaluationMetrics = new { type = "array", items = new { type = "string" } },
            nextSteps = new { type = "array", items = new { type = "string" } }
        },
        required = new[]
        {
            "summary", "assumptions", "objectives", "alwaysOnBehaviors",
            "conditions", "evaluationMetrics", "nextSteps"
        }
    };
}
