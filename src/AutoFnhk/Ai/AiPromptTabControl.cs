using System.Text;

namespace AutoFnhk.Ai;

/// <summary>
/// Natural-language AI command UI. Ctrl+Enter opens/submits the prompt and Ctrl
/// arms the resulting plan. Execution is intentionally separated from planning.
/// </summary>
public sealed class AiPromptTabControl : UserControl
{
    private readonly GeminiPromptService _gemini = new();
    private readonly TextBox _prompt = new();
    private readonly RichTextBox _result = new();
    private readonly Label _status = new();
    private AiPlan? _pendingPlan;
    private bool _promptOpen;

    public AiPromptTabControl()
    {
        Dock = DockStyle.Fill;
        BackColor = Color.FromArgb(22, 22, 30);
        ForeColor = Color.White;
        BuildUi();
        SetStatus("Press Ctrl+Enter to open the AI prompt.");
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == (Keys.Control | Keys.Enter))
        {
            if (!_promptOpen)
            {
                _promptOpen = true;
                _prompt.Visible = true;
                _prompt.Focus();
                SetStatus("Type your instruction, then press Ctrl+Enter to generate the plan.");
            }
            else
            {
                _ = GeneratePlanAsync();
            }

            return true;
        }

        if (keyData == Keys.Control && !_promptOpen && _pendingPlan is not null)
        {
            SetStatus("AI plan armed. Execution will be connected to the future Auto Play layer.");
            return true;
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }

    private async Task GeneratePlanAsync()
    {
        var text = _prompt.Text.Trim();
        if (text.Length == 0)
        {
            SetStatus("Enter an instruction first.");
            return;
        }

        _promptOpen = false;
        _prompt.Visible = false;
        SetStatus("Gemini is understanding the instruction...");
        _result.Text = "Generating structured AI plan...";

        var response = await _gemini.CreatePlanAsync(new AiCommandRequest(text));

        if (!response.Success || response.Plan is null)
        {
            _pendingPlan = null;
            SetStatus(response.Message);
            _result.Text = response.Error ?? response.Message;
            return;
        }

        _pendingPlan = response.Plan;
        _result.Text = FormatPlan(response.Plan);
        SetStatus("Press Ctrl to begin.");
    }

    private static string FormatPlan(AiPlan plan)
    {
        var builder = new StringBuilder();
        builder.AppendLine("AI PLAN");
        builder.AppendLine(new string('=', 48));
        builder.AppendLine(plan.Summary);
        builder.AppendLine();

        AppendSection(builder, "OBJECTIVES", plan.Objectives.Select(o =>
            $"[{o.Priority}] {o.Name}: {o.Description}\n    Success: {string.Join("; ", o.SuccessCriteria)}"));
        AppendSection(builder, "ALWAYS-ON BEHAVIOR", plan.AlwaysOnBehaviors);
        AppendSection(builder, "CONDITIONS", plan.Conditions);
        AppendSection(builder, "EVALUATION", plan.EvaluationMetrics);
        AppendSection(builder, "NEXT STEPS", plan.NextSteps);

        return builder.ToString();
    }

    private static void AppendSection(StringBuilder builder, string title, IEnumerable<string> items)
    {
        builder.AppendLine(title);
        builder.AppendLine(new string('-', title.Length));
        foreach (var item in items)
            builder.AppendLine($"• {item}");
        builder.AppendLine();
    }

    private void BuildUi()
    {
        var title = new Label
        {
            Text = "AI PLAY PROMPT",
            Dock = DockStyle.Top,
            Height = 48,
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            Padding = new Padding(16, 8, 0, 0)
        };
        Controls.Add(title);

        var statusPanel = new Panel { Dock = DockStyle.Bottom, Height = 42 };
        _status.Dock = DockStyle.Fill;
        _status.Padding = new Padding(14, 8, 8, 8);
        _status.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        statusPanel.Controls.Add(_status);
        Controls.Add(statusPanel);

        var promptPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 120,
            Padding = new Padding(14)
        };
        _prompt.Multiline = true;
        _prompt.AcceptsReturn = true;
        _prompt.Visible = false;
        _prompt.Dock = DockStyle.Fill;
        _prompt.Font = new Font("Segoe UI", 11);
        _prompt.BackColor = Color.FromArgb(14, 14, 20);
        _prompt.ForeColor = Color.White;
        promptPanel.Controls.Add(_prompt);
        Controls.Add(promptPanel);

        _result.Dock = DockStyle.Fill;
        _result.ReadOnly = true;
        _result.BackColor = Color.FromArgb(14, 14, 20);
        _result.ForeColor = Color.Gainsboro;
        _result.Font = new Font("Consolas", 9.5f);
        _result.Text = "No AI plan yet.";
        Controls.Add(_result);
    }

    private void SetStatus(string text) => _status.Text = text;
}
