using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TranQuik.Configuration;

namespace TransQuikConfiguration.Pages
{
    /// <summary>
    /// Interaction logic for DatabaseConfiguration.xaml
    /// </summary>
    public partial class DatabaseConfiguration : Page
    {
        public DatabaseConfiguration()
        {
            InitializeComponent();

            LocalDBIP.Text = DatabaseSettings.LocalDbServer;
            LocalDBName.Text = DatabaseSettings.LocalDbName;
            LocalDBUsername.Password = DatabaseSettings.LocalDbUser;
            LocalDBPassword.Password = DatabaseSettings.LocalDbPassword;
            LocalDBPort.Text = DatabaseSettings.LocalDbPort.ToString() ;
            LocalStatus.Foreground = (Brush)Application.Current.FindResource("ErrorColor");
            
            CloudDBIP.Text = DatabaseSettings.CloudDbServer;
            CloudDBName.Text = DatabaseSettings.CloudDbName;
            CloudDBUsername.Password = DatabaseSettings.CloudDbUser;
            CloudDBPassword.Password = DatabaseSettings.CloudDbPassword;
            CloudDPPort.Text = DatabaseSettings.CloudDbPort.ToString();
            CloudStatus.Foreground = (Brush)Application.Current.FindResource("ErrorColor");
        }

        private void checkLocalConnection_Click(object sender, RoutedEventArgs e)
        {
            string localDbPortPart = int.TryParse(LocalDBPort.Text, out int localDbPort) && localDbPort != 0 ? $"Port={localDbPort};" : "";

            string connectionString = $"Server={LocalDBIP.Text};{localDbPortPart}" +
                                      $"Database={LocalDBName.Text};Uid={LocalDBUsername.Password};" +
                                      $"Pwd={LocalDBPassword.Password};";
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    LocalStatus.Foreground = (Brush)Application.Current.FindResource("SuccessColor");
                    MessageBox.Show("Local MySQL connection successful!", "Connection Status", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Local MySQL connection failed: {ex.Message}", "Connection Status", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void checkCloudConnection_Click(object sender, RoutedEventArgs e)
        {
            string cloudDbPortPart = int.TryParse(CloudDPPort.Text, out int cloudDbPort) && cloudDbPort != 0 ? $",{cloudDbPort}" : "";

            // Construct the connection string using the ternary operator for the port
            string connectionString = $"Server={CloudDBIP.Text}{cloudDbPortPart};" +
                                      $"Database={CloudDBName.Text};User Id={CloudDBUsername.Password};" +
                                      $"Password={CloudDBPassword.Password};";
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    CloudStatus.Foreground = (Brush)Application.Current.FindResource("SuccessColor");
                    MessageBox.Show("Cloud SQL Server connection successful!", "Connection Status", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Cloud SQL Server connection failed: {ex.Message}", "Connection Status", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void UpdateDatabaseSettings()
        {
            // Update Local Database Settings
            DatabaseSettings.LocalDbServer = LocalDBIP.Text;
            DatabaseSettings.LocalDbPort = int.Parse(LocalDBPort.Text); // Assuming this is an integer
            DatabaseSettings.LocalDbUser = LocalDBUsername.Password;
            DatabaseSettings.LocalDbPassword = LocalDBPassword.Password;
            DatabaseSettings.LocalDbName = LocalDBName.Text;

            // Update Cloud Database Settings
            DatabaseSettings.CloudDbServer = CloudDBIP.Text;
            DatabaseSettings.CloudDbPort = int.Parse(CloudDPPort.Text); // Assuming this is an integer
            DatabaseSettings.CloudDbUser = CloudDBUsername.Password;
            DatabaseSettings.CloudDbPassword = CloudDBPassword.Password;
            DatabaseSettings.CloudDbName = CloudDBName.Text;
        }
    }
}
