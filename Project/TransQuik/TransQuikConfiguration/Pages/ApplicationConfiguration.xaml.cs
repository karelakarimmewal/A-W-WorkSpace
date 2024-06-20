using System.Data.SqlClient;
using System;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using TranQuik.Configuration;
using ComboBox = System.Windows.Controls.ComboBox;
using MySql.Data.MySqlClient;
using System.Windows;

namespace TransQuikConfiguration.Pages
{
    public partial class ApplicationConfiguration : Page
    {
        public ApplicationConfiguration()
        {
            InitializeComponent();

            PopulateComboBoxesFromSettings();
        }

        private void PopulateComboBoxesFromSettings()
        {
            PopulateFontSizeComboBox();
            PopulateFontFamilyComboBox();
            PopulateAllowImageComboBox();
            PopulateApplicationStatus();
            PopulateLanguage();

            bool allowImageSetting = Properties.Settings.Default._AppAllowImage;
            string fontFamilySetting = Properties.Settings.Default._AppFontFamily;
            double fontSizeSetting = Properties.Settings.Default._AppFontSize;

            SelectComboBoxItemBySetting(comboAllowImage, allowImageSetting.ToString());
            SelectComboBoxItemBySetting(comboFontFamily, fontFamilySetting);
            SelectComboBoxItemBySetting(comboFontSize, fontSizeSetting.ToString());

            AppID.Text = Properties.Settings.Default._AppID;
            ComputerID.Text = Properties.Settings.Default._ComputerID.ToString();
            AnyDeskPath.Text = Config.anyDeskPath;
        }

        private void SelectComboBoxItemBySetting(ComboBox comboBox, string settingValue)
        {
            foreach (ComboBoxItem item in comboBox.Items)
            {
                if (item.Content != null && item.Content.ToString() == settingValue)
                {
                    comboBox.SelectedItem = item;
                    break;
                }
                else if (item.Tag != null && item.Tag.ToString() == settingValue)
                {
                    comboBox.SelectedItem = item;
                    break;
                }
            }
        }

        private void PopulateFontSizeComboBox()
        {
            for (int i = 10; i <= 14; i++)
            {
                comboFontSize.Items.Add(new ComboBoxItem { Content = i.ToString() });
            }
            // Select the first item by default
            comboFontSize.SelectedIndex = AppSettings.AppFontSize;
        }

        private void PopulateFontFamilyComboBox()
        {
            // Get the list of font families
            foreach (FontFamily fontFamily in Fonts.SystemFontFamilies)
            {
                comboFontFamily.Items.Add(new ComboBoxItem { Content = fontFamily.Source });
            }

            // Select the first item by default
            comboFontFamily.SelectedItem = AppSettings.AppFontFamily;
        }

        private void PopulateLanguage()
        {
            comboLanguage.Items.Add(new ComboBoxItem { Content = "Default", Tag = "1" });
            // Select the first item by default
            comboLanguage.SelectedIndex = 0;
        }

        private void PopulateAllowImageComboBox()
        {
            comboAllowImage.Items.Add(new ComboBoxItem { Content = "Allow", Tag = "1" });
            comboAllowImage.Items.Add(new ComboBoxItem { Content = "Don't Allow", Tag = "0" });
            // Select the first item by default
            comboAllowImage.SelectedIndex = 0;
        }

        private void PopulateApplicationStatus()
        {
            comboApplicationStatus.Items.Add(new ComboBoxItem { Content = "Commercial", Tag = "1" });
            comboApplicationStatus.Items.Add(new ComboBoxItem { Content = "Development", Tag = "0" });
            // Select the first item by default
            comboApplicationStatus.SelectedIndex = 1;
        }

