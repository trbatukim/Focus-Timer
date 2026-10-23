using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.System;
using Windows.UI.Shell;
using WinUIEx;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Focus_Timer
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private TrayIcon? _tray;
        private bool _isExiting;

        private static TimeSpan FocusLength = TimeSpan.FromMinutes(25);
        private static TimeSpan BreakLength = TimeSpan.FromMinutes(5);

        private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromSeconds(1) };
        private TimeSpan _remaining;
        private bool _isFocus = true;

        private TimeSpan _globalFocusTime;
        private int _globalSessionsCompleted;

        public MainWindow()
        {
            InitializeComponent();

            _globalSessionsCompleted = 0;
            _globalFocusTime = TimeSpan.Zero;

            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);

            _timer.Tick += Timer_Tick;

            AppWindow.Closing += AppWindow_Closing;
            Closed += Window_Closed;

            ResetToCurrentMode();
        }

        private void AppWindow_Closing(AppWindow sender, AppWindowClosingEventArgs args)
        {
            if (_isExiting || !_timer.IsEnabled) return;

            args.Cancel = true;
            MinimizeToTray();
        }

        private void MinimizeToTray()
        {
            _tray?.Dispose();
            _tray = new TrayIcon(1, Path.Combine(AppContext.BaseDirectory, "Assets", "tray_icon.ico"), "Focus Timer")
            {
                IsVisible = true
            };

            _tray.Selected += (s, e) => RestoreFromTray();

            _tray.ContextMenu += (s, e) =>
            {
                var open = new MenuFlyoutItem { Text = "Open" };
                open.Click += (_, _) => RestoreFromTray();

                var quit = new MenuFlyoutItem { Text = "Quit" };
                quit.Click += (_, _) => ExitApp();

                var flyout = new MenuFlyout();
                flyout.Items.Add(open);
                flyout.Items.Add(new MenuFlyoutSeparator());
                flyout.Items.Add(quit);
                e.Flyout = flyout;
            };

            AppWindow.Hide();
        }

        private void RestoreFromTray()
        {
            AppWindow.Show();
            Activate();
            _tray?.Dispose();
            _tray = null;
        }

        private void ExitApp()
        {
            _isExiting = true;
            _tray?.Dispose();
            _tray = null;
            Close();
        }

        private void Window_Closed(object sender, WindowEventArgs args)
        {
            _tray?.Dispose();
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
