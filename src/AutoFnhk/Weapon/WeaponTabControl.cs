using System.Drawing;
using System.Windows.Forms;

namespace AutoFnhk.Weapon;

/// <summary>Weapon recognition settings and diagnostics. No screen overlay is used.</summary>
public sealed class WeaponTabControl : UserControl
{
    private readonly ComboBox _weapon = new();
    private readonly NumericUpDown _confidence = new();
    private readonly Label _status = new();
    private readonly RichTextBox _log = new();

    public WeaponTabControl()
    {
        Dock = DockStyle.Fill;
        BackColor = Color.FromArgb(22, 22, 30);
        ForeColor = Color.White;
        BuildUi();
    }

    private void BuildUi()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(16),
            ColumnCount = 1,
            RowCount = 5
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        Controls.Add(layout);

        var title = new Label { Text = "WEAPON RECOGNITION", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 16, FontStyle.Bold) };
        layout.Controls.Add(title, 0, 0);

        _weapon.DropDownStyle = ComboBoxStyle.DropDownList;
        _weapon.Items.AddRange(["Unknown", "Shotgun", "Assault Rifle", "Sniper"]);
        _weapon.SelectedIndex = 0;
        layout.Controls.Add(MakeRow("Test weapon", _weapon), 0, 1);

        _confidence.Minimum = 0;
        _confidence.Maximum = 100;
        _confidence.Value = 70;
        _confidence.Width = 140;
        layout.Controls.Add(MakeRow("Minimum confidence %", _confidence), 0, 2);

        var test = new Button { Text = "Run recognition test", Width = 190, Height = 32 };
        test.Click += (_, _) => RunTest();
        layout.Controls.Add(test, 0, 3);

        var bottom = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
        bottom.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        bottom.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        _status.Text = "Status: idle";
        _status.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        _status.Padding = new Padding(4);
        bottom.Controls.Add(_status, 0, 0);
        _log.Dock = DockStyle.Fill;
        _log.ReadOnly = true;
        _log.BackColor = Color.FromArgb(14, 14, 20);
        _log.ForeColor = Color.Gainsboro;
        _log.Font = new Font("Consolas", 9.5f);
        bottom.Controls.Add(_log, 0, 1);
        layout.Controls.Add(bottom, 0, 4);
    }

    private static Control MakeRow(string label, Control editor)
    {
        var panel = new Panel { Dock = DockStyle.Fill };
        panel.Controls.Add(new Label { Text = label, AutoSize = true, Location = new Point(4, 10) });
        editor.Location = new Point(190, 5);
        panel.Controls.Add(editor);
        return panel;
    }

    private void RunTest()
    {
        var type = _weapon.SelectedIndex switch
        {
            1 => WeaponType.Shotgun,
            2 => WeaponType.AssaultRifle,
            3 => WeaponType.Sniper,
            _ => WeaponType.Unknown
        };
        var recognizer = new WeaponRecognizer();
        var result = recognizer.Recognize(new WeaponObservation(_weapon.Text, (float)_confidence.Value / 100f));
        _status.Text = $"Status: {result.Type}";
        _log.AppendText($"[{DateTime.Now:HH:mm:ss}] Recognized {result.DisplayName} → {result.Type}, confidence {result.Confidence:P0}.{Environment.NewLine}");
    }
}
