using System.Numerics;
using System.Windows.Forms;

namespace AutoFnhk.Aim;

/// <summary>
/// Settings and diagnostics tab for the Fortnoob private test harness.
/// No overlay is created; diagnostics stay inside this tab.
/// </summary>
public sealed class AimTabControl : UserControl
{
    private readonly AimSettings _settings = new();
    private readonly TargetDetector _detector = new();
    private readonly TargetSelector _selector = new();
    private readonly AimController _controller;
    private readonly CheckBox _enabled = new();
    private readonly CheckBox _headPriority = new();
    private readonly CheckBox _torsoFallback = new();
    private readonly CheckBox _confidenceRequired = new();
    private readonly NumericUpDown _minConfidence = new();
    private readonly NumericUpDown _closeSmoothing = new();
    private readonly NumericUpDown _farSmoothing = new();
    private readonly NumericUpDown _snapDistance = new();
    private readonly NumericUpDown _referenceDistance = new();
    private readonly NumericUpDown _maxDistance = new();
    private readonly ComboBox _targetPoint = new();
    private readonly Label _status = new();
    private readonly RichTextBox _log = new();

    public AimTabControl()
    {
        _controller = new AimController(_settings);
        Dock = DockStyle.Fill;
        BackColor = System.Drawing.Color.FromArgb(22, 22, 30);
        ForeColor = System.Drawing.Color.White;
        BuildUi();
        LoadSettingsIntoUi();
        WriteLog("Aim tab ready — waiting for Fortnoob test-target data.");
    }

    private void BuildUi()
    {
        var title = new Label
        {
            Text = "AIM",
            Dock = DockStyle.Top,
            Height = 46,
            Font = new System.Drawing.Font("Segoe UI", 18, System.Drawing.FontStyle.Bold),
            Padding = new Padding(16, 8, 0, 0)
        };
        Controls.Add(title);

        var split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Vertical,
            SplitterDistance = 330,
            Padding = new Padding(12)
        };
        Controls.Add(split);

