using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TranQuik.Configuration;
using TransQuikConfiguration.Model;
using TransQuikConfiguration.Pages;


namespace TransQuikConfiguration
{
    public partial class MainWindow : Window
    {
        private Dictionary<int, List<ConfigurationMenu>> ButtonDict = new Dictionary<int, List<ConfigurationMenu>>();
        private const int ColumnCount = 4; // Number of columns in a row
        private int currentPage = 0;
        private List<Button> buttons = new List<Button>();
        private string ConfiguratioOpened;

        private DatabaseConfiguration databaseConfiguration;
        private SecondaryMonitors secondaryMonitors;
        private PrinterConfiguration printerConfiguration;
        private SyncConfiguration syncConfiguration;
        private ApplicationConfiguration applicationConfiguration;

        public MainWindow()
        {
            InitializeComponent();
            Config.LoadAppSettings();
            PopulateConfiguration();
            AddButtonsToGrid();
            
            Framing.NavigationUIVisibility = System.Windows.Navigation.NavigationUIVisibility.Hidden;
            deviceID.Foreground = (Brush)Application.Current.FindResource("WarningColor");
            deviceID.Text = $"{Properties.Settings.Default._ShopKey}";

            applicationConfiguration = new ApplicationConfiguration();
            databaseConfiguration = new DatabaseConfiguration();
            secondaryMonitors = new SecondaryMonitors();
            printerConfiguration = new PrinterConfiguration();
            syncConfiguration = new SyncConfiguration();


            buttons[1].RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }

        private void PopulateConfiguration()
        {
                List<ConfigurationMenu> menus = new List<ConfigurationMenu>
            {
                new ConfigurationMenu(1, "Application Settings", "Settings"),
                new ConfigurationMenu(2, "Secondary Monitor", "Monitor"),
                new ConfigurationMenu(3, "Sync", "Sync"),
                new ConfigurationMenu(4, "Database Connection" , "Database"),
                new ConfigurationMenu(5, "Printer" , "Printer")
            };

            // Populate the dictionary with incrementing keys and the menu items
            int key = 1;
            foreach (var menu in menus)
            {
                ButtonDict[key] = new List<ConfigurationMenu> { menu };
                key++;
            }
        }

        private void AddButtonsToGrid()
        {
            buttons.Clear();
            ConfigurationMenu.Children.Clear();
            ConfigurationMenu.ColumnDefinitions.Clear();

            for (int i = 0; i < ColumnCount; i++)
            {
                ConfigurationMenu.ColumnDefinitions.Add(new ColumnDefinition());
            }

            foreach (var kvp in ButtonDict)
            {
                foreach (var menu in kvp.Value)
                {
                    Button button = CreateButton(menu);
                    buttons.Add(button);
                    // Attach a common event handler to all buttons
                    button.Click += Button_Click;
                }
            }

            UpdateVisibleButtons();
            UpdateNavigationButtons();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button clickedButton)
            {
                // Retrieve the associated menu object from the Tag property of the clicked button
                if (clickedButton.Tag is ConfigurationMenu menu)
                {
                    // Switch behavior based on the ButtonMenuID
                    switch (menu.ButtonMenuID)
                    {
                        case 1:
                            ConfiguratioOpened = "UpdateApplicationSettings";
                            DisplayApplicationConfiguration();
                            break;
                        case 2:
                            ConfiguratioOpened = "UpdateAppSecMonitorSettings";
                            DisplayAppSecMonitorSettings();
                            break;
                        case 3:
                            ConfiguratioOpened = "UpdateSyncSettings";
                            DisplaySyncConfiguration();
                            break;
                        // Add more cases for other ButtonMenuID values as needed
                        case 4:
                            ConfiguratioOpened = "UpdateDatabaseSettings";
                            DisplayDatabaseConfiguration();
                            break;
                        case 5:
                            ConfiguratioOpened = "UpdatePrinterSettings";
                            DisplayPrinterConfiguration();
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private void DisplayApplicationConfiguration()
        {
            Framing.Content = null;
            Framing.Content = applicationConfiguration;
        }

        private void DisplayPrinterConfiguration()
        {
            Framing.Content = null;
            Framing.Content = printerConfiguration;
        }

        private void DisplaySyncConfiguration()
        {
            Framing.Content = null;
            Framing.Content = syncConfiguration;
        }

        private void DisplayDatabaseConfiguration()
        {
            Framing.Content = null;
            Framing.Content = databaseConfiguration;
        }

        private void DisplayAppSecMonitorSettings()
        {
            Framing.Content = null;
            secondaryMonitors.SecondaryMonitorUrl.Text = Properties.Settings.Default._AppSecMonitorUrl;
            Framing.Content = secondaryMonitors;
        }

        private Button CreateButton(ConfigurationMenu menu)
        {
            Button button = new Button();

            // Set Margin property for the Button
            button.Margin = new Thickness(0, 0, 0, 2);

            StackPanel stackPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
            };

            string iconKind = menu.ButtonKindIcon;
            var icon = new PackIcon { Kind = (PackIconKind)Enum.Parse(typeof(PackIconKind), iconKind) };

            TextBlock textBlock = new TextBlock
            {
                Text = menu.ButtonMenuName,
                TextWrapping = TextWrapping.Wrap,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(5, 0, 0, 0),
                Foreground = Brushes.Black, // Set text color
                FontFamily = new FontFamily("Arial"), // Set font family
                FontSize = 14, // Set font size
                FontWeight = FontWeights.SemiBold // Set font weight
            };

            stackPanel.Children.Add(icon);
            stackPanel.Children.Add(textBlock);

            button.Content = stackPanel;
            button.Background = (Brush)Application.Current.FindResource("WarningColor"); // Set background color

            // Assign the menu object to the Tag property of the button
            button.Tag = menu;

            return button;
        }

        private void UpdateVisibleButtons()
        {
            ConfigurationMenu.Children.Clear();

            int startIndex = currentPage * ColumnCount;
            for (int i = 0; i < ColumnCount; i++)
            {
                int index = startIndex + i;
                if (index < buttons.Count)
                {
                    Grid.SetColumn(buttons[index], i);
                    ConfigurationMenu.Children.Add(buttons[index]);
                }
                else
                {
                    Button blankButton = new Button
                    {
                        Content = string.Empty,
                        Background = Brushes.Green,
                        IsEnabled = false,
                        Margin = new Thickness(0,0,0,2)
                    };
                    Grid.SetColumn(blankButton, i);
                    ConfigurationMenu.Children.Add(blankButton);
                }
            }
        }

        private void UpdateNavigationButtons()
        {
            Prev.IsEnabled = currentPage > 0;
            Next.IsEnabled = (currentPage + 1) * ColumnCount < buttons.Count;
        }

        private void Prev_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage > 0)
            {
                currentPage--;
                UpdateVisibleButtons();
                UpdateNavigationButtons();
            }
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            if ((currentPage + 1) * ColumnCount < buttons.Count)
            {
                currentPage++;
                UpdateVisibleButtons();
                UpdateNavigationButtons();
            }
        }

