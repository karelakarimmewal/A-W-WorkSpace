using MySql.Data.MySqlClient;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using TranQuik.Controller;
using TranQuik.Model;
namespace TranQuik.Pages
{
    public partial class LoginPage : Window
    {
        private Control currentFocusedControl;

        private MainWindow mainWindow;

        private SessionMethod sessionMethod;

        private NotificationPopup notificationPopup;

        private SyncMethod syncMethod;

        public LoginPage()
        {
            InitializeComponent();
            UsernameTextBox.GotFocus += TextBox_GotFocus;
            
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            currentFocusedControl = sender as Control;
        }

        private void PasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            currentFocusedControl = sender as Control;
        }

        private void Number_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                string buttonText = button.Content.ToString();

                if (currentFocusedControl == UsernameTextBox)
                {
                    UsernameTextBox.Password += buttonText;
                }
                else if (currentFocusedControl == PasswordBox)
                {
                    PasswordBox.Password += buttonText;
                }
            }
        }

        private void Enter_Click(object sender, RoutedEventArgs e)
        {
            if (currentFocusedControl == UsernameTextBox)
            {
                PasswordBox.Focus();
            }
            else
            {
                
            }
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            if (currentFocusedControl == UsernameTextBox)
            {
                UsernameTextBox.Password = string.Empty;
            }
            else if (currentFocusedControl == PasswordBox)
            {
                PasswordBox.Clear();
            }
        }

        private void Backspace_Click(object sender, RoutedEventArgs e)
        {
            if (currentFocusedControl == UsernameTextBox && UsernameTextBox.Password.Length > 0)
            {
                UsernameTextBox.Password = UsernameTextBox.Password.Substring(0, UsernameTextBox.Password.Length - 1);
            }
            else if (currentFocusedControl == PasswordBox && PasswordBox.Password.Length > 0)
            {
                PasswordBox.Password = PasswordBox.Password.Substring(0, PasswordBox.Password.Length - 1);
            }
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LocalDbConnector localDbConnector = new LocalDbConnector();

                using (MySqlConnection connection = localDbConnector.GetMySqlConnection())
                {
                    connection.Open();

                    // Query to select user information based on staff code
                    string query = "SELECT StaffID, StaffRoleID, StaffFirstName, StaffLastName FROM staffs WHERE StaffCode = @StaffCode";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@StaffCode", UsernameTextBox.Password);

                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            
                            if (reader.Read())
                            {
                                UserSessions userSessions = new UserSessions();
                                // Set the properties of UserSessions directly
                                userSessions.StaffID = reader.GetInt32("StaffID");
                                userSessions.StaffRoleID = reader.GetInt32("StaffRoleID");
                                userSessions.StaffFirstName = reader.GetString("StaffFirstName");
                                userSessions.StaffLastName = reader.GetString("StaffLastName");

                                syncMethod = new SyncMethod();
                                sessionMethod = new SessionMethod();

                                bool isOpen = await syncMethod.CheckUserSessions(userSessions);

                                if (isOpen)
                                {
                                    NotificationPopup notificationPopups = new NotificationPopup("ANOTHER SESSIONS WITH THIS USER", false);

                                    notificationPopups.ShowDialog();

                                    Environment.Exit(0);
                                }

                                NotificationPopup notificationPopup = new NotificationPopup("Login Sucessfully", false);

                                notificationPopup.ShowDialog();

                                this.Close();

                                string posName = Properties.Settings.Default._ComputerName;

                                int computerID = Properties.Settings.Default._ComputerID;

                                var mainWindow = new MainWindow();

                                await syncMethod.CreateNewSessionInLocalDatabaseAsync(Properties.Settings.Default._ComputerID, Properties.Settings.Default._AppID, 1, userSessions);

                                mainWindow.SessionNameText.Text = $"ID {userSessions.StaffID.ToString()} {userSessions.StaffLastName}";
                                mainWindow.ShowDialog();
                            }
                            else
                            {
                                NotificationPopup notificationPopup = new NotificationPopup("Login Failed", false);
                                notificationPopup.ShowDialog();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions, such as database connection errors
                Console.WriteLine("Error: " + ex.Message);
            }
        }
    }
}