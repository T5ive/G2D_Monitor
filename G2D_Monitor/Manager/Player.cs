using System.Numerics;
using System.Runtime.InteropServices;
using G2D_Monitor.Game;

namespace G2D_Monitor.Manager
{
    public sealed class Player
    {
        public const int OFFSET_START = 0xD4;
        public const int OFFSET_LENGTH = 0x3A5 - 0xD4 + 1;
        public const int OFFSET_NICKNAME = 0x1E0;

        public bool Active { get; private set; }

        public int ActorNumber { get; private set; }//0x1B0
        public bool HasKilledThisRound { get; private set; }//0x2FC
        public bool HasReportedProfesionalKill { get; private set; }//0xD4
        public bool InSmog { get; private set; }//0xD5
        public bool InTelepathic { get; private set; }//0x3A5 感应 todo 检测被还是感应别人
        public bool IsGhost { get; private set; } //0x198
        public bool IsInPelican { get; private set; }//0x3A4 鹈鹕体内
        public bool IsInvisible { get; private set; }//0x33A
        public bool IsLatchedOntoPlayer { get; private set; }//0x380
        public bool IsMorphed { get; private set; }//0x339 变形
        //public string KilledBy { get; private set; } = string.Empty; //0xD8
        public string Nickname { get; set; } = string.Empty; //0x1E0
        public int TimeOfDeath { get; private set; }//0x19C
        public Vector3 Position { get; private set; }//0x2D8

        public void Disable() => Active = false;

        public void Update(byte[] data)
        {
            Active = true;
            ActorNumber = BitConverter.ToInt32(data, 0x1B0 - OFFSET_START);
            HasKilledThisRound = BitConverter.ToBoolean(data, 0x2FC - OFFSET_START);
            HasReportedProfesionalKill = BitConverter.ToBoolean(data, 0xD4 - OFFSET_START);
            InSmog = BitConverter.ToBoolean(data, 0xD5 - OFFSET_START);
            InTelepathic = BitConverter.ToBoolean(data, 0x3A5 - OFFSET_START);
            IsGhost = BitConverter.ToBoolean(data, 0x198 - OFFSET_START);
            IsInPelican = BitConverter.ToBoolean(data, 0x3A4 - OFFSET_START);
            IsInvisible = BitConverter.ToBoolean(data, 0x33A - OFFSET_START);
            IsLatchedOntoPlayer = BitConverter.ToBoolean(data, 0x380 - OFFSET_START);
            IsMorphed = BitConverter.ToBoolean(data, 0x339 - OFFSET_START);
            TimeOfDeath = BitConverter.ToInt32(data, 0x19C - OFFSET_START);
            Position = new(BitConverter.ToSingle(data, 0x2D8 - OFFSET_START), BitConverter.ToSingle(data, 0x2D8 + sizeof(float) - OFFSET_START), BitConverter.ToSingle(data, 0x2D8 + sizeof(float) * 2 - OFFSET_START));
        }
    }
}
