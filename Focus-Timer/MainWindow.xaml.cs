using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.System;
using Windows.UI.ApplicationSettings;
using Windows.UI.Shell;
using Focus_Timer.Views;
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

        public MainWindow()
        {
            InitializeComponent();

            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);

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
            if (_isExiting || TimerPage.Current?.IsRunning != true) return;

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
