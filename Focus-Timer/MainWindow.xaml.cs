using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Focus_Timer.Views;
using WinUIEx;
using Focus_Timer.Services;

namespace Focus_Timer
{
    public sealed partial class MainWindow : WindowEx
    {
        private TrayIcon? _tray;
        private bool _isExiting;

        public MainWindow()
        {
            InitializeComponent();

            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);
            AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets", "tray_icon.ico"));

            AppWindow.Closing += AppWindow_Closing;
            Closed += Window_Closed;
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
                ContentFrame.Navigate(typeof(SettingsPage));
            else if ((args.SelectedItem as NavigationViewItem)?.Tag is "Timer")
                ContentFrame.Navigate(typeof(TimerPage));
            else if ((args.SelectedItem as NavigationViewItem)?.Tag is "Stopwatch")
                ContentFrame.Navigate(typeof(StopwatchPage));
        }

        private void AppWindow_Closing(AppWindow sender, AppWindowClosingEventArgs args)
        {
            if (_isExiting || !AppSettings.Current.CloseToTray) return;
            if (AppSettings.Current.CloseToTrayOnlyWhenTimerRunning && !TimerService.IsRunning
                && !StopwatchService.IsRunning) return;

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

                // The menu is rebuilt on every open, so IsEnabled reflects the current state
                var startTimer = new MenuFlyoutItem { Text = "Start Timer", IsEnabled = !TimerService.IsRunning };
                startTimer.Click += (_, _) => TimerService.StartTimer();

                var stopTimer = new MenuFlyoutItem { Text = "Stop Timer", IsEnabled = TimerService.IsRunning };
                stopTimer.Click += (_, _) => TimerService.StopTimer();

                var startStopwatch = new MenuFlyoutItem { Text = "Start Stopwatch", IsEnabled = !StopwatchService.IsRunning };
                startStopwatch.Click += (_, _) => StopwatchService.Start();

                var stopStopwatch = new MenuFlyoutItem { Text = "Stop Stopwatch", IsEnabled = StopwatchService.IsRunning };
                stopStopwatch.Click += (_, _) => StopwatchService.Stop();

                var quit = new MenuFlyoutItem { Text = "Quit" };
                quit.Click += (_, _) => ExitApp();

                var flyout = new MenuFlyout();
                flyout.Items.Add(open);
                flyout.Items.Add(new MenuFlyoutSeparator());
                flyout.Items.Add(startTimer);
                flyout.Items.Add(stopTimer);
                flyout.Items.Add(startStopwatch);
                flyout.Items.Add(stopStopwatch);
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
    }
}
