using G2D_Monitor.Manager;

namespace G2D_Monitor.Plugins
{
    public abstract class Plugin
    {
        private static readonly List<Plugin> plugins = new();

        public static void Register<T>() where T : Plugin, new() => Register(new T());

        public static void Register(Plugin plugin)
        {
            var tab = new TabPage(plugin.Name);
            MainForm.Instance.MainTabControl.SuspendLayout();
            tab.SuspendLayout();
            MainForm.Instance.MainTabControl.TabPages.Add(tab);
            plugin.Initialize(tab);
            MainForm.Instance.MainTabControl.ResumeLayout(false);
            tab.ResumeLayout(false);
            lock (plugins) plugins.Add(plugin);
        }

        public static void Update(Context? context)
        {
            var list = new List<Plugin>();
            lock (plugins) list.AddRange(plugins);
            try { MainForm.Instance.Invoke(() => { foreach (var plugin in list) plugin.DoUpdate(context); }); } catch { }
        }

        protected static ListView NewListView()
        {
            var listView = new ListView();
            listView.Dock = DockStyle.Fill;
            listView.FullRowSelect = true;
            listView.GridLines = true;
            listView.UseCompatibleStateImageBehavior = false;
            listView.View = View.Details;
            listView.Columns.Add("Key", 200);
            listView.Columns.Add("Value", 540);
            return listView;
        }

        protected abstract string Name { get; }

        protected abstract void Initialize(TabPage tab);

        protected abstract void DoUpdate(Context? context);
    }
}
