using G2D_Monitor.Manager;

namespace G2D_Monitor.Plugins
{
    public abstract class Plugin
    {

        private static readonly List<Plugin> Plugins = new();

        public static void Register<T>() where T : Plugin, new() => Register(new T());

        public static void Register(Plugin plugin)
        {
            if (plugin.UIRequired)
            {
                var tab = new TabPage(plugin.Name);
                MainForm.Instance.MainTabControl.SuspendLayout();
                tab.SuspendLayout();
                MainForm.Instance.MainTabControl.TabPages.Add(tab);
                plugin.Initialize(tab);
                MainForm.Instance.MainTabControl.ResumeLayout(false);
                tab.ResumeLayout(false);
            }
            plugin.Activate();
            Plugins.Add(plugin);
        }

        public static void Close()
        {
            foreach (var plugin in Plugins) plugin.OnClosing();
        }

        public static void GameExit()
        {
            try
            {
                MainForm.Instance.Invoke(() =>
                {
                    foreach (var plugin in Plugins) plugin.OnGameExit();
                });
            }
            catch { }

        }

        public static void Update(Context context)
        {
            try 
            {
                MainForm.Instance.Invoke(() =>
                {
                    foreach (var plugin in Plugins) plugin.OnUpdate(context);
                });
            } 
            catch { }
        }

        protected virtual bool UIRequired => true;

        protected virtual string Name => string.Empty;

        protected virtual void Initialize(TabPage tab) { }

        protected virtual void Activate() { }

        protected virtual void OnUpdate(Context context) { }

        protected virtual void OnGameExit() { }

        protected virtual void OnClosing() { }
    }
}
