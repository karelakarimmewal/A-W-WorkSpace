using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TranQuik.Model;

namespace TranQuik.Pages
{
    public partial class ProductComponent : Window
    {
        public bool IsConfirmed { get; private set; } = false;

        private ModelProcessing modelProcessing;
        private LocalDbConnector localDbConnector;
        private MainWindow mainWindow;

        private List<Button> productComponentGroups = new List<Button>();
        private List<Button> productComponents = new List<Button>();
        private Dictionary<int, int> groupQuantities = new Dictionary<int, int>();
        private Dictionary<int, List<ProductComponentSelectedItems>> selectedItemsByProductComponentGroups = new Dictionary<int, List<ProductComponentSelectedItems>>();

        private int ProductIDSelected;
        private int ProductComponentGroupSelected;
        private int ProductComponenetGroupNoSelected;
        private int CartIndex;

        private int ProductComponentBtn;

        private int ProductComponentGroupButtonTot = 4;
        private int ProductComponentGroupButtonShiftAmount = 4;
        private int ProductComponentGroupButtonStartIndex = 0;
        private int ProductComponentGroupButtonEndtIndex = 0;

        private int ProductComponentButtonTot = 15;
        private int ProductComponentButtonShiftAmount = 15;
        private int ProductComponentButtonStartIndex = 0;
        private int ProductComponentButtonEndtIndex = 0;

        private int setGroupNos;
        private bool reOpen;

        private int saleMode;

        public ProductComponent(ModelProcessing modelProcessing, Product product, MainWindow mainWindow, int SaleMode, int cartIndex, bool reOpen)
        {
            InitializeComponent();
            this.modelProcessing = modelProcessing;
            this.mainWindow = mainWindow;
            this.localDbConnector = new LocalDbConnector();
            this.saleMode = SaleMode;
            this.ProductIDSelected = product.ProductId;
            this.CartIndex = cartIndex;
            this.reOpen = reOpen;
            clear();

            ProductComponentGroup();
            ProductComponentGroup minNonZeroSetGroup = modelProcessing.componentGroups
            .Where(cg => cg.SetGroupNo != 0)
            .OrderBy(cg => cg.SetGroupNo)
            .FirstOrDefault();

            // Check if a valid group was found
            if (minNonZeroSetGroup != null)
            {
                groupQuantities.Clear();
                foreach (var group in modelProcessing.componentGroups)
                {
                    groupQuantities[group.SetGroupNo] = 0;
                }
                int pGroupID = minNonZeroSetGroup.PGroupID;
                int RequiredAmount = minNonZeroSetGroup.RequireAddAmountForProduct;
                int MinAmount = minNonZeroSetGroup.MinQty;
                int MaxAmount = minNonZeroSetGroup.MaxQty;
                ProductComponenetGroupNoSelected = minNonZeroSetGroup.SetGroupNo;
                setGroupNos = minNonZeroSetGroup.SetGroupNo;
                // Call LoadProductComponent with the found PGroupID
                LoadProductComponent(pGroupID, ProductComponenetGroupNoSelected, RequiredAmount, MinAmount, MaxAmount);
            }
            CartIndex = cartIndex;
        }

        private void clear()
        {
            productComponentGroups.Clear();
            productComponents.Clear();
            Product selectedProduct = modelProcessing.cartProducts.Values.FirstOrDefault(p => p.ProductId == ProductIDSelected);
            quantityDisplay.Text = selectedProduct.Quantity.ToString();
        }

