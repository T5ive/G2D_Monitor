using G2D_Monitor.Manager;
using G2D_Monitor.Properties;
using System.Drawing.Text;
using System.Numerics;

namespace G2D_Monitor.Plugins
{
    internal class MapPlugin : Plugin
    {
        private readonly PictureBox FramePanel = new();
        private readonly ContextMenuStrip PanelMenu = new(MainForm.Instance.components);
        private readonly ToolStripStatusLabel ProgressStatusLabel = NewStatusLabel(148);
        private readonly ToolStripStatusLabel FrameStatusLabel = NewStatusLabel();

        private readonly List<List<Frame>> AllFrames = new();
        private int SelectedRound;
        private string FrameInfo = string.Empty;
        private int FrameIndex;
        private bool MousePressed;

        #region UI Initialization

        protected override string Name => "Map";

        protected override bool UIRequired => true;

        private static ToolStripStatusLabel NewStatusLabel(int width = 0)
        {
            var label = new ToolStripStatusLabel();
            label.TextAlign = ContentAlignment.MiddleLeft;
            if (width > 0) { label.AutoSize = false; label.Width = width; }
            else label.AutoSize = true;
            MainForm.Instance.MainStatusStrip.Items.Add(label);
            return label;
        }

        protected override void Initialize(TabPage tab)
        {
            ((System.ComponentModel.ISupportInitialize)FramePanel).BeginInit();
            FramePanel.Dock = DockStyle.Fill;
            FramePanel.SizeMode = PictureBoxSizeMode.Zoom;
            FramePanel.TabStop = false;
            tab.Controls.Add(FramePanel);
            ((System.ComponentModel.ISupportInitialize)FramePanel).EndInit();
            FramePanel.MouseWheel += (_, e) => PanelMouseWheel(e);
            FramePanel.MouseMove += (_, e) => PanelMouseMove(e);
            FramePanel.MouseDown += (_, _) => MousePressed = true;
            FramePanel.MouseLeave += (_, _) => MousePressed = false;
            FramePanel.MouseUp += (_, _) => MousePressed = false;
        }

        private void SwitchFrame(int index, bool force = false)
        {
            if (SelectedRound >= AllFrames.Count || index < 0 || !force && index == FrameIndex) return;
            var frames = AllFrames[SelectedRound];
            if (index >= frames.Count) return;
            var frame = frames[FrameIndex = index];
            FramePanel.Image = GetImage(frame);
            FrameStatusLabel.Text = string.Format(FrameInfo, ToSecondText(frame.Time), index + 1);
        }

        private void PanelMouseWheel(MouseEventArgs e) => SwitchFrame(e.Delta > 0 ? FrameIndex + 1 : FrameIndex - 1);

        private void PanelMouseMove(MouseEventArgs e)
        {
            if (MousePressed)
            {
                if (SelectedRound >= AllFrames.Count) { MousePressed = false; return; }
                var max = AllFrames[SelectedRound].Count;
                var index = (int)Math.Round((e.X + 0.0) / FramePanel.Width * max);
                if (index < 0) index = 0;
                else if (index >= max) index = max - 1;
                SwitchFrame(index);
            }
        }

        #endregion

        #region Recoding

        private const float MIN_INTERVAL = 1000f / 10;
        private const int FIRST_ROUND_LOADING_TIME = 10000;
        private const int OTHER_ROUND_LOADING_TIME = 1000;

        private GameState LastState = GameState.Unknown;
        private long LastCaptureTime;
        private long RecodingStartTime;
        private List<Frame>? RecodingFrames;

