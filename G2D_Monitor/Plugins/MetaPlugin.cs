using G2D_Monitor.Manager;
using System.Reflection;

namespace G2D_Monitor.Plugins
{
    internal class MetaPlugin : Plugin
    {
        private readonly ListView MainListView = NewListView();

        protected override string Name => "Meta";

        protected override void OnGameExit()
        {
            foreach (ListViewItem item in MainListView.Items) item.SubItems[1].Text = string.Empty;
        }

        protected override void DoUpdate(Context context)
        {
            foreach (ListViewItem item in MainListView.Items)
            {
                if (item.Tag is PropertyInfo prop)
                {
                    var text = prop.GetValue(context)?.ToString() ?? string.Empty;
                    if (!item.SubItems[1].Text.Equals(text)) item.SubItems[1].Text = text;
                }
            }
        }

        protected override void Initialize(TabPage tab)
        {
            tab.Controls.Add(MainListView);
            var props = typeof(Context).GetProperties().ToList();
            props.Sort((a, b) => a.Name.CompareTo(b.Name));
            foreach (var prop in props)
            {
                var item = MainListView.Items.Add(prop.Name);
                item.Tag = prop;
                item.SubItems.Add(string.Empty);
            }
        }
    }
}
