using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Focus_Timer.Services;

namespace Focus_Timer.Views
{
    public sealed partial class TimerPage : Page
    {
        public TimerPage()
        {
            InitializeComponent();

            TimerService.Changed += (_, _) => UpdateDisplay();
            UpdateDisplay();
        }

        private void FocusInput_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs e)
        {
            if (FocusInputMinutes is null || FocusInputSeconds is null || TimeText is null)
                return;

            TimerService.FocusLength = ReadLength(FocusInputMinutes, FocusInputSeconds);
        }

        private void BreakInput_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs e)
        {
            if (BreakInputMinutes is null || BreakInputSeconds is null || TimeText is null)
                return;

            TimerService.BreakLength = ReadLength(BreakInputMinutes, BreakInputSeconds);
        }

        private static TimeSpan ReadLength(NumberBox minutesBox, NumberBox secondsBox)
        {
            double minutes = double.IsNaN(minutesBox.Value) ? 0 : minutesBox.Value;
            double seconds = double.IsNaN(secondsBox.Value) ? 0 : secondsBox.Value;
            return TimeSpan.FromMinutes(minutes) + TimeSpan.FromSeconds(seconds);
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (TimerService.IsRunning)
                TimerService.StopTimer();
            else
                TimerService.StartTimer();
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e) => TimerService.Reset();

        private void SkipButton_Click(object sender, RoutedEventArgs e) => TimerService.Skip();

        private void UpdateDisplay()
        {
            var remaining = TimerService.Remaining;

            ModeText.Text = TimerService.IsFocus ? "Focus" : "Break";
            TimeText.Text = remaining >= TimeSpan.FromMinutes(60)
                ? remaining.ToString(@"hh\:mm\:ss")
                : remaining.ToString(@"mm\:ss");

            if (TimerService.CurrentLength > TimeSpan.Zero)
                Progress.Value = 1 - (remaining / TimerService.CurrentLength);

            StartButton.Content = TimerService.IsRunning ? "Pause" : "Start";

            bool editable = !TimerService.IsRunning;
            FocusInputMinutes.IsEnabled = editable;
            FocusInputSeconds.IsEnabled = editable;
            BreakInputMinutes.IsEnabled = editable;
            BreakInputSeconds.IsEnabled = editable;

            GlobalFocusTimeLabel.Text = TimerService.GlobalFocusTime.ToString();
            GlobalSessionsNumberLabel.Text = TimerService.GlobalSessionsCompleted.ToString();
        }
    }
}
