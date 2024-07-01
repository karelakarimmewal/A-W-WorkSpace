using MySql.Data.MySqlClient;
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
        public MainWindow mainWindow;
        public SecondaryMonitor secondaryMonitor;
        public ProductComponent productComponent;

        // Fields related to cart products
        public Dictionary<int, Product> cartProducts = new Dictionary<int, Product>();
        private List<Button> productButtons = new List<Button>();
        public Dictionary<int, PaymentDetails> multiplePaymentList = new Dictionary<int, PaymentDetails> ();

        // Fields related to product VAT and payment
        public decimal productVATPercent;
        public string productVatText;
        public string productVatCode;
        public string isPaymentChange;

        // Fields for tracking application state
        private bool isReset;
        private int ProdTotalCount;
        private int CartIndex = 0;
        private int paymentIndex = 0;
        public int idProduct = 0;
        public decimal multiplePaymentAmount = 0;

        public ModelProcessing(MainWindow mainWindow = null)
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
            // Check if the product with the same ProductId already exists in the cart
            bool productExists = false;
            if (cartProducts.Count > 0)
            {
                var lastEntry = cartProducts.Last();
                if (lastEntry.Value.ProductId == product.ProductId && lastEntry.Value.Status)
                {
                    // Last product exists and matches the product ID, update the quantity
                    
                    lastEntry.Value.Quantity += ProductQuantityClass.ProductQTY > 1 ? ProductQuantityClass.ProductQTY : 1;
                    productExists = true;
                }
            }

            // If the product does not exist, add it as a new product
            if (!productExists)
            {
                // Increment the CartIndex to get a unique index for the new product
                CartIndex++;

                CurrentComponentGroupItem.CPGI.Clear();

                int ComponentLevel = 1;
                // Set the desired status for the new product

                Product newProduct = new Product(product.ProductId, product.ProductName, product.ProductPrice, product.ProductButtonColor, ComponentLevel);
                newProduct.Status = true;
                newProduct.Quantity = ProductQuantityClass.ProductQTY;
                mainWindow.productQuantitySelectorText.Text = "0";
                //SettQuantity.SettQuantityCTOR(newProduct);
                // Add the new product to the cart at the new CartIndex
                cartProducts.Add(CartIndex, newProduct);

                // Show product component dialog if necessary
                ShowProductComponentIfNeeded(CartIndex, newProduct);
            }

            // Update cart UI
            UpdateCartUI();
            mainWindow.childItemsSelected.Clear();
        }

        private void ShowProductComponentIfNeeded(int cartIndex, Product product)
        {
            CheckProductComponent(product);
            if (CurrentComponentGroupItem.CPGI.Count > 1)
            {
                product.ProductComponentLevel = 2;
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

        public void CheckProductComponent(Product product)
        {

            int productID = product.ProductId;
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
                        int PGroupID = Convert.ToInt32(reader["PGroupID"].ToString());
                        int ProductID = Convert.ToInt32(reader["ProductID"].ToString());
                        string SetGroupName = reader["SetGroupName"].ToString();
                        int SetGroupNo = Convert.ToInt32(reader["SetGroupNo"].ToString());
                        int RequireAddAmountForProduct = Convert.ToInt32(reader["RequireAddAmountForProduct"].ToString());
                        int MinQty = Convert.ToInt32(reader["MinQty"].ToString());
                        int MaxQty = Convert.ToInt32(reader["MaxQty"].ToString());

                        CurrentComponentGroupItem currentComponentGroupItem = new CurrentComponentGroupItem(SetGroupNo, SetGroupName, PGroupID, ProductID, RequireAddAmountForProduct * product.Quantity, MinQty * product.Quantity, MaxQty * product.Quantity);
                    }
                    reader.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("An error occurred: " + ex.Message);
                }
            }
        }

        public void MultiplePaymentProcess(int PayTypeID = 1)
        {
            string paymentTotalText;

            if (PayTypeID == 1)
            {
                // Remove thousand separators by replacing "." with an empty string
                paymentTotalText = mainWindow.displayText.Text.Replace(".", "");

                // Convert the cleaned string to a decimal value
                if (decimal.TryParse(paymentTotalText, out decimal paymentAmount))
                {
                    // Increment the payment index
                    paymentIndex++;

                    // Retrieve the payment type name from the database
                    string paymentTypeName = GetPaymentTypeNameById(PayTypeID);

                    // Create a PaymentDetails object
                    PaymentDetails paymentDetail = new PaymentDetails(
                        paymentTypeID: PayTypeID,
                        paymentTypeName: paymentTypeName,
                        paymentAmount: paymentAmount
                    );

                    // Calculate the total amount
                    multiplePaymentAmount += paymentAmount;

                    // Insert the PaymentDetails object into the dictionary
                    multiplePaymentList[paymentIndex] = paymentDetail;
                }
                else
                {
                    // Handle conversion error
                    MessageBox.Show("Invalid payment amount format.");
                }
            } else if (PayTypeID == 0)
            {
            
            }

            UpdateMultiplePaymentUI();
            UpdateCartUI();
        }

        private string GetPaymentTypeNameById(int paymentTypeID)
        {
            string paymentTypeName = null;
            using (MySqlConnection connection = localDbConnector.GetMySqlConnection())
            {
                string query = "SELECT PayTypeName FROM paytype WHERE PayTypeID = @paymentTypeID AND IsAvailable = 1";

                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@paymentTypeID", paymentTypeID);

                connection.Open();
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        paymentTypeName = reader["PayTypeName"].ToString();
                    }
                }
            }

            return paymentTypeName ?? "Unknown";
        }

        public void UpdateMultiplePaymentUI()
        {
            multiplePaymentAmount = 0;
            mainWindow.multiplePaymentDetails.Items.Clear();
            
            double rowHeight = 40;
            var listViewItemStyle = new Style(typeof(ListViewItem));
            listViewItemStyle.Setters.Add(new Setter(ListViewItem.HeightProperty, rowHeight)); 
            listViewItemStyle.Setters.Add(new Setter(ListViewItem.BorderBrushProperty, (Brush)Application.Current.FindResource("FontColor"))); 
            listViewItemStyle.Setters.Add(new Setter(ListViewItem.BorderThicknessProperty, new Thickness(0, 0, 0, 1))); 
            listViewItemStyle.Setters.Add(new Setter(ListViewItem.HorizontalAlignmentProperty, HorizontalAlignment.Center));

            mainWindow.multiplePaymentDetails.ItemContainerStyle = listViewItemStyle;

            foreach (KeyValuePair<int, PaymentDetails> kvp in multiplePaymentList)
            {
                PaymentDetails paymentDetails = kvp.Value;

                // Determine background and foreground colors based on product status
                Brush rowBackground = paymentDetails.PaymentIsAcitve ? Brushes.Transparent : Brushes.Red;
                Brush rowForeground = paymentDetails.PaymentIsAcitve ? Brushes.Black : Brushes.White;

                if (paymentDetails.PaymentIsAcitve)
                {
                    multiplePaymentAmount += paymentDetails.PaymentAmount;
                }

                // Add the main product to the ListView
                mainWindow.multiplePaymentDetails.Items.Add(new
                {
                    PaymentTypeName = paymentDetails.PaymentTypeName,
                    PaymentDetail = paymentDetails.PaymentAmount.ToString("C0"),
                    PaymentAmount = paymentDetails.PaymentAmount.ToString("#,0"),
                    Background = rowBackground,
                    Foreground = rowForeground,
                    HorizontalAlignment = HorizontalAlignment.Center,
                });
            }


            Style itemContainerStyle = new Style(typeof(ListViewItem));
            itemContainerStyle.Setters.Add(new Setter(ListViewItem.HeightProperty, rowHeight)); // Set the height of each ListViewItem
            itemContainerStyle.Setters.Add(new Setter(ListViewItem.BorderBrushProperty, (Brush)Application.Current.FindResource("FontColor"))); // Set border brush
            itemContainerStyle.Setters.Add(new Setter(ListViewItem.BorderThicknessProperty, new Thickness(0, 0, 0, 1))); // Set border thickness (bottom only)
            itemContainerStyle.Setters.Add(new Setter(ListViewItem.BackgroundProperty, new Binding("Background")));
            itemContainerStyle.Setters.Add(new Setter(ListViewItem.ForegroundProperty, new Binding("Foreground")));
            itemContainerStyle.Setters.Add(new Setter(ListViewItem.HorizontalAlignmentProperty, HorizontalAlignment.Center));

            mainWindow.multiplePaymentDetails.ItemContainerStyle = itemContainerStyle;

            mainWindow.totalMultiplePaymentText.Text = multiplePaymentAmount.ToString("#,0");

            if (mainWindow.multiplePaymentDetails.Items.Count > 0)
            {
                // Scroll into the last item
                mainWindow.multiplePaymentDetails.ScrollIntoView(mainWindow.multiplePaymentDetails.Items[mainWindow.multiplePaymentDetails.Items.Count - 1]);
            }
            UpdateCartUI();
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
                            totalProductPrice += (childItem.ChildPrice * childItem.ChildQuantity);
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
                            ProductName = childItem.ChildName,
                            ProductPrice = childItem.ChildPrice.ToString("#,0"),
                            Quantity = childItem.ChildQuantity,
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

            decimal thisTax = totalPrice * productVATPercent / 100;
            CurrentTransaction currentTransaction = new CurrentTransaction(totalPrice, thisTax);

            decimal beforeTax = CurrentTransaction.NeedToPay;
            decimal afterTax = CurrentTransaction.NeedToPay + CurrentTransaction.TaxAmount;

            // Update displayed total prices
            mainWindow.subTotal.Text = $"{beforeTax:C0}";
            mainWindow.total.Text = (afterTax  - multiplePaymentAmount).ToString("#,0");
            mainWindow.VATModeText.Text = $"{thisTax.ToString("#,0")}";
            mainWindow.GrandTextBlock.Text = $"{afterTax.ToString("C0")}";
            mainWindow.totalQty.Text = totalQuantity.ToString("0.00");
            mainWindow.GrandTotalCalculator.Text = $"{(afterTax - multiplePaymentAmount).ToString("#,0")}";
            
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
                            childItem.ChildStatus = false;
                            childItem.ChildQuantity = 0;
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
                            Console.WriteLine($"- {childItem.ChildName}, Price: {childItem.ChildPrice:C}, Quantity: {childItem.ChildQuantity}, Status: {(childItem.ChildStatus ? "Active" : "Canceled")}");
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

        public void ResetUI(string transactionStatus)
        {
            // Retrieve necessary variables
            int customerId = mainWindow.OrderID;
            int saleMode = mainWindow.SaleMode;

            // Create a new cart
            Cart cart = new Cart(customerId, cartProducts.Values.ToList(), saleMode, isReset, transactionStatus);

            // Clear the display after processing the input
            ClearDisplay();

            // Clear payment details
            ClearDetails();

            // Reset variables
            ResetVariables();

            // Reset UI elements
            ResetMainUI();
        }

        private void ClearDisplay()
        {
            mainWindow.total.Text = "0";
            mainWindow.GrandTotalCalculator.Text = "0";
            mainWindow.displayText.Text = "0";
            Calculating();
        }

        private void ClearDetails()
        {
            cartProducts.Clear();
            multiplePaymentList.Clear();
        }

        private void ResetMainUI()
        {
            mainWindow.isNew = true;
            mainWindow.PayementProcess.Visibility = Visibility.Collapsed;
            mainWindow.MainContentProduct.Visibility = Visibility.Visible;
            mainWindow.SaleMode = 0;
            UpdateMultiplePaymentUI();
            UpdateCartUI();
            mainWindow.SaleModeView();
        }

        private void ResetVariables()
        {
            isReset = true;
            CartIndex = 0;
            paymentIndex = 0;
            multiplePaymentAmount = 0;
            isReset = false;
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
                                        id = childItem.ChildId,
                                        price = childItem.ChildPrice,
                                        quantity = childItem.ChildQuantity,
                                        name = childItem.ChildName
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

                    decimal remainingQris = CurrentTransaction.NeedToPay + CurrentTransaction.TaxAmount - multiplePaymentAmount;

                    if (multiplePaymentAmount != 0)
                    {
                        itemDetails.Clear();
                        var itemDetail = new
                        {
                            id = 0101,
                            price = remainingQris,
                            quantity = 1,
                            name = $"MultiplePayment {mainWindow.OrderID}",
                        };
                        itemDetails.Add(itemDetail);
                    }

                    // Define the request body
                    var requestBody = new
                    {
                        payment_type = "qris",
                        transaction_details = new
                        {
                            order_id = orderId,
                            gross_amount = multiplePaymentAmount != 0 ? remainingQris : cartTotal + taxAmount, // Include tax in the total amount
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

            string Message = "Waiting For QRIS Payment";
            NotificationPopup notificationPopup = new NotificationPopup(Message, false, null, false, true);
            notificationPopup.Top = 99;  
            notificationPopup.Show();
            // Disable main window interaction
            mainWindow.IsEnabled = false;

            bool isDone = false;
            while (!isDone) // Continue indefinitely
            {
                try
                {
                    if (notificationPopup.IsConfirmed)
                    {
                        AddPaymentDetail(int.Parse(PayTypeId), PayTypeName, CurrentTransaction.NeedToPay + CurrentTransaction.TaxAmount - multiplePaymentAmount, false);
                        mainWindow.IsEnabled = true;
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
                            decimal remainingQris = (CurrentTransaction.NeedToPay + CurrentTransaction.TaxAmount - multiplePaymentAmount);
                            switch (transactionStatus)
                            {
                                case "settlement":
                                    // Light green background for Settlement status
                                    backgroundBrush = Brushes.LightGreen;
                                    transactionStatus = "Settlement";
                                    AddPaymentDetail(int.Parse(PayTypeId), PayTypeName, remainingQris);
                                    OrderTransactionFunction(2, PayTypeId);
                                    notificationPopup.Close();
                                    mainWindow.IsEnabled = true;
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
                                    AddPaymentDetail(int.Parse(PayTypeId), PayTypeName, remainingQris, false);
                                    OrderTransactionFunction(91, PayTypeId);
                                    notificationPopup.Close();
                                    mainWindow.IsEnabled = true;
                                    mainWindow.QrisDone(transactionStatus, PayTypeId, PayTypeName);
                                    isDone = true;
                                    break;
                                case "expire":
                                    // Light gray background for Expire status
                                    backgroundBrush = Brushes.LightGray;
                                    transactionStatus = "Expire";
                                    AddPaymentDetail(int.Parse(PayTypeId), PayTypeName, remainingQris, false);
                                    OrderTransactionFunction(91, PayTypeId);
                                    notificationPopup.Close();
                                    mainWindow.IsEnabled = true;
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

        public async Task OrderTransactionFunction(int transactionStatus = 2, string PayTypeID = "1")
        {
            decimal totalPrice = 0;
            foreach (KeyValuePair<int, Product> kvp in cartProducts)
            {
                Product product = kvp.Value;
                decimal totalProductPrice = product.ProductPrice * product.Quantity;

                if (product.Status)
                {
                    if (product.ChildItems != null && product.ChildItems.Any())
                    {
                        foreach (ChildItem childItem in product.ChildItems)
                        {
                            totalProductPrice += childItem.ChildPrice * childItem.ChildQuantity;
                        }
                    }

                    decimal taxAmount = totalProductPrice * productVATPercent / 100;
                    totalProductPrice += taxAmount;
                    totalPrice += totalProductPrice;
                }
            }
            OrderTransaction orderTransaction = await CreateOrderTransaction(transactionStatus, PayTypeID);
            TransactionService transactionService = new TransactionService();
            await transactionService.InsertTransaction(orderTransaction);

            int insertOrderNo = 0;
            short DisplayOrdering = 1;
            short TemporaryID = 0;
            int orderDetailID = 0;


            
            foreach (var items in cartProducts)
            {

                TransactionServiceOrderDetail transactionServiceOrderDetail = new TransactionServiceOrderDetail();
                TemporaryID = 0;
                TemporaryID++;
                orderDetailID += TemporaryID;
                insertOrderNo = items.Key;
                int orderStatusID = items.Value.Status ? 2 : 92;
                (OrderDetailStatus ods, ActionDesp ad) = ChecksTransactionStatus(transactionStatus);
                int orderEditID = 0;



                int voidStaffID = (orderStatusID >= 90 && orderStatusID <= 99) ? orderTransaction.OpenStaffID : 0;
                string voidStaff = (orderStatusID >= 90 && orderStatusID <= 99) ? orderTransaction.OpenStaff : "";
                DateTime? voidDateTime = (orderStatusID >= 90 && orderStatusID <= 99) ? items.Value.dateTime : (DateTime?)null;
                DateTime? submitOrderDate = (ods != null) ? DateTime.Now : (DateTime?)null;
                string voidManualText = (orderStatusID >= 90 && orderStatusID <= 99) ? ad.ActionDesp_Name : "";
                string voidManualReasonText = (orderStatusID >= 90 && orderStatusID <= 99) ? ad.ActionDesp_Name : "";

                decimal ProductVat = items.Value.ProductPrice * productVATPercent;
                decimal ProductNetSale = items.Value.ProductPrice + ProductVat;
                decimal DiscVat = 0;

                OrderDetail orderDetail = CreateOrderDetail(
                                    orderTransaction,            // orderTransaction: OrderTransaction object
                                    orderDetailID,               // orderDetailID: int
                                    Convert.ToByte(items.Value.ProductComponentLevel), // componentLevel: byte
                                    0,                           // orderDetailLinkID: int
                                    Convert.ToByte(insertOrderNo), // insertOrderNo: short
                                    0,                           // indentLevel: byte
                                    DisplayOrdering,             // displayOrdering: short
                                    items.Value.ProductId,       // productID: int
                                    0,                           // productSetType: int
                                    (short)orderStatusID,        // orderStatusID: short
                                    (short)orderEditID,          // orderEditStatus: short
                                    0,                           // saleType: short
                                    items.Value.Quantity,        // totalQty: int
                                    ProductNetSale,    // pricePerUnit: decimal
                                    ProductNetSale,    // totalRetailPrice: decimal
                                    ProductNetSale,    // orgPricePerUnit: decimal
                                    ProductNetSale,    // orgTotalRetailPrice: decimal
                                    0,                           // discPricePercent: decimal
                                    0,                           // discPrice: decimal
                                    0,                           // discPercent: decimal
                                    0,                           // discAmount: decimal
                                    0,                           // discOtherPercent: decimal
                                    0,                           // discOther: decimal
                                    0,                           // totalItemDisc: decimal
                                    ProductNetSale,    // salePrice: decimal
                                    0,                           // discBill: decimal
                                    0,                           // totalDiscount: decimal
                                    ProductNetSale,                           // netSale: decimal
                                    0,                           // adjFromSaleType: decimal
                                    ProductNetSale,                           // vatable: decimal
                                    productVatCode,                          // productVATCode: string
                                    productVatCode,                          // vatDisplay: string
                                    productVATPercent,           // productVATPercent: decimal
                                    ProductVat,                  // productVAT: decimal
                                    items.Value.ProductPrice,    // productBeforeVAT: decimal
                                    Math.Round(ProductVat, 1),                           // totalRetailVAT: decimal
                                    0,                           // discVAT: decimal
                                    0,                           // isSCBeforeDisc: byte
                                    0,                           // hasServiceCharge: byte
                                    0,                           // scPercent: decimal
                                    0,                           // scAmount: decimal
                                    0,                           // scVAT: decimal
                                    0,                           // scBeforeVAT: decimal
                                    ProductNetSale,                           // wVatable: decimal
                                    0,                           // scWAmount: decimal
                                    0,                           // scWVAT: decimal
                                    0,                           // scWBeforeVAT: decimal
                                    ProductNetSale,                           // weightPrice: decimal
                                    ProductVat,                           // weightPriceVAT: decimal
                                    items.Value.ProductPrice,                           // weightBeforeVAT: decimal
                                    ProductVat,                           // paymentVAT: decimal
                                    "",                          // otherFoodName: string
                                    0,                           // otherProductGroupID: int
                                    0,                           // discountAllow: byte
                                    0,                           // itemDiscAllow: byte
                                    0,                           // alreadyDiscQty: short
                                    0,                           // lastTransactionID: int
                                    0,                           // lastComputerID: int
                                    "",                          // printerID: string
                                    0,                           // inventoryID: int
                                    orderTransaction.OpenStaffID, // orderStaffID: int
                                    orderTransaction.OpenStaff,  // orderStaff: string
                                    orderTransaction.ComputerID, // orderComputerID: int
                                    Properties.Settings.Default._ComputerName,                          // orderComputer: string
                                    0,                           // orderTableID: int
                                    "",                          // orderTable: string
                                    0,                           // voidTypeID: byte
                                    voidStaffID,                 // voidStaffID: int
                                    voidStaff,                   // voidStaff: string
                                    voidDateTime,                // voidDateTime: DateTime?
                                    voidManualText,              // voidManualText: string
                                    voidManualReasonText,        // voidReasonText: string
                                    1,                           // vatType: byte
                                    1,                           // printGroup: byte
                                    0,                           // noPrintBill: int
                                    0,                           // noRePrintOrder: byte
                                    null,                        // startTime: DateTime?
                                    null,                        // finishTime: DateTime?
                                    0,                           // printStatus: byte
                                    null,                        // printOrderDateTime: DateTime?
                                    null,                        // lastPrintOrderDateTime: DateTime?
                                    null,                        // printErrorMsg: string
                                    0,                           // cancelPrintStaffID: int
                                    null,                        // cancelPrintDateTime: DateTime?
                                    null,                        // cancelPrintReason: string
                                    0,                           // processID: int
                                    items.Value.dateTime,                        // insertOrderDateTime: DateTime?
                                    submitOrderDate,                        // submitOrderDateTime: DateTime?
                                    null,                        // modifyOrderDateTime: DateTime?
                                    0,                           // modifyStaffID: int
                                    null,                        // comment: string
                                    0,                           // isComment: byte
                                    0,                           // billCheckID: byte
                                    0,                           // pGroupID: short
                                    0,                           // setGroupNo: short
                                    0,                           // qtyRatio: decimal
                                    0,                           // freeItem: byte
                                    0,                           // summaryID: int
                                    0                            // deleted: byte
                                );
                await transactionServiceOrderDetail.InsertTransaction(orderDetail);

                foreach (var childItems in items.Value.ChildItems)
                {
                    TemporaryID++;
                    orderDetailID ++;

                    decimal ProductVAT = childItems.ChildPrice * productVATPercent;
                    decimal ProductBeforeVAT = childItems.ChildPrice;
                    decimal ProductAfterVAT = ProductBeforeVAT + ProductVAT;

                    OrderDetail orderDetails = CreateOrderDetail(
                                    orderTransaction,            // orderTransaction: OrderTransaction object
                                    orderDetailID,               // orderDetailID: int
                                    1, // componentLevel: byte
                                    Convert.ToByte(insertOrderNo),                           // orderDetailLinkID: int
                                    Convert.ToByte(insertOrderNo), // insertOrderNo: short
                                    1,                           // indentLevel: byte
                                    TemporaryID += DisplayOrdering,             // displayOrdering: short
                                    childItems.ChildId,       // productID: int
                                    0,                           // productSetType: int
                                    (short)orderStatusID,        // orderStatusID: short
                                    (short)orderEditID,          // orderEditStatus: short
                                    0,                           // saleType: short
                                    childItems.ChildQuantity,        // totalQty: int
                                    ProductAfterVAT,    // pricePerUnit: decimal
                                    ProductAfterVAT,    // totalRetailPrice: decimal
                                    ProductAfterVAT,    // orgPricePerUnit: decimal
                                    ProductAfterVAT,    // orgTotalRetailPrice: decimal
                                    0,                           // discPricePercent: decimal
                                    0,                           // discPrice: decimal
                                    0,                           // discPercent: decimal
                                    0,                           // discAmount: decimal
                                    0,                           // discOtherPercent: decimal
                                    0,                           // discOther: decimal
                                    0,                           // totalItemDisc: decimal
                                    childItems.ChildPrice,    // salePrice: decimal
                                    0,                           // discBill: decimal
                                    0,                           // totalDiscount: decimal
                                    0,                           // netSale: decimal
                                    0,                           // adjFromSaleType: decimal
                                    0,                           // vatable: decimal
                                    productVatCode,                          // productVATCode: string
                                    productVatCode,                          // vatDisplay: string
                                    productVATPercent,           // productVATPercent: decimal
                                    ProductVAT,                  // productVAT: decimal
                                    ProductBeforeVAT,    // productBeforeVAT: decimal
                                    Math.Round(ProductVAT, 1),                           // totalRetailVAT: decimal
                                    0,                           // discVAT: decimal
                                    0,                           // isSCBeforeDisc: byte
                                    0,                           // hasServiceCharge: byte
                                    0,                           // scPercent: decimal
                                    0,                           // scAmount: decimal
                                    0,                           // scVAT: decimal
                                    0,                           // scBeforeVAT: decimal
                                    0,                           // wVatable: decimal
                                    0,                           // scWAmount: decimal
                                    0,                           // scWVAT: decimal
                                    0,                           // scWBeforeVAT: decimal
                                    ProductAfterVAT,                           // weightPrice: decimal
                                    ProductVAT,                           // weightPriceVAT: decimal
                                    ProductBeforeVAT,                           // weightBeforeVAT: decimal
                                    ProductVAT,                           // paymentVAT: decimal
                                    "",                          // otherFoodName: string
                                    0,                           // otherProductGroupID: int
                                    0,                           // discountAllow: byte
                                    0,                           // itemDiscAllow: byte
                                    0,                           // alreadyDiscQty: short
                                    0,                           // lastTransactionID: int
                                    0,                           // lastComputerID: int
                                    "",                          // printerID: string
                                    0,                           // inventoryID: int
                                    orderTransaction.OpenStaffID, // orderStaffID: int
                                    orderTransaction.OpenStaff,  // orderStaff: string
                                    orderTransaction.ComputerID, // orderComputerID: int
                                    Properties.Settings.Default._ComputerName,                          // orderComputer: string
                                    0,                           // orderTableID: int
                                    "",                          // orderTable: string
                                    0,                           // voidTypeID: byte
                                    voidStaffID,                 // voidStaffID: int
                                    voidStaff,                   // voidStaff: string
                                    voidDateTime,                // voidDateTime: DateTime?
                                    voidManualText,              // voidManualText: string
                                    voidManualReasonText,        // voidReasonText: string
                                    1,                           // vatType: byte
                                    1,                           // printGroup: byte
                                    0,                           // noPrintBill: int
                                    0,                           // noRePrintOrder: byte
                                    null,                        // startTime: DateTime?
                                    null,                        // finishTime: DateTime?
                                    0,                           // printStatus: byte
                                    null,                        // printOrderDateTime: DateTime?
                                    null,                        // lastPrintOrderDateTime: DateTime?
                                    null,                        // printErrorMsg: string
                                    0,                           // cancelPrintStaffID: int
                                    null,                        // cancelPrintDateTime: DateTime?
                                    null,                        // cancelPrintReason: string
                                    0,                           // processID: int
                                    childItems.dateTime,                        // insertOrderDateTime: DateTime?
                                    submitOrderDate,                        // submitOrderDateTime: DateTime?
                                    null,                        // modifyOrderDateTime: DateTime?
                                    0,                           // modifyStaffID: int
                                    null,                        // comment: string
                                    0,                           // isComment: byte
                                    0,                           // billCheckID: byte
                                    (short)childItems.ChildPGroupID,                           // pGroupID: short
                                    (short)childItems.ChildSetGroupNo,                           // setGroupNo: short
                                    0,                           // qtyRatio: decimal
                                    0,                           // freeItem: byte
                                    0,                           // summaryID: int
                                    0                            // deleted: byte
                                );
                    await transactionServiceOrderDetail.InsertTransaction(orderDetails);
                }
                
                DisplayOrdering += 100;
            }


            TransactionServicePayDetail transactionServicePayDetail = new TransactionServicePayDetail();
            int payDetailID = 0;
            foreach (var items in multiplePaymentList.Values)
            {
                if (items.PaymentIsAcitve)
                {
                    decimal TotalChange = 0;
                    if (totalPrice < items.PaymentAmount)
                    {
                        TotalChange = items.PaymentAmount - totalPrice;
                    }
                    totalPrice -= items.PaymentAmount;
                    payDetailID++;
                    OrderPayDetail orderPayDetail = CreateOrderPayDetail(orderTransaction, payDetailID, items.PaymentTypeID.ToString(), totalPrice, items.PaymentAmount, TotalChange, items.PaymentPayRemarks);
                    await transactionServicePayDetail.InsertTransaction(orderPayDetail);
                }
            }
        }

        public async Task<OrderTransaction> CreateOrderTransaction(int transactionStatus, string PayTypeID)
        {
            OrderTransaction orderTransaction = new OrderTransaction();

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
                decimal totalProductPrice = product.ProductPrice * product.Quantity;
                totalQuantity += product.Quantity;

                if (product.Status)
                {
                    if (product.ChildItems != null && product.ChildItems.Any())
                    {
                        foreach (ChildItem childItem in product.ChildItems)
                        {
                            totalProductPrice += childItem.ChildPrice * childItem.ChildQuantity;
                        }
                    }

                    decimal taxAmount = totalProductPrice * productVATPercent / 100;
                    totalProductPrice += taxAmount;
                    totalPrice += totalProductPrice;
                }
            }

            (OrderDetailStatus ods, ActionDesp ad) = ChecksTransactionStatus(transactionStatus);

            decimal TotalPay = CurrentTransaction.NeedToPay + CurrentTransaction.NeedToPay;
            decimal TotalChange = TotalPay - totalPrice;
            string CurrencyCode = "Rp";
            string ReferenceNo = OrderTransaction.NumberGenerator.GenerateNumber(Properties.Settings.Default._ComputerID.ToString(), generatedTransactionID);
            string LogoIMG = Properties.Settings.Default._PrinterLogo;
            string key = $"{orderTransaction.TransactionID}:{ComputerID}";
            string TransactionName = ad.ActionDesp_ID == 9 ? ad.ActionDesp_Name : "";
            short customerID = (short)mainWindow.OrderID;
            short TransactionStatus = (short)transactionStatus;
            string receiptNumber = ad != null ? MainWindow.GenerateReceiptNumber(mainWindow.OrderID.ToString()) : "";
            decimal DisctountItem = 0;
            decimal DisctountBill = 0;
            decimal DisctountOther = 0;
            decimal TotalDiscount = 0;
            decimal TransactionVATable = totalPrice + (totalPrice * productVATPercent / 100);
            decimal TransactionVat = totalPrice * productVATPercent / 100;

            // Populate order transaction object
            orderTransaction.TransactionID = generatedTransactionID;
            orderTransaction.ComputerID = ComputerID;
            orderTransaction.TransactionUUID = orderTransaction.GenerateUUID();
            orderTransaction.TranKey = key;
            orderTransaction.ReserveTime = null;
            orderTransaction.ReserveStaffID = 0;
            orderTransaction.OpenTime = now;
            orderTransaction.OpenStaffID = UserSessions.Current_StaffID;
            orderTransaction.OpenStaff = $"{UserSessions.Current_StaffFirstName} {UserSessions.Current_StaffLastName}";
            orderTransaction.PaidTime = now;
            orderTransaction.PaidStaffID = ad.ActionDesp_ID == 9 ? 0 : UserSessions.Current_StaffID;
            orderTransaction.PaidStaff = ad.ActionDesp_ID == 9 ? "" : $"{UserSessions.Current_StaffFirstName} {UserSessions.Current_StaffLastName}";
            orderTransaction.PaidComputerID = ad.ActionDesp_ID == 9 ? 0 : ComputerID;
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
            orderTransaction.TransactionName = TransactionName;
            orderTransaction.QueueName = string.Empty;
            orderTransaction.NoCustomer = customerID;
            orderTransaction.NoCustomerWhenOpen = 0;
            orderTransaction.DocType = 8;
            orderTransaction.ReceiptYear = now.Year;
            orderTransaction.ReceiptMonth = now.Month;
            orderTransaction.ReceiptDay = now.Day;
            orderTransaction.ReceiptID = customerID;
            orderTransaction.ReceiptNumber = receiptNumber;
            orderTransaction.SaleDate = now;
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

            return orderTransaction;
        }


        public OrderPayDetail CreateOrderPayDetail(OrderTransaction orderTransaction,int payDetailID, string PayTypeID, decimal totalPrice, decimal TotalPay, decimal TotalChange, string payRemark)
        {
            OrderPayDetail orderPayDetail = new OrderPayDetail();

            DateTime now = DateTime.Now;
            int ComputerID = Properties.Settings.Default._ComputerID;
            int ShopID = int.Parse(Properties.Settings.Default._AppID);

            string CurrencyCode = "Rp";
            string LogoIMG = Properties.Settings.Default._PrinterLogo;
            string key = $"{orderTransaction.TransactionID}:{ComputerID}";

            // Populate order pay detail object
            orderPayDetail.PayDetailID = payDetailID;
            orderPayDetail.TransactionID = orderTransaction.TransactionID;
            orderPayDetail.ComputerID = ComputerID;
            orderPayDetail.TranKey = key;
            orderPayDetail.PayTypeID = int.Parse(PayTypeID);
            orderPayDetail.PayAmount = TotalPay;
            orderPayDetail.CashChange = TotalChange;
            orderPayDetail.CashChangeMainCurrency = TotalChange;
            orderPayDetail.CashChangeMainCurrencyCode = CurrencyCode;
            orderPayDetail.CashChangeCurrencyAmount = TotalChange;
            orderPayDetail.CashChangeCurrencyCode = CurrencyCode;
            orderPayDetail.CashChangeCurrencyName = "IDN - Rupiah";
            orderPayDetail.CashChangeCurrencyRatio = 1; // Adjust as necessary
            orderPayDetail.CashChangeExchangeRate = 1; // Adjust as necessary
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
            orderPayDetail.CurrencyRatio = 1; // Adjust as necessary
            orderPayDetail.ExchangeRate = 1; // Adjust as necessary
            orderPayDetail.CurrencyAmount = TotalPay;
            orderPayDetail.ShopID = ShopID;
            orderPayDetail.SaleDate = now;
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
            return orderPayDetail;
        }

        public OrderDetail CreateOrderDetail(
        OrderTransaction orderTransaction,
        int orderDetailID = 0,
        byte componentLevel = 0,
        int orderDetailLinkID = 0,
        short insertOrderNo = 0,
        byte indentLevel = 0,
        short displayOrdering = 0,
        int productID = 0,
        int productSetType = 0,
        short orderStatusID = 2,
        short orderEditStatus = 0,
        short saleType = 0,
        int totalQty = 0,
        decimal pricePerUnit = 0.0000M,
        decimal totalRetailPrice = 0.0000M,
        decimal orgPricePerUnit = 0.0000M,
        decimal orgTotalRetailPrice = 0.0000M,
        decimal discPricePercent = 0.0000M,
        decimal discPrice = 0.0000M,
        decimal discPercent = 0.0000M,
        decimal discAmount = 0.0000M,
        decimal discOtherPercent = 0.0000M,
        decimal discOther = 0.0000M,
        decimal totalItemDisc = 0.0000M,
        decimal salePrice = 0.0000M,
        decimal discBill = 0.0000M,
        decimal totalDiscount = 0.0000M,
        decimal netSale = 0.0000M,
        decimal adjFromSaleType = 0.0000M,
        decimal vatable = 0.0000M,
        string productVATCode = "",
        string vatDisplay = "",
        decimal productVATPercent = 0.0000M,
        decimal productVAT = 0.0000M,
        decimal productBeforeVAT = 0.0000M,
        decimal totalRetailVAT = 0.0000M,
        decimal discVAT = 0.0000M,
        byte isSCBeforeDisc = 0,
        byte hasServiceCharge = 0,
        decimal scPercent = 0.0000M,
        decimal scAmount = 0.0000M,
        decimal scVAT = 0.0000M,
        decimal scBeforeVAT = 0.0000M,
        decimal wVatable = 0.0000M,
        decimal scWAmount = 0.0000M,
        decimal scWVAT = 0.0000M,
        decimal scWBeforeVAT = 0.0000M,
        decimal weightPrice = 0.0000M,
        decimal weightPriceVAT = 0.0000M,
        decimal weightBeforeVAT = 0.0000M,
        decimal paymentVAT = 0.0000M,
        string otherFoodName = "",
        int otherProductGroupID = 0,
        byte discountAllow = 0,
        byte itemDiscAllow = 0,
        short alreadyDiscQty = 0,
        int lastTransactionID = 0,
        int lastComputerID = 0,
        string printerID = "",
        int inventoryID = 0,
        int orderStaffID = 0,
        string orderStaff = "",
        int orderComputerID = 0,
        string orderComputer = "",
        int orderTableID = 0,
        string orderTable = "",
        byte voidTypeID = 0,
        int voidStaffID = 0,
        string voidStaff = "",
        DateTime? voidDateTime = null,
        string voidManualText = null,
        string voidReasonText = null,
        byte vatType = 1,
        byte printGroup = 0,
        int noPrintBill = 0,
        byte noRePrintOrder = 0,
        DateTime? startTime = null,
        DateTime? finishTime = null,
        byte printStatus = 0,
        DateTime? printOrderDateTime = null,
        DateTime? lastPrintOrderDateTime = null,
        string printErrorMsg = null,
        int cancelPrintStaffID = 0,
        DateTime? cancelPrintDateTime = null,
        string cancelPrintReason = null,
        int processID = 0,
        DateTime? insertOrderDateTime = null,
        DateTime? submitOrderDateTime = null,
        DateTime? modifyOrderDateTime = null,
        int modifyStaffID = 0,
        string comment = null,
        byte isComment = 0,
        byte billCheckID = 0,
        short pGroupID = 0,
        short setGroupNo = 0,
        decimal qtyRatio = 0.00M,
        byte freeItem = 0,
        int summaryID = 0,
        byte deleted = 0
)
        {
            OrderDetail orderDetail = new OrderDetail();
            DateTime dateTime = DateTime.Now;

            orderDetail.OrderDetailID = orderDetailID;
            orderDetail.TransactionID = orderTransaction.TransactionID;
            orderDetail.ComputerID = orderTransaction.ComputerID;
            orderDetail.TranKey = orderTransaction.TranKey;
            orderDetail.OrgOrderDetailID = 0;
            orderDetail.OrgTransactionID = 0;
            orderDetail.OrgComputerID = 0;
            orderDetail.OrgTranKey = null;
            orderDetail.ComponentLevel = componentLevel;
            orderDetail.OrderDetailLinkID = orderDetailLinkID;
            orderDetail.InsertOrderNo = insertOrderNo;
            orderDetail.IndentLevel = indentLevel;
            orderDetail.DisplayOrdering = displayOrdering;
            orderDetail.SaleDate = orderTransaction.SaleDate;
            orderDetail.ShopID = orderTransaction.ShopID;
            orderDetail.ProductID = productID;
            orderDetail.ProductSetType = productSetType;
            orderDetail.OrderStatusID = orderStatusID;
            orderDetail.OrderEditStatus = orderEditStatus;
            orderDetail.SaleMode = orderTransaction.SaleMode;
            orderDetail.SaleType = saleType;
            orderDetail.TotalQty = totalQty;
            orderDetail.PricePerUnit = pricePerUnit;
            orderDetail.TotalRetailPrice = totalRetailPrice;
            orderDetail.OrgPricePerUnit = orgPricePerUnit;
            orderDetail.OrgTotalRetailPrice = orgTotalRetailPrice;
            orderDetail.DiscPricePercent = discPricePercent;
            orderDetail.DiscPrice = discPrice;
            orderDetail.DiscPercent = discPercent;
            orderDetail.DiscAmount = discAmount;
            orderDetail.DiscOtherPercent = discOtherPercent;
            orderDetail.DiscOther = discOther;
            orderDetail.TotalItemDisc = totalItemDisc;
            orderDetail.SalePrice = salePrice;
            orderDetail.DiscBill = discBill;
            orderDetail.TotalDiscount = totalDiscount;
            orderDetail.NetSale = netSale;
            orderDetail.AdjFromSaleType = adjFromSaleType;
            orderDetail.Vatable = vatable;
            orderDetail.ProductVATCode = productVATCode;
            orderDetail.VATDisplay = vatDisplay;
            orderDetail.ProductVATPercent = productVATPercent;
            orderDetail.ProductVAT = productVAT;
            orderDetail.ProductBeforeVAT = productBeforeVAT;
            orderDetail.TotalRetailVAT = totalRetailVAT;
            orderDetail.DiscVAT = discVAT;
            orderDetail.IsSCBeforeDisc = isSCBeforeDisc;
            orderDetail.HasServiceCharge = hasServiceCharge;
            orderDetail.SCPercent = scPercent;
            orderDetail.SCAmount = scAmount;
            orderDetail.SCVAT = scVAT;
            orderDetail.SCBeforeVAT = scBeforeVAT;
            orderDetail.WVatable = wVatable;
            orderDetail.SCWAmount = scWAmount;
            orderDetail.SCWVAT = scWVAT;
            orderDetail.SCWBeforeVAT = scWBeforeVAT;
            orderDetail.WeightPrice = weightPrice;
            orderDetail.WeightPriceVAT = weightPriceVAT;
            orderDetail.WeightBeforeVAT = weightBeforeVAT;
            orderDetail.PaymentVAT = paymentVAT;
            orderDetail.OtherFoodName = otherFoodName;
            orderDetail.OtherProductGroupID = otherProductGroupID;
            orderDetail.DiscountAllow = discountAllow;
            orderDetail.ItemDiscAllow = itemDiscAllow;
            orderDetail.AlreadyDiscQty = alreadyDiscQty;
            orderDetail.LastTransactionID = orderTransaction.TransactionID;
            orderDetail.LastComputerID = orderTransaction.ComputerID;
            orderDetail.PrinterID = printerID;
            orderDetail.InventoryID = inventoryID;
            orderDetail.OrderStaffID = orderTransaction.PaidStaffID;
            orderDetail.OrderStaff = orderTransaction.PaidStaff;
            orderDetail.OrderComputerID = orderTransaction.ComputerID;
            orderDetail.OrderComputer = Properties.Settings.Default._ComputerName;
            orderDetail.OrderTableID = orderTableID;
            orderDetail.OrderTable = orderTable;
            orderDetail.VoidTypeID = voidTypeID;
            orderDetail.VoidStaffID = voidStaffID;
            orderDetail.VoidStaff = voidStaff;
            orderDetail.VoidDateTime = voidDateTime;
            orderDetail.VoidManualText = voidManualText;
            orderDetail.VoidReasonText = voidReasonText;
            orderDetail.VATType = vatType;
            orderDetail.PrintGroup = printGroup;
            orderDetail.NoPrintBill = noPrintBill;
            orderDetail.NoRePrintOrder = noRePrintOrder;
            orderDetail.StartTime = startTime;
            orderDetail.FinishTime = finishTime;
            orderDetail.PrintStatus = printStatus;
            orderDetail.PrintOrderDateTime = printOrderDateTime;
            orderDetail.LastPrintOrderDateTime = lastPrintOrderDateTime;
            orderDetail.PrintErrorMsg = printErrorMsg;
            orderDetail.CancelPrintStaffID = cancelPrintStaffID;
            orderDetail.CancelPrintDateTime = cancelPrintDateTime;
            orderDetail.CancelPrintReason = cancelPrintReason;
            orderDetail.ProcessID = processID;
            orderDetail.InsertOrderDateTime = insertOrderDateTime;
            orderDetail.SubmitOrderDateTime =  submitOrderDateTime;
            orderDetail.ModifyOrderDateTime = modifyOrderDateTime;
            orderDetail.ModifyStaffID = modifyStaffID;
            orderDetail.Comment = comment;
            orderDetail.IsComment = isComment;
            orderDetail.BillCheckID = billCheckID;
            orderDetail.PGroupID = pGroupID;
            orderDetail.SetGroupNo = setGroupNo;
            orderDetail.QtyRatio = qtyRatio;
            orderDetail.FreeItem = freeItem;
            orderDetail.SummaryID = summaryID;
            orderDetail.Deleted = deleted;

            return orderDetail;
        }
        public void AddPaymentDetail(int payTypeID, string paymentTypeName, decimal paymentAmount, bool paymentIsActive = true, string paymentPayRemarks = "")
        {
            // Create a PaymentDetails object
            PaymentDetails paymentDetail = new PaymentDetails(
                paymentTypeID: payTypeID,
                paymentTypeName: paymentTypeName,
                paymentAmount: paymentAmount,
                paymentIsActive: paymentIsActive,
                paymentPayRemarks: paymentPayRemarks
            );

            // Calculate the total amount
            multiplePaymentAmount += paymentAmount;

            // Insert the PaymentDetails object into the dictionary
            paymentIndex++;
            multiplePaymentList[paymentIndex] = paymentDetail;

            UpdateMultiplePaymentUI();
            UpdateCartUI();
        }

        public (OrderDetailStatus ods, ActionDesp ad) ChecksTransactionStatus(int TransactionStatusID)
        {
            OrderDetailStatus ods = OrderDetailStatus.ODS
                .Find(F => F.OrderDetailStatus_ID == TransactionStatusID);
            ActionDesp ad = ActionDesp.AD
                .Find(FX => FX.ActionDesp_ID == TransactionStatusID);

            return (ods, ad);
        }

    }
}
