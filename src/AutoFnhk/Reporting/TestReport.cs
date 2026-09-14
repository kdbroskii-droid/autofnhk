using System.Text.Json;

namespace AutoFnhk.Reporting;

public sealed class TestReport
{
    public string ReportId { get; init; } = Guid.NewGuid().ToString("N");
    public DateTimeOffset StartedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? FinishedAt { get; set; }
    public string Game { get; init; } = "Fortnoob";
    public string Environment { get; init; } = "Private test environment";
    public string Result { get; set; } = "Unknown";
    public int SkillLevel { get; init; }
    public List<TestEvent> Events { get; } = [];
    public List<string> Findings { get; } = [];

    public void AddEvent(string type, string message, object? data = null)
    {
        Events.Add(new TestEvent
        {
            Timestamp = DateTimeOffset.UtcNow,
            Type = type,
            Message = message,
            Data = data
        });
    }

    public void AddFinding(string finding)
    {
        if (!string.IsNullOrWhiteSpace(finding))
            Findings.Add(finding);
    }

    public string ToJson()
    {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }
}

public sealed class TestEvent
{
    public DateTimeOffset Timestamp { get; init; }
    public string Type { get; init; } = "info";
    public string Message { get; init; } = string.Empty;
    public object? Data { get; init; }
}
