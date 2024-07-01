using MySql.Data.MySqlClient;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using TranQuik.Configuration;
using TranQuik.Controller;
using TranQuik.Model;

namespace TranQuik.Pages
{
    public partial class StartUpSelector : Window
    {
        private LocalDbConnector dbConnector = new LocalDbConnector();
        public StartUpSelector()
        {
            InitializeComponent();
            InitializeApplicationAsync();

        }

        private async Task InitializeApplicationAsync()
        {
            Config.LoadAppSettings();
            await UpdateLastSyncTimeAsync();
            ConfigureAutoSyncSettings();
            UpdatingModel();
        }

        private void UpdatingModel()
        {
            StaffRoleManager.SetStaffRoleManager(dbConnector);
            ActionDesp.GetActionDespsAsync();
            OrderDetailStatus.OrderDetailStatusAsync();
        }

        private async Task UpdateLastSyncTimeAsync()
        {
            try
            {
                // Create a new instance of LocalDbConnector to get a MySqlConnection
                LocalDbConnector localDbConnector = new LocalDbConnector();

                // Define your SQL query to get the most recent SyncLastUpdate
                string query = @"
            SELECT SyncLastUpdate 
            FROM log_lastsync 
            ORDER BY SyncLastUpdate DESC 
            LIMIT 1";

                using (MySqlConnection connection = localDbConnector.GetMySqlConnection())
                {
                    await connection.OpenAsync();

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        // Execute the query and get the result
                        object result = await command.ExecuteScalarAsync();

                        if (result != null)
                        {
                            DateTime lastSyncTime = Convert.ToDateTime(result);
                            LastSync.Text = lastSyncTime.ToString("G"); // Use the desired date-time format here

                            // Optionally, you can update the Properties.Settings as well
                            Properties.Settings.Default._LastSync = lastSyncTime;
                            Properties.Settings.Default.Save();
                        }
                        else
                        {
                            LastSync.Text = "No sync record found.";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions
                Console.WriteLine("Error: " + ex.Message);
                LastSync.Text = "Error retrieving sync time.";
            }
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
            UpdateLastSyncTimeAsync();
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
            UserSessions userSessions = new UserSessions();

            AuthWin authWin = new AuthWin(userSessions);
            authWin.ShowDialog();

            bool iscan = IsCan(userSessions.Authentication_StaffRoleID);

            if (iscan)
            {
                ReportPage reportPage = new ReportPage();
                reportPage.ShowDialog();
            } else
            {
                Notification.NotificationNotPermitted();
            }
            
        }

        private void Utility_Click(object sender, RoutedEventArgs e)
        {
            UserSessions userSessions = new UserSessions();

            AuthWin authWin = new AuthWin(userSessions);
            authWin.ShowDialog();

            bool iscan = IsCan(userSessions.Authentication_StaffRoleID);


            if (iscan)
            {
                UtilityPage utilityPage = new UtilityPage();
                utilityPage.ShowDialog();
            }
            else
            {
                Notification.NotificationNotPermitted();
            }
        }

        private bool IsCan(int staffRoleID)
        {
            if (staffRoleID == 5 )
            {
                return true;
            }
            return false; // or false based on your logic
        }

    }
}
