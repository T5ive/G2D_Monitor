using G2D_Monitor.Manager;
using G2D_Monitor.Plugins.Common;

namespace G2D_Monitor.Plugins
{
    internal class MetaPlugin : Plugin
    {
        protected override string Name => "Meta";

        private readonly HashSet<int> HasKilled = new();
        private readonly HashSet<int> HasBeenInTelepathic = new();
        private readonly HashSet<int> HasBeenInvisible = new();
        private readonly HashSet<int> HasMorphed = new();
        private readonly HashSet<int> Deads = new();

        private GameState LastState = GameState.Unknown;

        protected override void OnGameExit() => Clear();

        protected override void OnUpdate(Context context)
        {
            RomeIdSetter?.Invoke(context.RoomId);
            if (context.State == GameState.InGame)
            {
                var listKiller = new List<string>();
                var listSmog = new List<string>();
                var listTelepathic = new List<string>();
                var listDead = new List<string>();
                var listInPelican = new List<string>();
                var listInvisible = new List<string>();
                var listMorphed = new List<string>();
                foreach (var player in context.Players)
                {
                    if (!player.Active) break;
                    if (GetOrAdd(HasKilled, player.ActorNumber, () => player.HasKilledThisRound))
                        listKiller.Add(player.Nickname);
                    if (player.InSmog) listSmog.Add(player.Nickname);
                    if (GetOrAdd(HasBeenInTelepathic, player.ActorNumber, () => player.InTelepathic))
                        listTelepathic.Add(player.Nickname);
                    if (GetOrAdd(Deads, player.ActorNumber, () => player.IsGhost))
                        listDead.Add(player.Nickname);
                    if (player.IsInPelican) listInPelican.Add(player.Nickname);
                    if (GetOrAdd(HasBeenInvisible, player.ActorNumber, () => player.IsInvisible))
                        listInvisible.Add(player.Nickname);
                    if (GetOrAdd(HasMorphed, player.ActorNumber, () => player.IsMorphed))
                        listMorphed.Add(player.Nickname);
                }
                HasKilledSetter?.Invoke(string.Join(' ', listKiller));
                InSmogSetter?.Invoke(string.Join(' ', listSmog));
                TelepathicSetter?.Invoke(string.Join(' ', listTelepathic));
                DeadsSetter?.Invoke(string.Join(' ', listDead));
                InPelicanSetter?.Invoke(string.Join(' ', listInPelican));
                InvisibleSetter?.Invoke(string.Join(' ', listInvisible));
                MorphedSetter?.Invoke(string.Join(' ', listMorphed));
                PelicanSuspectsSetter?.Invoke(string.Join(' ', MapPlugin.PelicanSuspects.Values));
                KillerSuspectsSetter?.Invoke(string.Join(' ', MapPlugin.KillerSuspects.Values));
                LastState = context.State;
            }
            else if (context.State != LastState)
            {
                if (context.State == GameState.InLobby) Clear();
                LastState = context.State;
            }
        }

        private static bool GetOrAdd(HashSet<int> set, int id, Func<bool> func)
        {
            if (set.Contains(id)) return true;
            if (!func()) return false;
            set.Add(id);
            return true;
        }

        private void Clear()
        {
            RomeIdSetter?.Invoke(string.Empty);
            HasKilledSetter?.Invoke(string.Empty);
            InSmogSetter?.Invoke(string.Empty);
            TelepathicSetter?.Invoke(string.Empty);
            DeadsSetter?.Invoke(string.Empty);
            InPelicanSetter?.Invoke(string.Empty);
            InvisibleSetter?.Invoke(string.Empty);
            MorphedSetter?.Invoke(string.Empty);
            PelicanSuspectsSetter?.Invoke(string.Empty);
            KillerSuspectsSetter?.Invoke(string.Empty);
            HasKilled.Clear();
            HasBeenInTelepathic.Clear();
            HasBeenInvisible.Clear();
            HasMorphed.Clear();
            Deads.Clear();
        }

        private Action<string>? RomeIdSetter;
        private Action<string>? HasKilledSetter;
        private Action<string>? InSmogSetter;
        private Action<string>? TelepathicSetter;
        private Action<string>? DeadsSetter;
        private Action<string>? InPelicanSetter;
        private Action<string>? InvisibleSetter;
        private Action<string>? MorphedSetter;
        private Action<string>? PelicanSuspectsSetter;
        private Action<string>? KillerSuspectsSetter;
        private KeyValuePanel? Panel;

        protected override void Initialize(TabPage tab) =>
            Panel = new KeyValuePanel()
                .AddReadOnlyTextBox("RoomId", out RomeIdSetter, 80)
                .AddReadOnlyTextBox("HasKilled", out HasKilledSetter)
                .AddReadOnlyTextBox("In Smog", out InSmogSetter)
                .AddReadOnlyTextBox("Telepathic", out TelepathicSetter)
                .AddReadOnlyTextBox("Deads", out DeadsSetter)
                .AddReadOnlyTextBox("In Pelican", out InPelicanSetter)
                .AddReadOnlyTextBox("Invisible", out InvisibleSetter)
                .AddReadOnlyTextBox("Morphed", out MorphedSetter)
                .AddReadOnlyTextBox("Pelican Suspects", out PelicanSuspectsSetter)
                .AddReadOnlyTextBox("Killer Suspects", out KillerSuspectsSetter)
                .Attach(tab);

        protected override void Activate() => Panel?.Align();
    }
}
