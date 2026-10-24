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
        }

        private void AppWindow_Closing(AppWindow sender, AppWindowClosingEventArgs args)
        {
            if (_isExiting || !AppSettings.Current.CloseToTray) return;
            if (AppSettings.Current.CloseToTrayOnlyWhenTimerRunning && TimerPage.Current?.IsRunning != true) return;

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
    }
}
