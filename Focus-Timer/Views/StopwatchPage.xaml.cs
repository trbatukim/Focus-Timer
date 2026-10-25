using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Focus_Timer.Views
{
    public sealed partial class StopwatchPage : Page
    {
        public static StopwatchPage? Current { get; private set; }
        public bool IsRunning => _stopwatch.IsRunning;

        private readonly Stopwatch _stopwatch = new();
        private readonly DispatcherTimer _refreshTimer = new() { Interval = TimeSpan.FromMilliseconds(50) };

        private readonly ObservableCollection<LapEntry> _laps = new();
        private TimeSpan _lastLapTotal = TimeSpan.Zero;

        public StopwatchPage()
        {
            InitializeComponent();
            Current = this;

            LapsList.ItemsSource = _laps;
            _refreshTimer.Tick += (_, _) => UpdateDisplay();
            UpdateDisplay();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (_stopwatch.IsRunning)
            {
                _stopwatch.Stop();
                _refreshTimer.Stop();
            }
            else
            {
                _stopwatch.Start();
                _refreshTimer.Start();
            }
            UpdateDisplay();
        }

        private void LapButton_Click(object sender, RoutedEventArgs e)
        {
            var total = _stopwatch.Elapsed;
            _laps.Insert(0, new LapEntry(_laps.Count + 1, total - _lastLapTotal, total));
            _lastLapTotal = total;
            LapsCard.Visibility = Visibility.Visible;
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            _stopwatch.Reset();
            _refreshTimer.Stop();
            _laps.Clear();
            _lastLapTotal = TimeSpan.Zero;
            LapsCard.Visibility = Visibility.Collapsed;
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            TimeText.Text = Format(_stopwatch.Elapsed);
            StartButton.Content = _stopwatch.IsRunning ? "Pause"
                : _stopwatch.Elapsed > TimeSpan.Zero ? "Resume" : "Start";
            LapButton.IsEnabled = _stopwatch.IsRunning;
            ResetButton.IsEnabled = _stopwatch.Elapsed > TimeSpan.Zero;
        }

        internal static string Format(TimeSpan t) =>
            $"{(int)t.TotalHours:00}:{t.Minutes:00}:{t.Seconds:00}.{t.Milliseconds / 10:00}";
    }

    public sealed class LapEntry
    {
        public LapEntry(int number, TimeSpan lapTime, TimeSpan total)
        {
            Name = $"Lap {number}";
            LapTimeText = StopwatchPage.Format(lapTime);
            TotalText = StopwatchPage.Format(total);
        }

        public string Name { get; }
        public string LapTimeText { get; }
        public string TotalText { get; }
    }
}
