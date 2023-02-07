namespace G2D_Monitor.Plugins.Common
{
    internal readonly struct MapFrame : IFrame
    {
        public long Time => _time;
        public readonly UnitCollection Units;

        private readonly long _time;

        public MapFrame(long time, UnitCollection units)
        {
            _time = time;
            Units = units;
        }
    }
}
