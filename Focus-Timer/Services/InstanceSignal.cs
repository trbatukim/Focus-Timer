using System.Runtime.InteropServices;
using Microsoft.Windows.AppNotifications;

namespace Focus_Timer.Services
{
    public static class InstanceSignal
    {
        public const string NotificationArgument = "instance";

        private static EventWaitHandle? _signal;

        [DllImport("user32.dll")]
        private static extern bool AllowSetForegroundWindow(uint dwProcessId);

        private static string SignalName(int processId) => $"FocusTimer-Activate-{processId}";

        public static void Listen(Action onSignaled)
        {
            _signal = new EventWaitHandle(false, EventResetMode.AutoReset, SignalName(Environment.ProcessId));
            ThreadPool.RegisterWaitForSingleObject(_signal, (_, _) => onSignaled(), null, Timeout.Infinite, false);
        }

        public static bool TrySignalSender(AppNotificationActivatedEventArgs args)
        {
            if (!args.Arguments.TryGetValue(NotificationArgument, out var value)
                || !int.TryParse(value, out var processId)
                || processId == Environment.ProcessId)
                return false;

            if (!EventWaitHandle.TryOpenExisting(SignalName(processId), out var signal))
                return false;

            using (signal)
            {
                AllowSetForegroundWindow((uint)processId);
                signal.Set();
            }
            return true;
        }
    }
}
