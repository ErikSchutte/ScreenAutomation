namespace ScreenAutomation.Util
{
    using System.Diagnostics;

    public sealed class ActionLoop
    {
        private int _frames;
        private readonly Stopwatch _sw = Stopwatch.StartNew();
        private double _lastLog;
        public double Fps => _frames / System.Math.Max(0.0001, _sw.Elapsed.TotalSeconds);
        public void Tick() => _frames++;
        public bool ShouldPrint()
        {
            if (_sw.Elapsed.TotalSeconds - _lastLog > 1.0) { _lastLog = _sw.Elapsed.TotalSeconds; return true; }
            return false;
        }
    }
}
