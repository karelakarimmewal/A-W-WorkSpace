using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TranQuik.Model;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Xml.Linq;

namespace TranQuik.Pages
{
    /// <summary>
    /// Interaction logic for ReportPage.xaml
    /// </summary>
    public partial class ReportPage : Window
    {
        private int currentPage = 0;
        private const int PageSize = 16; // 4 columns * 4 rows
        private Dictionary<string, PackIconKind> ReportItemDictionary;

        public ReportPage()
        {
            InitializeComponent();
            ReportItemDictionary = ReportManager.GetReportManagerItem();
            PopulateGrid();
        }

        private void closeWindow_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void PopulateGrid()
        {
            int totalItems = ReportItemDictionary.Count;
            int startIndex = currentPage * PageSize;
            int endIndex = Math.Min(startIndex + PageSize, totalItems);

            // Clear previous content
            myGrid.Children.Clear();

            int row = 0, column = 0;

            for (int i = startIndex; i < endIndex; i++)
            {
                var item = ReportItemDictionary.ElementAt(i);
                Button button = CreateButton(item.Key, item.Value);
                myGrid.Children.Add(button);
                Grid.SetRow(button, row);
                Grid.SetColumn(button, column);

                column++;
                if (column >= 4)
                {
                    column = 0;
                    row++;
                }
            }

            // Add remaining disabled buttons if necessary
            while (row < 4)
            {
                Button button = new Button { IsEnabled = false };
                button.Margin = new Thickness(3);
                button.Effect = (Effect)Application.Current.FindResource("DropShadowEffect");
                myGrid.Children.Add(button);
                Grid.SetRow(button, row);
                Grid.SetColumn(button, column);

                column++;
                if (column >= 4)
                {
                    column = 0;
                    row++;
                }
            }

            // Add navigation buttons if needed
            AddNavigationButtons(totalItems);
        }

        private Button CreateButton(string name, PackIconKind iconKind)
        {
            PackIcon icon = new PackIcon
            {
                Kind = iconKind,
                Width = 24,
                Height = 24,
                Margin = new Thickness(2),
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = Brushes.Black
            };

            TextBlock textBlock = new TextBlock
            {
                Text = name,
                FontFamily = new FontFamily("Arial"), // Example font family
                FontSize = 12, // Example font size
                FontStyle = FontStyles.Normal, // Example font style
                Foreground = Brushes.Black, // Example font color
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(2),
                FontWeight = FontWeights.Regular,
            };

            Button button = new Button
            {
                Content = new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Children =
                    {
                        icon,
                        textBlock
                    }
                }, Tag = name,
                 Background = Brushes.Azure,
                 Effect = (Effect)Application.Current.FindResource("DropShadowEffect"),
                 Margin = new Thickness(3),
            };

            // Attach click event handler
            button.Click += ReportItemClicked;

            return button;
        }

        private void ReportItemClicked(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                if (button.Tag is string reportName)
                {
                    switch (reportName)
                    {
                        case "End Day Report":
                            MessageBox.Show(reportName);
                            break;
                        case "Session Report":
                            MessageBox.Show(reportName);
                            break;
                        case "Receipt Report":
                            MessageBox.Show(reportName);
                            break;
                        case "Sales By Pord Report":
                            MessageBox.Show(reportName);
                            break;
                        case "Product Hourly Report":
                            MessageBox.Show(reportName);
                            break;
                        case "Product Price Report":
                            MessageBox.Show(reportName);
                            break;
                        case "Sales Type Report":
                            MessageBox.Show(reportName);
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private void AddNavigationButtons(int totalItems)
        {
            if (totalItems <= PageSize)
                return;

            if (currentPage > 0)
            {
                Button prevButton = new Button { Content = "Prev" };
                prevButton.Click += PrevButton_Click;
                myGrid.Children.Add(prevButton);
                Grid.SetRow(prevButton, 4);
                Grid.SetColumn(prevButton, 0);
            }

            if ((currentPage + 1) * PageSize < totalItems)
            {
                Button nextButton = new Button { Content = "Next" };
                nextButton.Click += NextButton_Click;
                myGrid.Children.Add(nextButton);
                Grid.SetRow(nextButton, 4);
                Grid.SetColumn(nextButton, 3);
            }
        }

        private void PrevButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage > 0)
            {
                currentPage--;
                PopulateGrid();
            }
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if ((currentPage + 1) * PageSize < ReportItemDictionary.Count)
            {
                currentPage++;
                PopulateGrid();
            }
        }
    }
}
