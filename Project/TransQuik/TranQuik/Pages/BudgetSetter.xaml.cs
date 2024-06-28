﻿using System.Windows;
using System.Windows.Controls;
using TranQuik.Model;

namespace TranQuik.Pages
{
    public partial class BudgetSetter : Window
    {
        private int OpenClose;
        public BudgetSetter(int WhatsTypeSetter)
        {
            InitializeComponent();
            BudgetTypeSetter(WhatsTypeSetter);
        }

        private void BudgetTypeSetter(int WhatsTypeSetter)
        {
            OpenClose = WhatsTypeSetter;
            BudgetSetterNameText.Text = WhatsTypeSetter == 0 ? "INITIAL OPEN SESSION CASH" : "INITIAL CLOSE SESSION CASH" ;
        }

        private void Number_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                string buttonText = button.Content.ToString();
                string currentText = ProductQuantityTextBlock.Text;

                if (currentText == "0")
                {
                    // Replace the current "0" with the new number
                    ProductQuantityTextBlock.Text = buttonText;
                }
                else if (currentText.Length > 0 && buttonText == "0" && currentText[0] == '0')
                {
                    // Do nothing, we don't want to allow numbers with leading zero
                    return;
                }
                else
                {
                    ProductQuantityTextBlock.Text += buttonText;
                }
            }
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            ProductQuantityTextBlock.Text = string.Empty;
            if (ProductQuantityTextBlock.Text == string.Empty)
            {
                ProductQuantityTextBlock.Text = "0";
            }
        }

        private void Backspace_Click(object sender, RoutedEventArgs e)
        {
            string currentText = ProductQuantityTextBlock.Text;

            if (currentText.Length > 0)
            {
                currentText = currentText.Substring(0, currentText.Length - 1);

                if (currentText.Length == 0)
                {
                    ProductQuantityTextBlock.Text = "0";
                }
                else
                {
                    ProductQuantityTextBlock.Text = currentText;
                }
            }
        }

        private void Enter_Click(object sender, RoutedEventArgs e)
        {
            int finQuantity;
            bool isValidNumber = int.TryParse(ProductQuantityTextBlock.Text, out finQuantity);

            if (!isValidNumber || finQuantity < 0)
            {
                finQuantity = 0;
            }

            if (OpenClose == 0)
            {
                PosSession.POS_SESSION_OPENCASH = finQuantity;
            }
            else
            {
                PosSession.POS_SESSION_CLOSECASH = finQuantity;
            }

            this.Close();
        }
    }
}