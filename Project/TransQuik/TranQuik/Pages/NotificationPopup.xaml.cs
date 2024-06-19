﻿using System.Windows;
using System.Windows.Controls;
using TranQuik.Controller;

namespace TranQuik.Pages
{
    public partial class NotificationPopup : Window
    {
        public bool IsConfirmed { get; private set; } = false;

        private MainWindow mainWindow;

        private bool IsUpdate = false;

        public NotificationPopup(string Text, bool isShow, MainWindow mainWindow = null, bool isUpdate = false)
        {
            InitializeComponent();

            MessagePopup.Text = Text;
            
            this.mainWindow = mainWindow;

            IsUpdate = isUpdate;
            
            if (!isShow)
            {
                CancelButton.Visibility = Visibility.Collapsed;
                Grid.SetColumnSpan(YesButton, 2); // Set YesButton to span two columns if CancelButton is hidden
            }
        }

        private async void YesButton_Click(object sender, RoutedEventArgs e)
        {
            IsConfirmed = true;

            if (IsUpdate)
            {
                if (mainWindow != null)
                {
                    RoutedEventArgs args = new RoutedEventArgs(Button.ClickEvent);
                    mainWindow.HoldButton_Click(mainWindow.HoldButton, args);
                    IsUpdate = false;
                }
                // Forcefully exit the application
                Application.Current.Shutdown();
            }

            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            IsConfirmed = false;
            this.Close();
        }
    }
}