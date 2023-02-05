using G2D_Monitor.Game;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace G2D_Monitor.Manager
{
    public sealed class Context
    {
        public const int PLAYER_NUM = 16;

        private const string PLAYER_NAME_SPACE = "Handlers.GameHandlers.PlayerHandlers";
        private const string PLAYER_CLASS_NAME = "PlayerController";
        private const string MAIN_NAME_SPACE = "Managers";
        private const string MAIN_CLASS_NAME = "MainManager";
        private const string LOBBY_NAME_SPACE = "Handlers.LobbyHandlers";
        private const string LOBBY_CLASS_NAME = "LobbySceneHandler";

        public GameState State { get; private set; } //GameManager 0x18
        public string RoomId { get; private set; } = string.Empty; //Lobby 0x100
        public string RoomMap { get; private set; } = string.Empty; //Lobby 0x130
        public string MapTheme { get; private set; } = string.Empty; //Lobby 0x450
        public float TimeInRoom { get; private set; }  //Lobby 0x458
        public int DeadPlayersCount { get; private set; } //Player static 0x28
        public bool APlayerHasDied { get; private set; } //Player static 0x34

        public readonly ReadOnlyCollection<Player> Players;

        public readonly Process GameProcess;

        private readonly HandleRef hProc;
        private readonly IntPtr addrPlayer;
        private readonly IntPtr addrMain;
        private readonly IntPtr addrLobby;

        public Context(Process process)
        {
            GameProcess = process;
            hProc = new(process, process.Handle);
            var list = new List<Player>(PLAYER_NUM);
            for (var i = 0; i < PLAYER_NUM; i++) list.Add(new());
            Players = new(list);
            using (var mono = new Mono(hProc))
            {
                addrPlayer = mono.GetStaticFields(PLAYER_NAME_SPACE, PLAYER_CLASS_NAME);
                addrMain = mono.GetStaticFields(MAIN_NAME_SPACE, MAIN_CLASS_NAME);
                addrLobby = mono.GetStaticFields(LOBBY_NAME_SPACE, LOBBY_CLASS_NAME);
            }
        }

        public void Update()
        {
            if (addrPlayer != IntPtr.Zero)
            {
                DeadPlayersCount = BitConverter.ToInt32(Memory.Read(hProc, addrPlayer + 0x28, sizeof(int)));
                APlayerHasDied = BitConverter.ToBoolean(Memory.Read(hProc, addrPlayer + 0x34, sizeof(bool)));
                var addrStart = Memory.ReadIntPtr(hProc, Memory.ReadIntPtr(hProc, addrPlayer + 0x20) + 0x18);
                if (addrStart != IntPtr.Zero)
                {
                    for (var i = 0; i < Players.Count; i++)
                    {
                        var addr = Memory.ReadIntPtr(hProc, addrStart + 0x30 + 0x18 * i);
                        if (addr == IntPtr.Zero) Players[i].Disable();
                        else
                        {
                            var data = Memory.Read(hProc, addr + Player.OFFSET_START, Player.OFFSET_LENGTH);
                            Players[i].Nickname = Memory.ReadString(hProc, new IntPtr(BitConverter.ToInt64(data, Player.OFFSET_NICKNAME - Player.OFFSET_START)));
                            Players[i].Update(data);
                        }
                    }
                }
            }
            if (addrMain != IntPtr.Zero)
            {
                var addrGame = Memory.ReadIntPtr(hProc, Memory.ReadIntPtr(hProc, addrMain) + 0xB0);
                if (addrGame != IntPtr.Zero) State = (GameState)BitConverter.ToUInt16(Memory.Read(hProc, addrGame + 0x18, sizeof(ushort)));
            }
            if (addrLobby != IntPtr.Zero)
            {
                var addr = Memory.ReadIntPtr(hProc, addrLobby);
                if (addr != IntPtr.Zero)
                {
                    RoomId = Memory.ReadString(hProc, Memory.ReadIntPtr(hProc, addr + 0x100));
                    RoomMap = Memory.ReadString(hProc, Memory.ReadIntPtr(hProc, addr + 0x130));
                    MapTheme = Memory.ReadString(hProc, Memory.ReadIntPtr(hProc, addr + 0x450));
                    TimeInRoom = BitConverter.ToSingle(Memory.Read(hProc, addr + 0x458, sizeof(float)));
                }
            }
        }
    }
}
