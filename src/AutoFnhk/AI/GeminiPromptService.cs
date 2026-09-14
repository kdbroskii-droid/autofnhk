using System.Text;
using System.Text.Json;

namespace AutoFnhk.AI;

/// <summary>
/// The only AI file that requires a Gemini API key.
/// Gemini is used for natural-language planning only; it does not directly
/// send mouse/keyboard input or control the game.
/// </summary>
public sealed class GeminiPromptService
{
    // Paste your Gemini API key between the quotes when you configure the app.
    // Do not commit a real key to a public repository.
    private const string GoogleApiKey = "AQ.Ab8RN6IQS3ei5KWQAI7sW3gdQT7fDbxqSSNIjtsl-bkYcfIyQA";
    private const string DefaultModel = "gemini-2.5-flash";

    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;

    public GeminiPromptService(HttpClient? httpClient = null)
    {
        _httpClient = httpClient ?? new HttpClient();
        _apiKey = GoogleApiKey;
        _model = DefaultModel;
    }

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(_apiKey) &&
        !string.Equals(_apiKey, "PASTE_YOUR_GOOGLE_API_KEY_HERE;

    public async Task<AiResponse> CreatePlanAsync(
        AiCommandRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.UserPrompt))
            return new AiResponse(false, "Enter an AI instruction first.");

        if (!IsConfigured)
            return new AiResponse(false, "Gemini is not configured. Paste your key into GoogleApiKey in GeminiPromptService.cs.");

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
                    parts = new[] { new { text = BuildUserPrompt(request) } }
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
                JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            using var response = await _httpClient.PostAsync(endpoint, content, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
                return new AiResponse(false, "Gemini request failed.", Error: $"HTTP {(int)response.StatusCode}: {body}");

            using var document = JsonDocument.Parse(body);
            var candidates = document.RootElement.GetProperty("candidates");
            if (candidates.GetArrayLength() == 0)
                return new AiResponse(false, "Gemini returned no candidates.");

            var text = candidates[0]
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
        Translate natural-language instructions into a precise machine-readable gameplay plan.
        Produce a plan only. Never claim that an action happened and never invent unavailable game state.

        Always account for aggression, passive/safe behavior, third-party awareness, rotation, survival,
        the elimination objective, adaptation, combo/sequence handling, conditional decisions, difficulty,
        and goal priorities. These are core logic, not optional macros.

        Supported objectives include 1v1, box fight, build fight, free build, edit course, quad edit,
        triple edit, piece control, prefire, copy style, and timed practice.

        Natural language matters: understand phrases such as "float", "prefire", "between fights",
        "play safe", "push hard", and combinations of objectives. Do not add unrelated goals.
        """;

    private static string BuildUserPrompt(AiCommandRequest request) => $"""
        USER INSTRUCTION:
        {request.UserPrompt.Trim()}

        SKILL LEVEL: {Math.Clamp(request.SkillLevel, 1, 5)}
        MODE: {request.Mode}

        Return a concise plan with objectives, always-on behaviors, conditions, evaluation metrics,
        and next steps. Preserve the user's intent while resolving obvious synonyms.
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
