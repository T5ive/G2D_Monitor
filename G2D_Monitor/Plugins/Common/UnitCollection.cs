using System.Collections;

namespace G2D_Monitor.Plugins.Common
{
    internal readonly struct UnitCollection : IEnumerable<Unit>
    {
        public readonly int DeadNum;
        private readonly List<Unit> Units;

        public Unit this[int index] => Units[index];

        public int Count => Units.Count;

        public UnitCollection(int deadNum, List<Unit> units)
        {
            DeadNum = deadNum;
            Units = units;
        }

        public IEnumerator<Unit> GetEnumerator() => Units.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => Units.GetEnumerator();
    }
}
