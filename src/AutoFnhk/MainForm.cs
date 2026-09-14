using AutoFnhk.Aim;

namespace AutoFnhk;

public sealed class MainForm : Form
{
    public MainForm()
    {
        Text = "AutoFnhk — Fortnoob Test Harness";
        Width = 1000;
        Height = 700;
        MinimumSize = new Size(760, 520);
        BackColor = Color.FromArgb(18, 18, 25);
        ForeColor = Color.White;

        var tabs = new TabControl { Dock = DockStyle.Fill };
        var aimTab = new TabPage("Aim") { BackColor = Color.FromArgb(22, 22, 30) };
        aimTab.Controls.Add(new AimTabControl());
        tabs.TabPages.Add(aimTab);
        Controls.Add(tabs);
    }
}
