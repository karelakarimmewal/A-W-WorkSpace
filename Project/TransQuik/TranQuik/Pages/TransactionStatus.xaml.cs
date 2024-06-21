using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using TranQuik.Model;

namespace TranQuik.Pages
{
    /// <summary>
    /// Interaction logic for TransactionStatus.xaml
    /// </summary>
    public partial class TransactionStatus : Window
    {
        private ModelProcessing modelProcessing;
        private string tStatus;
        public TransactionStatus(string TStatus, ModelProcessing modelProcessing)
        {
            InitializeComponent();

            // Store the reference to ModelProcessing
            this.modelProcessing = modelProcessing;

            // Call the method to handle transaction status
            tStatus = TStatus;
            HandleTransactionStatus(TStatus);
        }

        private void HandleTransactionStatus(string TStatus)
        {
            switch (TStatus)
            {
                case "Success":
                    // Transaction is successful
                    SuccessCase();
                    break;
                case "Cancel":
                    // Transaction has failed
                    FailedCase();
                    break;
                case "Void":
                    // Transaction has failed
                    VoidCase();
                    break;
                default:
                    // Handle other cases if needed
                    break;
            }
        }

        private void VoidCase()
        {
            transactionStatus.Text = "Transaction Void";
            transactionStatus.Foreground = (Brush)Application.Current.FindResource("ErrorColor");
            Close.Background = (Brush)Application.Current.FindResource("ErrorColor");
        }

        private void SuccessCase()
        {
            transactionStatus.Text = $"Transaction Success, Change = {modelProcessing.mainWindow.TotalReturnCalculator.Text}";
            transactionStatus.TextWrapping = TextWrapping.Wrap;
            transactionStatus.Foreground = (Brush)Application.Current.FindResource("SuccessColor");
            Close.Background = (Brush)Application.Current.FindResource("SuccessColor");
        }

        private void FailedCase()
        {
            transactionStatus.Text = "Transaction Unsuccessfully";
            transactionStatus.Foreground = (Brush)Application.Current.FindResource("ErrorColor");
            Close.Background = (Brush)Application.Current.FindResource("ErrorColor");
        }

        private async void doneButton(object sender, RoutedEventArgs e)
        {
            // Close the window asynchronously
            await CloseWindowAsync();
        }

        private async Task CloseWindowAsync()
        {
            // Close the window
            this.Close();

            // Wait a moment for the window to close
            await Task.Delay(100); // Adjust delay time as needed

            // Reset the UI
            modelProcessing.ResetUI(tStatus);
        }
    }
}
