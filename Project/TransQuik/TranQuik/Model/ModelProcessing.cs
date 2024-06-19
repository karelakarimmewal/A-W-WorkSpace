﻿using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TranQuik.Controller;
using TranQuik.Pages;

namespace TranQuik.Model
{
    public class ModelProcessing
    {
        // Fields related to database and UI components
        private LocalDbConnector localDbConnector;
        private MainWindow mainWindow;
        public SecondaryMonitor secondaryMonitor;
        public ProductComponent productComponent;

        // Fields related to cart products
        public Dictionary<int, Product> cartProducts = new Dictionary<int, Product>();
        private List<Button> productButtons = new List<Button>();
        public List<ProductComponentGroup> componentGroups = new List<ProductComponentGroup>();

        // Fields related to product VAT and payment
        public decimal productVATPercent;
        public string productVatText;
        public string productVatCode;
        public string isPaymentChange;

        // Fields for tracking application state
        private bool isReset;
        private int ProdTotalCount;
        private int CartIndex = 0;
        public int idProduct = 0;

        public ModelProcessing(MainWindow mainWindow)
        {
            this.localDbConnector = new LocalDbConnector(); // Instantiate LocalDbConnector
            this.mainWindow = mainWindow; // Assign the MainWindow instance
            GetProductVATInfo();

        }
        
        public void UpdateSecondaryMonitor(SecondaryMonitor secondaryMonitors)
        {
            secondaryMonitor = secondaryMonitors;
        }

        public void GetMenuGroup(out List<string> menuGroupNames, out List<int> menuGroupIds)
        {
            menuGroupNames = new List<string>();
            menuGroupIds = new List<int>();

            try
            {
                using (MySqlConnection connection = localDbConnector.GetMySqlConnection())
                {
                    string query = @"
                        SELECT Pg.`ProductGroupID`, PG.`ProductGroupCode`, PG.`ProductGroupName` FROM ProductGroup PG 
                        WHERE PG.ProductGroupActivate = 1 AND PG.`Deleted` != 1 AND PG.`IsComment` = 0";

                    MySqlCommand command = new MySqlCommand(query, connection);
                    connection.Open();
                    MySqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        string groupName = reader["ProductGroupName"].ToString();
                        int groupId = Convert.ToInt32(reader["ProductGroupID"]);

                        menuGroupNames.Add(groupName);
                        menuGroupIds.Add(groupId);
                    }
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving product group data from the database.", ex);
            }
        }

        public void MenuGroupButtonCreate(List<string> menuGroupNames, List<int> menuGroupIds)
        {
            mainWindow.MenuGroupButton.Children.Clear();
            mainWindow.menuGroupButtons.Clear();

            // Define the total number of buttons required
            int totalButtonCount = mainWindow.MenuGroupButtonButtonCount;

            // Create buttons for each menu group
            for (int i = 0; i < menuGroupNames.Count; i++)
            {
                TextBlock textBlock = new TextBlock
                {
                    Text = menuGroupNames[i],
                    TextWrapping = TextWrapping.Wrap,
                    TextAlignment = TextAlignment.Center, // Center-align the text
                    VerticalAlignment = VerticalAlignment.Center, // Center-align vertically
                    HorizontalAlignment = HorizontalAlignment.Center, // Center-align horizontally
                    Foreground = (Brush)Application.Current.FindResource("FontColor"),
                };

                Button button = new Button
                {
                    Content = textBlock,
                    Height = 50,
                    Width = 98,
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(1),
                    BorderThickness = new Thickness(2),
                    Tag = menuGroupIds[i],
                    Background = Brushes.Azure,
                    Effect = (System.Windows.Media.Effects.Effect)Application.Current.FindResource("DropShadowEffect"),
                    Style = (System.Windows.Style)Application.Current.FindResource("ButtonStyle"),
                };

                button.Click += MenuGroupButtonClicked;

                // Calculate the row and column index for the grid
                int row = i / 4;
                int column = i % 4;
                Grid.SetRow(button, row);
                Grid.SetColumn(button, column);

                mainWindow.MenuGroupButton.Children.Add(button);
                mainWindow.menuGroupButtons.Add(button);
            }

            // Add blank buttons to fill the remaining slots if needed
            for (int i = menuGroupNames.Count; i < totalButtonCount; i++)
            {
                Button blankButton = new Button
                {
                    IsEnabled = false,
                    Effect = (System.Windows.Media.Effects.Effect)Application.Current.FindResource("DropShadowEffect"),
                    Style = (System.Windows.Style)Application.Current.FindResource("ButtonStyle"),
                    Margin = new Thickness(1),
                    BorderThickness = new Thickness(2),
                    Background = (Brush)Application.Current.FindResource("ButtonDisabledColor"), // Optional: color for the blank buttons
                };

                // Calculate the row and column index for the grid
                int row = i / 4;
                int column = i % 4;
                Grid.SetRow(blankButton, row);
                Grid.SetColumn(blankButton, column);

                mainWindow.MenuGroupButton.Children.Add(blankButton);
            }
        }

        public void MenuGroupButtonClicked(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                int productGroupId = Convert.ToInt32(button.Tag);
                MenuGroupButtonCreateSubContent(productGroupId);
                mainWindow.PayementProcess.Visibility = Visibility.Collapsed;
                mainWindow.MenuBorder.Visibility = Visibility.Visible;
                mainWindow.MenuSubCategoryContentButton.Visibility = Visibility.Visible;
                mainWindow.MenuSubCategoryContentProduct.Visibility = Visibility.Visible;
                mainWindow.MenuSubCategoryContent.Visibility = Visibility.Visible;
            }
        }