        private void ProductComponentGroup()
        {
            ProductComponentGroupButtonStartIndex = 0;
            ProductComponentGroupButtonGrid.Children.Clear();

            // Define the total number of buttons required
            int totalButtonCount = ProductComponentGroupButtonTot;
            // Create buttons for each component group
            for (int i = 0; i < modelProcessing.componentGroups.Count; i++)
            {
                ProductComponentGroup group = modelProcessing.componentGroups[i];
                if (group.SetGroupNo == 0)
                {
                    continue;
                }

                TextBlock textBlock = new TextBlock
                {
                    Text = group.SetGroupName,
                    TextWrapping = TextWrapping.Wrap,
                    TextAlignment = TextAlignment.Center, // Center-align the text
                    VerticalAlignment = VerticalAlignment.Center, // Center-align vertically
                    HorizontalAlignment = HorizontalAlignment.Center, // Center-align horizontally
                    Foreground = (Brush)Application.Current.FindResource("PrimaryBackgroundColor"),
                    FontSize = 14,
                };

                // Create a new TextBlock for displaying additional information
                TextBlock additionalInfoTextBlock = new TextBlock
                {
                    Text = $"Req: {group.RequireAddAmountForProduct} ; Min: {group.MinQty} ; Max: {group.MaxQty}",
                    TextWrapping = TextWrapping.Wrap,
                    TextAlignment = TextAlignment.Center, // Center-align the text
                    VerticalAlignment = VerticalAlignment.Center, // Center-align vertically
                    HorizontalAlignment = HorizontalAlignment.Center, // Center-align horizontally
                    Foreground = (Brush)Application.Current.FindResource("PrimaryBackgroundColor"),
                    FontSize = 10, // Adjust the font size as needed
                };

                // Combine the text blocks to create the content of the button
                StackPanel buttonContent = new StackPanel();
                buttonContent.Children.Add(textBlock);
                buttonContent.Children.Add(additionalInfoTextBlock);

                Button ProductComponentGroupButtons = new Button
                {
                    Content = buttonContent, // Use the combined content
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(1),
                    BorderThickness = new Thickness(2),
                    Tag = group, // Use the PGroupID as the tag
                    Background = (Brush)Application.Current.FindResource("PrimaryButtonColor"),
                    Effect = (System.Windows.Media.Effects.Effect)Application.Current.FindResource("DropShadowEffect"),
                    Style = (System.Windows.Style)Application.Current.FindResource("ButtonStyle"),
                };

                ProductComponentGroupButtons.Click += ProductComponentGroupButtonsClicked;

                // Calculate the row and column index for the grid
                int row = i / 4;
                int column = i % 4;
                Grid.SetRow(ProductComponentGroupButtons, row);
                Grid.SetColumn(ProductComponentGroupButtons, column);

                ProductComponentGroupButtonGrid.Children.Add(ProductComponentGroupButtons);
                productComponentGroups.Add(ProductComponentGroupButtons);
            }
            UpdateVisibleProductComponentGroupButtons();
        }

        private void ProductComponentGroupButtonsClicked(object sender, RoutedEventArgs e)
        {
            // Clear the existing product buttons before loading products from the new group
            productComponents.Clear();
            ProductComponentButtonGrid.Children.Clear();

            // Handle button click event
            Button clickedButton = (Button)sender;
            ProductComponentGroup group = (ProductComponentGroup)clickedButton.Tag;
            int PgGroupID = group.PGroupID;
            int RequiredAmount = group.RequireAddAmountForProduct;
            int MinAmount = group.MinQty;
            int MaxAmount = group.MaxQty;
            int setGroupNo = group.SetGroupNo;
            ProductComponentGroupSelected = PgGroupID;

            setGroupNos = setGroupNo;

            // Load products for the clicked group
            LoadProductComponent(PgGroupID, setGroupNo, RequiredAmount, MinAmount, MaxAmount);

            // Check if there are any existing selected items for the clicked Product Group ID
            if (selectedItemsByProductComponentGroups.ContainsKey(PgGroupID))
            {
                List<ProductComponentSelectedItems> selectedItems = selectedItemsByProductComponentGroups[PgGroupID];

                // Update the visual state of the buttons to reflect the selected items
                foreach (var child in ProductComponentButtonGrid.Children)
                {
                    if (child is Button button)
                    {
                        Product product = button.Tag as Product;
                        if (product != null)
                        {
                            var selectedItem = selectedItems.FirstOrDefault(item =>
                                item.Name == product.ProductName &&
                                item.Price == product.ProductPrice);

                            if (selectedItem != null)
                            {
                                // Update the button content to reflect the selected item quantity
                                UpdateButtonVisualState(button, true, selectedItem.Quantity, false, PgGroupID, RequiredAmount, MaxAmount, modelProcessing.cartProducts.Values.FirstOrDefault(item => item.ProductId == ProductIDSelected)?.Quantity ?? 0);

                            }
                        }
                    }
                }
            }
        }

