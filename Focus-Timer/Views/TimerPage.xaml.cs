using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using System;
using Windows.System;

namespace Focus_Timer.Views
{
    public sealed partial class TimerPage : Page
    {
        // MainWindow reads this to decide whether closing should minimize to tray.
        public static TimerPage? Current { get; private set; }
        public bool IsRunning => _timer.IsEnabled;

        private static TimeSpan FocusLength = TimeSpan.FromMinutes(25);
        private static TimeSpan BreakLength = TimeSpan.FromMinutes(5);

        private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromSeconds(1) };
        private TimeSpan _remaining;
        private bool _isFocus = true;

        private TimeSpan _globalFocusTime;
        private int _globalSessionsCompleted;

        public TimerPage()
        {
            InitializeComponent();
            Current = this;

            _globalSessionsCompleted = 0;
            _globalFocusTime = TimeSpan.Zero;

            _timer.Tick += Timer_Tick;

            ResetToCurrentMode();
        }

        private TimeSpan CurrentLength => _isFocus ? FocusLength : BreakLength;

        private void Timer_Tick(object? sender, object e)
        {
            _remaining -= TimeSpan.FromSeconds(1);

            if (_isFocus)
            {
                _globalFocusTime += TimeSpan.FromSeconds(1);
            }

            if (_remaining <= TimeSpan.Zero)
            {
                NotifySessionEnded(_isFocus);

                if (_isFocus) _globalSessionsCompleted++;

                SwitchMode();   // focus -> break -> focus
                return;
            }
            UpdateDisplay();
        }

        private void FocusInput_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs e)
        {
            double newValue = e.NewValue;
            double oldValue = e.OldValue;

            if (sender == FocusInputMinutes || sender == FocusInputSeconds) {
                UpdateFocusTime();
            }
        }

        private void BreakInput_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs e)
        {
            double newValue = e.NewValue;
            double oldValue = e.OldValue;

            if (sender == BreakInputMinutes || sender == BreakInputSeconds)
            {
                UpdateBreakTime();
            }
        }

        private void UpdateFocusTime()
        {
            if (BreakInputMinutes is null || BreakInputSeconds is null || TimeText is null)
                return;

            double minutes = double.IsNaN(FocusInputMinutes.Value) ? 0 : FocusInputMinutes.Value;
            double seconds = double.IsNaN(FocusInputSeconds.Value) ? 0 : FocusInputSeconds.Value;

            FocusLength = TimeSpan.FromMinutes(minutes) + TimeSpan.FromSeconds(seconds);

            if (_isFocus && !_timer.IsEnabled)
                ResetToCurrentMode();
        }

        private void UpdateBreakTime()
        {
            if (BreakInputMinutes is null || BreakInputSeconds is null || TimeText is null)
                return;

            double minutes = double.IsNaN(BreakInputMinutes.Value) ? 0 : BreakInputMinutes.Value;
            double seconds = double.IsNaN(BreakInputSeconds.Value) ? 0 : BreakInputSeconds.Value;

            BreakLength = TimeSpan.FromMinutes(minutes) + TimeSpan.FromSeconds(seconds);

            if (!_isFocus && !_timer.IsEnabled)
                ResetToCurrentMode();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentLength <= TimeSpan.Zero) return;

            if (_timer.IsEnabled)
            {
                _timer.Stop();
                StartButton.Content = "Start";

                BreakInputMinutes.IsEnabled = true;
                BreakInputSeconds.IsEnabled = true;

                FocusInputMinutes.IsEnabled = true;
                FocusInputSeconds.IsEnabled = true;
            }
            else
            {
                _timer.Start();
                StartButton.Content = "Pause";

                BreakInputMinutes.IsEnabled = false;
                BreakInputSeconds.IsEnabled = false;

                FocusInputMinutes.IsEnabled = false;
                FocusInputSeconds.IsEnabled = false;
            }
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
            StartButton.Content = "Start";

            BreakInputMinutes.IsEnabled = true;
            BreakInputSeconds.IsEnabled = true;

            FocusInputMinutes.IsEnabled = true;
            FocusInputSeconds.IsEnabled = true;

            ResetToCurrentMode();
        }

        private void SkipButton_Click(object sender, RoutedEventArgs e) => SwitchMode();

        private void SwitchMode()
        {
            _isFocus = !_isFocus;
            ResetToCurrentMode();
        }

        private void ResetToCurrentMode()
        {
            _remaining = CurrentLength;
            UpdateDisplay();
        }

        private async void OpenDNDSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("ms-settings:notifications"));
        }

        private void UpdateDisplay()
        {
            ModeText.Text = _isFocus ? "Focus" : "Break";

            if (_remaining >= TimeSpan.FromMinutes(60))
            {
                TimeText.Text = _remaining.ToString(@"hh\:mm\:ss");
            }
            else
            {
                TimeText.Text = _remaining.ToString(@"mm\:ss");
            }

            if (CurrentLength > TimeSpan.Zero)
                Progress.Value = 1 - (_remaining / CurrentLength);

            GlobalFocusTimeLabel.Text = _globalFocusTime.ToString();
            GlobalSessionsNumberLabel.Text = _globalSessionsCompleted.ToString();
        }

        private void NotifySessionEnded(bool focusEnded)
        {
            var builder = new AppNotificationBuilder()
                .AddText(focusEnded ? "Focus session done" : "Break's over")
                .AddText(focusEnded ? "Time for a break." : "Back to focus.")
                .SetAudioEvent(AppNotificationSoundEvent.Reminder);

            if (AppNotificationBuilder.IsUrgentScenarioSupported())
                builder.SetScenario(AppNotificationScenario.Urgent);

            AppNotificationManager.Default.Show(builder.BuildNotification());
        }
    }
}