        protected override void OnUpdate(Context context)
        {
            var state = context.State;
            if (state == GameState.InGame)
            {
                var time = Environment.TickCount64;
                if (RecodingFrames == null)
                {
                    LastCaptureTime = time;
                    RecodingStartTime = time + (AllFrames.Count == 0 ? FIRST_ROUND_LOADING_TIME : OTHER_ROUND_LOADING_TIME);
                    RecodingFrames = new();
                    MousePressed = false;
                }
                else if (time >= RecodingStartTime && time - LastCaptureTime >= MIN_INTERVAL)
                {
                    LastCaptureTime = time;
                    time -= RecodingStartTime;
                    ProgressStatusLabel.Text = $"{Enum.GetName(state)}: {ToSecondText(time)}";

                    var frame = GetFrame(context, time);
                    RecodingFrames.Add(frame);
                    //todo: No need to consider 'MousePressed'
                    FramePanel.Image = GetImage(frame); 
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
                    RecodingFrames = null;
                    FrameStatusLabel.Text = string.Empty;
                    //Reset Components
                    FramePanel.Image = null;
                    FramePanel.ContextMenuStrip = null;
                    PanelMenu.Items.Clear();
                    MousePressed = false;
                    //Reset Data
                    AllFrames.Clear();
                    OnGameEnding();
                }
                else if (RecodingFrames != null)
                {
                    int num;
                    AllFrames.Add(RecodingFrames);
                    RecodingFrames = null;
                    num = AllFrames.Count;
                    var item = new ToolStripMenuItem("Round " + num);
                    item.Tag = num - 1;
                    item.Click += RoundItemClick;
                    PanelMenu.Items.Add(item);
                    FramePanel.ContextMenuStrip = PanelMenu;
                    SelectRound(item, true);
                }
                ProgressStatusLabel.Text = Enum.GetName(state);
                LastState = state;
            }
        }

        private void RoundItemClick(object? sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem item) SelectRound(item);
        }

        private void SelectRound(ToolStripMenuItem item, bool force = false)
        {
            var idx = (int)item.Tag;
            if (idx >= AllFrames.Count || !force && SelectedRound == idx) return;
            MousePressed = false;
            var frames = AllFrames[SelectedRound = idx];
            foreach (ToolStripMenuItem subItem in item.Owner.Items) subItem.Checked = item == subItem;
            var fps = Math.Round(frames.Count * 1000.0 / frames[^1].Time, 1);
            FrameInfo = "FPS: " + fps + ", Time: {0:F3} / " + ToSecondText(frames[^1].Time) + ", Frame: {1} / " + frames.Count;
            SwitchFrame(frames.Count - 1, true);
        }

        protected override void OnGameExit() => OnGameEnding();

        protected override void OnClosing() => OnGameEnding();

        private static string ToSecondText(long ms) => (ms / 1000.0).ToString("f3") + 's';

        #endregion

        #region Frame Computing

        private const string DEAD_TEXT = "Dead: ";
        private const int META_OFFSET = 12;
        private const int META_FONT_OFFSET = 4;
        private const int META_HEIGHT = 44;
        private const int META_FONT_SIZE = 24;

        public static int Radius { get => Settings.Default.Radius; set { Settings.Default.Radius = value; } }
        public static int FontSize { get => Settings.Default.FontSize; set { Settings.Default.FontSize = value; _font = new(new FontFamily(GenericFontFamilies.Serif), value, FontStyle.Bold); } }
        public static Color ColorGoose { get => Settings.Default.CGoose; set { Settings.Default.CGoose = value; _bGoose = new SolidBrush(value); } }
        public static Color ColorSuspect { get => Settings.Default.CSuspect; set { Settings.Default.CSuspect = value; _bSuspect = new SolidBrush(value); } }
        public static Color ColorDuck { get => Settings.Default.CDuck; set { Settings.Default.CDuck = value; _bDuck = new SolidBrush(value); } }

        public static readonly Dictionary<int, string> PelicanSuspects = new();
        public static readonly Dictionary<int, string> KillerSuspects = new();

        private static Font _font = new(new FontFamily(GenericFontFamilies.Serif), Settings.Default.FontSize, FontStyle.Bold);
        private static Brush _bGoose = new SolidBrush(Settings.Default.CGoose);
        private static Brush _bSuspect = new SolidBrush(Settings.Default.CSuspect);
        private static Brush _bDuck = new SolidBrush(Settings.Default.CDuck);

