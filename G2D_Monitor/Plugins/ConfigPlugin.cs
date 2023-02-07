using G2D_Monitor.Plugins.Common;
using G2D_Monitor.Properties;

namespace G2D_Monitor.Plugins
{
    internal sealed class ConfigPlugin : Plugin
    {
        public static bool TopMost { get => Settings.Default.TopMost; set { MainForm.Instance.TopMost = Settings.Default.TopMost = value; } }

        protected override string Name => "Settings";

        private KeyValuePanel? Panel;

        protected override void Initialize(TabPage tab) =>
            Panel = new KeyValuePanel()
                .AddCheckBox("TopMost", MainForm.Instance.TopMost = TopMost, v => TopMost = v)
                .AddNumeric("Radius", MapPlugin.Radius, 4, 16, v => MapPlugin.Radius = v)
                .AddNumeric("Nickname Size", MapPlugin.Radius, 12, 32, v => MapPlugin.FontSize = v)
                .AddColorSelector("Goose Color", MapPlugin.ColorGoose, v => MapPlugin.ColorGoose = v)
                .AddColorSelector("Suspect Color", MapPlugin.ColorSuspect, v => MapPlugin.ColorSuspect = v)
                .AddColorSelector("Duck Color", MapPlugin.ColorDuck, v => MapPlugin.ColorDuck = v)
                .Attach(tab);

        protected override void Activate() => Panel?.Align();

        protected override void OnClosing() => Settings.Default.Save();
    }
}
