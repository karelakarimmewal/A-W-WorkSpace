using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Timers;

namespace TranQuik.Configuration
{
    internal class Scheduler
    {
        public int times = 60000;
        private readonly Timer timer;

        public Scheduler()
        {
            timer = new Timer();
            timer.Interval = times; // Set the interval to 15 seconds (15000 milliseconds)
            timer.Elapsed += async (sender, e) => await RunJobAsync();
        }

        public void Start()
        {
            timer.Start();
        }

        public void Stop()
        {
            timer.Stop();
        }

        private async Task RunJobAsync()
        {
            try
            {
                // Stop the timer to prevent overlapping executions
                timer.Stop();

                // Start the SJA.exe process with the specified arguments
                string sjaExecutablePath = @"C:\Program Files\SQLyog\SJA.exe";
                string jobFilePath = @"D:\Practice WPF\Project\ReplicatedtoSlave2.xml";
                string logFilePath = @"D:\Practice WPF\Project\log2.log";
                string sessionFilePath = @"C:\Users\USER_PC\AppData\Roaming\SQLyog\sjasession.xml";

                // Construct the arguments string
                string arguments = $"\"{jobFilePath}\" -l\"{logFilePath}\" -s\"{sessionFilePath}\"";

                ProcessStartInfo psi = new ProcessStartInfo(sjaExecutablePath, arguments);
                psi.WindowStyle = ProcessWindowStyle.Hidden; // Hide the command window

                using (Process process = Process.Start(psi))
                {
                    process.WaitForExit();

                    // Check the exit code to see if it was successful
                    if (process.ExitCode == 0)
                    {
                        Console.WriteLine("SJA.exe completed successfully.");
                    }
                    else
                    {
                        Console.WriteLine($"SJA.exe exited with error code {process.ExitCode}.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error running job: {ex.Message}");
            }
            finally
            {
                // Restart the timer to schedule the next job execution
                timer.Start();
            }
        }


    }
}
