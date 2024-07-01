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
            this.ShowInTaskbar = false;
            syncMethod = new SyncMethod();
        }

        private async void ShutdownButton(object sender, RoutedEventArgs e)
        {
            // Log that the application is stopping
            Log.ForContext("LogType", "ApplicationLog").Information("Application Stopped");

            NotificationPopup notificationPopup = new NotificationPopup("DO CLOSE SESSION OR NOT", true);
            notificationPopup.ShowInTaskbar = false;
            notificationPopup.Topmost = true;
            this.Hide();
            notificationPopup.ShowDialog();
            StaffLoginLogOutTime staffLoginLogOutTime = new StaffLoginLogOutTime();
            staffLoginLogOutTime.CloseStaffSession();
            if (notificationPopup.IsConfirmed)
            {
                notificationPopup.Hide();
                BudgetSetter budgetSetter = new BudgetSetter(1);
                budgetSetter.Topmost = true;
                budgetSetter.ShowDialog();
                await syncMethod.CreateNewSessionInLocalDatabaseAsync(Properties.Settings.Default._ComputerID, Properties.Settings.Default._AppID, 2);

                // Forcefully exit the application
                Environment.Exit(0);
            }
            

            // Forcefully exit the application
            Environment.Exit(0);
        }
        private void CancelButton(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
