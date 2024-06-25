using System.Windows;
using System.Windows.Controls;
using TranQuik.Model;

namespace TranQuik.Pages.ChildPage
{
    public partial class EndDayReports : Page
    {
        public EndDayReports()
        {
            InitializeComponent();
            InitializePopulate();
            
        }

        private void InitializePopulate()
        {
            LocalDbConnector localDbConnector = new LocalDbConnector();
            ComputerAccessData.PopulateComputerAccessData(localDbConnector);

            PopulatePOS();

            var shop18 = ShopData.shopDatas.Find(shop => shop.ShopId == 18);
            if (shop18 != null)
            {
                sessionReportsShopName.Text = shop18.ShopName;
            }
        }

        private void PopulatePOS()
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
            selectPOS.SelectedIndex = 1;
        }

        private void showSessionReports_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