        private void CloseApplication_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            Config.SaveAppSettings();
            saveButton.IsEnabled = false;
            applyButton.IsEnabled = true;

            MessageBox.Show("Settings saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void applyButton_Click(object sender, RoutedEventArgs e)
        {
            bool settingsApplied = false;

            switch (ConfiguratioOpened)
            {
                case "UpdateApplicationSettings":
                    saveButton.IsEnabled = true;
                    applyButton.IsEnabled = false;
                    settingsApplied = true;
                    break;
                case "UpdateDatabaseSettings":
                    databaseConfiguration.UpdateDatabaseSettings();
                    saveButton.IsEnabled = true;
                    applyButton.IsEnabled = false;
                    settingsApplied = true;
                    break;
                case "UpdateAppSecMonitorSettings":
                    secondaryMonitors.ApplySecMonitor();
                    saveButton.IsEnabled = true;
                    applyButton.IsEnabled = false;
                    settingsApplied = true;
                    break;
                case "UpdatePrinterSettings":
                    printerConfiguration.Apply();
                    saveButton.IsEnabled = true;
                    applyButton.IsEnabled = false;
                    settingsApplied = true;
                    break;
                case "UpdateSyncSettings":
                    Properties.Settings.Default.Save();
                    saveButton.IsEnabled = true;
                    applyButton.IsEnabled = false;
                    settingsApplied = true;
                    break;
                default:
                    break;
            }

            if (settingsApplied)
            {
                MessageBox.Show("Settings applied successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void configPath_Click(object sender, RoutedEventArgs e)
        {
            // Create an instance of OpenFileDialog
            var openFileDialog = new System.Windows.Forms.OpenFileDialog
            {
                Title = "Select Application Path",
                ValidateNames = false,
                CheckFileExists = false,
                CheckPathExists = true,
                FileName = "Select Folder"
            };

            // Set the dialog to pick .exe files only
            openFileDialog.Filter = "Executable Files (*.exe)|*.exe";

            // Show the dialog and check if the user clicked OK
            System.Windows.Forms.DialogResult result = openFileDialog.ShowDialog();

            // Show the dialog and check if the user clicked OK
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                // Check if the selected file is TranQuik.exe
                if (Path.GetFileName(openFileDialog.FileName) != "TranQuik.exe")
                {
                    // Notify the user and exit the method
                    MessageBox.Show("Please select TranQuik.exe", "Invalid File", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string exeDirectory = Path.GetDirectoryName(openFileDialog.FileName);
                string configDirectory = Path.Combine(exeDirectory, "Configuration");

                // Combine the directory with the config file path
                string configFilePath = Path.Combine(configDirectory, Config.ConfigFileName);

                // Set the value of ConfigFilePath
                Config.ConfigFilePath = configFilePath;

                // Ensure the config directory exists
                if (!Directory.Exists(configDirectory))
                {
                    Directory.CreateDirectory(configDirectory);
                }

                // Ensure the config file exists
                if (!File.Exists(Config.ConfigFilePath))
                {
                    // Create a new AppSettings.json file with default values
                    Config.CreateDefaultAppSettingsFile();
                }

                // Save the last location
                Properties.Settings.Default._LastLocation = exeDirectory;
                Properties.Settings.Default.Save();
            }
        }

    }
}