        var settingsPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true
        };
        split.Panel1.Controls.Add(settingsPanel);

        ConfigureCheck(_enabled, "Aim enabled");
        ConfigureCheck(_headPriority, "Prefer head");
        ConfigureCheck(_torsoFallback, "Allow torso fallback");
        ConfigureCheck(_confidenceRequired, "Require confidence");

        settingsPanel.Controls.Add(_enabled);
        settingsPanel.Controls.Add(_headPriority);
        settingsPanel.Controls.Add(_torsoFallback);
        settingsPanel.Controls.Add(_confidenceRequired);

        ConfigureNumber(_minConfidence, "Minimum confidence", 0, 1, 0.01m, 2);
        ConfigureNumber(_closeSmoothing, "Close smoothing", 0, 100, 0.5m, 1);
        ConfigureNumber(_farSmoothing, "Far smoothing", 0, 100, 0.5m, 1);
        ConfigureNumber(_snapDistance, "Snap distance", 0, 100, 0.1m, 1);
        ConfigureNumber(_referenceDistance, "Reference distance", 1, 10000, 50, 0);
        ConfigureNumber(_maxDistance, "Maximum target distance", 1, 10000, 50, 0);

        AddSetting(settingsPanel, "Minimum confidence", _minConfidence);
        AddSetting(settingsPanel, "Close smoothing", _closeSmoothing);
        AddSetting(settingsPanel, "Far smoothing", _farSmoothing);
        AddSetting(settingsPanel, "Snap distance", _snapDistance);
        AddSetting(settingsPanel, "Reference distance", _referenceDistance);
        AddSetting(settingsPanel, "Maximum distance", _maxDistance);

        _targetPoint.DropDownStyle = ComboBoxStyle.DropDownList;
        _targetPoint.Items.AddRange(["Head", "Torso"]);
        AddSetting(settingsPanel, "Target point", _targetPoint);

        var apply = new Button { Text = "Apply settings", Width = 290, Height = 36 };
        apply.Click += (_, _) => ApplySettings();
        settingsPanel.Controls.Add(apply);

        var clear = new Button { Text = "Clear log", Width = 290, Height = 32 };
        clear.Click += (_, _) => _log.Clear();
        settingsPanel.Controls.Add(clear);

        var right = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
        right.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        right.RowStyles.Add(new RowStyle(SizeType.Absolute, 75));
        right.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        split.Panel2.Controls.Add(right);

        _status.Text = "Status: idle";
        _status.Dock = DockStyle.Fill;
        _status.Font = new System.Drawing.Font("Segoe UI", 11, System.Drawing.FontStyle.Bold);
        _status.Padding = new Padding(8);
        right.Controls.Add(_status, 0, 0);

        var info = new Label
        {
            Dock = DockStyle.Fill,
            Text = "All diagnostics stay inside this tab. Example: target detected → target selected → aim point calculated. No screen overlay is used.",
            AutoSize = false,
            Padding = new Padding(8)
        };
        right.Controls.Add(info, 0, 1);

        _log.Dock = DockStyle.Fill;
        _log.ReadOnly = true;
        _log.BackColor = System.Drawing.Color.FromArgb(14, 14, 20);
        _log.ForeColor = System.Drawing.Color.Gainsboro;
        _log.Font = new System.Drawing.Font("Consolas", 9.5f);
        right.Controls.Add(_log, 0, 2);
    }

    private static void ConfigureCheck(CheckBox box, string text)
    {
        box.Text = text;
        box.AutoSize = true;
        box.Margin = new Padding(5, 7, 5, 7);
    }

    private static void ConfigureNumber(NumericUpDown box, string label, decimal min, decimal max, decimal increment, int decimals)
    {
        box.Minimum = min;
        box.Maximum = max;
        box.Increment = increment;
        box.DecimalPlaces = decimals;
        box.Width = 125;
        box.Tag = label;
    }

    private static void AddSetting(FlowLayoutPanel parent, string label, Control editor)
    {
        var row = new Panel { Width = 300, Height = 42, Margin = new Padding(3, 2, 3, 2) };
        var text = new Label
        {
            Text = label,
            AutoSize = false,
            Width = 170,
            Height = 35,
            TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        };
        editor.Location = new System.Drawing.Point(170, 2);
        row.Controls.Add(text);
        row.Controls.Add(editor);
        parent.Controls.Add(row);
    }

    private void LoadSettingsIntoUi()
    {
        _enabled.Checked = _settings.Enabled;
        _headPriority.Checked = _settings.HeadPriority;
        _torsoFallback.Checked = _settings.AllowTorsoFallback;
        _confidenceRequired.Checked = _settings.RequireConfidence;
        _minConfidence.Value = (decimal)_settings.MinimumConfidence;
        _closeSmoothing.Value = (decimal)_settings.CloseSmoothing;
        _farSmoothing.Value = (decimal)_settings.FarSmoothing;
        _snapDistance.Value = (decimal)_settings.SnapDistance;
        _referenceDistance.Value = (decimal)_settings.ReferenceDistance;
        _maxDistance.Value = (decimal)_settings.MaximumTargetDistance;
        _targetPoint.SelectedIndex = _settings.HeadPriority ? 0 : 1;
    }

    private void ApplySettings()
    {
        _settings.Enabled = _enabled.Checked;
        _settings.HeadPriority = _targetPoint.SelectedIndex == 0;
        _settings.AllowTorsoFallback = _torsoFallback.Checked;
        _settings.RequireConfidence = _confidenceRequired.Checked;
        _settings.MinimumConfidence = (float)_minConfidence.Value;
        _settings.CloseSmoothing = (float)_closeSmoothing.Value;
        _settings.FarSmoothing = (float)_farSmoothing.Value;
        _settings.SnapDistance = (float)_snapDistance.Value;
        _settings.ReferenceDistance = (float)_referenceDistance.Value;
        _settings.MaximumTargetDistance = (float)_maxDistance.Value;
        _status.Text = $"Status: {(_settings.Enabled ? "enabled" : "disabled")}";
        WriteLog("Settings applied.");
    }

    /// <summary>Feeds game-owned test observations into the aim pipeline.</summary>
    public AimTarget? ProcessTestObservations(IEnumerable<TargetObservation> observations, Vector2 currentAim, float deltaSeconds)
    {
        ApplySettings();
        var targets = _detector.Detect(observations, _settings);
        WriteLog($"Detected {targets.Count} valid Fortnoob test target(s).");

        var target = _selector.Select(targets, _settings);
        if (target is null)
        {
            _status.Text = "Status: no valid target";
            WriteLog("No valid target selected.");
            return null;
        }

        var next = _controller.CalculateAimPoint(currentAim, target.Value, deltaSeconds);
        _status.Text = $"Status: target {target.Value.ClassName} @ {target.Value.Distance:0}m";
        WriteLog($"Target selected: {target.Value.ClassName}, confidence {target.Value.Confidence:P0}, aim → ({next.X:0.0}, {next.Y:0.0}).");
        return target;
    }

    private void WriteLog(string message)
    {
        _log.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
    }
}
