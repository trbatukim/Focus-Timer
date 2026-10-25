using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Focus_Timer.Services
{
    public static class StopwatchService
    {
        private static readonly Stopwatch _stopwatch = new();
        private static TimeSpan _lastLapTotal = TimeSpan.Zero;

        public static bool IsRunning => _stopwatch.IsRunning;
        public static TimeSpan Elapsed => _stopwatch.Elapsed;

        public static ObservableCollection<LapEntry> Laps { get; } = new();

        public static event EventHandler? Changed;

        public static void Start()
        {
            if (IsRunning) return;
            _stopwatch.Start();
            OnChanged();
        }

        public static void Stop()
        {
            if (!IsRunning) return;
            _stopwatch.Stop();
            OnChanged();
        }

        public static void Lap()
        {
            var total = _stopwatch.Elapsed;
            Laps.Insert(0, new LapEntry(Laps.Count + 1, total - _lastLapTotal, total));
            _lastLapTotal = total;
            OnChanged();
        }

        public static void Reset()
        {
            _stopwatch.Reset();
            Laps.Clear();
            _lastLapTotal = TimeSpan.Zero;
            OnChanged();
        }

        public static string Format(TimeSpan t) =>
            $"{(int)t.TotalHours:00}:{t.Minutes:00}:{t.Seconds:00}.{t.Milliseconds / 10:00}";

        private static void OnChanged() => Changed?.Invoke(null, EventArgs.Empty);
    }

    public sealed class LapEntry
    {
        public LapEntry(int number, TimeSpan lapTime, TimeSpan total)
        {
            Name = $"Lap {number}";
            LapTimeText = StopwatchService.Format(lapTime);
            TotalText = StopwatchService.Format(total);
        }

        public string Name { get; }
        public string LapTimeText { get; }
        public string TotalText { get; }
    }
}
