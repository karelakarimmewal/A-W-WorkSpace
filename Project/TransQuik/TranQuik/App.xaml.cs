using Serilog;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TranQuik.Configuration;
using TranQuik.Controller;
using TranQuik.Model;
using TranQuik.Pages;

namespace TranQuik
{
    public partial class App : Application
    {
        private const string MutexName = "TranQuikApplicationMutex";
        private Mutex singleInstanceMutex;

        public App()
        {
            // Set the default culture to Indonesian (id-ID)
            CultureInfo indonesianCulture = new CultureInfo("id-ID");
            CultureInfo.DefaultThreadCurrentCulture = indonesianCulture;
            CultureInfo.DefaultThreadCurrentUICulture = indonesianCulture;

            ConfigurationLogging configLogging = new ConfigurationLogging();
            configLogging.ConfigureLogging();

            singleInstanceMutex = new Mutex(true, MutexName, out bool isFirstInstance);

            if (!isFirstInstance)
            {
                MessageBox.Show("Another instance of the application is already running. Please wait or close the existing instance.",
                    "Application Already Running",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                // Terminate this instance
                Environment.Exit(0);
            }

            Log.ForContext("LogType", "ApplicationLog").Information($"Application Started");
        }

        private void MyEventHandler(object sender, TouchEventArgs e)
        {
            Log.Information($"Touch event occurred on element: {sender}");
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Scheduler();
            Process currentProcess = Process.GetCurrentProcess();
            currentProcess.PriorityClass = ProcessPriorityClass.High;
            EventManager.RegisterClassHandler(typeof(UIElement), UIElement.TouchDownEvent, new EventHandler<TouchEventArgs>(MyEventHandler));
        }

        private async Task PerformCleanupAsync()
        {
            try
            {
                if (!string.IsNullOrEmpty(UserSessions.Current_StaffID.ToString()))
                {
                    SyncMethod syncMethod = new SyncMethod();
                    await syncMethod.CreateNewSessionInLocalDatabaseAsync(TranQuik.Properties.Settings.Default._ComputerID, TranQuik.Properties.Settings.Default._AppID, 2);
                }                
            }
            catch (Exception ex)
            {
                // Handle exceptions appropriately
                MessageBox.Show($"An error occurred during cleanup: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void Scheduler()
        {
            // Define the path to the job file
            if (TranQuik.Properties.Settings.Default._ComputerName == "POS1")
            {
                Scheduler scheduler = new Scheduler();
                scheduler.Start();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Release the mutex on exit
            singleInstanceMutex?.Close();
            base.OnExit(e);
            PerformCleanupAsync();
        }
    }
}
