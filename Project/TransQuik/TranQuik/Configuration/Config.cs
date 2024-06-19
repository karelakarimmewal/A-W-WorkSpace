using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace TranQuik.Configuration
{
    public class Config
    {
        private const string ConfigFileName = "AppSettings.json";
        private static readonly string ConfigFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configuration", ConfigFileName);
        /// Static variable to store the AnyDesk ID
        public static string AnyDeskIDs;

        public static string anyDeskPath;

        public static void AnyDeskID()
        {
            // Search for AnyDesk.exe in common locations
            anyDeskPath = FindAnyDeskExecutable();

            if (anyDeskPath != null)
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = "& '" + anyDeskPath + "' --get-id",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(processInfo))
                {
                    if (process != null)
                    {
                        process.WaitForExit();
                        string output = process.StandardOutput.ReadToEnd().Trim();
                        Console.WriteLine("AnyDesk ID: " + output);

                        // Format the output to the desired pattern
                        output = FormatAnyDeskID(output);
                        output = output == "SERVICE_NOT_RUNNING" ? "Open AnyDesk" : output;

                        // Assign the formatted output to the static variable
                        AnyDeskIDs = output;
                    }
                    else
                    {
                        AnyDeskIDs = "ANYDESK NOT FOUND";
                    }
                }
            }
            else
            {
                AnyDeskIDs = "ANYDESK NOT FOUND";
            }
        }

        private static string FindAnyDeskExecutable()
        {
            // List of common installation paths for AnyDesk
            string[] possiblePaths =
            {
                @"C:\Program Files (x86)\AnyDesk\AnyDesk.exe",
                @"C:\Program Files\AnyDesk\AnyDesk.exe",
                @"C:\Users\USER_PC\Downloads\AnyDesk.exe"
                // Add more paths if needed
            };

            foreach (string path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    return path;
                }
            }

            return "ServicesNotFound"; // AnyDesk executable not found
        }

        private static string FormatAnyDeskID(string anyDeskID)
        {
            // Ensure the AnyDesk ID has the correct length (9 digits)
            if (anyDeskID.Length != 10)
            {
                // If the length is not as expected, return the original ID
                return anyDeskID;
            }

            // Format the AnyDesk ID as "1-744-276-174"
            string formattedID = $"1-{anyDeskID.Substring(0, 3)}-{anyDeskID.Substring(3, 3)}-{anyDeskID.Substring(6, 3)}";
            return formattedID;
        }

        public static Dictionary<string, string> LoadPrinterSettings()
        {
            string settingsFilePath = "Configuration/printer.txt";

            // Check if the settings file exists
            if (!File.Exists(settingsFilePath))
            {
                // Create the directory if it doesn't exist
                Directory.CreateDirectory(Path.GetDirectoryName(settingsFilePath));

                // Create the settings file with default values
                var defaultSettings = new Dictionary<string, string>
                {
                    { "BusinessAddress", "KALAU INI BERUBAH BERARTI TESTING" },
                    { "BusinessPhone", "123456789012" },
                    { "FooterText1", "share your feedback" },
                    { "FooterText2", "awrestaurant.co.id/feedback" },
                    { "FooterText3", "MUI Halal Cert 0016005754021" },
                    { "FooterText4", "Untuk mendapatkan e-coupon dan promo" },
                    { "FooterText5", "Follow instagram @awrestaurantid" }
                };

                // Write the default settings to the file
                using (var writer = new StreamWriter(settingsFilePath))
                {
                    foreach (var kvp in defaultSettings)
                    {
                        writer.WriteLine($"{kvp.Key}={kvp.Value}");
                    }
                }
            }

            // Load the settings from the file
            var settings = new Dictionary<string, string>();
            var lines = File.ReadAllLines(settingsFilePath);

            foreach (var line in lines)
            {
                var parts = line.Split(new[] { '=' }, 2);
                if (parts.Length == 2)
                {
                    settings[parts[0]] = parts[1];
                }
            }

            return settings;
        }

        public static void LoadAppSettings()
        {
            try
            {
                EnsureConfigDirectoryAndFile(); // Ensure the directory and file exist

                Dictionary<string, string> appSettings = new Dictionary<string, string>();

                string json = File.ReadAllText(ConfigFilePath);
                appSettings = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

                // Update application properties with loaded settings
                UpdateAppSettings(appSettings);
                UpdateDatabaseSettings(appSettings);
                UpdatePrinterSettings(appSettings);

                // Save the loaded settings back to file to ensure consistency

                SaveAppSettings();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading AppSettings: {ex.Message}");
            }
        }

        private static void EnsureConfigDirectoryAndFile()
        {
            string directory = Path.GetDirectoryName(ConfigFilePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (!File.Exists(ConfigFilePath))
            {
                // Create a new AppSettings.json file with default values
                CreateDefaultAppSettingsFile();
            }
        }

        private static void CreateDefaultAppSettingsFile()
        {
            Dictionary<string, string> defaultSettings = new Dictionary<string, string>
            {
                // THIS IS FOR APPLICATION
                { "_ShopKey", "0000-0000-0000-0000" },
                { "_AppFontSize", "18" },
                { "_AppFontFamily", "Arial" },
                { "_AppSaleMode", "3" },
                { "_AppID", "00" },
                { "_AppSecMonitor", "False" },
                { "_AppSecMonitorBorder", "1" },
                { "_AppSecMonitorUrl", "" },
                { "_AppSecMonitorLoop", "0" },
                { "_AppAllowImage", "False" },
                { "_AppStatus", "False" },
                { "_ComputerID", "000" },
                { "_ComputerName", "POS0" },
                { "_AutoSync", "False" },
                { "_LastSync", "" },

                // THIS IS FOR PRINTER
                { "_PrinterStatus", "False" },
                { "_PrinterName", "" },
                { "_PrinterDevMode", "1" },
                { "_PrinterMarginTop", "0" },
                { "_PrinterMarginLeft", "0" },
                { "_PrinterMarginRight", "0" },
                { "_PrinterMarginBottom", "0" },
                { "_PrinterLogo", "receiptlogo.bmp" },

                // THIS IS FOR DATABASE
                { "_LocalDbServer", "localhost" },
                { "_LocalDbPort", "3308" },
                { "_LocalDbUser", "vtecPOS" },
                { "_LocalDbPassword", "vtecpwnet" },
                { "_LocalDbName", "vtectestaw" },
                { "_CloudDbServer", "" },
                { "_CloudDbPort", "0" },
                { "_CloudDbUser", "" },
                { "_CloudDbPassword", "" },
                { "_CloudDbName", "" }
            };

            string json = JsonSerializer.Serialize(defaultSettings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(ConfigFilePath, json);
        }

        private static void UpdateAppSettings(Dictionary<string, string> appSettings)
        {
            AppSettings.ShopKey = GetSettingString(appSettings, "_ShopKey", AppSettings.ShopKey);
            AppSettings.AppFontSize = GetSettingInt(appSettings, "_AppFontSize", AppSettings.AppFontSize);
            AppSettings.AppFontFamily = GetSettingString(appSettings, "_AppFontFamily", AppSettings.AppFontFamily);
            AppSettings.AppSaleMode = GetSettingInt(appSettings, "_AppSaleMode", AppSettings.AppSaleMode);
            AppSettings.AppID = GetSettingString(appSettings, "_AppID", AppSettings.AppID);
            AppSettings.AppSecMonitor = GetSettingBool(appSettings, "_AppSecMonitor", AppSettings.AppSecMonitor);
            AppSettings.AppSecMonitorBorder = GetSettingInt(appSettings, "_AppSecMonitorBorder", AppSettings.AppSecMonitorBorder);
            AppSettings.AppSecMonitorUrl = GetSettingString(appSettings, "_AppSecMonitorUrl", AppSettings.AppSecMonitorUrl);
            AppSettings.AppSecMonitorLoop = GetSettingInt(appSettings, "_AppSecMonitorLoop", AppSettings.AppSecMonitorLoop);
            AppSettings.AppAllowImage = GetSettingBool(appSettings, "_AppAllowImage", AppSettings.AppAllowImage);
            AppSettings.AppStatus = GetSettingBool(appSettings, "_AppStatus", AppSettings.AppStatus);
            AppSettings.ComputerID = GetSettingInt(appSettings, "_ComputerID", AppSettings.ComputerID);
            AppSettings.ComputerName = GetSettingString(appSettings, "_ComputerName", AppSettings.ComputerName);
            AppSettings.AutoSync = GetSettingBool(appSettings, "_AutoSync", AppSettings.AutoSync);
            AppSettings.LastSync = GetSettingDateTime(appSettings, "_LastSync", AppSettings.LastSync);
            SavedSettings.SaveAppSettings();
        }

        private static void UpdateDatabaseSettings(Dictionary<string, string> appSettings)
        {
            DatabaseSettings.LocalDbServer = GetSettingString(appSettings, "_LocalDbServer", DatabaseSettings.LocalDbServer);
            DatabaseSettings.LocalDbPort = GetSettingInt(appSettings, "_LocalDbPort", DatabaseSettings.LocalDbPort);
            DatabaseSettings.LocalDbUser = GetSettingString(appSettings, "_LocalDbUser", DatabaseSettings.LocalDbUser);
            DatabaseSettings.LocalDbPassword = GetSettingString(appSettings, "_LocalDbPassword", DatabaseSettings.LocalDbPassword);
            DatabaseSettings.LocalDbName = GetSettingString(appSettings, "_LocalDbName", DatabaseSettings.LocalDbName);

            DatabaseSettings.CloudDbServer = GetSettingString(appSettings, "_CloudDbServer", DatabaseSettings.CloudDbServer);
            DatabaseSettings.CloudDbPort = GetSettingInt(appSettings, "_CloudDbPort", DatabaseSettings.CloudDbPort);
            DatabaseSettings.CloudDbUser = GetSettingString(appSettings, "_CloudDbUser", DatabaseSettings.CloudDbUser);
            DatabaseSettings.CloudDbPassword = GetSettingString(appSettings, "_CloudDbPassword", DatabaseSettings.CloudDbPassword);
            DatabaseSettings.CloudDbName = GetSettingString(appSettings, "_CloudDbName", DatabaseSettings.CloudDbName);
        }

        private static void UpdatePrinterSettings(Dictionary<string, string> appSettings)
        {
            PrinterSettings.PrinterStatus = GetSettingBool(appSettings, "_PrinterStatus", PrinterSettings.PrinterStatus);
            PrinterSettings.PrinterName = GetSettingString(appSettings, "_PrinterName", PrinterSettings.PrinterName);
            PrinterSettings.PrinterDevMode = GetSettingBool(appSettings, "_PrinterDevMode", PrinterSettings.PrinterDevMode);
            PrinterSettings.PrinterMarginTop = GetSettingInt(appSettings, "_PrinterMarginTop", PrinterSettings.PrinterMarginTop);
            PrinterSettings.PrinterMarginBottom = GetSettingInt(appSettings, "_PrinterMarginBottom", PrinterSettings.PrinterMarginBottom);
            PrinterSettings.PrinterMarginLeft = GetSettingInt(appSettings, "_PrinterMarginLeft", PrinterSettings.PrinterMarginLeft);
            PrinterSettings.PrinterMarginRight = GetSettingInt(appSettings, "_PrinterMarginRight", PrinterSettings.PrinterMarginRight);
            PrinterSettings.PrinterLogo = GetSettingString(appSettings, "_PrinterLogo", PrinterSettings.PrinterLogo);
            PrinterSettings.SavedSettings.SaveAppSettings();
        }

        private static int GetSettingInt(Dictionary<string, string> settings, string key, int defaultValue)
        {
            if (settings.TryGetValue(key, out string value) && int.TryParse(value, out int result))
            {
                return result;
            }
            return defaultValue;
        }

        private static string GetSettingString(Dictionary<string, string> settings, string key, string defaultValue)
        {
            if (settings.TryGetValue(key, out string value))
            {
                return value;
            }
            return defaultValue;
        }

        private static bool GetSettingBool(Dictionary<string, string> settings, string key, bool defaultValue)
        {
            if (settings.TryGetValue(key, out string value) && bool.TryParse(value, out bool result))
            {
                return result;
            }
            return defaultValue;
        }

        private static DateTime GetSettingDateTime(Dictionary<string, string> settings, string key, DateTime defaultValue)
        {
            if (settings.TryGetValue(key, out string value) && DateTime.TryParse(value, out DateTime result))
            {
                return result;
            }
            return defaultValue;
        }
        // Method to save current application settings to AppSettings.json
        public static void SaveAppSettings()
        {
            try
            {
                Dictionary<string, string> appSettings = new Dictionary<string, string>
                {
                    { "_ShopKey", AppSettings.ShopKey},
                    { "_AppFontSize", AppSettings.AppFontSize.ToString() },
                    { "_AppFontFamily", AppSettings.AppFontFamily },
                    { "_AppSaleMode", AppSettings.AppSaleMode.ToString() },
                    { "_AppID", AppSettings.AppID },
                    { "_AppSecMonitor", AppSettings.AppSecMonitor.ToString() },
                    { "_AppSecMonitorBorder", AppSettings.AppSecMonitorBorder.ToString() },
                    { "_AppSecMonitorUrl", AppSettings.AppSecMonitorUrl },
                    { "_AppSecMonitorLoop", AppSettings.AppSecMonitorLoop.ToString() },
                    { "_AppAllowImage", AppSettings.AppAllowImage.ToString() },
                    { "_AppStatus", AppSettings.AppStatus.ToString() },
                    { "_ComputerID", AppSettings.ComputerID.ToString() },
                    { "_ComputerName", AppSettings.ComputerName.ToString() },
                    { "_AutoSync", AppSettings.AutoSync.ToString() },
                    { "_LastSync", AppSettings.LastSync.ToString() },

                    { "_PrinterStatus", PrinterSettings.PrinterStatus.ToString() },
                    { "_PrinterName", PrinterSettings.PrinterName },
                    { "_PrinterDevMode", PrinterSettings.PrinterDevMode.ToString() },
                    { "_PrinterMarginTop", PrinterSettings.PrinterMarginTop.ToString() },
                    { "_PrinterMarginBottom", PrinterSettings.PrinterMarginBottom.ToString() },
                    { "_PrinterMarginLeft", PrinterSettings.PrinterMarginLeft.ToString() },
                    { "_PrinterMarginRight", PrinterSettings.PrinterMarginRight.ToString() },
                    { "_PrinterLogo", PrinterSettings.PrinterLogo},

                    { "_LocalDbServer", DatabaseSettings.LocalDbServer },
                    { "_LocalDbPort", DatabaseSettings.LocalDbPort.ToString() },
                    { "_LocalDbUser", DatabaseSettings.LocalDbUser },
                    { "_LocalDbPassword", DatabaseSettings.LocalDbPassword },
                    { "_LocalDbName", DatabaseSettings.LocalDbName },

                    { "_CloudDbServer", DatabaseSettings.CloudDbServer },
                    { "_CloudDbPort", DatabaseSettings.CloudDbPort.ToString() },
                    { "_CloudDbUser", DatabaseSettings.CloudDbUser },
                    { "_CloudDbPassword", DatabaseSettings.CloudDbPassword },
                    { "_CloudDbName", DatabaseSettings.CloudDbName }
                };

                string json = JsonSerializer.Serialize(appSettings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(ConfigFilePath, json);

                Console.WriteLine("AppSettings updated and saved.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving AppSettings: {ex.Message}");
            }
        }
    }
}