        public void LoadProductComponent(int pGroupID, int SetGroupNo, int RequiredAmount, int MinAmount, int MaxAmount)
        {

            // Clear existing product buttons
            ProductComponenetGroupNoSelected = SetGroupNo;
            List<ProductComponentProduct> productComponentProducts = new List<ProductComponentProduct>();

            ProductComponentButtonGrid.Children.Clear();
            ProductComponentButtonStartIndex = 0;

            string query = @"
                            SELECT PC.PGroupID, PC.ProductID, P.ProductID as ChildProductID, P.ProductName, P.ProductName2, PP.ProductPrice
                            FROM ProductComponent PC
                            JOIN Products P ON PC.MaterialID = P.ProductID
                            JOIN ProductPrice PP ON P.ProductID = PP.ProductID
                            WHERE PC.SaleMode = @SaleMode AND PP.SaleMode = @SaleMode AND PC.PGroupID = @PGroupID
                            ORDER BY PC.ProductID ASC";

            int i = 0; // Counter for the number of product buttons created

            using (MySqlConnection connection = localDbConnector.GetMySqlConnection())
            {
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@SaleMode", saleMode);
                command.Parameters.AddWithValue("@PGroupID", pGroupID);

                connection.Open();
                MySqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    ProductComponentProduct productComponentProduct = new ProductComponentProduct();
                    productComponentProduct.ProductComponentProductPGroupID = Convert.ToInt32(reader["PGroupID"]);
                    productComponentProduct.ProductComponentProductID = Convert.ToInt32(reader["ChildProductID"]);
                    productComponentProduct.ProductComponentProductName = reader["ProductName"].ToString();
                    productComponentProduct.ProductComponentProductPrice = Convert.ToDecimal(reader["ProductPrice"]);
                    productComponentProduct.ProductComponentProductSetGroupNo = SetGroupNo;
                    productComponentProduct.ProductComponentProductQuantity++;
                    productComponentProducts.Add(productComponentProduct);

                    string productName = reader["ProductName"].ToString();
                    int productId = Convert.ToInt32(reader["ProductID"]);

                    decimal productPrice = Convert.ToDecimal(reader["ProductPrice"]);

                    // Create product instance
                    Product product = new Product(productId, productName, productPrice, null);

                    // Determine the image path
                    string imgFolderPath = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    string imagePath = System.IO.Path.Combine(imgFolderPath, "Resource/Product", $"{productName}.jpg");

                    // Create the product button
                    Button productButton = CreateComponentProductButton(product, imagePath);
                    productButton.Click += (sender, e) => productComponent_Click(sender, e, productComponentProduct, RequiredAmount, MinAmount, MaxAmount, pGroupID);

                    // Calculate the row and column index for the grid
                    int row = i / 5; // Assuming 4 columns per row
                    int column = i % 5;
                    Grid.SetRow(productButton, row);
                    Grid.SetColumn(productButton, column);

                    // Add the product button to the grid
                    ProductComponentButtonGrid.Children.Add(productButton);
                    productComponents.Add(productButton);
                    i++;
                }
                reader.Close();
                ProductComponentBtn = i;
            }
            Console.WriteLine(ProductComponentBtn);
            UpdateVisibleProductComponentButtons();
        }

