using System.Numerics;

namespace G2D_Monitor.Plugins.Common
{
    internal readonly struct Unit
    {
        public readonly int Id;
        public readonly bool IsSuspect;
        public readonly bool IsDuck;
        public readonly bool Dead;
        public readonly string Nickname;
        public readonly Vector3 Position;

        public Unit(int id, bool isSuspect, bool isDuck, bool dead, string nickname, Vector3 position)
        {
            Id = id;
            IsSuspect = isSuspect;
            IsDuck = isDuck;
            Dead = dead;
            Nickname = nickname;
            Position = position;
        }
    }
}
