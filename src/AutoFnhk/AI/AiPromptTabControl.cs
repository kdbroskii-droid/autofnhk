using System.Text;

namespace AutoFnhk.AI;

/// <summary>
/// AI planning UI. Ctrl+Enter opens/submits the prompt; Ctrl arms a generated plan.
/// This control never sends game input by itself.
/// </summary>
public sealed class AiPromptTabControl : UserControl
{
    private readonly GeminiPromptService _gemini = new();
    private readonly AiPromptExpander _localExpander = new();
    private readonly TextBox _prompt = new();
    private readonly RichTextBox _result = new();
    private readonly Label _status = new();
    private readonly ComboBox _skill = new();

    private AiPlan? _pendingPlan;
    private AiSessionState _state = AiSessionState.Idle;

    public AiPromptTabControl()
    {
        Dock = DockStyle.Fill;
        BackColor = Color.FromArgb(22, 22, 30);
        ForeColor = Color.White;
        TabStop = true;
        BuildUi();
        SetState(AiSessionState.Idle, "Press Ctrl+Enter to open the AI prompt.");
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == (Keys.Control | Keys.Enter))
        {
            if (_state is AiSessionState.Generating)
                return true;

            if (_state is AiSessionState.Idle or AiSessionState.WaitingToBegin or AiSessionState.Stopped or AiSessionState.Error)
            {
                OpenPrompt();
                return true;
            }

            if (_state == AiSessionState.PromptOpen)
            {
                _ = GeneratePlanAsync();
                return true;
            }
        }

        if (keyData == Keys.Control && _pendingPlan is not null && _state == AiSessionState.WaitingToBegin)
        {
            SetState(AiSessionState.Running, "AI plan started. Execution can be connected to the Auto Play layer.");
            return true;
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }

    private void OpenPrompt()
    {
        _state = AiSessionState.PromptOpen;
        _prompt.Visible = true;
        _prompt.Focus();
        SetStatus("Type an instruction, then press Ctrl+Enter.");
    }

    private async Task GeneratePlanAsync()
    {
        var text = _prompt.Text.Trim();
        if (text.Length == 0)
        {
            SetStatus("Enter an instruction first.");
            return;
        }

        _prompt.Visible = false;
        SetState(AiSessionState.Generating, "AI is understanding the instruction...");
        _result.Text = "Generating structured plan...";

        var skill = Math.Clamp(_skill.SelectedIndex + 1, 1, 5);
        var request = new AiCommandRequest(text, skill, AiCommandMode.PlanOnly);
        var response = await _gemini.CreatePlanAsync(request);

        if (response.Success && response.Plan is not null)
        {
            _pendingPlan = response.Plan;
            _result.Text = FormatPlan(response.Plan);
            SetState(AiSessionState.WaitingToBegin, "Press Ctrl to begin.");
            return;
        }

        // The local parser keeps the AI tab usable even when Gemini is not configured.
        var local = _localExpander.Expand(new AiPromptRequest(text, skill));
        _pendingPlan = new AiPlan(
            local.Summary,
            ["Gemini unavailable; using the local deterministic planner."],
            local.Objectives.Select((name, index) => new AiObjective(
                name,
                "Requested objective understood locally.",
                Math.Max(1, 100 - index * 10),
                ["Make measurable progress toward the objective."])).ToArray(),
            local.Behaviors.ToArray(),
            local.Conditions.ToArray(),
            local.EvaluationRules.ToArray(),
            ["Validate the plan, then connect it to the execution layer."]);

        _result.Text = FormatPlan(_pendingPlan);
        SetState(AiSessionState.WaitingToBegin, "Gemini unavailable — local plan ready. Press Ctrl to begin.");
    }

    private static string FormatPlan(AiPlan plan)
    {
        var builder = new StringBuilder();
        builder.AppendLine("AI PLAN");
        builder.AppendLine(new string('=', 52));
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
        Controls.Add(new Label
        {
            Text = "AI PLAY",
            Dock = DockStyle.Top,
            Height = 48,
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            Padding = new Padding(16, 8, 0, 0)
        });

        var options = new Panel { Dock = DockStyle.Top, Height = 48, Padding = new Padding(14, 4, 14, 4) };
        options.Controls.Add(new Label { Text = "Skill:", AutoSize = true, Location = new Point(2, 9) });
        _skill.DropDownStyle = ComboBoxStyle.DropDownList;
        _skill.Items.AddRange(["1 — Beginner", "2 — Basic", "3 — Good", "4 — Strong", "5 — Elite"]);
        _skill.SelectedIndex = 2;
        _skill.Location = new Point(48, 4);
        _skill.Width = 150;
        options.Controls.Add(_skill);
        Controls.Add(options);

        var statusPanel = new Panel { Dock = DockStyle.Bottom, Height = 42 };
        _status.Dock = DockStyle.Fill;
        _status.Padding = new Padding(14, 8, 8, 8);
        _status.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        statusPanel.Controls.Add(_status);
        Controls.Add(statusPanel);

        var promptPanel = new Panel { Dock = DockStyle.Top, Height = 120, Padding = new Padding(14) };
        _prompt.Multiline = true;
        _prompt.AcceptsReturn = true;
        _prompt.Dock = DockStyle.Fill;
        _prompt.Visible = false;
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

    private void SetState(AiSessionState state, string status)
    {
        _state = state;
        SetStatus(status);
    }

    private void SetStatus(string text) => _status.Text = text;
}
