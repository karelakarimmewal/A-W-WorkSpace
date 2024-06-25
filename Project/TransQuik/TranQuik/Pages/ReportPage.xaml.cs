using MaterialDesignThemes.Wpf;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using TranQuik.Model;
using TranQuik.Pages.ChildPage;
using Application = System.Windows.Application;

namespace TranQuik.Pages
{
    public partial class ReportPage : Window
    {
        private int currentPage = 0;
        private const int PageSize = 5; // 5 items per page

        private Dictionary<string, PackIconKind> ReportItemDictionary;
        private List<Button> ButtonList = new List<Button>();

        private EndDayReports EndDayReports = new EndDayReports();
        private SessionReports SessionReports = new SessionReports();
        private ReceiptReports ReceiptReports = new ReceiptReports();
        private SalesByProductReports SalesByProductReports = new SalesByProductReports();
        private PorductHourlyReports PorductHourlyReports = new PorductHourlyReports();
        private ProductPriceReports ProductPriceReports = new ProductPriceReports();
        private SalesTypeReports SalesTypeReports = new SalesTypeReports();


        public ReportPage()
        {
            InitializeComponent();
            framingItems.NavigationUIVisibility = System.Windows.Navigation.NavigationUIVisibility.Hidden;
            ReportItemDictionary = ReportManager.GetReportManagerItem();
            LocalDbConnector localDbConnector = new LocalDbConnector();
            PopulateButton();
            AddButtonsToGrid();
            ShopData.PopulateShopData(localDbConnector);
            var shop18 = ShopData.shopDatas.Find(shop => shop.ShopId == 18);
            if (shop18 != null)
            {
                EndDayReports.sessionReportsShopName.Text = shop18.ShopName;
                SessionReports.sessionReportsShopName.Text = shop18.ShopName;
                SalesByProductReports.sessionReportsShopName.Text = shop18.ShopName;
                PorductHourlyReports.sessionReportsShopName.Text = shop18.ShopName;
                ProductPriceReports.sessionReportsShopName.Text = shop18.ShopName;
                SalesTypeReports.sessionReportsShopName.Text = shop18.ShopName;
            }
        }

        private void closeWindow_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void PopulateButton()
        {
            Dictionary<string, PackIconKind> ReportItemDictionary = ReportManager.GetReportManagerItem();
            foreach (var items in ReportItemDictionary)
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
            button.Click += ReportItemClicked;

            return button;
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

        private void ReportItemClicked(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                if (button.Tag is string reportName)
                {
                    switch (reportName)
                    {
                        case "End Day Report":
                            DisplayEndDayReportsPage();
                            break;
                            
                        case "Session Report":
                            DisplaySessionReportsPage();
                            break;

                        case "Receipt Report":
                            DisplayReceiptReportsPage(); 
                            break;

                        case "Sales By Prod Report":
                            DisplaySalesByProductReportsPage();
                            break;

                        case "Product Hourly Report":
                            DisplayProductHourlyReportsPage(); 
                            break;

                        case "Product Price Report":
                            DiplayProductPriceReportsPage(); 
                            break;

                        case "Sales Type Report":
                            DisplaySalesTypeReportsPage(); 
                            break;

                        default:
                            break;
                    }
                }
            }
        }

        private void DisplayEndDayReportsPage()
        {
            framingItems.Content = null;
            framingItems.Content = EndDayReports;
        }

        private void DisplaySessionReportsPage()
        {
            framingItems.Content = null;
            framingItems.Content = SessionReports;
        }
        
        private void DisplayReceiptReportsPage()
        {
            framingItems.Content = null;
            framingItems.Content = ReceiptReports;
        }
        private void DisplayProductHourlyReportsPage()
        {
            framingItems.Content = null;
            framingItems.Content = PorductHourlyReports;
        }
        private void DisplaySalesByProductReportsPage()
        {
            framingItems.Content = null;
            framingItems.Content = SalesByProductReports;
        }
        private void DiplayProductPriceReportsPage()
        {
            framingItems.Content = null;
            framingItems.Content = ProductPriceReports;
        }
        private void DisplaySalesTypeReportsPage()
        {
            framingItems.Content = null;
            framingItems.Content = SalesTypeReports;
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
