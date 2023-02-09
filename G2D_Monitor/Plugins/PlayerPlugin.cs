using G2D_Monitor.Manager;
using System.Reflection;

namespace G2D_Monitor.Plugins
{
    internal sealed class PlayerPlugin : Plugin
    {
        private readonly ListView MainListView;

        protected override string Name => "Players";

        private PlayerPlugin()
        {
            MainListView = new ListView
            {
                Dock = DockStyle.Fill,
                FullRowSelect = true,
                GridLines = true,
                UseCompatibleStateImageBehavior = false,
                View = View.Details
            };
            MainListView.Columns.Add("Key", 200);
            MainListView.Columns.Add("Value", 540);
        }

        protected override void OnGameExit()
        {
            foreach (ListViewItem item in MainListView.Items) item.SubItems[1].Text = string.Empty;
        }

        protected override void OnUpdate(Context context)
        {
            foreach (ListViewItem item in MainListView.Items)
            {
                if (item.Tag is KeyValuePair<int, PropertyInfo> pair)
                {
                    var text = pair.Value.GetValue(context.Players[pair.Key])?.ToString() ?? string.Empty;
                    if (!item.SubItems[1].Text.Equals(text)) item.SubItems[1].Text = text;
                }
            }
        }

        protected override void Initialize(TabPage tab)
        {
            tab.Controls.Add(MainListView);
            var props = typeof(Player).GetProperties().ToList();
            props.Sort((a, b) => a.Name.CompareTo(b.Name));
            for (var i = 0; i < Context.PLAYER_NUM; i++)
            {
                var group = new ListViewGroup("Player " + (i + 1));
                MainListView.Groups.Add(group);
                foreach (var prop in props)
                {
                    var item = MainListView.Items.Add(prop.Name);
                    item.Group = group;
                    item.Tag = new KeyValuePair<int, PropertyInfo>(i, prop);
                    item.SubItems.Add(string.Empty);
                }
            }
            MainListView.MouseDoubleClick += (_, e) => OnDoubleClick(e);
        }

        private void OnDoubleClick(MouseEventArgs e)
        {
            var item = MainListView.GetItemAt(e.X, e.Y);
            if (item != null) Clipboard.SetText(item.SubItems[1].Text);
        }
    }
}
