using System.Numerics;

namespace G2D_Monitor.Plugins.Common
{
    internal readonly struct Unit
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
}
