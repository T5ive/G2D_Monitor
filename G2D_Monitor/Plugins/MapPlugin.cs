using G2D_Monitor.Manager;
using G2D_Monitor.Plugins.Common;
using System.Drawing.Text;
using System.Numerics;

namespace G2D_Monitor.Plugins
{
    internal sealed class MapPlugin : FramePlugin<List<Unit>>
    {
        private const int RADIUS = 8;
        private static readonly Vector3[] MAPPINGS = new Vector3[]
        {
            new Vector3(-36.21f, -40.23f, 0.08f),
            new Vector3(-28.12f, -42.44f, 0.0585f),
            new Vector3(-68.08f, -44.96f, 0.0948f),
            new Vector3(-28.12f, -42.44f, 0.05f),
            new Vector3(-66.84f, -28.49f, 0.0947f),
            Vector3.Zero,
            new Vector3(-41.08f, -40.5f, 0.0847f),
            new Vector3(-45.6f, -24.82f, 0.0766f),
            new Vector3(-53.24f, -29.55f, 0.086f),
        };
        private static readonly Font DEFAULT_FONT = new(new FontFamily(GenericFontFamilies.Serif), 16, FontStyle.Bold);
        private static readonly Brush BRUSH_NORMAL = new SolidBrush(Color.White); 
        private static readonly Brush BRUSH_SUSPECT = new SolidBrush(Color.DeepPink);
        private static readonly Brush BRUSH_DUCK = new SolidBrush(Color.DarkRed);

        protected override string Name => "Map";

        protected override bool AlwaysShowNewestFrame => true;

        protected override bool AlwaysDisposeLastImage => false;

        //黑天鹅 Space 3  鹅教堂 Victorian 0
        //沙漠 Desert 8  地下室 Vitorian 7
        //丛林 Jungle 6 马拉德 Victorian 1
        //殖民地 Space 2 飞船 Space 4
        //休息室 Lounge 5

        private readonly HashSet<int> Suspects = new ();
        private readonly HashSet<int> Ducks = new ();
        private readonly HashSet<int> Deads = new ();

        private int CurrentMap;
        private Image? LoadedMapImage;

        protected override List<Unit> GetFrameData(Context context)
        {
            if (int.TryParse(context.RoomMap, out var v) && v >= 0 && v < 9) CurrentMap = v;
            var list = new List<Unit>();
            foreach (var player in context.Players)
            {
                if (!player.Active) break;
                list.Add(new(player.ActorNumber, 
                    GetOrAdd(Suspects, player.ActorNumber, () => player.HasKilledThisRound || player.InSmog), 
                    GetOrAdd(Ducks, player.ActorNumber, () => player.IsInvisible || player.IsLatchedOntoPlayer || player.IsMorphed), 
                    GetOrAdd(Deads, player.ActorNumber, () => player.IsGhost || player.IsLatchedOntoPlayer || player.TimeOfDeath > 0),
                    player.Nickname, player.Position));
            }
            return list;
        }

        private static bool GetOrAdd(HashSet<int> set, int id, Func<bool> func)
        {
            if (set.Contains(id)) return true;
            if (!func()) return false;
            set.Add(id);
            return true;
        }

        protected override Image? GetImage(List<Unit> data)
        {
            var map = GetMapImage();
            if (map == null) return null;
            if (LoadedMapImage == null || LoadedMapImage.Width != map.Width || LoadedMapImage.Height != map.Height)
            {
                LoadedMapImage?.Dispose();
                LoadedMapImage = new Bitmap(map.Width, map.Height);
            }
            using var g = Graphics.FromImage(LoadedMapImage);
            g.DrawImage(map, 0, 0);
            foreach (var unit in data)
            {
                if (unit.Dead) continue;
                var brush = unit.IsDuck ? BRUSH_DUCK : (unit.IsSuspect ? BRUSH_SUSPECT : BRUSH_NORMAL);
                var pos = (unit.Position - MAPPINGS[CurrentMap]) / MAPPINGS[CurrentMap].Z;
                pos.Y = map.Height - pos.Y;
                g.FillEllipse(brush, pos.X - RADIUS, pos.Y - RADIUS, RADIUS * 2, RADIUS * 2);
                g.DrawString(unit.Nickname, DEFAULT_FONT, brush, pos.X - RADIUS, pos.Y + RADIUS * 1.5f);
            }
            return LoadedMapImage;
        }

        private Image? GetMapImage()
        {
            return CurrentMap switch
            {
                0 => Properties.Resources._0,
                1 => Properties.Resources._1,
                2 => Properties.Resources._2,
                3 => Properties.Resources._3,
                4 => Properties.Resources._4,
                6 => Properties.Resources._6,
                7 => Properties.Resources._7,
                8 => Properties.Resources._8,
                _ => null,
            };
        }

        protected override void OnGameEnding()
        {
            Suspects.Clear();
            Ducks.Clear();
            Deads.Clear();
        }
    }
}
