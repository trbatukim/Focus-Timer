using Microsoft.UI.Xaml;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;

namespace Focus_Timer.Services
{
    public static class TimerService
    {
        private static readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromSeconds(1) };

        private static TimeSpan _focusLength = TimeSpan.FromMinutes(25);
        private static TimeSpan _breakLength = TimeSpan.FromMinutes(5);

        public static bool IsRunning => _timer.IsEnabled;
        public static bool IsFocus { get; private set; } = true;
        public static TimeSpan Remaining { get; private set; } = _focusLength;
        public static TimeSpan CurrentLength => IsFocus ? _focusLength : _breakLength;

        public static TimeSpan GlobalFocusTime { get; private set; }
        public static int GlobalSessionsCompleted { get; private set; }

        public static event EventHandler? Changed;

        static TimerService()
        {
            _timer.Tick += Timer_Tick;
        }

        public static TimeSpan FocusLength
        {
            get => _focusLength;
            set
            {
                _focusLength = value;
                if (IsFocus && !IsRunning) ResetToCurrentMode();
            }
        }

        public static TimeSpan BreakLength
        {
            get => _breakLength;
            set
            {
                _breakLength = value;
                if (!IsFocus && !IsRunning) ResetToCurrentMode();
            }
        }

        public static void StartTimer()
        {
            if (CurrentLength <= TimeSpan.Zero || IsRunning) return;
            _timer.Start();
            OnChanged();
        }

        public static void StopTimer()
        {
            if (!IsRunning) return;
            _timer.Stop();
            OnChanged();
        }

        public static void Reset()
        {
            _timer.Stop();
            ResetToCurrentMode();
        }

        public static void Skip() => SwitchMode();

        private static void Timer_Tick(object? sender, object e)
        {
            Remaining -= TimeSpan.FromSeconds(1);

            if (IsFocus)
                GlobalFocusTime += TimeSpan.FromSeconds(1);

            if (Remaining <= TimeSpan.Zero)
            {
                NotifySessionEnded(IsFocus);

                if (IsFocus) GlobalSessionsCompleted++;

                SwitchMode();   // focus -> break -> focus
                return;
            }
            OnChanged();
        }

        private static void SwitchMode()
        {
            IsFocus = !IsFocus;
            ResetToCurrentMode();
        }

        private static void ResetToCurrentMode()
        {
            Remaining = CurrentLength;
            OnChanged();
        }

        private static void OnChanged() => Changed?.Invoke(null, EventArgs.Empty);

        private static void NotifySessionEnded(bool focusEnded)
        {
            if (!AppSettings.Current.NotificationsEnabled) return;

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
