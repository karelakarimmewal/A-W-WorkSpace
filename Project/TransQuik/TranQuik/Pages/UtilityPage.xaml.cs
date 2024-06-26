using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using TranQuik.Model;

namespace TranQuik.Pages
{
    public partial class UtilityPage : Window
    {
        private int currentPage = 0;
        private const int PageSize = 5; // 5 items per page

        private Dictionary<string, PackIconKind> UtilityItemDictionary;
        private List<Button> ButtonList = new List<Button>();
        public UtilityPage()
        {
            InitializeComponent();
            UtilityItemDictionary = UtilityManager.GetUtilityManagerItem();
            PopulateButton();
            AddButtonsToGrid();

        }

        private void closeWindow_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


        private void PopulateButton()
        {
            foreach (var items in UtilityItemDictionary)
            {
                Button button = CreateButton(items.Key, items.Value);
                ButtonList.Add(button);
            }
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
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = Brushes.Black
            };

            TextBlock textBlock = new TextBlock
            {
                Text = name,
                FontFamily = new FontFamily("Arial"),
                FontSize = 12,
                FontStyle = FontStyles.Normal,
                Foreground = Brushes.Black,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(2),
                FontWeight = FontWeights.Regular,
            };

            Button button = new Button
            {
                Content = new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Children =
                    {
                        icon,
                        textBlock
                    }
                },
                Tag = name,
                Background = Brushes.Azure,
                Effect = (Effect)Application.Current.FindResource("DropShadowEffect"),
            };

            // Attach click event handler
            button.Click += UtilityItemClicked;

            return button;
        }

        private void UtilityItemClicked(object sender, RoutedEventArgs e)
        {
        }

        private void AddButtonsToGrid()
        {
            navigationReportPage.Children.Clear();

            int startIndex = currentPage * PageSize;
            for (int i = 0; i < PageSize; i++)
            {
                int index = startIndex + i;
                if (index < ButtonList.Count)
                {
                    Grid.SetColumn(ButtonList[index], i);
                    navigationReportPage.Children.Add(ButtonList[index]);
                }
                else
                {
                    Button blankButton = new Button
                    {
                        Content = string.Empty,
                        Background = Brushes.Green,
                        IsEnabled = false,
                        Margin = new Thickness(0, 0, 0, 2)
                    };
                    Grid.SetColumn(blankButton, i);
                    navigationReportPage.Children.Add(blankButton);
                }
            }
            UpdateNavigationButtons();
        }

        private void UpdateNavigationButtons()
        {
            PrevButton.IsEnabled = currentPage > 0;
            nextButton.IsEnabled = (currentPage + 1) * PageSize < ButtonList.Count;
        }

        private void PrevButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage > 0)
            {
                currentPage--;
                AddButtonsToGrid();
                UpdateNavigationButtons();
            }
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if ((currentPage + 1) * PageSize < ButtonList.Count)
            {
                currentPage++;
                AddButtonsToGrid();
                UpdateNavigationButtons();
            }
        }
    }
}
