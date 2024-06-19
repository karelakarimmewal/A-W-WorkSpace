using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using TranQuik.Configuration;

namespace TransQuikConfiguration.Pages
{
    /// <summary>
    /// Interaction logic for PrinterConfiguration.xaml
    /// </summary>
    public partial class PrinterConfiguration : Page
    {
        public PrinterConfiguration()
        {
            InitializeComponent();
            LoadPrinters();
            HeaderFooterScripts();
            printerList.SelectedItem = Properties.Settings.Default._PrinterName;
            developmentModeToggle.SelectedItem = Properties.Settings.Default._PrinterDevMode ? FindComboBoxItemByContent("Yes") : FindComboBoxItemByContent("No");
            marginBottomValue.Text = Properties.Settings.Default._PrinterMarginBottom.ToString();
            marginLeftValue.Text = Properties.Settings.Default._PrinterMarginLeft.ToString();
            marginTopValue.Text = Properties.Settings.Default._PrinterMarginTop.ToString();
            marginRightValue.Text = Properties.Settings.Default._PrinterMarginRight.ToString();
            PrinterLogoPath.Text = Properties.Settings.Default._PrinterLogo;
        }

        private void HeaderFooterScripts()
        {
            // Load printer settings
            Dictionary<string, string> printerSettings = Config.LoadPrinterSettings();

            // Construct the header footer text
            string headerFooterText = string.Join("\n", printerSettings.Select(kvp => $"{kvp.Key}={kvp.Value}"));

            // Replace '\n' with your chosen delimiter
            headerFooterText = headerFooterText.Replace("\n", "||"); // You can choose any delimiter you like

            HeaderFooterScript.Text = headerFooterText;
        }

        private void saveConfig_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // Load printer settings
            Dictionary<string, string> printerSettings = Config.LoadPrinterSettings();

