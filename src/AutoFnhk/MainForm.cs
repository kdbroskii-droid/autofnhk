using AutoFnhk.Aim;
using AutoFnhk.Settings;
using AutoFnhk.Weapon;

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

        var weaponTab = new TabPage("Weapon") { BackColor = Color.FromArgb(22, 22, 30) };
        weaponTab.Controls.Add(new WeaponTabControl());
        tabs.TabPages.Add(weaponTab);

        var settingsTab = new TabPage("Settings") { BackColor = Color.FromArgb(22, 22, 30) };
        settingsTab.Controls.Add(new SettingsTabControl());
        tabs.TabPages.Add(settingsTab);

        Controls.Add(tabs);
    }
}