        private static readonly Brush META_BACK_BRASH = new SolidBrush(Color.White);
        private static readonly Brush META_FORE_BRASH = new SolidBrush(Color.Black);
        private static readonly Font META_FONT = new(new FontFamily(GenericFontFamilies.Serif), META_FONT_SIZE, FontStyle.Bold);
        private static readonly Vector2[] MAPPINGS_B = new Vector2[]
        {
            new Vector2(36.2639f, 39.68626f),
            new Vector2(27.629484f, 41.955616f),
            new Vector2(67.8007f, 45.026608f),
            new Vector2(75.18318f, 51.779053f),
            new Vector2(66.647026f, 27.624222f),
            Vector2.Zero,
            new Vector2(42.40496f, 40.4429f),
            new Vector2(45.73738f, 24.93071f),
            new Vector2(53.172234f, 29.226412f),
        };
        private static readonly Vector2[] MAPPINGS_A = new Vector2[]
        {
            new Vector2(12.480058f, 12.432981f),
            new Vector2(17.190145f, 17.089575f),
            new Vector2(10.453817f, 10.317171f),
            new Vector2(7.718966f, 7.7245717f),
            new Vector2(10.445533f, 10.471038f),
            Vector2.Zero,
            new Vector2(11.580055f, 11.362657f),
            new Vector2(12.940321f, 12.910908f),
            new Vector2(11.51932f, 11.565213f)
        };

        //黑天鹅 Space 3  鹅教堂 Victorian 0
        //沙漠 Desert 8  地下室 Vitorian 7
        //丛林 Jungle 6 马拉德 Victorian 1
        //殖民地 Space 2 飞船 Space 4
        //休息室 Lounge 5

        private readonly HashSet<int> Suspects = new();
        private readonly HashSet<int> Ducks = new();
        private readonly HashSet<int> Deads = new();
        private readonly HashSet<int> InPelican = new();

        private int CurrentMap;
        private Image? LoadedMapImage;

        private Frame GetFrame(Context context, long time)
        {
            if (int.TryParse(context.RoomMap, out var v) && v >= 0 && v < 9) CurrentMap = v;
            var list = new List<Unit>();
            var victimsFromKiller = new List<Player>();
            var victimsFromPelican = new List<Player>();
            foreach (var player in context.Players)
            {
                if (player.Active)
                {
                    if (context.State == GameState.InGame && time > 15000)
                    {
                        if (player.IsGhost && !Deads.Contains(player.ActorNumber)) victimsFromKiller.Add(player);
                        else if (player.IsInPelican && !InPelican.Contains(player.ActorNumber)) { InPelican.Add(player.ActorNumber); victimsFromPelican.Add(player); }
                    }
                    list.Add(new(player.ActorNumber,
                    GetOrAdd(Suspects, player.ActorNumber, () => player.HasKilledThisRound),
                    GetOrAdd(Ducks, player.ActorNumber, () => player.IsInvisible || player.InTelepathic || player.IsMorphed),
                    GetOrAdd(Deads, player.ActorNumber, () => player.IsGhost), player.IsInPelican,
                    player.Nickname, player.Position));
                }
            }
            GetNearestPlayer(victimsFromKiller, context.Players, KillerSuspects);
            GetNearestPlayer(victimsFromPelican, context.Players, PelicanSuspects);
            return new Frame(time, new(context.DeadPlayersCount, list));
        }

        private static void GetNearestPlayer(List<Player> victims, ICollection<Player> players, Dictionary<int, string> resultMap)
        {
            if (victims.Count == 0) return;
            foreach (var victim in victims)
            {
                var distance = float.MaxValue;
                Player? target = null;
                foreach (Player player in players)
                {
                    if (player.ActorNumber == victim.ActorNumber || player.IsGhost || player.IsInPelican) continue;
                    var d = Vector2.Distance(victim.Position, player.Position);
                    if (d < distance)
                    {
                        distance = d;
                        target = player;
                    }
                }
                if (target != null && !resultMap.ContainsKey(target.ActorNumber))
                    resultMap.Add(target.ActorNumber, target.Nickname);
            }
        }