        private void productComponent_Click(object sender, RoutedEventArgs e, ProductComponentProduct productComponentProduct, int RequiredAmount, int MinAmount, int MaxAmount, int PGroupID)
        {
            int currentQuantity = 0;
            bool isNext = false;

            int ProductQty = modelProcessing.cartProducts.Values
                                .Where(product => product.ProductId == ProductIDSelected)
                                .Select(product => product.Quantity)
                                .DefaultIfEmpty(0) // Handle the case where no matching product is found
                                .Max();

            // Get the list of selected items for the current product group ID, or create a new list if it doesn't exist
            List<ProductComponentSelectedItems> selectedItems = selectedItemsByProductComponentGroups.ContainsKey(PGroupID) ? selectedItemsByProductComponentGroups[PGroupID] : new List<ProductComponentSelectedItems>();

            // Check if the selected item already exists in the list
            ProductComponentSelectedItems existingItem = selectedItems.FirstOrDefault(item =>
            item.Name == productComponentProduct.ProductComponentProductName &&
            item.Price == productComponentProduct.ProductComponentProductPrice && item.SetGroupNo == productComponentProduct.ProductComponentProductSetGroupNo );

            if (existingItem != null)
            {
                // Increment the quantity of the existing item
                existingItem.Quantity++;
            }
            else if (Math.Max(RequiredAmount, MaxAmount) > 0)
            {
                existingItem = new ProductComponentSelectedItems
                {
                    ID = productComponentProduct.ProductComponentProductID,
                    Name = productComponentProduct.ProductComponentProductName,
                    Price = productComponentProduct.ProductComponentProductPrice,
                    SetGroupNo = productComponentProduct.ProductComponentProductSetGroupNo,
                    Quantity = MinAmount > 0 ? MinAmount : 1 // Initialize with MinQuantity if greater than 0, else 1
                };

                selectedItems.Add(existingItem);
                selectedItemsByProductComponentGroups[PGroupID] = selectedItems;

                mainWindow.childItemsSelected.Add(new ChildItem(
                    productComponentProduct.ProductComponentProductID,
                    productComponentProduct.ProductComponentProductName,
                    productComponentProduct.ProductComponentProductPrice,
                    currentQuantity,
                    true,
                    productComponentProduct.ProductComponentProductSetGroupNo,
                    productComponentProduct.ProductComponentProductPGroupID
                ));
            }

            if (RequiredAmount > 0)
            {
                if (groupQuantities.ContainsKey(setGroupNos))
                {
                    groupQuantities[setGroupNos]++;

                }
                else
                {
                    groupQuantities[setGroupNos]++;
                }
            }

            // Update the selected item quantity if needed
            if (existingItem != null)
            {
                int maxAllowedQuantity = Math.Max(RequiredAmount, MaxAmount);
                if (existingItem.Quantity >= maxAllowedQuantity || groupQuantities[setGroupNos] >= maxAllowedQuantity)
                {
                    foreach (UIElement child in ProductComponentButtonGrid.Children)
                    {
                        child.IsEnabled = false;
                        isNext = true;
                    }
                }
            }

            // Set the current quantity
            currentQuantity = existingItem?.Quantity ?? 0;

            // Check if an item with the same name and price already exists in childItemsSelected

            bool itemExists = mainWindow.childItemsSelected.Any(item =>
            item.ChildName == productComponentProduct.ProductComponentProductName &&
            item.ChildPrice == productComponentProduct.ProductComponentProductPrice);

            if (!itemExists)
            {
                // Add the selected item to the mainWindow's childItemsSelected collection
                mainWindow.childItemsSelected.Add(new ChildItem(
                    productComponentProduct.ProductComponentProductID,
                    productComponentProduct.ProductComponentProductName,
                    productComponentProduct.ProductComponentProductPrice,
                    currentQuantity,
                    true,
                    productComponentProduct.ProductComponentProductSetGroupNo,
                    productComponentProduct.ProductComponentProductPGroupID
                ));
            }
            else
            {
                // Find the existing item
                var existingItems = mainWindow.childItemsSelected.FirstOrDefault(item =>
                    item.ChildName == productComponentProduct.ProductComponentProductName &&
                    item.ChildPrice == productComponentProduct.ProductComponentProductPrice);

                // Add the selected item only if the quantity of the existing item is less than or equal to the quantity of the selected item
                if (existingItems != null && existingItems.ChildQuantity <= productComponentProduct.ProductComponentProductQuantity)
                {
                    existingItems.ChildQuantity = currentQuantity;
                }
            }

            // Update the visual state of the button to reflect selection
            UpdateButtonVisualState(sender as Button, true, currentQuantity, isNext, PGroupID, RequiredAmount, MaxAmount, ProductQty);

            if (isNext)
            {
                NextGroup();
            }
        }

