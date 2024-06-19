using System;
using System.Drawing;
using System.Windows;
using TranQuik.Configuration;
using TranQuik.Controller;
using System.Windows.Controls;
using ZstdSharp.Unsafe;


namespace TranQuik.Pages
{
    public partial class StartUpSelector : Window
    {
        

        public StartUpSelector()
        {
            InitializeComponent();
            
            Config.LoadAppSettings();
            AutoSyncFunction();
        }

        private void AutoSyncFunction()
        {
            LastSync.Text = Properties.Settings.Default._LastSync.ToString();

            // Determine the text and color based on the value of _AutoSync
            string autoSyncText = Properties.Settings.Default._AutoSync ? "ON" : "OFF";
            string syncColor = Properties.Settings.Default._AutoSync ? "ButtonEnabledColor1" : "ErrorColor";

            string serverStatus = Properties.Settings.Default._ComputerName;
            string serverColor = Properties.Settings.Default._ComputerName == "POS1" ? "ErrorColor" : "FontColor";

            if (Properties.Settings.Default._ComputerName != "POS1")
            {
                Grid.SetColumnSpan(POS, 2);

                Sync.Visibility = Visibility.Collapsed;
                Sync.IsEnabled = false;
            }


            // Set the text and background color
            AutoSync.Text = autoSyncText;
            AutoSync.Background = (System.Windows.Media.Brush)Application.Current.FindResource(syncColor);

            deviceType.Text = serverStatus;
            deviceType.Background = (System.Windows.Media.Brush)Application.Current.FindResource(serverColor);

            
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private async void POS_Click(object sender, RoutedEventArgs e)
        {
            LoginPage loginPage = new LoginPage();

            loginPage.ShowDialog();

            this.Close();
        }

        private async void SyncButton_Click(object sender, RoutedEventArgs e)
        {
            SyncMethod syncMethod = new SyncMethod(UpdateProgress);
            await syncMethod.SyncDataAsync();
            LastSync.Text = Properties.Settings.Default._LastSync.ToString();
            string NotifMessage = "Data synchronization complete!";
            NotificationPopup notificationPopup = new NotificationPopup(NotifMessage, false);
            notificationPopup.ShowDialog();
        }

        private void UpdateProgress(int value)
        {
            Dispatcher.Invoke(() => ProgressBarx.Value = value);
        }
    }
}
