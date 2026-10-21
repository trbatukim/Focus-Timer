using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Windowing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Focus_Timer
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private static TimeSpan FocusLength = TimeSpan.FromMinutes(25);
        private static TimeSpan BreakLength = TimeSpan.FromMinutes(5);

        private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromSeconds(1) };
        private TimeSpan _remaining;
        private bool _isFocus = true;

        private TimeSpan globalFocusTime;

        public MainWindow()
        {
            InitializeComponent();

            AppWindow.TitleBar.ExtendsContentIntoTitleBar = true;
            AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Standard;

            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);

            _timer.Tick += Timer_Tick;
            ResetToCurrentMode();
        }

        private TimeSpan CurrentLength => _isFocus ? FocusLength : BreakLength;

        private void Timer_Tick(object? sender, object e)
        {
            _remaining -= TimeSpan.FromSeconds(1);

            if (_isFocus)
            {
                globalFocusTime += TimeSpan.FromSeconds(1);
            }
            
            if (_remaining <= TimeSpan.Zero)
            {
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
            if (FocusInputMinutes is null || FocusInputSeconds is null || TimeText is null)
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

        private void UpdateDisplay()
        {
            ModeText.Text = _isFocus ? "Focus" : "Break";
            TimeText.Text = _remaining.ToString(@"mm\:ss");
            Progress.Value = 1 - (_remaining / CurrentLength);
            GlobalFocusTimeLabel.Text = globalFocusTime.ToString();
        }
    }
}