            // Parse the text from HeaderFooterScript.Text
            string[] lines = HeaderFooterScript.Text.Split(new[] { "||" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                string[] parts = line.Split('=');
                if (parts.Length == 2)
                {
                    string key = parts[0].Trim();
                    string value = parts[1].Trim();
                    if (printerSettings.ContainsKey(key))
                    {
                        printerSettings[key] = value;
                    }
                    else
                    {
                        // If the key doesn't exist, you may choose to add it or handle the situation accordingly
                    }
                }
                else
                {
                    // Handle incorrectly formatted lines as needed
                }
            }

            string settingsFilePath = "printer.txt";
            string configDirectory = Path.Combine(Config.lastLocation, "Configuration");
            string configFilePath = Path.Combine(configDirectory, settingsFilePath);

            // Write the settings to the file
            using (var writer = new StreamWriter(configFilePath))
            {
                foreach (var kvp in printerSettings)
                {
                    writer.WriteLine($"{kvp.Key}={kvp.Value}");
                }
            }

            // Optionally, you can notify the user that the settings have been saved
            MessageBox.Show("Printer settings saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void loadConfig_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
            openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;
                var settings = new Dictionary<string, string>();
                var lines = File.ReadAllLines(filePath);

                foreach (var line in lines)
                {
                    var parts = line.Split(new[] { '=' }, 2);
                    if (parts.Length == 2)
                    {
                        settings[parts[0]] = parts[1];
                    }
                }

                string headerFooterText = string.Join("\n", settings.Select(kvp => $"{kvp.Key}={kvp.Value}"));

                // Replace '\n' with your chosen delimiter
                headerFooterText = headerFooterText.Replace("\n", "||"); // You can choose any delimiter you like

                HeaderFooterScript.Text = headerFooterText;

                // Optionally, you can notify the user that the settings have been loaded
                MessageBox.Show("Printer settings loaded successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private ComboBoxItem FindComboBoxItemByContent(string content)
        {
            foreach (ComboBoxItem item in developmentModeToggle.Items)
            {
                if (item.Content.ToString() == content)
                {
                    return item;
                }
            }
            return null; // If no item is found
        }

        private void LoadPrinters()
        {
            printerList.Items.Clear();
            PrintServer printServer = new PrintServer();
            PrintQueueCollection printQueues = printServer.GetPrintQueues(new[] { EnumeratedPrintQueueTypes.Local, EnumeratedPrintQueueTypes.Connections });

            foreach (PrintQueue printer in printQueues)
            {
                printerList.Items.Add(printer.Name);
            }
        }

        private void refreshPrinter_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            LoadPrinters();
        }

        public void Apply()
        {
            ComboBoxItem selectedItem = (ComboBoxItem)developmentModeToggle.SelectedItem;
            string tagValue = selectedItem.Tag.ToString();
            bool printerDevMode = tagValue == "1"; // Assuming "1" represents true, and "0" represents false
            PrinterSettings.PrinterName = printerList.SelectedItem.ToString();
            PrinterSettings.PrinterDevMode = printerDevMode;
            PrinterSettings.PrinterMarginTop = int.Parse(marginTopValue.Text);
            PrinterSettings.PrinterMarginBottom = int.Parse(marginBottomValue.Text);
            PrinterSettings.PrinterMarginLeft = int.Parse(marginLeftValue.Text);
            PrinterSettings.PrinterMarginRight = int.Parse(marginRightValue.Text);
            PrinterSettings.PrinterLogo = PrinterLogoPath.Text;
        }

        private void loadDefault_Click(object sender, RoutedEventArgs e)
        {
            // Default settings
            var defaultSettings = new Dictionary<string, string>
            {
                { "BusinessAddress", "FILL HERE" },
                { "BusinessPhone", "FILL HERE" },
                { "FooterText1", "share your feedback" },
                { "FooterText2", "awrestaurant.co.id/feedback" },
                { "FooterText3", "MUI Halal Cert 0016005754021" },
                { "FooterText4", "Untuk mendapatkan e-coupon dan promo" },
                { "FooterText5", "Follow instagram @awrestaurantid" }
            };

            // Construct the text from the default settings
            string headerFooterText = string.Join("\n", defaultSettings.Select(kvp => $"{kvp.Key}={kvp.Value}"));

            // Set the text to the HeaderFooterScript TextBox
            HeaderFooterScript.Text = headerFooterText;

            // Optionally, you can notify the user that the default settings have been loaded
            MessageBox.Show("Default settings loaded successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OpenPathDialog_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();

            // Set initial directory to the combined path
            string baseDirectory = System.IO.Path.GetDirectoryName(Config.ConfigFilePath);
            string relativePath = @"..\Resource\Logo\";
            string combinedPath = System.IO.Path.Combine(baseDirectory, relativePath);

            // Log the combined path to check if it's correct
            Console.WriteLine($"Combined Path: {combinedPath}");

            // Check if the combined path exists
            if (System.IO.Directory.Exists(combinedPath))
            {
                // Set filter for file extension and default file extension
                openFileDialog.Filter = "BMP files (*.bmp)|*.bmp|All files (*.*)|*.*";

                // Display OpenFileDialog by calling ShowDialog method
                System.Windows.Forms.DialogResult result = openFileDialog.ShowDialog();

                // Get the selected file name and display in a TextBox
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    // Open document
                    string filename = openFileDialog.FileName;

                    // Assuming you have a TextBox named 'PrinterLogoPath'
                    PrinterLogoPath.Text = Path.GetFileName(filename);

                    // Check if the file is already in the combinedPath directory
                    string destinationPath = System.IO.Path.Combine(combinedPath, System.IO.Path.GetFileName(filename));

                    if (!System.IO.File.Exists(destinationPath))
                    {
                        // Copy the file to the combined path
                        System.IO.File.Copy(filename, destinationPath);

                        // Update the TextBox to show the new path
                        string filenameWithExtension = openFileDialog.FileName;
                        PrinterLogoPath.Text = Path.GetFileName(filenameWithExtension);
                    }

                    if (PrinterLogoPath.Text != null)
                    {
                        PrinterSettings.PrinterLogo = PrinterLogoPath.Text;
                    }
                }
            }
            else
            {
                // Display error message with the combined path
                MessageBox.Show($"The directory does not exist: {combinedPath}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void loadFromDB_Click(object sender, RoutedEventArgs e)
        {
            // Create a DataTable to hold the retrieved data
            DataTable dataTable = new DataTable();
            string connectionString = $"Server={DatabaseSettings.LocalDbServer};Port={DatabaseSettings.LocalDbPort};" +
                                       $"Database={DatabaseSettings.LocalDbName};Uid={DatabaseSettings.LocalDbUser};" +
                                       $"Pwd={DatabaseSettings.LocalDbPassword};";
            // Define your SQL query
            string query = "SELECT ID, TextInLine, LineType, LineOrder, ShopID, DocumentTypeID FROM receiptheaderfooter";

            try
            {
                // Create a MySqlConnection using the connection string
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    // Open the connection
                    connection.Open();

                    // Create a MySqlCommand with the SQL query and connection
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        // Execute the SQL command and retrieve the data reader
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            // Create the columns in the DataTable
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                dataTable.Columns.Add(reader.GetName(i));
                            }

                            // Read the data from the reader and add it to the DataTable
                            while (reader.Read())
                            {
                                DataRow row = dataTable.NewRow();
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    row[i] = reader.GetValue(i);
                                }
                                dataTable.Rows.Add(row);
                            }
                        }
                    }
                }

                // Dictionary to store footer text by ID
                Dictionary<int, string> footerTexts = new Dictionary<int, string>();

                foreach (DataRow row in dataTable.Rows)
                {
                    // Access data for each row
                    int id = Convert.ToInt32(row["ID"]);
                    string textInLine = row["TextInLine"].ToString();

                    // Check if the ID is within the range of footer text IDs
                    if (id >= 3003 && id <= 3007)
                    {
                        // Store the footer text
                        footerTexts[id] = textInLine;
                    }
                }

                // Set the default footer text
                var defaultSettings = new Dictionary<string, string>
        {
            { "BusinessAddress", "FILL HERE" },
            { "BusinessPhone", "FILL HERE" },
            { "FooterText1", "share your feedback" },
            { "FooterText2", "awrestaurant.co.id/feedback" },
            { "FooterText3", "MUI Halal Cert 0016005754021" },
            { "FooterText4", "Untuk mendapatkan e-coupon dan promo" },
            { "FooterText5", "Follow instagram @awrestaurantid" }
        };

                // Fill in the footer text with database values if available, otherwise use default values
                for (int i = 1; i <= 5; i++)
                {
                    string key = $"FooterText{i}";
                    if (footerTexts.ContainsKey(3002 + i))
                    {
                        defaultSettings[key] = footerTexts[3002 + i];
                    }
                }

                // Construct the text from the default settings with the delimiter
                string headerFooterText = string.Join("||", defaultSettings.Select(kvp => $"{kvp.Key}={kvp.Value}"));

                // Set the text to the HeaderFooterScript TextBox
                HeaderFooterScript.Text = headerFooterText;
            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur
                MessageBox.Show("An error occurred: " + ex.Message, "Error");
            }
        }
    }
}
