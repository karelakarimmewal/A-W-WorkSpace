using System.Windows;
using TransQuik_Sync_App.Controller;

namespace TransQuik_Sync_App
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void SyncButton_Click(object sender, RoutedEventArgs e)
        {
            SyncMethod syncMethod = new SyncMethod(UpdateProgress);
            await syncMethod.SyncDataAsync();
            MessageBox.Show("Data synchronization complete!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void UpdateProgress(int value)
        {
            Dispatcher.Invoke(() => ProgressBarx.Value = value);
        }
    }
}
