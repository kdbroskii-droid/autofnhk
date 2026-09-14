using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AutoFnhk.AI;

/// <summary>
/// Gemini planning service for the user's private Fortnoob test harness.
/// This is the only AI source file that contains the API-key placeholder.
/// Gemini produces a structured gameplay plan; local code remains responsible
/// for validating and executing supported actions.
/// </summary>
public sealed class GeminiPromptService
{
    // Replace only this value with your Gemini API key.
    private const string GoogleApiKey = "AQ.Ab8RN6IQS3ei5KWQAI7sW3gdQT7fDbxqSSNIjtsl-bkYcfIyQA";
    private const string DefaultModel = "gemini-2.5-flash-lite";
    private const string EndpointBase = "https://generativelanguage.googleapis.com/v1beta/models/";

    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;

    public GeminiPromptService(HttpClient? httpClient = null, string? model = null)
    {
        _httpClient = httpClient ?? new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(45)
        };

        _apiKey = GoogleApiKey.Trim();
        _model = string.IsNullOrWhiteSpace(model) ? DefaultModel : model.Trim();
    }

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(_apiKey) &&
        !_apiKey.StartsWith("PASTE_", StringComparison.OrdinalIgnoreCase);

    public string Model => _model;

    public async Task<AiResponse> CreatePlanAsync(
        AiCommandRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.UserPrompt))
            return new AiResponse(false, "Enter an AI instruction first.");

        if (!IsConfigured)
            return new AiResponse(false, "Gemini is not configured. Add your API key to GoogleApiKey in GeminiPromptService.cs.");

        var endpoint = $"{EndpointBase}{Uri.EscapeDataString(_model)}:generateContent?key={Uri.EscapeDataString(_apiKey)}";
        var payload = BuildPayload(request);

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
                var detail = TryGetGeminiError(body);
                return new AiResponse(
                    false,
                    "Gemini request failed.",
                    Error: $"HTTP {(int)response.StatusCode}: {detail}");
            }

            return ParseResponse(body);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return new AiResponse(false, "Gemini request cancelled.");
        }
        catch (TaskCanceledException)
        {
            return new AiResponse(false, "Gemini request timed out.");
        }
        catch (JsonException ex)
        {
            return new AiResponse(false, "Gemini returned invalid JSON.", Error: ex.Message);
        }
        catch (HttpRequestException ex)
        {
            return new AiResponse(false, "Could not connect to Gemini.", Error: ex.Message);
        }
        catch (Exception ex)
        {
            return new AiResponse(false, "Gemini request failed.", Error: ex.Message);
        }
    }

    private static object BuildPayload(AiCommandRequest request) => new
    {
        system_instruction = new
        {
            parts = new[]
            {
                new { text = BuildSystemPrompt() }
            }
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

    private static AiResponse ParseResponse(string body)
    {
        using var document = JsonDocument.Parse(body);
        var root = document.RootElement;

        if (!root.TryGetProperty("candidates", out var candidates) ||
            candidates.ValueKind != JsonValueKind.Array ||
            candidates.GetArrayLength() == 0)
        {
            return new AiResponse(false, "Gemini returned no candidates.");
        }

        var candidate = candidates[0];
        if (!candidate.TryGetProperty("content", out var content) ||
            !content.TryGetProperty("parts", out var parts) ||
            parts.ValueKind != JsonValueKind.Array ||
            parts.GetArrayLength() == 0 ||
            !parts[0].TryGetProperty("text", out var textElement))
        {
            return new AiResponse(false, "Gemini returned an incomplete plan.");
        }

        var text = textElement.GetString();
        if (string.IsNullOrWhiteSpace(text))
            return new AiResponse(false, "Gemini returned an empty plan.");

        var plan = JsonSerializer.Deserialize<AiPlan>(text, JsonOptions);
        if (plan is null)
            return new AiResponse(false, "Gemini returned an invalid plan.");

        if (string.IsNullOrWhiteSpace(plan.Summary) || plan.Objectives is null)
            return new AiResponse(false, "Gemini returned an incomplete plan.");

        return new AiResponse(true, plan.Summary, plan);
    }

    private static string TryGetGeminiError(string body)
    {
        try
        {
            using var document = JsonDocument.Parse(body);
            if (document.RootElement.TryGetProperty("error", out var error))
            {
                if (error.TryGetProperty("message", out var message))
                    return message.GetString() ?? "Unknown Gemini error.";
            }
        }
        catch (JsonException)
        {
            // Fall back to a bounded response body below.
        }

        return string.IsNullOrWhiteSpace(body)
            ? "Unknown Gemini error."
            : body.Length > 500 ? body[..500] : body;
    }

    private static string BuildSystemPrompt() => """
        You are the planning intelligence for AutoFnhk, a private test harness for the user's own game Fortnoob.

        Translate natural-language instructions into a precise machine-readable gameplay plan.
        Produce a plan only. Never claim that an action happened and never invent unavailable game state.

        Always account for these core systems when relevant:
        aggression, passive/safe behavior, third-party awareness, rotation, survival,
        elimination objective, adaptation, combo/sequence handling, conditional decisions,
        difficulty scaling, and goal priorities.

        Supported objectives include:
        1v1, box fight, build fight, free build, edit course, quad edit, triple edit,
        piece control, prefire, copy style, and timed skill practice.

        Understand natural language and synonyms. Examples include:
        float, prefire, between fights, play safe, push hard, free build, practice quad edits,
        and combinations of objectives.

        Do not add unrelated objectives. If the user gives multiple goals, preserve their order
        and distinguish the primary goal from secondary practice or behavior goals.
        """;

    private static string BuildUserPrompt(AiCommandRequest request) => $"""
        USER INSTRUCTION:
        {request.UserPrompt.Trim()}

        SKILL LEVEL: {Math.Clamp(request.SkillLevel, 1, 5)}
        MODE: {request.Mode}

        Return a concise structured plan containing:
        - a summary
        - assumptions only when needed
        - prioritized objectives
        - always-on behaviors
        - conditional rules
        - evaluation metrics
        - next steps

        Preserve the user's intent while resolving obvious synonyms.
        """;

    private static object BuildSchema() => new
    {
        type = "object",
        properties = new
        {
            summary = new { type = "string" },
            assumptions = new
            {
                type = "array",
                items = new { type = "string" }
            },
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
                        successCriteria = new
                        {
                            type = "array",
                            items = new { type = "string" }
                        }
                    },
                    required = new[]
                    {
                        "name",
                        "description",
                        "priority",
                        "successCriteria"
                    }
                }
            },
            alwaysOnBehaviors = new
            {
                type = "array",
                items = new { type = "string" }
            },
            conditions = new
            {
                type = "array",
                items = new { type = "string" }
            },
            evaluationMetrics = new
            {
                type = "array",
                items = new { type = "string" }
            },
            nextSteps = new
            {
                type = "array",
                items = new { type = "string" }
            }
        },
        required = new[]
        {
            "summary",
            "assumptions",
            "objectives",
            "alwaysOnBehaviors",
            "conditions",
            "evaluationMetrics",
            "nextSteps"
        }
    };

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
}
