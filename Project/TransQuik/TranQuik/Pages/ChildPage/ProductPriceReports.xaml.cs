using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using TranQuik.Model;

namespace TranQuik.Pages.ChildPage
{

    public class ReportProductPrice
    {
        public static List<ReportProductPrice> reportProductPrices = new List<ReportProductPrice>();
        public string ProductName { get; set; }
        public decimal ProductPrice { get; set; }
        public int SaleMode { get; set; } // Assuming SaleMode is an int

        public ReportProductPrice(string productName, decimal productPrice, int saleMode)
        {
            ProductName = productName;
            ProductPrice = productPrice;
            SaleMode = saleMode;
        }

        public static void PopulateReports(LocalDbConnector localDbConnector, int ProductGroup, int ProductDept, int SaleMode, string DatePicker)
        {
            string baseQuery = "SELECT P.`ProductName`, PP.`ProductPrice`, PP.`SaleMode` FROM products P " +
                               "JOIN productprice PP ON P.`ProductID` = PP.`ProductID`";

            List<string> conditions = new List<string>();

            if (ProductGroup != 0)
            {
                conditions.Add("P.ProductGroupID = @ProductGroupID");
            }
            if (ProductDept != 0)
            {
                conditions.Add("P.ProductDeptID = @ProductDeptID");
            }
            if (SaleMode != 0)
            {
                conditions.Add("PP.SaleMode = @SaleMode");
            }
            if (DatePicker != "")
            {
                conditions.Add("PP.Date >= @DatePicker");
            }

            if (conditions.Count > 0)
            {
                baseQuery += " WHERE " + string.Join(" AND ", conditions);
            }

            try
            {
                baseQuery += " ORDER BY P.ProductName ASC"; // Ensure space before ORDER
                reportProductPrices.Clear();
                using (MySqlConnection connection = localDbConnector.GetMySqlConnection())
                {
                    connection.Open();
                    using (MySqlCommand command = new MySqlCommand(baseQuery, connection))
                    {
                        // Add parameters
                        if (ProductGroup != 0)
                            command.Parameters.AddWithValue("@ProductGroupID", ProductGroup);
                        if (ProductDept != 0)
                            command.Parameters.AddWithValue("@ProductDeptID", ProductDept);
                        if (SaleMode != 0)
                            command.Parameters.AddWithValue("@SaleMode", SaleMode);
                        if (DatePicker != "")
                            command.Parameters.AddWithValue("@DatePicker", DatePicker);

                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string productName = reader.GetString("ProductName");
                                decimal productPrice = reader.GetDecimal("ProductPrice");
                                int saleMode = reader.GetInt32("SaleMode");

                                ReportProductPrice report = new ReportProductPrice(productName, productPrice, saleMode);
                                reportProductPrices.Add(report);
                            }
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                // Handle exception
                Console.WriteLine("Error: " + ex.Message);
            }
        }

    }

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
                productpricereportListView.Items.Add(new
                {
                    Index = indexing,
                    ProductName = item.ProductName,
                    ProductPrice = item.ProductPrice.ToString("N0")
                });
            }

        }

        private void printReports_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
