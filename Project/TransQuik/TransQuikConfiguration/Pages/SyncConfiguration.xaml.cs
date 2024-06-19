using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TranQuik.Configuration;

namespace TransQuikConfiguration.Pages
{
    public partial class SyncConfiguration : Page
    {
        public SyncConfiguration()
        {
            InitializeComponent();
            // Initialize toggle buttons based on _AutoSync setting
            if (Properties.Settings.Default._AutoSync)
            {
                ActiveToggleButton.IsChecked = true;
                ActiveToggleButton.Background = (Brush)Application.Current.FindResource("ErrorColor");
                ManualToggleButton.IsChecked = false;
                ManualToggleButton.Background = (Brush)Application.Current.FindResource("DisabledButtonColor");
            }
            else
            {
                ManualToggleButton.IsChecked = true;
                ManualToggleButton.Background = (Brush)Application.Current.FindResource("ErrorColor");
                ActiveToggleButton.IsChecked = false;
                ActiveToggleButton.Background = (Brush)Application.Current.FindResource("DisabledButtonColor");
            }

            // Attach the Checked event handler
            ActiveToggleButton.Checked += ToggleButton_Checked;
            ManualToggleButton.Checked += ToggleButton_Checked;
        }

        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == ActiveToggleButton && ActiveToggleButton.IsChecked == true)
            {
                ManualToggleButton.IsChecked = false;
                ManualToggleButton.Background = (Brush)Application.Current.FindResource("DisabledButtonColor");
                ActiveToggleButton.Background = (Brush)Application.Current.FindResource("ErrorColor");

                // Update the setting
                AppSettings.AutoSync = true;
                MessageBox.Show("AutoSync DB Active");
            }
            else if (sender == ManualToggleButton && ManualToggleButton.IsChecked == true)
            {
                ActiveToggleButton.IsChecked = false;
                ActiveToggleButton.Background = (Brush)Application.Current.FindResource("DisabledButtonColor");
                ManualToggleButton.Background = (Brush)Application.Current.FindResource("ErrorColor");

                // Update the setting
                AppSettings.AutoSync = false;
                MessageBox.Show("AutoSync DB Deactive");
            }
            
            Properties.Settings.Default.Save();
        }
    }
}
