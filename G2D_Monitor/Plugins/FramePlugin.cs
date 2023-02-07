using G2D_Monitor.Manager;
using G2D_Monitor.Plugins.Common;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace G2D_Monitor.Plugins
{
    internal abstract class FramePlugin : Plugin
    {
        private const float MIN_INTERVAL = 1000f / 10;
        private const int FIRST_ROUND_LOADING_TIME = 10000;
        private const int OTHER_ROUND_LOADING_TIME = 1000;

        private readonly TrackBar FrameBar = new();
        private readonly PictureBox FramePanel = new();
        private readonly ContextMenuStrip PanelMenu = new(MainForm.Instance.components);
        private static readonly ToolStripStatusLabel ProgressStatusLabel = NewStatusLabel(148);
        private static readonly ToolStripStatusLabel FrameStatusLabel = NewStatusLabel();

        private static readonly List<FramePlugin> FramePlugins = new();
        private static GameState LastState = GameState.Unknown;
        private static long CurrentRoundStartTime;
        private static long LastCaptureTime;
        private static bool NewRound = true;
        private static bool FirstRound = true;

        private readonly List<List<IFrame>> AllFrames = new();
        private int SelectedRound;
        private string FrameInfo = string.Empty;
        private List<IFrame>? CurrentRoundFrames;

        protected override void Initialize(TabPage tab)
        {
            FramePlugins.Add(this);
            FrameBar.BeginInit();
            ((System.ComponentModel.ISupportInitialize)FramePanel).BeginInit();
            FrameBar.AutoSize = false;
            FrameBar.Dock = DockStyle.Bottom;
            FrameBar.Enabled = false;
            FrameBar.TickStyle = TickStyle.None;
            FrameBar.Height = 28;

            FramePanel.Dock = DockStyle.Fill;
            FramePanel.SizeMode = PictureBoxSizeMode.Zoom;
            FramePanel.TabStop = false;
            tab.Controls.Add(FramePanel);
            tab.Controls.Add(FrameBar);
            FrameBar.EndInit();
            ((System.ComponentModel.ISupportInitialize)FramePanel).EndInit();
            FramePanel.MouseWheel += (_, e) => PanelMouseWheel(e);
            FrameBar.ValueChanged += (_, _) => ValueChanged();
        }

        private void PanelMouseWheel(MouseEventArgs e)
        {
            var v = e.Delta > 0 ? FrameBar.Value + 1 : FrameBar.Value - 1;
            if (v >= FrameBar.Minimum && v <= FrameBar.Maximum) FrameBar.Value = v;
        }

        private void ValueChanged()
        {
            if (SelectedRound >= AllFrames.Count) return;
            var frames = AllFrames[SelectedRound];
            if (frames == null || FrameBar.Value >= frames.Count) return;
            var frame = frames[FrameBar.Value];
            ShowImage(frame);
            FrameStatusLabel.Text = string.Format(FrameInfo, ToSecondText(frame.Time), FrameBar.Value + 1);
        }

        private void ShowImage(IFrame frame)
        {
            if (IsTabActive)
            {
                var img = FramePanel.Image;
                FramePanel.Image = GetImage(frame);
                if (AlwaysDisposeLastImage) img?.Dispose();
            }
        }

        private void CleanImage()
        {
            var img = FramePanel.Image;
            FramePanel.Image = null;
            if (AlwaysDisposeLastImage) img?.Dispose();
        }

        private static ToolStripStatusLabel NewStatusLabel(int width = 0)
        {
            var label = new ToolStripStatusLabel();
            label.TextAlign = ContentAlignment.MiddleLeft;
            if (width > 0) { label.AutoSize = false; label.Width = width; }
            else label.AutoSize = true;
            MainForm.Instance.MainStatusStrip.Items.Add(label);
            return label;
        }

        public static void FrameUpdate(Context context)
        {
            var state = context.State;
            if (state == GameState.InGame)
            {
                var time = Environment.TickCount64;
                if (NewRound)
                {
                    NewRound = false;
                    LastCaptureTime = time;
                    CurrentRoundStartTime = time + (FirstRound ? FIRST_ROUND_LOADING_TIME : OTHER_ROUND_LOADING_TIME);
                    foreach (var plugin in FramePlugins) plugin.CurrentRoundFrames = new();
                }
                else if (time >= CurrentRoundStartTime && time - LastCaptureTime >= MIN_INTERVAL)
                {
                    LastCaptureTime = time;
                    time -= CurrentRoundStartTime;
                    ProgressStatusLabel.Text = $"{Enum.GetName(state)}: {ToSecondText(time)}";
                    foreach (var plugin in FramePlugins) plugin.RoundBegin(context, time);
                }
                if (LastState != state)
                {
                    ProgressStatusLabel.Text = Enum.GetName(state);
                    LastState = state;
                }
            }
            else if (LastState != state)
            {
                FirstRound = false;
                if (state == GameState.InLobby)
                {
                    FirstRound = NewRound = true;
                    FrameStatusLabel.Text = string.Empty;
                    foreach (var plugin in FramePlugins) plugin.NewGame();
                }
                else if (!NewRound)
                {
                    NewRound = true;
                    foreach (var plugin in FramePlugins) plugin.RoundEnd();
                }
                ProgressStatusLabel.Text = Enum.GetName(state);
                LastState = state;
            }
        }

        private void NewGame()
        {
            CleanImage();
            FramePanel.ContextMenuStrip = null;
            PanelMenu.Items.Clear();
            FrameBar.Enabled = false;
            OnGameEnding();
        }

        private void RoundBegin(Context context, long time)
        {
            FrameBar.Enabled = false;
            var frame = GetFrame(context, time);
            CurrentRoundFrames?.Add(frame);
            if (AlwaysShowNewestFrame && IsTabActive) ShowImage(frame);
        }

        private void RoundEnd()
        {
            if (CurrentRoundFrames == null) return;
            int num;
            AllFrames.Add(CurrentRoundFrames);
            num = AllFrames.Count;
            FrameBar.Enabled = true;
            var item = new ToolStripMenuItem("Round " + num);
            item.Tag = num - 1;
            item.Click += RoundItemClick;
            PanelMenu.Items.Add(item);
            if (PanelMenu.Items.Count == 2) FramePanel.ContextMenuStrip = PanelMenu;
            var val = SelectRound(item);
            if (IsTabActive) FrameBar.Value = val;
        }

        private void RoundItemClick(object? sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem item) FrameBar.Value = SelectRound(item);
        }

        private int SelectRound(ToolStripMenuItem item)
        {
            SelectedRound = (int)item.Tag;
            if (SelectedRound >= AllFrames.Count) return 0;
            var frames = AllFrames[SelectedRound];
            foreach (ToolStripMenuItem subItem in item.Owner.Items) subItem.Checked = item == subItem;
            var fps = Math.Round(frames.Count * 1000.0 / frames[^1].Time, 1);
            FrameInfo = "FPS: " + fps + ", Time: {0:F3} / " + ToSecondText(frames[^1].Time) + ", Frame: {1} / " + frames.Count;
            FrameBar.LargeChange = (int)Math.Round(fps);
            return FrameBar.Maximum = frames.Count - 1;
        }

        protected override bool UIRequired => true;

        protected override void OnGameExit() => OnGameEnding();

        protected override void OnClosing() => OnGameEnding();

        private static string ToSecondText(long ms) => (ms / 1000.0).ToString("f3") + 's';

        protected abstract bool AlwaysShowNewestFrame { get; }

        protected abstract bool AlwaysDisposeLastImage { get; }

        protected abstract IFrame GetFrame(Context context, long time);

        protected abstract Image? GetImage(IFrame frame);

        protected virtual void OnGameEnding() { }
    }
}
