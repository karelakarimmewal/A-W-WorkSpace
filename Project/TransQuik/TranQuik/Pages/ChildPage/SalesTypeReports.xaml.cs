using System.Windows;
using System.Windows.Controls;
using TranQuik.Model;

namespace TranQuik.Pages.ChildPage
{
    /// <summary>
    /// Interaction logic for SalesTypeReports.xaml
    /// </summary>
    public partial class SalesTypeReports : Page
    {
        public SalesTypeReports()
        {
            InitializeComponent();
            InitializePopulate();
        }

        private void InitializePopulate()
        {
            LocalDbConnector localDbConnector = new LocalDbConnector();
            ComputerAccessData.PopulateComputerAccessData(localDbConnector);
            SaleMode.PopulateSaleMode(localDbConnector);
            PopulateComputerAccess();
            PopulateSaleMode();

            var shop18 = ShopData.shopDatas.Find(shop => shop.ShopId == 18);
            if (shop18 != null)
            {
                sessionReportsShopName.Text = shop18.ShopName;
            }

        }


        private void PopulateComputerAccess()
        {
            selectPOS.Items.Add(new ComboBoxItem
            {
                Content = "--All POS--",
                Tag = 0,
            });
            foreach (var items in ComputerAccessData.ComputerAccessDatas)
            {
                if (items.ComputerType == 0)
                {
                    selectPOS.Items.Add(new ComboBoxItem
                    {
                        Content = items.ComputerName,
                        Tag = items.ComputerID,
                    });
                }
            }
            selectPOS.SelectedIndex = 0;
        }

        private void PopulateSaleMode()
        {
            selectSaleMode.Items.Add(new ComboBoxItem
            {
                Content = "--All SaleMode--",
                Tag = 0,
            });
            foreach (var items in SaleMode.populateSaleModeList)
            {
                selectSaleMode.Items.Add(new ComboBoxItem
                {
                    Content = items.SaleModeName,
                    Tag = items.SaleModeID,
                });
            }
            selectSaleMode.SelectedIndex = 1;
        }



        private void showReports_Click(object sender, RoutedEventArgs e)
        {

        }

        private void printReports_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