        private bool ValidateSetGroups(List<ProductComponentGroup> componentGroups)
        {
            foreach (var group in groupQuantities)
            {
                Console.WriteLine($"============== START =============");
                Console.WriteLine($"{group.Key} Dengan Selected Item Size: {group.Value}");
                bool groupFound = false;

                foreach (var item in componentGroups)
                {
                    Console.WriteLine($"Checking Item with SetGroupNo: {item.SetGroupNo}");

                    if (item.SetGroupNo == group.Key)
                    {
                        Console.WriteLine($"Match found: {item.SetGroupName}");
                        Console.WriteLine($"Details: RequireAddAmountForProduct = {item.RequireAddAmountForProduct}, Group Value = {group.Value}");
                        groupFound = true;
                        if (item.RequireAddAmountForProduct != group.Value)
                        {
                            Console.WriteLine("Validation failed: Required amount does not match.");
                            return false; // Validation failed
                        }
                    }
                }

                if (!groupFound)
                {
                    Console.WriteLine($"Validation failed: No matching SetGroupNo found for {group.Key}");
                    return false; // Validation failed
                }
            }
            Console.WriteLine($"============== END =============");

            return true; // All validations passed
        }

        private void UpdateButtonVisualState(Button button, bool isSelected, int quantity, bool isNext, int PGroupID, int RequiredAmount, int MaxAmount, int ProductQty)
        {
            // Update button appearance based on selection state
            if (isSelected)
            {
                button.Background = Brushes.LightSkyBlue;
            }
            else
            {
                button.Background = Brushes.Azure;
            }

            // Get the product from the button's Tag property
            Product product = button.Tag as Product;

            if (product != null)
            {
                // Use the ProductName from the Product object
                string buttonText = product.ProductName;

                // Find the position of "x" in the button content
                int indexOfX = buttonText.LastIndexOf("x");

                if (indexOfX != -1)
                {
                    // If "x" is found, remove the previous quantity and update with the new one
                    buttonText = buttonText.Substring(0, indexOfX + 1) + "" + quantity;
                }
                else
                {
                    // If "x" is not found, append the quantity
                    buttonText += $" x{quantity}";
                }

                // Update the button content
                button.Content = buttonText;

                // Disable the button if the sum quantity of items in PGroupID >= Math.Max(RequiredAmount, MaxAmount)
                int sumQuantity = 0;
                if (selectedItemsByProductComponentGroups.ContainsKey(PGroupID))
                {
                    sumQuantity = selectedItemsByProductComponentGroups[PGroupID].Sum(item => item.Quantity);
                }

                if (sumQuantity >= Math.Max(RequiredAmount, MaxAmount) * ProductQty)
                {
                    foreach (UIElement child in ProductComponentButtonGrid.Children)
                    {
                        child.IsEnabled = false;
                    }
                }
                else
                {
                    button.IsEnabled = true;
                }
            }
        }

        private void NextGroup()
        {

            var Check = modelProcessing.componentGroups
                                .Where(g => g.ProductID == ProductIDSelected && g.SetGroupNo == ProductComponenetGroupNoSelected)
                                .FirstOrDefault();
            if (Check.RequireAddAmountForProduct == groupQuantities[setGroupNos])
            {
                foreach (Button button in ProductComponentGroupButtonGrid.Children)
                {
                    ProductComponentGroup group = button.Tag as ProductComponentGroup;
                    if (group != null && group.SetGroupNo == Check.SetGroupNo)
                    {
                        // Change the background color of the button
                        button.Background = (Brush)Application.Current.FindResource("SuccessColor"); // Change to your desired color
                        break; // No need to continue looping once the button is found
                    }
                }
            }

            var nextGroup = modelProcessing.componentGroups
            .Where(g => g.ProductID == ProductIDSelected && g.SetGroupNo > ProductComponenetGroupNoSelected)
            .FirstOrDefault();

            if (nextGroup != null)
            {
                // Logic to handle the selection of the next group
                setGroupNos = nextGroup.SetGroupNo;
                LoadProductComponent(nextGroup.PGroupID, nextGroup.SetGroupNo, nextGroup.RequireAddAmountForProduct, nextGroup.MinQty, nextGroup.MaxQty);
            }
            else
            {
                Console.WriteLine("No next component group found.");
            }
        }

