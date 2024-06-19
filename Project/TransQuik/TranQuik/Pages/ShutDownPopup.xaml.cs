using Serilog;
using System;
using System.Diagnostics;
using System.Windows;
using TranQuik.Controller;

namespace TranQuik.Pages
{
    public partial class ShutDownPopup : Window
    {
        private SyncMethod syncMethod;
        public ShutDownPopup()
        {
            InitializeComponent();
            syncMethod = new SyncMethod();
        }

        private async void ShutdownButton(object sender, RoutedEventArgs e)
        {
            // Log that the application is stopping
            Log.ForContext("LogType", "ApplicationLog").Information("Application Stopped");

            // Close any secondary monitor processes if they are running
            CloseSecondaryMonitorProcess();

            // Create a new session in the local database
            await syncMethod.CreateNewSessionInLocalDatabaseAsync(Properties.Settings.Default._ComputerID, Properties.Settings.Default._AppID, 2);

            // Forcefully exit the application
            Environment.Exit(0);
        }


        private void CloseSecondaryMonitorProcess()
        {
            try
            {
                Process[] processes = Process.GetProcessesByName("TranQuikSecondaryMonitor");
                foreach (Process process in processes)
                {
                    process.Kill();
                }
            }
            catch (Exception ex)
            {
                Log.ForContext("LogType", "ErrorLog").Error(ex, "Error occurred while trying to close the secondary monitor process.");
            }
        }

        private void CancelButton(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
