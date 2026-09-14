namespace AutoFnhk.Input;

public sealed class KeyboardTabControl : UserControl
{
    private readonly KeyboardInputService _keyboard = new();
    private readonly ListBox _actions = new();
    private readonly Label _status = new();

    public KeyboardTabControl()
    {
        Dock = DockStyle.Fill;
        BackColor = Color.FromArgb(22, 22, 30);
        ForeColor = Color.White;

        var title = new Label
        {
            Text = "Keyboard Input — Fortnoob Test Harness",
            Dock = DockStyle.Top,
            Height = 48,
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            Padding = new Padding(12, 10, 0, 0)
        };

        var info = new Label
        {
            Text = "Test each action below. Bindings can be changed in KeyboardSettings.cs.",
            Dock = DockStyle.Top,
            Height = 32,
            Padding = new Padding(12, 0, 0, 0)
        };

        _actions.Dock = DockStyle.Fill;
        _actions.BackColor = Color.FromArgb(30, 30, 40);
        _actions.ForeColor = Color.White;
        _actions.BorderStyle = BorderStyle.FixedSingle;

        foreach (var binding in _keyboard.Settings.Bindings)
            _actions.Items.Add($"{binding.Key}  →  {binding.Value}");

        var pressButton = new Button
        {
            Text = "Press Selected",
            Width = 140,
            Height = 36,
            Margin = new Padding(6)
        };
        pressButton.Click += async (_, _) => await PressSelectedAsync();

        var holdButton = new Button
        {
            Text = "Hold Selected (500ms)",
            Width = 170,
            Height = 36,
            Margin = new Padding(6)
        };
        holdButton.Click += async (_, _) => await HoldSelectedAsync();

        _status.Text = "Ready";
        _status.AutoSize = true;
        _status.Padding = new Padding(6, 12, 0, 0);

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 60,
            Padding = new Padding(6),
            BackColor = Color.FromArgb(26, 26, 35)
        };
        buttons.Controls.Add(pressButton);
        buttons.Controls.Add(holdButton);
        buttons.Controls.Add(_status);

        Controls.Add(_actions);
        Controls.Add(buttons);
        Controls.Add(info);
        Controls.Add(title);
    }

    private async Task PressSelectedAsync()
    {
        if (_actions.SelectedIndex < 0)
        {
            _status.Text = "Select an action first.";
            return;
        }

        var action = GetSelectedAction();
        if (action is null)
            return;

        _status.Text = $"Pressing {action.Value.Key}...";
        var success = await _keyboard.PressBindingAsync(action.Value.Action);
        _status.Text = success ? $"Pressed {action.Value.Key}" : "Input failed";
    }

    private async Task HoldSelectedAsync()
    {
        if (_actions.SelectedIndex < 0)
        {
            _status.Text = "Select an action first.";
            return;
        }

        var action = GetSelectedAction();
        if (action is null)
            return;

        _status.Text = $"Holding {action.Value.Key}...";
        var success = await _keyboard.HoldBindingAsync(action.Value.Action, 500);
        _status.Text = success ? $"Held {action.Value.Key}" : "Input failed";
    }

    private (string Action, string Key)? GetSelectedAction()
    {
        if (_actions.SelectedIndex < 0)
            return null;

        var item = _actions.Items[_actions.SelectedIndex]?.ToString();
        if (string.IsNullOrWhiteSpace(item))
            return null;

        var separator = item.IndexOf("  →  ", StringComparison.Ordinal);
        if (separator < 0)
            return null;

        var action = item[..separator].Trim();
        var key = item[(separator + 5)..].Trim();
        return (action, key);
    }
}