        private Button CreateComponentProductButton(Product product, string imagePath)
        {
            bool allowImage = bool.Parse(Properties.Settings.Default["_AppAllowImage"].ToString());

            Button productButton = new Button
            {
                FontWeight = FontWeights.Bold,
                Background = (Brush)Application.Current.FindResource("AccentColor"),
                BorderThickness = new Thickness(0.8),
                Tag = product // Assign product instance to Tag property
            };

            // Create a StackPanel to hold the image and text
            StackPanel stackPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            // Add product name text to the stack panel
            TextBlock productNameTextBlock = new TextBlock
            {
                Text = product.ProductName,
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center,
                Foreground = Brushes.WhiteSmoke,
                Margin = new Thickness(0, 0, 0, 0)
            };
            stackPanel.Children.Add(productNameTextBlock);

            // Conditionally add price text below the product name if price is greater than zero
            if (product.ProductPrice > 0)
            {
                TextBlock priceTextBlock = new TextBlock
                {
                    Text = $"{product.ProductPrice:N0}", // Format price without decimal places
                    TextWrapping = TextWrapping.Wrap,
                    TextAlignment = TextAlignment.Center,
                    Foreground = Brushes.WhiteSmoke,
                    FontSize = 10, // Adjust the font size as needed
                };
                stackPanel.Children.Add(priceTextBlock);
            }

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

        public void UpdateVisibleProductComponentGroupButtons()
        {
            int col = 4;
            modelProcessing.UpdateVisibleProductButtons(ProductComponentGroupButtonGrid, ProductComponentGroupButtonStartIndex, ProductComponentGroupButtonTot, NextComponentGroup_Button, col);
        }

        public void UpdateVisibleProductComponentButtons()
        {
            int col = 5;
            modelProcessing.UpdateVisibleProductButtons(ProductComponentButtonGrid, ProductComponentButtonStartIndex, ProductComponentButtonShiftAmount, NextComponentButton, col);
        }

        private void quantityDisplay_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (modelProcessing == null)
            {
                Console.WriteLine("Error: modelProcessing is null.");
                return;
            }

            // Retrieve the product details using the productID
            Product selectedProduct = modelProcessing.cartProducts.Values.FirstOrDefault(p => p.ProductId == ProductIDSelected);

            if (selectedProduct != null)
            {
                if (int.TryParse(quantityDisplay.Text, out int newQuantity))
                {
                    if (newQuantity >= 1)
                    {
                        selectedProduct.Quantity = newQuantity;

                        // Update the RequireAddAmountForProduct, MinQty, and MaxQty properties of each ProductComponentGroup
                        foreach (var group in modelProcessing.componentGroups)
                        {
                            group.RequireAddAmountForProduct *= newQuantity;
                            group.MinQty *= newQuantity;
                            group.MaxQty *= newQuantity;
                        }
                    }
                    else
                    {
                        // If the new quantity is less than 1, reset to the last valid quantity
                        quantityDisplay.Text = "1";
                        selectedProduct.Quantity = 1;
                    }
                }
            }
        }

        private void NumberButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                string number = button.Content.ToString();