        public void MenuGroupButtonCreateSubContent(int productGroupId)
        {
            // Clear existing product buttons
            mainWindow.MenuSubCategoryContentButton.Children.Clear();

            string query = (productGroupId == -1)
                ? @"SELECT ProductDeptID, ProductDeptCode, ProductDeptName FROM ProductDept WHERE ProductDeptActivate = 1"
                : @"
                SELECT ProductDeptID, ProductDeptCode, ProductDeptName 
                FROM ProductDept 
                WHERE ProductDeptActivate = 1 AND ProductGroupID = @ProductDeptGroup";

            using (MySqlConnection connection = localDbConnector.GetMySqlConnection())
            {
                MySqlCommand command = new MySqlCommand(query, connection);

                if (productGroupId != -1)
                    command.Parameters.AddWithValue("@ProductDeptGroup", productGroupId);

                connection.Open();
                MySqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    string productNameSubContent = reader["ProductDeptName"].ToString();
                    int productIdSubContent = Convert.ToInt32(reader["ProductDeptID"]);

                    // Create product instance (if needed)
                    // Product product = new Product(productIdSubContent, productNameSubContent, 0, null);

                    // Create the product button
                    Button productButton = CreateSubContentButton(productIdSubContent, productNameSubContent);
                    productButton.Click += SubContentButtonClicked;

                    // Calculate the row and column index for the grid
                    int row = mainWindow.MenuSubCategoryContentButton.Children.Count / 5; // Assuming 5 columns per row
                    int column = mainWindow.MenuSubCategoryContentButton.Children.Count % 5;
                    Grid.SetRow(productButton, row);
                    Grid.SetColumn(productButton, column);

                    // Add the product button to the grid
                    mainWindow.MenuSubCategoryContentButton.Children.Add(productButton);
                }

                reader.Close();
                updateVisibleContentButton();
            }
        }

        private void SubContentButtonClicked(object sender, RoutedEventArgs e)
        {
            // Handle button click event
            Button clickedButton = (Button)sender;
            int productDeptId = (int)clickedButton.Tag;
            mainWindow.subContentProductStartIndex = 0;
            LoadSubContentProduct(productDeptId);
        }

        public void LoadSubContentProduct(int productDeptId)
        {
            // Clear existing product buttons
            mainWindow.MenuSubCategoryContentProduct.Children.Clear();

            string query = (productDeptId == -1)
                ? @"
                SELECT P.`ProductDeptID`, PD.`ProductDeptName`, P.`ProductID`, P.`ProductName`, P.`ProductName2`, PP.`ProductPrice`, PP.`SaleMode`
                FROM Products P
                JOIN ProductPrice PP ON P.`ProductID` = PP.`ProductID`
                JOIN ProductDept PD ON PD.`ProductDeptID` = P.`ProductDeptID`
                WHERE P.`ProductActivate` = 1 AND PP.`SaleMode` = @SaleModeID
                ORDER BY P.`ProductName`;"
                            : @"
                SELECT P.`ProductDeptID`, PD.`ProductDeptName`, P.`ProductID`, P.`ProductName`, P.`ProductName2`, PP.`ProductPrice`, PP.`SaleMode`
                FROM Products P
                JOIN ProductPrice PP ON P.`ProductID` = PP.`ProductID`
                JOIN ProductDept PD ON PD.`ProductDeptID` = P.`ProductDeptID`
                WHERE P.`ProductActivate` = 1 AND PP.`SaleMode` = @SaleModeID AND PD.ProductDeptID = @ProductDeptID
                ORDER BY P.`ProductName`;";

            int productButtonCount = mainWindow.subContentProductButtonCount; // Total number of buttons to display
            int i = 0; // Counter for the number of product buttons created

            using (MySqlConnection connection = localDbConnector.GetMySqlConnection())
            {
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@SaleModeID", mainWindow.SaleMode);

                if (productDeptId != -1)
                    command.Parameters.AddWithValue("@ProductDeptID", productDeptId);

                connection.Open();
                MySqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    string productName = reader["ProductName"].ToString();
                    int productId = Convert.ToInt32(reader["ProductID"]);
                    decimal productPrice = Convert.ToDecimal(reader["ProductPrice"]);

                    // Create product instance
                    Product product = new Product(productId, productName, productPrice, null);

                    // Determine the image path
                    string imgFolderPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    string imagePath = Path.Combine(imgFolderPath, "Resource/Product", $"{productName}.jpg");

                    // Create the product button
                    Button productButton = CreateSubContentProduct(product, imagePath);
                    productButton.Click += ProductButton_Click;

                    // Calculate the row and column index for the grid
                    int row = i / 6; // Assuming 5 columns per row
                    int column = i % 6;
                    Grid.SetRow(productButton, row);
                    Grid.SetColumn(productButton, column);

                    // Add the product button to the grid
                    mainWindow.MenuSubCategoryContentProduct.Children.Add(productButton);
                    i++;
                }

                reader.Close();

                // Update the visibility of product buttons
                UpdateProductSubcontent();
            }
        }

        private Button CreateSubContentProduct(Product product, string imagePath)
        {
            bool allowImage = bool.Parse(Properties.Settings.Default["_AppAllowImage"].ToString());

            // Create product button
            Button productButton = new Button
            {
                FontWeight = FontWeights.Bold,
                Background = (Brush)Application.Current.FindResource("PrimaryBackgroundColor"),
                BorderThickness = new Thickness(0.8),
                Tag = product // Assign product instance to Tag property
            };

            // Create a StackPanel to hold the image and text
            StackPanel stackPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                
            };

            // Add text to the stack panel
            TextBlock textBlock = new TextBlock
            {
                Text = product.ProductName,
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 5)
            };
            stackPanel.Children.Add(textBlock);

            // Check if image creation is allowed
            if (allowImage && File.Exists(imagePath))
            {
                // Load the image
                BitmapImage image = new BitmapImage(new Uri(imagePath, UriKind.RelativeOrAbsolute));
                Image img = new Image
                {
                    Source = image,
                };
                stackPanel.Children.Insert(0, img); // Insert image at the beginning
            }


            // Set the content of the button to the stack panel
            productButton.Content = stackPanel;

            return productButton;
        }

        private Button CreateSubContentButton(int productIdSubContent, string productNameSubContent)
        {
            // Create product button
            Button productButton = new Button
            {
                FontWeight = FontWeights.Bold,
                BorderThickness = new Thickness(0.8),
                Margin = new Thickness(0, 2, 0, 3),
                Background = (Brush)Application.Current.FindResource("SecondaryButtonColor"),
                Tag = productIdSubContent // Assign product instance to Tag property
            };

            // Create a StackPanel to hold the image and text
            StackPanel stackPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            // Add text to the stack panel
            TextBlock textBlock = new TextBlock
            {
                Text = productNameSubContent,
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0)
            };
            stackPanel.Children.Add(textBlock);


            // Set the content of the button to the stack panel
            productButton.Content = stackPanel;

            return productButton;
        }

        public void GetProductGroupNamesAndIds(out List<string> productGroupNames, out List<int> productGroupIds)
        {
            productGroupNames = new List<string>();
            productGroupIds = new List<int>();

            try
            {
                using (MySqlConnection connection = localDbConnector.GetMySqlConnection())
                {
                    string query = @"
                        SELECT F.`PageIndex`, F.`PageName`, F.`PageOrder`, F.`ButtonColorCode`, F.`ButtonColorHexCode` 
                        FROM favoritepageindex F";

                    MySqlCommand command = new MySqlCommand(query, connection);
                    connection.Open();
                    MySqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        string groupName = reader["PageName"].ToString();
                        int groupId = Convert.ToInt32(reader["PageIndex"]);

                        productGroupNames.Add(groupName);
                        productGroupIds.Add(groupId);
                    }
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving product group data from the database.", ex);
            }
        }

        public void FavGroupButtonCreate(List<string> productGroupNames, List<int> productGroupIds)
        {
            for (int i = 0; i < productGroupNames.Count; i++)
            {
                TextBlock textBlock = new TextBlock
                {
                    Text = productGroupNames[i],
                    TextWrapping = TextWrapping.Wrap,
                    TextAlignment = TextAlignment.Center, // Optional: Center-align the text
                    VerticalAlignment = VerticalAlignment.Center, // Center-align vertically
                    HorizontalAlignment = HorizontalAlignment.Center, // Center-align horizontally
                    Foreground = (Brush)Application.Current.FindResource("FontColor"),
                };

                Button button = new Button
                {
                    Content = textBlock,
                    Height = 50,
                    Width = 98,
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(1),
                    BorderThickness = new Thickness(2),
                    Tag = productGroupIds[i],
                    Background = Brushes.Azure,
                    Effect = (System.Windows.Media.Effects.Effect)Application.Current.FindResource("DropShadowEffect"),
                    Style = (System.Windows.Style)Application.Current.FindResource("ButtonStyle"),
                };

                button.Click += mainWindow.GroupClicked;

                // Calculate the row and column index for the grid
                int row = i / 4; // Assuming 5 columns per row
                int column = i % 4;
                Grid.SetRow(button, row);
                Grid.SetColumn(button, column);

                mainWindow.ProductGroupName.Children.Add(button);
                mainWindow.productGroupButtons.Add(button);

            }
        }

        public void LoadProductDetails(int pageIndex)
        {
            // Clear existing product buttons
            mainWindow.MainContentProduct.Children.Clear();

            string query = (pageIndex == -1)
                ? @"
                SELECT 
                    FPI.`PageIndex`, FPI.`PageName`, P.`ProductID`,P.`ProductName`, P.`ProductName1`, P.`ProductName2`, PP.`ProductPrice`, FPI.`PageOrder`, 
                    FPI.`ButtonColorCode`, FP.`ButtonOrder`, FP.`ButtonColorCode`, FP.`ButtonColorHexCode`
                FROM favoritepageindex FPI 
                JOIN favoriteproducts FP ON FPI.`PageIndex` = FP.`PageIndex`
                JOIN products P ON FP.`ProductCode` = P.`ProductCode`
                JOIN ProductPrice PP ON P.`ProductID` = PP.`ProductID`
                WHERE PP.`SaleMode` = @SaleModeID AND P.`ProductActivate` = 1
                ORDER BY FP.`ButtonOrder` ASC;"
                        : @"
                SELECT 
                    FPI.`PageIndex`, FPI.`PageName`, P.`ProductID` ,P.`ProductName` , P.`ProductName1`, P.`ProductName2`, PP.`ProductPrice`, FPI.`PageOrder`, 
                    FPI.`ButtonColorCode`, FP.`ButtonOrder`, FP.`ButtonColorCode`, FP.`ButtonColorHexCode`
                FROM favoritepageindex FPI 
                JOIN favoriteproducts FP ON FPI.`PageIndex` = FP.`PageIndex`
                JOIN products P ON FP.`ProductCode` = P.`ProductCode`
                JOIN ProductPrice PP ON P.`ProductID` = PP.`ProductID`
                WHERE PP.`SaleMode` = @SaleModeID AND FP.`PageIndex` = @PageIndex AND P.`ProductActivate` = 1  ORDER BY FP.`ButtonOrder` ASC";

            using (MySqlConnection connection = localDbConnector.GetMySqlConnection())
            {
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@SaleModeID", mainWindow.SaleMode);

                if (pageIndex != -1)
                    command.Parameters.AddWithValue("@PageIndex", pageIndex);

                connection.Open();
                MySqlDataReader reader = command.ExecuteReader();

                int productButtonCount = mainWindow.productButtonCount; // Total number of buttons to display
                int i = 0; // Counter for the number of product buttons created
                ProdTotalCount = 0;

                while (reader.Read())
                {
                    
                    bool isEnableButton = true;
                    int productId = Convert.ToInt32(reader["ProductID"]);
                    string productNames = reader["ProductName"].ToString();
                    string productName1 = reader["ProductName1"].ToString();
                    string productName2 = reader["ProductName2"].ToString();

                    // Skip processing if both ProductName1 and ProductName2 are null or empty
                    if (string.IsNullOrEmpty(productNames) && string.IsNullOrEmpty(productName1) && string.IsNullOrEmpty(productName2))
                    {
                        isEnableButton = false;
                    }
                    string productName = !string.IsNullOrEmpty(productNames) ? productNames :
                        !string.IsNullOrEmpty(productName1) ? productName1 : productName2;

                    string productButtonColor = reader["ButtonColorHexCode"].ToString();

                    decimal productPrice = Convert.ToDecimal(reader["ProductPrice"]);

                    // Create product instance
                    Product product = new Product(productId, productName, productPrice, productButtonColor);

                    // Determine the image path
                    string imgFolderPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    string imagePath = Path.Combine(imgFolderPath, "Resource/Product", $"{productName}.jpg");

                    // Create the product button
                    Button productButton = CreateProductButton(product, imagePath, productButtonColor, isEnableButton);
                    productButton.Click += ProductButton_Click;

                    // Calculate the row and column index for the grid
                    int row = i / 6; // Assuming 6 columns per row
                    int column = i % 6;
                    Grid.SetRow(productButton, row);
                    Grid.SetColumn(productButton, column);

                    // Add the product button to the grid
                    mainWindow.MainContentProduct.Children.Add(productButton);
                    i++;
                    ProdTotalCount++;
                }

                UpdateVisibleProductButtons();
                reader.Close();
                
            }
        }

        private Button CreateProductButton(Product product, string imagePath, string productButtonColor, bool isEnable)
        {
            // Check if image creation is allowed based on application setting
            bool allowImage = bool.Parse(Properties.Settings.Default["_AppAllowImage"].ToString());

            // Create product button
            Button productButton = new Button
            {
                Margin = new Thickness(0),
                FontWeight = FontWeights.Bold,
                BorderThickness = new Thickness(0.8),
                IsEnabled = isEnable,
                Tag = product // Assign product instance to Tag property
            };

            if (!string.IsNullOrEmpty(productButtonColor) && !productButtonColor.StartsWith("#"))
            {
                productButtonColor = "#" + productButtonColor;
            }
            else
            {
                productButtonColor = "#2196F3";
            }

            // Validate the color code
            if (IsValidHexColorCode(productButtonColor))
            {
                try
                {
                    // Set the button background color using the hex code
                    productButton.Background = (SolidColorBrush)new BrushConverter().ConvertFromString(productButtonColor);
                }
                catch (FormatException)
                {
                    // Handle invalid color format exception if necessary
                    MessageBox.Show($"Error: Invalid color format '{productButtonColor}'", "Invalid Color", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            // Create a StackPanel to hold the image and text
            StackPanel stackPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            // Add text to the stack panel
            TextBlock textBlock = new TextBlock
            {
                Text = product.ProductName,
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 3)
            };
            stackPanel.Children.Add(textBlock);

            // Check if image creation is allowed
            if (allowImage && File.Exists(imagePath))
            {
                // Load the image
                BitmapImage image = new BitmapImage(new Uri(imagePath, UriKind.RelativeOrAbsolute));
                Image img = new Image
                {
                    Source = image,
                };
                stackPanel.Children.Insert(0, img); // Insert image at the beginning
            }
            // Set the content of the button to the stack panel
            productButton.Content = stackPanel;

            return productButton;
        }

        private bool IsValidHexColorCode(string hexColor)
        {
            // Check if the hex color code is valid
            if (string.IsNullOrEmpty(hexColor)) return false;

            // Valid formats are #RRGGBB and #AARRGGBB
            string pattern = "^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{8})$";
            return Regex.IsMatch(hexColor, pattern);
        }

        private void ProductButton_Click(object sender, RoutedEventArgs e)
        {
            // Handle button click event
            Button clickedButton = (Button)sender;

            Product product = (Product)clickedButton.Tag as Product;
            
            if (product != null)
            {
                AddToCart(product);
            }
        }

        private void AddToCart(Product product)
        {
            // Increment the CartIndex to get a unique index for the new product
            CartIndex++;

            // Clear component groups (assuming this is necessary based on your logic)
            componentGroups.Clear();

            // Set the desired status for the new 

            Product newProduct = new Product(product.ProductId, product.ProductName, product.ProductPrice, product.ProductButtonColor);
            newProduct.Status = true;
            // Add the new product to the cart at the new CartIndex
            cartProducts.Add(CartIndex, newProduct);

            // Show product component dialog if necessary
            ShowProductComponentIfNeeded(CartIndex, newProduct);

            // Update cart UI
            UpdateCartUI();
            mainWindow.childItemsSelected.Clear();
        }

        private void ShowProductComponentIfNeeded(int cartIndex, Product product)
        {
            CheckProductComponent(product, out componentGroups);
            if (componentGroups.Count > 1)
            {
                ProductComponent productComponent = new ProductComponent(this, product, mainWindow, mainWindow.SaleMode, cartIndex, false);
                productComponent.ShowDialog();

                // Check if the user confirmed the product component selection
                if (!productComponent.IsConfirmed)
                {
                    // Remove the product from the cart if the selection was not confirmed
                    cartProducts.Remove(cartIndex);
                    CartIndex--;
                }
            }
        }

        public void CheckProductComponent(Product product, out List<ProductComponentGroup> componentGroups)
        {

            int productID = product.ProductId;
            componentGroups = new List<ProductComponentGroup>();
            componentGroups.Clear();
            string query = "SELECT PGroupID, ProductID, SaleMode, SetGroupName, SetGroupNo, RequireAddAmountForProduct, MinQty, MaxQty FROM productcomponentgroup WHERE ProductID = @ProductID AND SaleMode = @SaleMode ORDER BY SetGroupNo ASC";

            using (MySqlConnection connection = localDbConnector.GetMySqlConnection())
            {
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@ProductID", productID);
                command.Parameters.AddWithValue("@SaleMode", mainWindow.SaleMode);

                try
                {
                    connection.Open();
                    MySqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        ProductComponentGroup group = new ProductComponentGroup
                        {
                            PGroupID = Convert.ToInt32(reader["PGroupID"].ToString()),
                            ProductID = Convert.ToInt32(reader["ProductID"].ToString()),
                            SaleMode = reader["SaleMode"].ToString(),
                            SetGroupName = reader["SetGroupName"].ToString(),
                            SetGroupNo = Convert.ToInt32(reader["SetGroupNo"].ToString()),
                            RequireAddAmountForProduct = Convert.ToInt32(reader["RequireAddAmountForProduct"].ToString()),
                            MinQty = Convert.ToInt32(reader["MinQty"].ToString()),
                            MaxQty = Convert.ToInt32(reader["MaxQty"].ToString())
                        };

                        componentGroups.Add(group);
                    }
                    reader.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("An error occurred: " + ex.Message);
                }
            }
        }

        public void UpdateCartUI()
        {
            mainWindow.cartGridListView.Items.Clear();
            decimal totalPrice = 0;
            int totalQuantity = 0;

            // Define the height of each row
            double rowHeight = 40; // Set the desired height for each row (in pixels)

            // Create a Style for ListViewItem
            var listViewItemStyle = new Style(typeof(ListViewItem));

            // Setters for ListViewItem properties
            listViewItemStyle.Setters.Add(new Setter(ListViewItem.HeightProperty, rowHeight)); // Set the height of each ListViewItem
            listViewItemStyle.Setters.Add(new Setter(ListViewItem.BorderBrushProperty, (Brush)Application.Current.FindResource("FontColor"))); // Set border brush
            listViewItemStyle.Setters.Add(new Setter(ListViewItem.BorderThicknessProperty, new Thickness(0, 0, 0, 1))); // Set border thickness (bottom only)
            listViewItemStyle.Setters.Add(new Setter(ListViewItem.HorizontalContentAlignmentProperty, HorizontalAlignment.Center)); // Center content horizontally

            // Apply the style to the ItemContainerStyle of ListView
            mainWindow.cartGridListView.ItemContainerStyle = listViewItemStyle;

            int index = 1;

            // Loop through each product in the cart and add corresponding UI elements
            foreach (KeyValuePair<int, Product> kvp in cartProducts)
            {
                Product product = kvp.Value;
                decimal totalProductPrice = product.ProductPrice * product.Quantity; // Total price for the product
                totalQuantity += product.Quantity;

                // Determine background and foreground colors based on product status
                Brush rowBackground = product.Status ? Brushes.Transparent : Brushes.Red;
                Brush rowForeground = product.Status ? Brushes.Black : Brushes.White;
                TextDecorationCollection textDecorations = product.Status ? null : TextDecorations.Strikethrough;

                if (product.Status)
                {
                    if (product.ChildItems != null && product.ChildItems.Any())
                    {
                        foreach (ChildItem childItem in product.ChildItems)
                        {
                            totalProductPrice += (childItem.Price * childItem.Quantity);
                        }
                    }
                    totalPrice += totalProductPrice; // Only add to totalPrice if status is true
                }

                // Add the main product to the ListView
                mainWindow.cartGridListView.Items.Add(new
                {
                    Index = kvp.Key, // Assuming the dictionary key represents the index
                    ProductName = product.ProductName,
                    ProductPrice = product.ProductPrice.ToString("#,0"), // Format ProductPrice without currency symbol
                    Quantity = product.Quantity,
                    TotalPrice = totalProductPrice.ToString("#,0"),
                    ProductId = product.ProductId,
                    Background = rowBackground,
                    Foreground = rowForeground,
                    TextDecorations = textDecorations
                });

                // Add child items if they exist
                if (product.ChildItems != null && product.ChildItems.Any())
                {
                    foreach (ChildItem childItem in product.ChildItems)
                    {
                        // Add each child item to the ListView
                        mainWindow.cartGridListView.Items.Add(new
                        {
                            Index = "-", // Indent child items with a dash for visual separation
                            ProductName = childItem.Name,
                            ProductPrice = childItem.Price.ToString("#,0"),
                            Quantity = childItem.Quantity,
                            Background = rowBackground, // Inherit parent's background color
                            Foreground = rowForeground, // Inherit parent's foreground color
                            TextDecorations = textDecorations
                        });
                    }
                }
            }

            // Define and apply the custom item container style for ListViewItems
            Style itemContainerStyle = new Style(typeof(ListViewItem));
            itemContainerStyle.Setters.Add(new Setter(ListViewItem.HeightProperty, rowHeight)); // Set the height of each ListViewItem
            itemContainerStyle.Setters.Add(new Setter(ListViewItem.BorderBrushProperty, (Brush)Application.Current.FindResource("FontColor"))); // Set border brush
            itemContainerStyle.Setters.Add(new Setter(ListViewItem.BorderThicknessProperty, new Thickness(0, 0, 0, 1))); // Set border thickness (bottom only)
            itemContainerStyle.Setters.Add(new Setter(ListViewItem.BackgroundProperty, new Binding("Background")));
            itemContainerStyle.Setters.Add(new Setter(ListViewItem.ForegroundProperty, new Binding("Foreground")));

            mainWindow.cartGridListView.ItemContainerStyle = itemContainerStyle;

            // Update displayed total prices
            mainWindow.subTotal.Text = $"{totalPrice:C0}";
            mainWindow.total.Text = (totalPrice + (totalPrice * productVATPercent / 100)).ToString("#,0");
            mainWindow.VATModeText.Text = $"{(totalPrice * productVATPercent / 100).ToString("#,0")}";
            mainWindow.GrandTextBlock.Text = $"{totalPrice + (totalPrice * productVATPercent / 100):C0}";
            mainWindow.totalQty.Text = totalQuantity.ToString("0.00");
            mainWindow.GrandTotalCalculator.Text = $"{(totalPrice + (totalPrice * productVATPercent / 100)).ToString("#,0")}";


            bool hasItemsInCart = cartProducts.Any();
            // Enable or disable the PayButton based on whether there are items in cartProducts
            mainWindow.PayButton.IsEnabled = hasItemsInCart;
            mainWindow.HoldButton.IsEnabled = hasItemsInCart;
            mainWindow.ClearButton.IsEnabled = hasItemsInCart;
            mainWindow.UpdateSecondayMonitor();

            if (mainWindow.cartGridListView.Items.Count > 0)
            {
                // Scroll into the last item
                mainWindow.cartGridListView.ScrollIntoView(mainWindow.cartGridListView.Items[mainWindow.cartGridListView.Items.Count - 1]);
            }
        }

        private void DeleteFromCart(object sender, RoutedEventArgs e)
        {
            // Retrieve the index from the command parameter
            if (e.Source is Button button && button.CommandParameter is int index)
            {
                // Check if the index is within the valid range
                if (index >= 0 && index < cartProducts.Count)
                {
                    // Retrieve the product at the specified index
                    Product productToCancel = cartProducts.ElementAt(index).Value;

                    // Set the status of the product to "Canceled"
                    productToCancel.Status = false;
                    productToCancel.Quantity = 0;

                    // Cancel child items associated with the product
                    if (productToCancel.ChildItems != null && productToCancel.ChildItems.Any())
                    {
                        foreach (ChildItem childItem in productToCancel.ChildItems)
                        {
                            childItem.Status = false;
                            childItem.Quantity = 0;
                        }
                    }

                    // Debug: Print confirmation
                    Console.WriteLine($"Product at index {index} and associated child items marked as canceled.");

                    // Update cart UI to reflect the status change
                    UpdateCartUI();
                }
                else
                {
                    // Debug: Print message if index is out of range
                    Console.WriteLine($"Invalid index: {index}");
                }

                // Debug: Print details of cartProducts
                Console.WriteLine("Cart Products:");
                foreach (var kvp in cartProducts)
                {
                    Product product = kvp.Value;
                    Console.WriteLine($"ProductId: {product.ProductId}, ProductName: {product.ProductName}, Quantity: {product.Quantity}, Status: {(product.Status ? "Active" : "Canceled")}");
                    if (product.ChildItems != null && product.ChildItems.Any())
                    {
                        Console.WriteLine("Child Items:");
                        foreach (var childItem in product.ChildItems)
                        {
                            Console.WriteLine($"- {childItem.Name}, Price: {childItem.Price:C}, Quantity: {childItem.Quantity}, Status: {(childItem.Status ? "Active" : "Canceled")}");
                        }
                    }
                }
            }
        }

        public void UpdateVisibleButtons()
        {
            int col = 4;
            UpdateVisibleProductButtons(mainWindow.ProductGroupName, mainWindow.startIndex, 8, mainWindow.NextGroupButton, col);
        }

        public void UpdateVisibleProductButtons(Grid grid, int startIndex, int shiftAmount, Button nextButton, int col)
        {
            nextButton.IsEnabled = true;
            int itemsPerPage = shiftAmount;
            int totalItems = grid.Children.Count;
            int endIndex = startIndex + itemsPerPage;
            int itemsDisplayed = 0;

            for (int i = 0; i < totalItems; i++)
            {
                if (i >= startIndex && i < endIndex)
                {
                    grid.Children[i].Visibility = Visibility.Visible;

                    int row = itemsDisplayed / col; // Assuming 6 columns per row
                    int column = itemsDisplayed % col;

                    Grid.SetRow(grid.Children[i], row);
                    Grid.SetColumn(grid.Children[i], column);

                    itemsDisplayed++;
                }
                else
                {
                    grid.Children[i].Visibility = Visibility.Collapsed;
                }
            }

            if (itemsDisplayed < itemsPerPage)
            {
                nextButton.IsEnabled = false;
                int remainingItems = itemsPerPage - itemsDisplayed;
                for (int i = 0; i < remainingItems; i++)
                {
                    int row = (itemsDisplayed + i) / col;
                    int column = (itemsDisplayed + i) % col;

                    Button blankButton = new Button
                    {
                        IsEnabled = false,
                        Effect = (System.Windows.Media.Effects.Effect)Application.Current.FindResource("DropShadowEffect"),
                        Style = (System.Windows.Style)Application.Current.FindResource("ButtonStyle"),
                        Margin = new Thickness(1),
                        BorderThickness = new Thickness(0.8),
                        Background = (Brush)Application.Current.FindResource("ButtonDisabledColor") // Optional: color for the blank buttons
                    };

                    Grid.SetRow(blankButton, row);
                    Grid.SetColumn(blankButton, column);
                    grid.Children.Add(blankButton);
                }
            }
        }

        public void UpdateVisibleProductButtons()
        {
            int col = 6;
            UpdateVisibleProductButtons(mainWindow.MainContentProduct, mainWindow.productButtonStartIndex, mainWindow.ProductButtonShiftAmount, mainWindow.nextButton, col);
        }

        public void UpdateProductSubcontent()
        {
            int col = 6;
            UpdateVisibleProductButtons(mainWindow.MenuSubCategoryContentProduct, mainWindow.subContentProductStartIndex, mainWindow.subContentProductButtonShiftAmount, mainWindow.nextButton, col);
        }

        public void updateVisibleContentButton()
        {
            int col = 5;
            UpdateVisibleProductButtons(mainWindow.MenuSubCategoryContentButton, mainWindow.subContentStartIndex, mainWindow.subContentButtonShiftAmount, mainWindow.nextContentMenu, col);
        }

        public void Calculating()
        {
            // Parse the text values into double (assuming they represent numbers)
            if (double.TryParse(mainWindow.displayText.Text, out double currentTextValue) &&
                double.TryParse(mainWindow.GrandTotalCalculator.Text, out double grandTotalValue))
            {
                // Perform the subtraction
                double returnAmount = currentTextValue - grandTotalValue;

                // Update the TotalReturnCalculator with the calculated value
                if (returnAmount < 0)
                {
                    mainWindow.TotalReturnCalculator.Text = "0";
                }
                else
                {
                    mainWindow.TotalReturnCalculator.Text = returnAmount.ToString("#,0");
                }
                
            }
            else
            {
                // Handle parsing failure or invalid input (e.g., non-numeric text)
                MessageBox.Show("Invalid input. Please enter valid numeric values.");
            }
        }

        public void UpdateFormattedDisplay()
        {
            // Get the raw numerical value from the displayed text
            if (double.TryParse(mainWindow.displayText.Text.Replace(".", ""), out double number))
            {
                // Format the number with dots as thousands separators
                mainWindow.displayText.Text = number.ToString("#,##0");
            }
            else
            {
                // Invalid input handling (e.g., if parsing fails)
                mainWindow.displayText.Text = "0";
            }
        }

        public void GetProductVATInfo()
        {
            string query = "SELECT ProductVATCode, ProductVATPercent, VATDesp FROM ProductVAT WHERE ProductVATCode = 'V'";

            try
            {
                using (MySqlConnection connection = localDbConnector.GetMySqlConnection())
                {
                    MySqlCommand command = new MySqlCommand(query, connection);
                    connection.Open();

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            productVATPercent = reader.GetDecimal("ProductVATPercent");
                            productVatText = reader.GetString("VATDesp");
                            productVatCode = reader.GetString("ProductVATCode");

                        }
                        else
                        {
                            Console.WriteLine("No data found for ProductVATCode = 'V'.");
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"MySQL Error: {ex.Message}");
                Console.WriteLine($"Error Code: {ex.ErrorCode}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving ProductVAT information: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            }
        }

        public void ResetUI(string tStatus)
        {
            // Assuming you have access to the necessary variables: lastCartId, mainWindow, cartProducts, SaleMode
            int customerID = mainWindow.OrderID;
            Cart cart = new Cart(customerID, cartProducts.Values.ToList(), mainWindow.SaleMode, isReset, tStatus);
            // Clear the display after processing the input
            isReset = true;
            if (isReset)
            {
                cart = new Cart(customerID, cartProducts.Values.ToList(), mainWindow.SaleMode, isReset, tStatus);
                isReset = false;
            }
            mainWindow.isNew = true;
            mainWindow.displayText.Text = "0";
            cartProducts.Clear();
            UpdateCartUI();
            Calculating();
            mainWindow.PayementProcess.Visibility = Visibility.Collapsed;
            mainWindow.MainContentProduct.Visibility = Visibility.Visible;
            mainWindow.SaleMode = 0;
            mainWindow.SaleModeView();
            CartIndex = 0;
        }

        public async void qrisProcess(string PayTypeId, string PayTypeName)
        {
            try
            {
                // Create transaction asynchronously
                (string transactionOrderId, string transactionQrUrl) = await CreateTransactionAsync(cartProducts, productVATPercent);

                if (!string.IsNullOrEmpty(transactionQrUrl))
                {
                    // Download the QR code image asynchronously
                    byte[] imageData = await DownloadImageData(transactionQrUrl);

                    if (imageData != null && imageData.Length > 0)
                    {
                        // Display the QR code image
                        BitmapImage bitmapImage = new BitmapImage();
                        using (MemoryStream stream = new MemoryStream(imageData))
                        {
                            bitmapImage.BeginInit();
                            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                            bitmapImage.StreamSource = stream;
                            bitmapImage.EndInit();
                        }

                        mainWindow.UpdateQRCodeImage(imageData, transactionQrUrl);
                        // Start timer to check transaction status periodically
                        GetTransactionStatus(transactionOrderId, PayTypeId, PayTypeName);
                    }
                    else
                    {
                        Console.WriteLine("Failed to download QR code image data.");
                    }
                }
                else
                {
                    Console.WriteLine("Failed to retrieve QR code URL.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
        
        private async Task<byte[]> DownloadImageData(string imageUrl)
        {
            try
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    // Download the image data asynchronously
                    HttpResponseMessage response = await httpClient.GetAsync(imageUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        // Read the image data as a byte array
                        return await response.Content.ReadAsByteArrayAsync();
                    }
                    else
                    {
                        Console.WriteLine($"Error downloading image: {response.StatusCode}");
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error downloading image: {ex.Message}");
                return null;
            }
        }

        public async Task<(string, string)> CreateTransactionAsync(Dictionary<int, Product> cartProducts, decimal TaxPercentage)
        {
            // Your Midtrans server key and base URL
            string serverKey = "SB-Mid-server-GNryM0x4oekS6OrCVt5ahnjv";
            string baseUrl = "https://api.sandbox.midtrans.com/v2/charge";
            string orderId = Guid.NewGuid().ToString();

            try
            {
                // Prepare the HTTP client
                using (HttpClient httpClient = new HttpClient())
                {
                    // Add authorization header
                    string authHeader = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{serverKey}:"));
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authHeader);

                    // Filter and create item details from the cart products with status set to true
                    var itemDetails = new List<object>();
                    foreach (var kvp in cartProducts)
                    {
                        var product = kvp.Value;
                        if (product.Status)
                        {
                            var itemDetail = new
                            {
                                id = product.ProductId,
                                price = product.ProductPrice,
                                quantity = product.Quantity,
                                name = product.ProductName,
                            };

                            itemDetails.Add(itemDetail);

                            // Add child items if present
                            if (product.HasChildItems())
                            {
                                foreach (var childItem in product.ChildItems)
                                {
                                    var childItemDetail = new
                                    {
                                        id = childItem.Name,
                                        price = childItem.Price,
                                        quantity = childItem.Quantity,
                                        name = childItem.Name
                                    };

                                    itemDetails.Add(childItemDetail);
                                }
                            }
                        }
                    }

                    // Calculate total amount of the cart
                    decimal cartTotal = itemDetails.Sum(item =>
                    {
                        var itemPrice = (decimal)item.GetType().GetProperty("price").GetValue(item);
                        var itemQuantity = (int)item.GetType().GetProperty("quantity").GetValue(item);
                        return itemQuantity * itemPrice;
                    });

                    // Calculate tax amount
                    decimal taxAmount = cartTotal * (TaxPercentage / 100);

                    // Add tax item to the item details
                    itemDetails.Add(new
                    {
                        id = 0, // Assuming tax item ID is an integer (adjust as needed)
                        price = taxAmount,
                        quantity = 1,
                        name = "Tax"
                    });

                    // Define the request body
                    var requestBody = new
                    {
                        payment_type = "qris",
                        transaction_details = new
                        {
                            order_id = orderId,
                            gross_amount = cartTotal + taxAmount, // Include tax in the total amount
                        },
                        item_details = itemDetails,
                        customer_details = new
                        {
                            first_name = "John",
                            last_name = "Doe",
                            email = "john.doe@example.com",
                            phone = "000000"
                        }
                    };

                    // Convert the request body to JSON
                    string json = JsonConvert.SerializeObject(requestBody);

                    // Send the POST request
                    HttpResponseMessage response = await httpClient.PostAsync(baseUrl, new StringContent(json, Encoding.UTF8, "application/json"));

                    if (response.IsSuccessStatusCode)
                    {
                        // Read the response content
                        string responseContent = await response.Content.ReadAsStringAsync();

                        // Parse the JSON response
                        var jsonResponse = JObject.Parse(responseContent);

                        // Retrieve transaction status
                        string transactionStatus = jsonResponse["transaction_status"]?.ToString();

                        // Retrieve URL from actions array (assuming there's only one action)
                        string url = jsonResponse["actions"]?[0]?["url"]?.ToString();

                        // Return transaction status and URL as a tuple
                        return (orderId, url);
                    }
                    else
                    {
                        // Handle unsuccessful request
                        string errorResponse = await response.Content.ReadAsStringAsync();
                        MessageBox.Show($"Error: {response.StatusCode}\n{errorResponse}");

                        // Return null values as a tuple
                        return (null, null);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");

                // Return null values as a tuple
                return (null, null);
            }
        }

        private async void GetTransactionStatus(string orderId, string PayTypeId, string PayTypeName)
        {
            string BaseUrl = "https://api.sandbox.midtrans.com";

            // Define your Midtrans server key
            string serverKey = "SB-Mid-server-GNryM0x4oekS6OrCVt5ahnjv";

            // Define the delay between each status check attempt
            int delayMilliseconds = 1000; // 1 second

            bool isDone = false;
            while (!isDone) // Continue indefinitely
            {
                try
                {
                    if (PayTypeId != isPaymentChange)
                    {
                        isDone = true;
                        mainWindow.QrisDone("Cancel", PayTypeId, PayTypeName);
                    }
                    Console.WriteLine("Chekcing");
                    // Construct the URL for the status check API
                    string statusCheckUrl = $"{BaseUrl}/v2/{orderId}/status";

                    // Prepare the HTTP client
                    using (HttpClient httpClient = new HttpClient())
                    {
                        // Add authorization header
                        string authHeader = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{serverKey}:"));
                        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authHeader);

                        // Send the GET request to the status check API
                        HttpResponseMessage response = await httpClient.GetAsync(statusCheckUrl);
                        if (response.IsSuccessStatusCode)
                        {
                            // Read the response content
                            string responseContent = await response.Content.ReadAsStringAsync();

                            // Parse the JSON response
                            var jsonResponse = JObject.Parse(responseContent);

                            // Extract status message and transaction status
                            string statusMessage = jsonResponse["status_message"]?.ToString();
                            string transactionStatus = jsonResponse["transaction_status"]?.ToString();
                            // Set background color based on transaction status
                            Brush backgroundBrush;
                            switch (transactionStatus)
                            {
                                case "settlement":
                                    // Light green background for Settlement status
                                    backgroundBrush = Brushes.LightGreen;
                                    transactionStatus = "Settlement";
                                    mainWindow.QrisDone(transactionStatus, PayTypeId, PayTypeName);
                                    
                                    isDone = true;
                                    break;
                                case "pending":
                                    // Light yellow background for Pending status
                                    backgroundBrush = Brushes.Yellow;
                                    transactionStatus = "Pending";
                                    break;
                                case "cancel":
                                    // Light red background for Cancel status
                                    backgroundBrush = Brushes.LightSalmon;
                                    transactionStatus = "Cancel";
                                    mainWindow.QrisDone(transactionStatus, PayTypeId, PayTypeName);
                                    isDone = true;
                                    break;
                                case "expire":
                                    // Light gray background for Expire status
                                    backgroundBrush = Brushes.LightGray;
                                    transactionStatus = "Expire";
                                    mainWindow.QrisDone(transactionStatus, PayTypeId, PayTypeName);
                                    isDone = true;
                                    break;
                                default:
                                    // Default background color for other statuses
                                    backgroundBrush = Brushes.White;
                                    transactionStatus = "IDLE";
                                    break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error getting transaction status: {ex.Message}");
                }

                // Delay before the next status check attempt
                await Task.Delay(delayMilliseconds);
            }
        }

        public async void OrderTransactionFunction()
        {
            OrderTransaction orderTransaction = new OrderTransaction();

            OrderPayDetail orderPayDetail = new OrderPayDetail();

            DateTime now = DateTime.Now;

            int ComputerID = Properties.Settings.Default._ComputerID;

            int SaleMode = mainWindow.SaleMode;

            int ShopID = int.Parse(Properties.Settings.Default._AppID);

            int ReceiptTotalQty = cartProducts.Count();

            int generatedTransactionID = await orderTransaction.GenerateTransactionID();

            int totalQuantity = 0;

            decimal totalPrice = 0;

            foreach (KeyValuePair<int, Product> kvp in cartProducts)
            {
                Product product = kvp.Value;
                decimal totalProductPrice = product.ProductPrice * product.Quantity; // Total price for the product including quantity
                totalQuantity += product.Quantity;

                if (product.Status)
                {
                    if (product.ChildItems != null && product.ChildItems.Any())
                    {
                        foreach (ChildItem childItem in product.ChildItems)
                        {
                            totalProductPrice += childItem.Price * childItem.Quantity; // Add the price of each child item
                        }
                    }

                    // Calculate tax amount
                    decimal taxAmount = totalProductPrice * productVATPercent / 100;

                    // Add tax amount to total product price
                    totalProductPrice += taxAmount;

                    // Add total product price to total price
                    totalPrice += totalProductPrice;
                }
            }

            string PayText = mainWindow.displayText.Text.Replace(".", "");

            decimal TotalPay = decimal.TryParse(PayText, out TotalPay) ? TotalPay : 0;
                        
            decimal CurrencyRatio = 1;

            decimal CurrencyExchangeRatio = 1;

            decimal TotalChange = TotalPay - totalPrice;

            string payRemark = string.Empty;

            string CurrencyCode = "Rp";

            string ReferenceNo = OrderTransaction.NumberGenerator.GenerateNumber(Properties.Settings.Default._ComputerID.ToString(), generatedTransactionID);

            string LogoIMG = Properties.Settings.Default._PrinterLogo;

            string key = $"{orderTransaction.TransactionID}:{ComputerID}";

            short customerID = (short)mainWindow.OrderID;

            short TransactionStatus = 2;

            string receiptNumber = MainWindow.GenerateReceiptNumber(mainWindow.OrderID.ToString());

            decimal DisctountItem = 0;

            decimal DisctountBill = 0;

            decimal DisctountOther = 0;

            decimal TotalDiscount = 0;

            decimal TransactionVATable = totalPrice + (totalPrice * productVATPercent / 100);

            decimal TransactionVat = totalPrice * productVATPercent / 100;

            DateTime TransactionStart = DateTime.Now;

            //THIS FOR GENERATING ID
            orderTransaction.TransactionID = generatedTransactionID;
            orderTransaction.ComputerID = ComputerID;
            orderTransaction.TransactionUUID = orderTransaction.GenerateUUID();
            orderTransaction.TranKey = key;
            orderTransaction.ReserveTime = null;
            orderTransaction.ReserveStaffID = 0;
            orderTransaction.OpenTime = TransactionStart;
            orderTransaction.OpenStaffID = CurrentSessions.StaffID;
            orderTransaction.OpenStaff = CurrentSessions.StaffFirstName;
            orderTransaction.PaidTime = DateTime.Now;
            orderTransaction.PaidStaffID = CurrentSessions.StaffID;
            orderTransaction.PaidStaff = $"{CurrentSessions.StaffFirstName} {CurrentSessions.StaffLastName}";
            orderTransaction.PaidComputerID = ComputerID;
            orderTransaction.VerifyPaidStaffID = 0;
            orderTransaction.VerifyPaidDateTime = null;
            orderTransaction.BuffetStartTime = null;
            orderTransaction.BuffetEndTime = null;
            orderTransaction.BuffetTime = 0;
            orderTransaction.BuffetType = 0;
            orderTransaction.CloseTime = null;
            orderTransaction.CommStaffID = 0;
            orderTransaction.DiscountItem = DisctountItem;
            orderTransaction.DiscountBill = DisctountBill;
            orderTransaction.DiscountOther = DisctountOther;
            orderTransaction.TotalDiscount = TotalDiscount;
            orderTransaction.TransactionStatusID = TransactionStatus;
            orderTransaction.SaleMode = SaleMode;
            orderTransaction.TransactionName = string.Empty;
            orderTransaction.QueueName = string.Empty;
            orderTransaction.NoCustomer = customerID;
            orderTransaction.NoCustomerWhenOpen = 0;
            orderTransaction.DocType = 8;
            orderTransaction.ReceiptYear = now.Year;
            orderTransaction.ReceiptMonth = now.Month;
            orderTransaction.ReceiptDay = now.Day;
            orderTransaction.ReceiptID = customerID;
            orderTransaction.ReceiptNumber = receiptNumber;
            orderTransaction.SaleDate = DateTime.Now;
            orderTransaction.ShopID = ShopID;
            orderTransaction.TransactionVAT = TransactionVat;
            orderTransaction.TransactionVATable = TransactionVATable;
            orderTransaction.TranBeforeVAT = totalPrice;
            orderTransaction.VATCode = productVatCode;
            orderTransaction.VATPercent = productVATPercent;
            orderTransaction.ProductVAT = TransactionVat;
            orderTransaction.ServiceChargePercent = 0;
            orderTransaction.ServiceCharge = 0;
            orderTransaction.ServiceChargePercent = 0;
            orderTransaction.SCBeforeVAT = 0;
            orderTransaction.OtherIncome = 0;
            orderTransaction.OtherIncomeVAT = 0;
            orderTransaction.ReceiptTotalQty = ReceiptTotalQty;
            orderTransaction.ReceiptRetailPrice = TransactionVATable;
            orderTransaction.ReceiptDiscount = 0;
            orderTransaction.ReceiptSalePrice = TransactionVATable;
            orderTransaction.ReceiptNetSale = TransactionVATable;
            orderTransaction.ReceiptPayPrice = TransactionVATable;
            orderTransaction.ReceiptRoundingBill = 0;
            orderTransaction.SessionID = SessionsID.SessionIDs;
            orderTransaction.CloseComputerID = ComputerID;
            orderTransaction.VoidStaffID = 0;
            orderTransaction.VoidStaff = string.Empty;
            orderTransaction.VoidReason = null;
            orderTransaction.IsCloneBill = 0;
            orderTransaction.VoidComID = 0;
            orderTransaction.DiffCloneBill = 0;
            orderTransaction.MemberID = 0;
            orderTransaction.MemberName = string.Empty;
            orderTransaction.HasOrder = 0;
            orderTransaction.NoPrintBillDetail = 0;
            orderTransaction.NoReprint = 0;
            orderTransaction.LastPayCheckBill = 0;
            orderTransaction.DiffPayCheckBill = 0;
            orderTransaction.BillDetailReferenceNo = 0;
            orderTransaction.CallForCheckBill = 0;
            orderTransaction.TransactionNote = null;
            orderTransaction.CurrentAccessComputer = ComputerID;
            orderTransaction.UpdateDate = null;
            orderTransaction.BeginTime = null;
            orderTransaction.EndTime = null;
            orderTransaction.PrintWarningTime = null;
            orderTransaction.PrintBeginTime = 0;
            orderTransaction.AlreadyCalculateStock = 0;
            orderTransaction.AlreadyExportToHQ = 0;
            orderTransaction.HasFullTax = 0;
            orderTransaction.TableID = 0;
            orderTransaction.TableName = null;
            orderTransaction.IsSplitTransaction = 0;
            orderTransaction.IsFromOtherTransaction = 0;
            orderTransaction.ReferenceNo = ReferenceNo;
            orderTransaction.FromDepositTransactionID = 0;
            orderTransaction.FromDepositComputerID = 0;
            orderTransaction.WifiUserName = string.Empty;
            orderTransaction.WifiPassword = string.Empty;
            orderTransaction.WifiExpire = null;
            orderTransaction.LogoImage = LogoIMG;
            orderTransaction.Deleted = 0;

            TransactionService transactionService = new TransactionService();
            transactionService.InsertTransaction(orderTransaction);

            orderPayDetail.PayDetailID = 1;
            orderPayDetail.TransactionID = generatedTransactionID;
            orderPayDetail.ComputerID = ComputerID;
            orderPayDetail.TranKey = key;
            orderPayDetail.PayAmount = TotalPay;
            orderPayDetail.CashChange = TotalChange;
            orderPayDetail.CashChangeMainCurrency = TotalChange;
            orderPayDetail.CashChangeMainCurrencyCode = CurrencyCode;
            orderPayDetail.CashChangeCurrencyAmount = TotalChange;
            orderPayDetail.CashChangeCurrencyCode = CurrencyCode;
            orderPayDetail.CashChangeCurrencyName = "IDN - Rupiah";
            orderPayDetail.CashChangeCurrencyRatio = CurrencyRatio;
            orderPayDetail.CashChangeExchangeRate = CurrencyExchangeRatio;
            orderPayDetail.CreditCardNo = null;
            orderPayDetail.CreditCardHolderName = string.Empty;
            orderPayDetail.CCApproveCode = string.Empty;
            orderPayDetail.ExpireMonth = 0;
            orderPayDetail.ExpireYear = 0;
            orderPayDetail.ChequeNumber = null;
            orderPayDetail.ChequeDate = null;
            orderPayDetail.BankNameID = 0;
            orderPayDetail.CreditCardType = 0;
            orderPayDetail.PaidByName = null;
            orderPayDetail.PayRemark = payRemark;
            orderPayDetail.Paid = 0;
            orderPayDetail.CardID = 0;
            orderPayDetail.CardNo = null;
            orderPayDetail.PrepaidDiscountPercent = 0;
            orderPayDetail.RevenueRatio = 0;
            orderPayDetail.IsFromEDC = false;
            orderPayDetail.CurrencyCode = CurrencyCode;
            orderPayDetail.CurrencyName = "IDN - Rupiah";
            orderPayDetail.CurrencyRatio = CurrencyRatio;
            orderPayDetail.ExchangeRate = CurrencyExchangeRatio;
            orderPayDetail.CurrencyAmount = TotalPay;
            orderPayDetail.ShopID = ShopID;
            orderPayDetail.SaleDate = DateTime.Now;
            orderPayDetail.OrgPayTypeID = 0;
            orderPayDetail.VoucherSellValue = 0;
            orderPayDetail.VoucherCostValue = 0;
            orderPayDetail.VoucherID = 0;
            orderPayDetail.VShopID = 0;
            orderPayDetail.VoucherNo = string.Empty;
            orderPayDetail.VoucherSN = string.Empty;
            orderPayDetail.RedeemSettingPoint = 0;
            orderPayDetail.RedeemPerPayAmount = 0;
            orderPayDetail.RedeemPoint = 0;

            TransactionServicePayDetail transactionServicePayDetail = new TransactionServicePayDetail();
            transactionServicePayDetail.InsertTransaction(orderPayDetail);

        }
    }
}