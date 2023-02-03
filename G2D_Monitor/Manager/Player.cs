using System.Numerics;
using System.Runtime.InteropServices;
using G2D_Monitor.Game;

namespace G2D_Monitor.Manager
{
    public sealed class Player
    {
        public bool Active { get; private set; }

        public int ActorNumber { get; private set; }//0x1B0
        public bool HasKilledThisRound { get; private set; }//0x2FC
        public bool HasReportedProfesionalKill { get; private set; }//0xD4
        public bool InSmog { get; private set; }//0xD5
        public bool InTelepathic { get; private set; }//0x3A5 感应
        public bool IsGhost { get; private set; } //0x198
        public bool IsInPelican { get; private set; }//0x3A4 鹈鹕体内
        public bool IsInvisible { get; private set; }//0x33A
        public bool IsLatchedOntoPlayer { get; private set; }//0x380
        public bool IsMorphed { get; private set; }//0x339 变形
        public string KilledBy { get; private set; } = string.Empty; //0xD8
        public string Nickname { get; private set; } = string.Empty; //0x1E0
        public int TimeOfDeath { get; private set; }//0x19C
        public Vector3 Position { get; private set; }//0x2D8

        public void Update(HandleRef hProc, IntPtr address)
        {
            var addr = Memory.ReadIntPtr(hProc, address);
            if (!(Active = addr != IntPtr.Zero)) return;
            ActorNumber = BitConverter.ToInt32(Memory.Read(hProc, addr + 0x1B0, sizeof(int)));
            HasKilledThisRound = BitConverter.ToBoolean(Memory.Read(hProc, addr + 0x2FC, sizeof(bool)));
            HasReportedProfesionalKill = BitConverter.ToBoolean(Memory.Read(hProc, addr + 0xD4, sizeof(bool)));
            InSmog = BitConverter.ToBoolean(Memory.Read(hProc, addr + 0xD5, sizeof(bool)));
            InTelepathic = BitConverter.ToBoolean(Memory.Read(hProc, addr + 0x3A5, sizeof(bool)));
            IsGhost = BitConverter.ToBoolean(Memory.Read(hProc, addr + 0x198, sizeof(bool)));
            IsInPelican = BitConverter.ToBoolean(Memory.Read(hProc, addr + 0x3A4, sizeof(bool)));
            IsInvisible = BitConverter.ToBoolean(Memory.Read(hProc, addr + 0x33A, sizeof(bool)));
            IsLatchedOntoPlayer = BitConverter.ToBoolean(Memory.Read(hProc, addr + 0x380, sizeof(bool)));
            IsMorphed = BitConverter.ToBoolean(Memory.Read(hProc, addr + 0x339, sizeof(bool)));
            KilledBy = Memory.ReadString(hProc, addr + 0xD8);
            Nickname = Memory.ReadString(hProc, addr + 0x1E0);
            TimeOfDeath = BitConverter.ToInt32(Memory.Read(hProc, addr + 0x19C, sizeof(int)));
            var size = sizeof(float);
            var data = Memory.Read(hProc, addr + 0x2D8, size * 3).AsSpan();
            Position = new(BitConverter.ToSingle(data[..size]), BitConverter.ToSingle(data[size..(size * 2)]), BitConverter.ToSingle(data[^size..]));
        }
    }
}