                // If the textbox is empty, replace the default value with the clicked number
                if (string.IsNullOrEmpty(quantityDisplay.Text))
                {
                    // Validate that the first number is not '0'
                    if (number != "0")
                    {
                        quantityDisplay.Text = number;
                        mainWindow.paxTotal = int.Parse(quantityDisplay.Text);
                    }
                }
                else
                {
                    // Ensure that the input can start with '0'
                    if (number != "0" || (!quantityDisplay.Text.StartsWith("0") && quantityDisplay.Text.Length == 1))
                    {
                        quantityDisplay.Text += number;
                        mainWindow.paxTotal = int.Parse(quantityDisplay.Text);
                    }
                }
            }
        }

        private void BackspaceButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(quantityDisplay.Text))
            {
                quantityDisplay.Text = quantityDisplay.Text.Substring(0, quantityDisplay.Text.Length - 1);
            }
        }

        private void NextComponentGroup_Click(object sender, RoutedEventArgs e)
        {
            if (ProductComponentGroupButtonStartIndex + ProductComponentGroupButtonTot < modelProcessing.componentGroups.Count)
            {
                // Increment productButtonStartIndex by ProductButtonShiftAmount to shift the visible range downwards
                ProductComponentGroupButtonStartIndex = Math.Min(modelProcessing.componentGroups.Count - ProductComponentGroupButtonTot, ProductComponentButtonStartIndex + ProductComponentGroupButtonShiftAmount);
                UpdateVisibleProductComponentGroupButtons();
            }

        }

        private void PrevComponentGroup_Click(object sender, RoutedEventArgs e)
        {
            if (ProductComponentGroupButtonStartIndex > 0)
            {
                // Decrement productButtonStartIndex by ProductButtonShiftAmount to shift the visible range upwards
                ProductComponentGroupButtonStartIndex = Math.Max(0, ProductComponentGroupButtonStartIndex - ProductComponentGroupButtonShiftAmount);
                UpdateVisibleProductComponentGroupButtons();
            }

        }

        private void NextComponent_Click(object sender, RoutedEventArgs e)
        {

            if (ProductComponentButtonStartIndex + ProductComponentButtonTot < ProductComponentBtn)
            {
                // Increment productButtonStartIndex by ProductButtonShiftAmount to shift the visible range downwards
                ProductComponentButtonStartIndex = Math.Min(ProductComponentBtn - ProductComponentButtonTot, ProductComponentButtonStartIndex + ProductComponentButtonShiftAmount);
                UpdateVisibleProductComponentButtons();
            }
        }

        private void PrevComponent_Click(object sender, RoutedEventArgs e)
        {
            if (ProductComponentButtonStartIndex > 0)
            {

                // Decrement productButtonStartIndex by ProductButtonShiftAmount to shift the visible range upwards
                ProductComponentButtonStartIndex = Math.Max(0, ProductComponentButtonStartIndex - ProductComponentButtonShiftAmount);
                UpdateVisibleProductComponentButtons();
            }
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            if (mainWindow.childItemsSelected != null && mainWindow.childItemsSelected.Any())
            {
                if (modelProcessing.cartProducts.ContainsKey(CartIndex))
                {
                    Product selectedProduct = modelProcessing.cartProducts[CartIndex];

                    foreach (var childItem in mainWindow.childItemsSelected)
                    {
                        selectedProduct.ChildItems.Add(childItem);
                    }

                    bool isValid = ValidateSetGroups(modelProcessing.componentGroups);

                    if (!isValid)
                    {
                        string NotificationText = "Need Required Amount, Please Add Quantity ! ! !";
                        NotificationPopup notificationPopup = new NotificationPopup(NotificationText, true);

                        notificationPopup.ShowDialog();

                        return;
                    }

                    IsConfirmed = true; // Mark as confirmed
                    modelProcessing.UpdateCartUI();
                }
                else
                {
                    // Handle the case where product is not found
                    Console.WriteLine("Error: Product with ID {0} not found in cart.", ProductIDSelected);
                }
            }
            else
            {
                bool isValid = ValidateSetGroups(modelProcessing.componentGroups);

                if (!isValid)
                {
                    string NotificationText = "Need Required Amount, Please Add Quantity ! ! !";
                    NotificationPopup notificationPopup = new NotificationPopup(NotificationText, true);

                    notificationPopup.ShowDialog();

                    return;
                }
            }
            isReopenComponent(reOpen);
            IsConfirmed = true; // Mark as confirmed
            modelProcessing.UpdateCartUI();
            this.Close();
        }

        public void isReopenComponent(bool reOpen)
        {
            if (!reOpen)
            {
                mainWindow.childItemsSelected.Clear();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            IsConfirmed = false; // Mark as not confirmed
            mainWindow.childItemsSelected.Clear();
            this.Close();
        }
    }
}