        private static bool GetOrAdd(HashSet<int> set, int id, Func<bool> func)
        {
            if (set.Contains(id)) return true;
            if (!func()) return false;
            set.Add(id);
            return true;
        }

        private Image? GetImage(Frame frame)
        {
            var map = GetMapImage();
            if (map == null) return null;
            if (LoadedMapImage == null || LoadedMapImage.Width != map.Width || LoadedMapImage.Height != map.Height)
            {
                LoadedMapImage?.Dispose();
                LoadedMapImage = new Bitmap(map.Width, map.Height);
            }
            var r = Radius;
            var uc = frame.Data;
            using var g = Graphics.FromImage(LoadedMapImage);
            g.DrawImage(map, 0, 0);
            g.FillRectangle(META_BACK_BRASH, META_OFFSET, META_OFFSET, META_FONT_SIZE * 6, META_HEIGHT);
            g.DrawString(DEAD_TEXT + uc.DeadNum, META_FONT, META_FORE_BRASH, META_OFFSET + META_FONT_OFFSET, META_OFFSET + META_FONT_OFFSET);
            foreach (var unit in uc.Units)
            {
                if (unit.Dead || unit.IsInPelican || string.IsNullOrEmpty(unit.Nickname)) continue;
                var brush = unit.IsDuck ? _bDuck : (unit.IsSuspect || PelicanSuspects.ContainsKey(unit.Id) ||
                    KillerSuspects.ContainsKey(unit.Id) ? _bSuspect : _bGoose);
                var pos = (unit.Position + MAPPINGS_B[CurrentMap]) * MAPPINGS_A[CurrentMap];
                pos.Y = map.Height - pos.Y;
                g.FillEllipse(brush, pos.X - r, pos.Y - r, r * 2, r * 2);
                g.DrawString(unit.Nickname, _font, brush, pos.X - r, pos.Y + r * 1.5f);
            }
            return LoadedMapImage;
        }

        private void OnGameEnding()
        {
            Suspects.Clear();
            Ducks.Clear();
            Deads.Clear();
            PelicanSuspects.Clear();
            KillerSuspects.Clear();
        }

        private Image? GetMapImage()
        {
            return CurrentMap switch
            {
                0 => Resources._0,
                1 => Resources._1,
                2 => Resources._2,
                3 => Resources._3,
                4 => Resources._4,
                6 => Resources._6,
                7 => Resources._7,
                8 => Resources._8,
                _ => null,
            };
        }

        #endregion

        #region Structure

        private readonly struct Frame
        {
            public readonly long Time;
            public readonly UnitCollection Data;

            public Frame(long time, UnitCollection data)
            {
                Time = time;
                Data = data;
            }
        }

        private readonly struct Unit
        {
            public readonly int Id;
            public readonly bool IsSuspect;
            public readonly bool IsDuck;
            public readonly bool Dead;
            public readonly bool IsInPelican;
            public readonly string Nickname;
            public readonly Vector2 Position;

            public Unit(int id, bool isSuspect, bool isDuck, bool dead, bool isInPelican, string nickname, Vector2 position)
            {
                Id = id;
                IsSuspect = isSuspect;
                IsDuck = isDuck;
                Dead = dead;
                IsInPelican = isInPelican;
                Nickname = nickname;
                Position = position;
            }
        }

        private readonly struct UnitCollection
        {
            public readonly int DeadNum;
            public readonly List<Unit> Units;

            public UnitCollection(int deadNum, List<Unit> units)
            {
                DeadNum = deadNum;
                Units = units;
            }
        }

        #endregion
    }
}