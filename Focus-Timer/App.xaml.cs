using Focus_Timer.Services;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using Microsoft.Windows.AppNotifications;

namespace Focus_Timer
{
    public partial class App : Application
    {
        private MainWindow? _window;

        public App()
        {
            InitializeComponent();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            AppNotificationManager.Default.NotificationInvoked += (_, e) =>
            {
                if (!InstanceSignal.TrySignalSender(e))
                    BringMainWindowToFront();
            };

            // Required for unpackaged builds (release zip); harmless when packaged
            AppNotificationManager.Default.Register();

            var activation = AppInstance.GetCurrent().GetActivatedEventArgs();
            if (activation.Kind == ExtendedActivationKind.AppNotification
                && InstanceSignal.TrySignalSender((AppNotificationActivatedEventArgs)activation.Data))
            {
                Exit();
                return;
            }

            InstanceSignal.Listen(BringMainWindowToFront);

            _window = new MainWindow();
            _window.Activate();
        }

        private void BringMainWindowToFront()
        {
            _window?.DispatcherQueue.TryEnqueue(() => _window.BringToForeground());
        }
    }
}
