using System.Drawing;
using System.Windows.Forms;

namespace AutoFnhk.Settings;

/// <summary>Central settings UI for the Fortnoob private test harness.</summary>
public sealed class SettingsTabControl : UserControl
{
    private readonly ComboBox _skill = new();
    private readonly CheckBox _aimLock = new();
    private readonly CheckBox _autoSmoothing = new();
    private readonly CheckedListBox _autoPlayFeatures = new();
    private readonly CheckBox _diagnostics = new();
    private readonly CheckBox _learning = new();
    private readonly CheckBox _randomVariation = new();
    private readonly TextBox _commandPrompt = new();
    private readonly RichTextBox _log = new();

    private static readonly string[] AutoPlayFeatures =
    [
        "Target detection",
        "Target selection",
        "Aim movement",
        "Auto smoothing",
        "Movement",
        "Positioning",
        "Building",
        "Editing",
        "Piece control",
        "Rotation",
        "Survival",
        "Elimination objective",
        "Third-party awareness",
        "Adaptation",
        "Difficulty scaling",
        "Goal priorities",
        "Combo / sequence handling",
        "Conditional decisions",
        "Learning / feedback"
    ];

    public SettingsTabControl()
    {
        Dock = DockStyle.Fill;
        BackColor = Color.FromArgb(22, 22, 30);
        ForeColor = Color.White;
        BuildUi();
    }

    private void BuildUi()
    {
        var split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Vertical, SplitterDistance = 360, Padding = new Padding(12) };
        Controls.Add(split);

        var left = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true };
        split.Panel1.Controls.Add(left);

        var title = new Label { Text = "SETTINGS", AutoSize = true, Font = new Font("Segoe UI", 18, FontStyle.Bold), Margin = new Padding(5, 5, 5, 15) };
        left.Controls.Add(title);

        ConfigureCheck(_aimLock, "Aim lock");
        ConfigureCheck(_autoSmoothing, "Auto smoothing");
        _aimLock.Checked = true;
        _autoSmoothing.Checked = true;
        left.Controls.Add(_aimLock);
        left.Controls.Add(_autoSmoothing);

        var autoPlayLabel = new Label
        {
            Text = "Auto Play features (double-click to select all)",
            AutoSize = true,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Margin = new Padding(5, 14, 5, 5)
        };
        left.Controls.Add(autoPlayLabel);

        _autoPlayFeatures.Width = 320;
        _autoPlayFeatures.Height = 260;
        _autoPlayFeatures.CheckOnClick = true;
        _autoPlayFeatures.BackColor = Color.FromArgb(14, 14, 20);
        _autoPlayFeatures.ForeColor = Color.White;
        _autoPlayFeatures.BorderStyle = BorderStyle.FixedSingle;
        _autoPlayFeatures.Items.AddRange(AutoPlayFeatures);
        _autoPlayFeatures.ItemCheck += AutoPlayFeatures_ItemCheck;
        left.Controls.Add(_autoPlayFeatures);

        ConfigureCheck(_diagnostics, "Diagnostics / event logging");
        ConfigureCheck(_learning, "Learning / performance feedback");
        ConfigureCheck(_randomVariation, "Human-like variation");
        left.Controls.Add(_diagnostics);
        left.Controls.Add(_learning);
        left.Controls.Add(_randomVariation);

        _skill.DropDownStyle = ComboBoxStyle.DropDownList;
        _skill.Items.AddRange(["1 — Beginner", "2 — Basic", "3 — Competent", "4 — Advanced", "5 — Expert"]);
        _skill.SelectedIndex = 2;
        left.Controls.Add(MakeRow("Skill level", _skill));

        var apply = new Button { Text = "Apply settings", Width = 300, Height = 36, Margin = new Padding(5, 12, 5, 5) };
        apply.Click += (_, _) => Apply();
        left.Controls.Add(apply);

        var right = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4 };
        right.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
        right.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
        right.RowStyles.Add(new RowStyle(SizeType.Absolute, 90));
        right.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        split.Panel2.Controls.Add(right);

        right.Controls.Add(new Label { Text = "AUTO PLAY COMMAND", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 14, FontStyle.Bold), Padding = new Padding(5, 8, 0, 0) }, 0, 0);
        right.Controls.Add(new Label { Text = "Type a high-level instruction for the AI planner. Selected Auto Play features control which capabilities are enabled.", Dock = DockStyle.Fill, Padding = new Padding(5) }, 0, 1);

        _commandPrompt.Multiline = true;
        _commandPrompt.Dock = DockStyle.Fill;
        _commandPrompt.ScrollBars = ScrollBars.Vertical;
        _commandPrompt.BackColor = Color.FromArgb(14, 14, 20);
        _commandPrompt.ForeColor = Color.White;
        _commandPrompt.PlaceholderText = "Example: practice a 1v1, then free build and improve edit speed.";
        right.Controls.Add(_commandPrompt, 0, 2);

        _log.Dock = DockStyle.Fill;
        _log.ReadOnly = true;
        _log.BackColor = Color.FromArgb(14, 14, 20);
        _log.ForeColor = Color.Gainsboro;
        _log.Font = new Font("Consolas", 9.5f);
        right.Controls.Add(_log, 0, 3);
    }

    private void AutoPlayFeatures_ItemCheck(object? sender, ItemCheckEventArgs e)
    {
        if (e.Index != 0 || e.NewValue != CheckState.Checked)
            return;

        // Double-click the list to select every Auto Play capability.
        if (MouseButtons != MouseButtons.Left)
            return;

        // ItemCheck fires before the state changes, so defer the select-all operation.
        BeginInvoke(new Action(() =>
        {
            if (_autoPlayFeatures.Focused && _autoPlayFeatures.SelectedIndex >= 0)
            {
                for (var i = 0; i < _autoPlayFeatures.Items.Count; i++)
                    _autoPlayFeatures.SetItemChecked(i, true);
            }
        }));
    }

    private static void ConfigureCheck(CheckBox box, string text)
    {
        box.Text = text;
        box.AutoSize = true;
        box.Margin = new Padding(5, 7, 5, 7);
    }

    private static Control MakeRow(string label, Control editor)
    {
        var panel = new Panel { Width = 310, Height = 42, Margin = new Padding(3, 5, 3, 5) };
        panel.Controls.Add(new Label { Text = label, AutoSize = false, Width = 150, Height = 32, TextAlign = ContentAlignment.MiddleLeft });
        editor.Location = new Point(150, 2);
        editor.Width = 155;
        panel.Controls.Add(editor);
        return panel;
    }

    private void Apply()
    {
        var selected = _autoPlayFeatures.CheckedItems.Count;
        _log.AppendText($"[{DateTime.Now:HH:mm:ss}] Settings applied: AimLock={_aimLock.Checked}, AutoSmoothing={_autoSmoothing.Checked}, AutoPlayFeatures={selected}/{_autoPlayFeatures.Items.Count}, Skill={_skill.SelectedIndex + 1}, Learning={_learning.Checked}, Variation={_randomVariation.Checked}.{Environment.NewLine}");
        if (!string.IsNullOrWhiteSpace(_commandPrompt.Text))
            _log.AppendText($"[{DateTime.Now:HH:mm:ss}] AI command queued: {_commandPrompt.Text.Trim()}{Environment.NewLine}");
    }
}
