﻿using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using TranQuik.Model;

namespace TranQuik.Pages
{
    /// <summary>
    /// Interaction logic for PaymentTypeWindow.xaml
    /// </summary>
    public partial class PaymentTypeWindow : Window
    {
        private MainWindow MainWindow;
        private ModelProcessing modelProcessing;
        private int PayTypeID;
        private string PayTypeName;

        public PaymentTypeWindow(MainWindow mainWindow, string GrandTotal, int PayTypeID, string PayTypeName)
        {
            InitializeComponent();
            TotalAmount.Text = GrandTotal;
            TotalPay.Text = GrandTotal.Replace(".", "");
            this.MainWindow = mainWindow;
            this.PayTypeID = PayTypeID;
            this.PayTypeName = PayTypeName;
            this.modelProcessing = mainWindow.modelProcessing;
        }

        private void Number_Click(object sender, RoutedEventArgs e)
        {
            // Remove leading zeros before appending the clicked number or dot to the displayed text
            RemoveLeadingZeros();
            if (sender is Button button)
            {
                string buttonText = button.Content.ToString();

                // Append the clicked number or dot to the displayed text
                if (buttonText == ".")
                {
                    // Check if dot is already present in the display text
                    if (!TotalPay.Text.Contains("."))
                    {
                        TotalPay.Text += buttonText; // Append dot if not already present
                    }
                }
                else
                {
                    // Append the clicked number to the display text
                    TotalPay.Text += buttonText;
                }
            }
        }

        private void RemoveLeadingZeros()
        {
            if (TotalPay.Text.StartsWith("0") && TotalPay.Text.Length > 0 && !TotalPay.Text.StartsWith("0."))
            {
                TotalPay.Text = TotalPay.Text.TrimStart('0');
            }
        }

        private void Backspace_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(TotalPay.Text))
            {
                TotalPay.Text = TotalPay.Text.Substring(0, TotalPay.Text.Length - 1);
            }
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            TotalPay.Text = "0";
        }

        private void KeyboardShow_Click(object sender, RoutedEventArgs e)
        {
            // Define the explicit path to the On-Screen Keyboard executable
            string oskPath = @"C:\Windows\System32\osk.exe";

            // Check if the On-Screen Keyboard executable exists at the specified path
            if (File.Exists(oskPath))
            {
                // Start the On-Screen Keyboard process
                Process.Start(oskPath);
            }
            else
            {
                string Message = "On ScreeKeyboard Not Found !!!";
                NotificationPopup notificationPopup = new NotificationPopup(Message, false);
                notificationPopup.ShowDialog();
            }
        }

        private void CancelButton(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void SubmitButton(object sender, RoutedEventArgs e)
        {
            string payRemark = !string.IsNullOrEmpty(NIKInput.Text) && !string.IsNullOrEmpty(NameInput.Text)
                                    ? $"{NIKInput.Text}|{NameInput.Text}"
                                    : PayRemarkText.Text;

            // Check if payRemark is null or empty
            if (string.IsNullOrEmpty(payRemark))
            {
                string message = "CANT BE PROCEED, NEED PAYREMARK !";
                NotificationPopup notificationPopup = new NotificationPopup(message, false);
                notificationPopup.ShowDialog();
                return; // Exit the method to prevent further processing
            }

            if (Properties.Settings.Default._PrinterStatus)
            {
                modelProcessing.OrderTransactionFunction(2, payRemark , TotalPay.Text, PayTypeID.ToString());
                MainWindow.TransactionDone(PayTypeID.ToString(), PayTypeName);
                TransactionStatus transactionStatus = new TransactionStatus("Success", modelProcessing);
                this.Close();
            }
        }


        private void MealCardButton_Click(object sender, RoutedEventArgs e)
        {
            PayRemarkText.Visibility = Visibility.Collapsed;
            CardMeal.Visibility = Visibility.Visible;
        }

        private void OtherCardButton_Click(object sender, RoutedEventArgs e)
        {
            PayRemarkText.Visibility = Visibility.Visible;
            CardMeal.Visibility = Visibility.Collapsed;
        }
    }
}
