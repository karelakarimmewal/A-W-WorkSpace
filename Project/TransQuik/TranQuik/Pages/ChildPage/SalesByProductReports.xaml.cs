using System.Windows;
using System.Windows.Controls;
using TranQuik.Model;

namespace TranQuik.Pages.ChildPage
{
    /// <summary>
    /// Interaction logic for SalesByProductReports.xaml
    /// </summary>
    public partial class SalesByProductReports : Page
    {
        public SalesByProductReports()
        {
            InitializeComponent();
            InitializePopulate();
        }

        private void InitializePopulate()
        {

            LocalDbConnector localDbConnector = new LocalDbConnector();

            ProductGroupPopulate.PopulateProductGroup(localDbConnector);
            ProductDeptPopulate.PopulateProductDept(localDbConnector);
            SaleMode.PopulateSaleMode(localDbConnector);


            PopulateProductGroup();
            PopulateProdutDept();

            var shop18 = ShopData.shopDatas.Find(shop => shop.ShopId == 18);
            if (shop18 != null)
            {
                sessionReportsShopName.Text = shop18.ShopName;
            }
        }


        private void PopulateProductGroup()
        {
            if (ProductGroupPopulate.productGroupPopulates.Count > 0)
            {
                selectProductGroup.Items.Add(new ComboBoxItem
                {
                    Content = "--All Product Group--", // Displayed text
                    Tag = 0// Value
                });
                foreach (var items in ProductGroupPopulate.productGroupPopulates)
                {
                    if (items.ProductGroupActivate)
                    {
                        selectProductGroup.Items.Add(new ComboBoxItem
                        {
                            Content = items.ProductGroupName, // Displayed text
                            Tag = items.ProductGroupID // Value
                        });
                    }
                }
                selectProductGroup.SelectedIndex = 0;
            }
        }

        private void PopulateProdutDept()
        {
            if (ProductDeptPopulate.productDeptPopulates.Count > 0)
            {
                // Clear previous items to avoid duplication
                selectProductDept.Items.Clear();

                // Retrieve the selected ComboBoxItem
                ComboBoxItem selectedProductGroupItem = selectProductGroup.SelectedItem as ComboBoxItem;
                if (selectedProductGroupItem != null)
                {
                    selectProductDept.Items.Add(new ComboBoxItem
                    {
                        Content = "--All Product Group--", // Displayed text
                        Tag = 0// Value
                    });
                    int selectedProductGroupID = (int)selectedProductGroupItem.Tag;

                    foreach (var items in ProductDeptPopulate.productDeptPopulates)
                    {
                        if (items.ProductDeptActivate)
                        {
                            if (selectedProductGroupID == 0 || items.ProductGroupID == selectedProductGroupID)
                            {
                                selectProductDept.Items.Add(new ComboBoxItem
                                {
                                    Content = items.ProductDeptName, // Displayed text
                                    Tag = items.ProductDeptID // Value
                                });
                            }
                        }
                    }
                    selectProductDept.SelectedIndex = 0;
                }
            }
        }

        private void selectProductGroup_Change(object sender, SelectionChangedEventArgs e)
        {
            PopulateProdutDept();
        }

        private void showReports_Click(object sender, RoutedEventArgs e)
        {

        }

        private void printReports_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
