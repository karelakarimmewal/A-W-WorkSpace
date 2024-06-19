using System;
using System.Configuration;

namespace TranQuik.Configuration
{
    public static class PrinterSettings
    {
        public static string PrinterName { get; set; }
        public static bool PrinterStatus { get; set; }
        public static bool PrinterDevMode { get; set; }
        public static int PrinterMarginTop { get; set; }
        public static int PrinterMarginLeft { get; set; }
        public static int PrinterMarginBottom { get; set; }
        public static int PrinterMarginRight { get; set; }
        public static string PrinterLogo { get; set; }

        public static class SavedSettings
        {
            public static void SaveAppSettings()
            {
                try
                {
                    // Update Application.Properties.Settings with AppSettings values
                    Properties.Settings.Default._PrinterStatus = PrinterSettings.PrinterStatus;
                    Properties.Settings.Default._PrinterName = PrinterSettings.PrinterName;
                    Properties.Settings.Default._PrinterDevMode = PrinterSettings.PrinterDevMode;
                    Properties.Settings.Default._PrinterMarginTop = PrinterSettings.PrinterMarginTop;
                    Properties.Settings.Default._PrinterMarginBottom = PrinterSettings.PrinterMarginBottom;
                    Properties.Settings.Default._PrinterMarginLeft = PrinterSettings.PrinterMarginLeft;
                    Properties.Settings.Default._PrinterMarginRight = PrinterSettings.PrinterMarginRight;
                    Properties.Settings.Default._PrinterLogo = PrinterSettings.PrinterLogo;

                    // Save the updated settings
                    Properties.Settings.Default.Save();
                    Console.WriteLine("AppSettings updated and saved to Application.Properties.Settings.");
                }
                catch (ConfigurationErrorsException ex)
                {
                    Console.WriteLine($"Error saving AppSettings: {ex.Message}");
                }
            }
        }
    }
}
