using Focus_Timer.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.System;

namespace Focus_Timer.Views
{
    public sealed partial class SettingsPage : Page
    {
        private bool _loading;

        public SettingsPage()
        {
            InitializeComponent();

            _loading = true;
            var s = AppSettings.Current;
            CloseToTrayToggle.IsOn = s.CloseToTray;
            CloseToTrayOnlyWhenRunningToggle.IsOn = s.CloseToTrayOnlyWhenTimerRunning;
            NotificationsToggle.IsOn = s.NotificationsEnabled;
            _loading = false;
        }

        private void Setting_Toggled(object sender, RoutedEventArgs e)
        {
            if (_loading) return;

            var s = AppSettings.Current;
            s.CloseToTray = CloseToTrayToggle.IsOn;
            s.CloseToTrayOnlyWhenTimerRunning = CloseToTrayOnlyWhenRunningToggle.IsOn;
            s.NotificationsEnabled = NotificationsToggle.IsOn;
            s.Save();
        }

        private async void OpenDNDSettings_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("ms-settings:notifications"));
        }
    }
}
