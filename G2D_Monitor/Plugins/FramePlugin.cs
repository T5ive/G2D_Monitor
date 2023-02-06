using G2D_Monitor.Manager;

namespace G2D_Monitor.Plugins
{
    internal abstract class FramePlugin<T> : Plugin
    {
        private const int FIRST_ROUND_LOADING_TIME = 10000;
        private const int OTHER_ROUND_LOADING_TIME = 1000;

        private readonly TrackBar FrameBar = new();
        private readonly PictureBox FramePanel = new();
        private readonly ContextMenuStrip PanelMenu = NewMenuStrip();
        private static readonly ToolStripStatusLabel ProgressStatusLabel = NewStatusLabel(148);
        private static readonly ToolStripStatusLabel FrameStatusLabel = NewStatusLabel();
        
        private readonly List<List<Frame>> AllFrames = new();

        private int SelectedRound;
        private string FrameInfo = string.Empty;

        private List<Frame>? CurrentRoundFrames;
        private GameState LastState = GameState.Unknown;
        private long CurrentRoundStartTime;
        private long LastCaptureTime;

        protected override void Initialize(TabPage tab)
        {
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

        private void ShowImage(Frame? frame)
        {
            if (IsTabActive || !frame.HasValue)
            {
                var img = FramePanel.Image;
                if (frame.HasValue) FramePanel.Image = GetImage(frame.Value.Data);
                if (AlwaysDisposeLastImage) img?.Dispose();
            }
        }

        protected override void DoUpdate(Context context)
        {
            var state = context.State;
            if (state == GameState.InGame)
            {
                var time = Environment.TickCount64;
                if (CurrentRoundFrames == null)
                {
                    LastCaptureTime = time;
                    CurrentRoundFrames = new();
                    CurrentRoundStartTime = time + (AllFrames.Count == 0 ? FIRST_ROUND_LOADING_TIME : OTHER_ROUND_LOADING_TIME);
                }
                else if (time >= CurrentRoundStartTime && time - LastCaptureTime >= 1000 / MaxFPS)
                {
                    LastCaptureTime = time;
                    time -= CurrentRoundStartTime;
                    ProgressStatusLabel.Text = $"{Enum.GetName(state)}: {ToSecondText(time)}";
                    var frame = new Frame(GetFrameData(context), time);
                    CurrentRoundFrames.Add(frame);
                    if (AlwaysShowNewestFrame && IsTabActive) ShowImage(frame);
                }
                if (LastState != state)
                {
                    ProgressStatusLabel.Text = Enum.GetName(state);
                    LastState = state;
                }
            }
            else if (LastState != state)
            {
                if (state == GameState.InLobby)
                {
                    ShowImage(null);
                    FramePanel.ContextMenuStrip = null;
                    PanelMenu.Items.Clear();
                    FrameBar.Enabled = false;
                    FrameStatusLabel.Text = string.Empty;
                    OnGameEnding();
                }
                else if (CurrentRoundFrames != null)
                {
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
                CurrentRoundFrames = null;
                ProgressStatusLabel.Text = Enum.GetName(state);
                LastState = state;
            }
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

        protected virtual float MaxFPS => 10;

        protected override void OnGameExit() => OnGameEnding();

        protected override void OnClosing() => OnGameEnding();

        private static string ToSecondText(long ms) => (ms / 1000.0).ToString("f3") + 's';

        protected abstract bool AlwaysShowNewestFrame { get; }

        protected abstract bool AlwaysDisposeLastImage { get; }

        protected abstract T GetFrameData(Context context);

        protected abstract Image? GetImage(T data);

        protected virtual void OnGameEnding() { }

        protected readonly struct Frame
        {
            public readonly T Data;
            public readonly long Time;

            public Frame(T data, long time)
            {
                Data = data;
                Time = time;
            }
        }
    }
}
