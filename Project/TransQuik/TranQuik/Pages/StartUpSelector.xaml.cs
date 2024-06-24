using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using TranQuik.Configuration;
using TranQuik.Controller;

namespace TranQuik.Pages
{
    public partial class StartUpSelector : Window
    {
        public StartUpSelector()
        {
            InitializeComponent();
            InitializeApplication();
        }

        private void InitializeApplication()
        {
            Config.LoadAppSettings();
            UpdateLastSyncTime();
            ConfigureAutoSyncSettings();
        }

        private void UpdateLastSyncTime()
        {
            LastSync.Text = Properties.Settings.Default._LastSync.ToString();
        }

        private void ConfigureAutoSyncSettings()
        {
            bool isAutoSyncEnabled = Properties.Settings.Default._AutoSync;
            string autoSyncText = isAutoSyncEnabled ? "ON" : "OFF";
            string syncColorResource = isAutoSyncEnabled ? "ButtonEnabledColor1" : "ErrorColor";

            string serverStatus = Properties.Settings.Default._ComputerName;
            string serverColorResource = serverStatus == "POS1" ? "ErrorColor" : "FontColor";

            if (serverStatus != "POS1")
            {
                AdjustUiForNonPos1Server();
            }

            SetAutoSyncStatus(autoSyncText, syncColorResource);
            SetServerStatus(serverStatus, serverColorResource);
        }

        private void AdjustUiForNonPos1Server()
        {
            Grid.SetColumnSpan(POS, 2);
            Sync.Visibility = Visibility.Collapsed;
            Sync.IsEnabled = false;
        }

        private void SetAutoSyncStatus(string text, string colorResource)
        {
            AutoSync.Text = text;
            AutoSync.Background = (System.Windows.Media.Brush)Application.Current.FindResource(colorResource);
        }

        private void SetServerStatus(string text, string colorResource)
        {
            deviceType.Text = text;
            deviceType.Background = (System.Windows.Media.Brush)Application.Current.FindResource(colorResource);
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private async void POS_Click(object sender, RoutedEventArgs e)
        {
            await OpenLoginPageAsync();
        }

        private Task OpenLoginPageAsync()
        {
            return Task.Run(() =>
            {
                Dispatcher.Invoke(() =>
                {
                    LoginPage loginPage = new LoginPage();
                    loginPage.ShowDialog();
                    Close();
                });
            });
        }

        private async void SyncButton_Click(object sender, RoutedEventArgs e)
        {
            await PerformDataSyncAsync();
        }

        private async Task PerformDataSyncAsync()
        {
            SyncMethod syncMethod = new SyncMethod(UpdateProgress);
            await syncMethod.SyncDataAsync();
            UpdateLastSyncTime();
            ShowNotification("Data synchronization complete!");
        }

        private void ShowNotification(string message)
        {
            NotificationPopup notificationPopup = new NotificationPopup(message, false);
            notificationPopup.ShowDialog();
        }

        private void UpdateProgress(int value)
        {
            Dispatcher.Invoke(() => ProgressBarx.Value = value);
        }

        private void Reports_Click(object sender, RoutedEventArgs e)
        {
            ReportPage reportPage = new ReportPage();
            reportPage.ShowDialog();
        }

        private void Utility_Click(object sender, RoutedEventArgs e)
        {
            NotificationPopup notificationPopup = new NotificationPopup("NOT AVAILABLE RIGHT NOW", false);
            notificationPopup.ShowDialog();
        }
    }
}
