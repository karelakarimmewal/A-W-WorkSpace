using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using TranQuik.Model;

namespace TranQuik.Pages.ChildPage
{
    public partial class ProductPriceReports : System.Windows.Controls.Page
    {
        private LocalDbConnector localDbConnector = new LocalDbConnector();
        
        public ProductPriceReports()
        {
            InitializeComponent();
            InitializeBox();
        }

        private void InitializeBox()
        {
            LocalDbConnector localDbConnector = new LocalDbConnector();

            ProductGroupPopulate.PopulateProductGroup(localDbConnector);
            ProductDeptPopulate.PopulateProductDept(localDbConnector);
            SaleMode.PopulateSaleMode(localDbConnector);

            PopulateProductGroup();
            PopulateProdutDept();
            PopulateSaleMode();

            var shop18 = ShopData.shopDatas.Find(shop => shop.ShopId == 18);
            if (shop18 != null)
            {
                sessionReportsShopName.Text = shop18.ShopName;
            }
        }

        private void PopulateSaleMode()
        {
            if (SaleMode.populateSaleModeList.Count > 0)
            {
                selectSaleMode.Items.Add(new ComboBoxItem
                {
                    Content = "--All Sale Mode--", // Displayed text
                    Tag = 0// Value
                });
                foreach (var items in SaleMode.populateSaleModeList)
                {
                    selectSaleMode.Items.Add(new ComboBoxItem
                    {
                        Content = items.SaleModeName, // Displayed text
                        Tag = items.SaleModeID // Value
                    });
                }
                selectSaleMode.SelectedIndex = 1;
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
            productpricereportListView.Items.Clear ();  
            string DatePicker = "";
            int SaleMode = 0;
            int ProductGroup = 0;
            int ProductDept = 0;

            // Check if Start Date is available
            bool isStartDateAvailable = !string.IsNullOrEmpty(startDatePicker.Text);
            if (isStartDateAvailable)
            {
                string dateString = startDatePicker.Text.Split(' ')[0];
                if (DateTime.TryParseExact(dateString, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime startDate))
                {
                    DatePicker = startDate.ToString("yyyy-MM-dd");
                }
            }

            // Get selected Sale Mode
            if (selectSaleMode.SelectedItem != null)
            {
                ComboBoxItem selectedSaleModeItem = (ComboBoxItem)selectSaleMode.SelectedItem;
                SaleMode = Convert.ToInt32(selectedSaleModeItem.Tag);
            }

            // Get selected Product Dept
            if (selectProductDept.SelectedItem != null)
            {
                ComboBoxItem selectedProductDeptItem = (ComboBoxItem)selectProductDept.SelectedItem;
                ProductDept = Convert.ToInt32(selectedProductDeptItem.Tag);
            }

            // Get selected Product Group
            if (selectProductGroup.SelectedItem != null)
            {
                ComboBoxItem selectedProductGroupItem = (ComboBoxItem)selectProductGroup.SelectedItem;
                ProductGroup = Convert.ToInt32(selectedProductGroupItem.Tag);
            }
           

            // Populate reports
            ReportProductPrice.PopulateReports(localDbConnector, ProductGroup, ProductDept, SaleMode, DatePicker);
            int indexing = 0;
            foreach (var item in ReportProductPrice.reportProductPrices)
            {
                indexing ++ ; // Start indexing from 1

                string ProdNames = "";
                if (SaleMode == 0)
                {
                    ProdNames = $"{item.PrefixText} {item.ProductName}";
                }
                else
                {
                    ProdNames = item.ProductName;
                }

                productpricereportListView.Items.Add(new
                {
                    Index = indexing,
                    ProductName = ProdNames,
                    ProductPrice = item.ProductPrice.ToString("N0")
                });
            }

        }

        private void printReports_Click(object sender, RoutedEventArgs e)
        {
            ThermalPrinter thermalPrinter = new ThermalPrinter();
            thermalPrinter.TemplateReport("Product Price Report");
        }
    }
}
