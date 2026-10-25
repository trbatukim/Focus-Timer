using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Focus_Timer.Services;

namespace Focus_Timer.Views
{
    public sealed partial class StopwatchPage : Page
    {
        private readonly DispatcherTimer _refreshTimer = new() { Interval = TimeSpan.FromMilliseconds(50) };

        public StopwatchPage()
        {
            InitializeComponent();

            LapsList.ItemsSource = StopwatchService.Laps;
            _refreshTimer.Tick += (_, _) => UpdateDisplay();
            StopwatchService.Changed += (_, _) => UpdateDisplay();
            UpdateDisplay();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (StopwatchService.IsRunning)
                StopwatchService.Stop();
            else
                StopwatchService.Start();
        }

        private void LapButton_Click(object sender, RoutedEventArgs e) => StopwatchService.Lap();

        private void ResetButton_Click(object sender, RoutedEventArgs e) => StopwatchService.Reset();

        private void UpdateDisplay()
        {
            bool running = StopwatchService.IsRunning;
            var t = StopwatchService.Elapsed;

            if (running && !_refreshTimer.IsEnabled) _refreshTimer.Start();
            else if (!running && _refreshTimer.IsEnabled) _refreshTimer.Stop();

            TimeRun.Text = $"{(int)t.TotalHours:00}:{t.Minutes:00}:{t.Seconds:00}";
            FractionRun.Text = $".{t.Milliseconds / 10:00}";
            StartButton.Content = running ? "Pause"
                : t > TimeSpan.Zero ? "Resume" : "Start";
            LapButton.IsEnabled = running;
            ResetButton.IsEnabled = t > TimeSpan.Zero;
            LapsCard.Visibility = StopwatchService.Laps.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