        private void comboFontFamily_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboFontFamily.SelectedItem != null)
            {
                string fontFamilyName = ((ComboBoxItem)comboFontFamily.SelectedItem).Content.ToString();
                textSample.FontFamily = new FontFamily(fontFamilyName);
                AppSettings.AppFontFamily = fontFamilyName;
            }

        }

        private void comboAllowImage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboAllowImage.SelectedItem != null)
            {
                ComboBoxItem selectedItem = (ComboBoxItem)comboAllowImage.SelectedItem;
                string selectedTag = selectedItem.Tag.ToString();
                AppSettings.AppAllowImage = selectedTag == "1";
            }
        }

        private void comboApplicationStatus_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboApplicationStatus.SelectedItem != null)
            {
                ComboBoxItem selectedItem = (ComboBoxItem)comboApplicationStatus.SelectedItem;
                string selectedTag = selectedItem.Tag.ToString();
                AppSettings.AppStatus = selectedTag == "1";
            }
        }

        private void comboFontSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboFontSize.SelectedItem != null)
            {
                ComboBoxItem selectedItem = (ComboBoxItem)comboFontSize.SelectedItem;
                string selectedTag = selectedItem.Content.ToString();
                if (int.TryParse(selectedTag, out int fontSize))
                {
                    AppSettings.AppFontSize = fontSize;
                }
            }
        }

        private void ConnectDB_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                using (MySqlConnection connection = DatabaseSettings.GetMySqlConnection())
                {
                    Console.WriteLine("Attempting to open MySQL connection...");
                    connection.Open();
                    Console.WriteLine("MySQL connection opened successfully.");

                    // Query to retrieve ShopName from shop_data based on ShopID
                    string shopQuery = "SELECT ShopCode, ShopKey, ShopName FROM shop_data WHERE ShopID = @ShopID";
                    MySqlCommand shopCommand = new MySqlCommand(shopQuery, connection);
                    shopCommand.Parameters.AddWithValue("@ShopID", AppID.Text);

                    // Execute the command to retrieve ShopName
                    MySqlDataReader shopReader = shopCommand.ExecuteReader();

                    // Clear existing items in the combo box
                    comboShopName.Items.Clear();

                    // Check if any rows were returned
                    if (shopReader.HasRows)
                    {
                        while (shopReader.Read())
                        {
                            // Retrieve ShopName from the reader
                            string shopCode = shopReader.GetString(0);
                            string shopKey = shopReader.GetString(1);
                            string shopName = shopReader.GetString(2);

                            AppSettings.ShopKey = shopKey;
                            AppSettings.ShopName = shopName;
                            AppSettings.ShopCode = shopCode;
                            
                            comboShopName.Items.Add(shopName);
                            comboShopName.SelectedItem = shopName;
                        }

                        // Close the shopReader
                        shopReader.Close();
                    }

                    // Query to retrieve ComputerName from computername based on ShopID and ComputerType
                    string computerQuery = "SELECT ComputerName, ComputerID FROM computername WHERE ShopID = @ShopID AND ComputerType = 0";
                    MySqlCommand computerCommand = new MySqlCommand(computerQuery, connection);
                    computerCommand.Parameters.AddWithValue("@ShopID", AppID.Text);

                    // Execute the command to retrieve ComputerName
                    MySqlDataReader computerReader = computerCommand.ExecuteReader();

                    // Clear existing items in the combo box
                    comboComputerName.Items.Clear();

                    // Variable to hold the selected computer name
                    string selectedComputerName = null;

                    // Check if any rows were returned
                    if (computerReader.HasRows)
                    {
                        while (computerReader.Read())
                        {
                            // Retrieve ComputerName and ComputerID from the reader
                            string computerName = computerReader.GetString(0);
                            int computerID = computerReader.GetInt32(1);

                            // Add the computer name to the combo box
                            comboComputerName.Items.Add(computerName);

                            // Check if the current computer ID matches the desired ID
                            if (computerID.ToString() == ComputerID.Text)
                            {
                                selectedComputerName = computerName;
                            }
                        }

                        // Set the selected item
                        comboComputerName.SelectedItem = selectedComputerName;

                        // Close the computerReader
                        computerReader.Close();
                    }
                }
            }
            catch (MySqlException ex)
            {
                // Handle MySQL exceptions
                Console.WriteLine("MySQL Error: " + ex.Message);
                Console.WriteLine("Error Number: " + ex.Number);
                Console.WriteLine("Error Source: " + ex.Source);
                Console.WriteLine("Error Stack Trace: " + ex.StackTrace);
            }
            catch (Exception ex)
            {
                // Handle any other exceptions
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        private void comboComputerName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (comboComputerName.SelectedItem != null)
                {
                    using (MySqlConnection connection = DatabaseSettings.GetMySqlConnection())
                    {
                        connection.Open();

                        // Query to retrieve ComputerID from computername based on selected ComputerName and ShopID
                        string computerIDQuery = "SELECT ComputerID FROM computername WHERE ComputerName = @ComputerName AND ShopID = @ShopID AND ComputerType = 0";
                        MySqlCommand computerIDCommand = new MySqlCommand(computerIDQuery, connection);
                        computerIDCommand.Parameters.AddWithValue("@ComputerName", comboComputerName.SelectedItem.ToString());
                        computerIDCommand.Parameters.AddWithValue("@ShopID", AppID.Text);

                        // Execute the command to retrieve ComputerID
                        object computerID = computerIDCommand.ExecuteScalar();

                        // Set the retrieved ComputerID to the TextBox
                        if (computerID != null)
                        {
                            AppSettings.ComputerID = int.Parse(computerID.ToString());
                            ComputerID.Text = computerID.ToString();
                            AppSettings.ComputerName = comboComputerName.SelectedItem.ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        private void comboShopName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboShopName.SelectedItem != null)
            {
                AppSettings.AppID = AppID.Text.ToString();
            }
        }

        private void AnyDeskPathDialog_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // Create OpenFileDialog
            OpenFileDialog openFileDialog = new OpenFileDialog();

            // Set dialog properties
            openFileDialog.Title = "Select AnyDesk.exe";
            openFileDialog.Filter = "Executable Files (*.exe)|*.exe";
            openFileDialog.CheckFileExists = true;

            // Show dialog and check if the user selected a file
            DialogResult result = openFileDialog.ShowDialog(); // Capture the result

            if (result == System.Windows.Forms.DialogResult.OK) // Compare with DialogResult.OK
            {
                // Get the selected file path
                string filePath = openFileDialog.FileName;

                // Check if the selected file is AnyDesk.exe
                if (System.IO.Path.GetFileName(filePath).Equals("AnyDesk.exe", StringComparison.OrdinalIgnoreCase))
                {
                    AnyDeskPath.Text = filePath;
                }
                else
                {
                    // File selected is not AnyDesk.exe, show an error message or take appropriate action
                    System.Windows.MessageBox.Show("Please select AnyDesk.exe", "Error", MessageBoxButton.OK, MessageBoxImage.Error); // Specify System.Windows.MessageBox
                }
            }
        }

    }

}

