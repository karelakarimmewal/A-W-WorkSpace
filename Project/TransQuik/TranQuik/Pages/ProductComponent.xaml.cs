using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
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

        private int ProductIDSelected;
        private int CurrentProductComponentGroupSelected;
        private int CurrentSetGroupNoSelected;
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

        private string SelectedItemNameFromList = "";

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
            //clear();
            ProductComponentGroup();
            DefineSetGroup();
            CartIndex = cartIndex;
            
        }

        private void DefineSetGroup()
        {
            // Get the first non-zero set group item
            CurrentComponentGroupItem firstNonZeroSetGroupItem = CurrentComponentGroupItem.CPGI
                .Where(cg => cg.CurrentSetGroupNo != 0)
                .OrderBy(cg => cg.CurrentSetGroupNo)
                .FirstOrDefault();

            // Check if a valid group was found
            if (firstNonZeroSetGroupItem != null)
            {
                CurrentPackageComponentValidation.currentPackageComponentValidations.Clear();

                foreach (var group in CurrentComponentGroupItem.CPGI)
                {
                    CurrentPackageComponentValidation currentPackageComponentValidation = new CurrentPackageComponentValidation(group.CurrentSetGroupNo, 0);
                }

                // Extract the necessary information from the first non-zero set group item
                int CurrentPGroupID = firstNonZeroSetGroupItem.CurrentPGroupID;
                int CurrentRequiredAmount = firstNonZeroSetGroupItem.CurrentSetGroupReq;
                int CurrentMinAmount = firstNonZeroSetGroupItem.CurrentSetGroupMinQty;
                int CurrentMaxAmount = firstNonZeroSetGroupItem.CurrentSetGroupMaxQty;
                CurrentSetGroupNoSelected = firstNonZeroSetGroupItem.CurrentSetGroupNo;



                // Call LoadProductComponent with the found PGroupID and other details
                LoadProductComponent(CurrentPGroupID, CurrentSetGroupNoSelected, CurrentRequiredAmount, CurrentMinAmount, CurrentMaxAmount);
            }
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
            for (int i = 0; i < CurrentComponentGroupItem.CPGI.Count; i++)
            {
                CurrentComponentGroupItem group = CurrentComponentGroupItem.CPGI[i];
                if (group.CurrentSetGroupNo == 0)
                {
                    continue;
                }

                TextBlock textBlock = new TextBlock
                {
                    Text = group.CurrentSetGroupName,
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
                    Text = $"Req: {group.CurrentSetGroupReq} ; Min: {group.CurrentSetGroupMinQty} ; Max: {group.CurrentSetGroupMaxQty}",
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
                    Tag = group, // Use the group as the tag
                    Background = (Brush)Application.Current.FindResource("PrimaryButtonColor"),
                    Effect = (System.Windows.Media.Effects.Effect)Application.Current.FindResource("DropShadowEffect"),
                    Style = (System.Windows.Style)Application.Current.FindResource("ButtonStyle"),
                };

                string ItemNeeded = group.CurrentSetGroupReq == 0 ? $"({group.CurrentSetGroupMinQty} - {group.CurrentSetGroupMaxQty}) : 0" : $"{group.CurrentSetGroupReq.ToString()} - 0";

                switch (i)
                {
                    case 1:
                        SetGroupNo1.Text = ItemNeeded;
                        break;
                    case 2:
                        SetGroupNo2.Text = ItemNeeded;
                        break;
                    case 3:
                        SetGroupNo3.Text = ItemNeeded;
                        break;
                    case 4:
                        SetGroupNo4.Text = ItemNeeded;
                        break;
                    default:
                        break;
                }

                ProductComponentGroupButtons.Click += ProductComponentGroupButtonsClicked;

                // Calculate the row and column index for the grid
                int row = i / 4;
                int column = i % 4;

                Grid.SetRow(ProductComponentGroupButtons, row);
                Grid.SetColumn(ProductComponentGroupButtons, column);

                ProductComponentGroupButtonGrid.Children.Add(ProductComponentGroupButtons);
                productComponentGroups.Add(ProductComponentGroupButtons);
            }

            // Clear the remaining SetGroupNo text blocks
            for (int i = CurrentComponentGroupItem.CPGI.Count; i < 4; i++)
            {
                switch (i)
                {
                    case 0:
                        SetGroupNo1.Text = string.Empty;
                        break;
                    case 1:
                        SetGroupNo2.Text = string.Empty;
                        break;
                    case 2:
                        SetGroupNo3.Text = string.Empty;
                        break;
                    case 3:
                        SetGroupNo4.Text = string.Empty;
                        break;
                }
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
            CurrentComponentGroupItem group = (CurrentComponentGroupItem)clickedButton.Tag;

            int CurrentClickedPGroupID = group.CurrentPGroupID;
            int CurrentClickedRequiredAmount = group.CurrentSetGroupReq;
            int CurrentClickedMinAmount = group.CurrentSetGroupMinQty;
            int CurrentClickedMaxAmount = group.CurrentSetGroupMaxQty;
            int CurrentClickedsetGroupNo = group.CurrentSetGroupNo;

            CurrentProductComponentGroupSelected = CurrentClickedPGroupID;
            CurrentSetGroupNoSelected = CurrentClickedsetGroupNo;

            // Load products for the clicked group
            LoadProductComponent(CurrentClickedPGroupID, CurrentClickedsetGroupNo, CurrentClickedRequiredAmount, CurrentClickedMinAmount, CurrentClickedMaxAmount);

           
        }

        public void LoadProductComponent(int pGroupID, int SetGroupNo, int RequiredAmount, int MinAmount, int MaxAmount)
        {

            // Clear existing product buttons
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
                    int ProductComponentProductPGroupID = Convert.ToInt32(reader["PGroupID"]);
                    int ProductComponentProductID = Convert.ToInt32(reader["ChildProductID"]);
                    string ProductComponentProductName = reader["ProductName"].ToString();
                    decimal ProductComponentProductPrice = Convert.ToDecimal(reader["ProductPrice"]);
                    int ProductComponentProductSetGroupNo = SetGroupNo;
                    int ProductComponentProductQuantity = 1;

                    ProductComponentProduct productComponentProduct = new ProductComponentProduct(ProductComponentProductPGroupID, ProductComponentProductID, ProductComponentProductName,
                        ProductComponentProductPrice, ProductComponentProductSetGroupNo, ProductComponentProductQuantity);

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

            CalculateTotalComponent();
            UpdateCartUI();
            UpdateTextBlocks();
            foreach (var items in CurrentComponentGroupItem.CPGI)
            {
                bool isMax = IsMaxQuantity(items.CurrentSetGroupNo);
                UpdateVisible(items.CurrentSetGroupNo, isMax);
            }
        }

        private void productComponent_Click(object sender, RoutedEventArgs e, ProductComponentProduct productComponentProduct, int RequiredAmount, int MinAmount, int MaxAmount, int PGroupID)
        {
            int currentQuantity = 0;
            bool isMaxQuantity = false;
            int GivenQuantity = int.Parse(packageItemQuantity.Text) == 0 ? 1 : int.Parse(packageItemQuantity.Text);

            // Get the current quantity of the selected product
            int ProductQty = modelProcessing.cartProducts.Values
                                .Where(product => product.ProductId == ProductIDSelected)
                                .Select(product => product.Quantity)
                                .DefaultIfEmpty(0) // Handle the case where no matching product is found
                                .Max();

            // Get the list of selected items for the current product group ID, or create a new list if it doesn't exist
            List<SelectedComponentItems> selectedItems = SelectedComponentItems.SCI;

            // Check if the selected item already exists in the list
            SelectedComponentItems existingItem = selectedItems.FirstOrDefault(item =>
                item.CurrentComponentID == productComponentProduct.ProductComponentProductID &&
                item.CurrentComponentName == productComponentProduct.ProductComponentProductName &&
                item.CurrentComponentPrice == productComponentProduct.ProductComponentProductPrice &&
                item.CurrentComponentSetGroupNo == productComponentProduct.ProductComponentProductSetGroupNo);

            if (existingItem != null)
            {
                isMaxQuantity = IsMaxQuantity(existingItem.CurrentComponentSetGroupNo);

                if (!isMaxQuantity)
                {
                    if (GivenQuantity > 1)
                    {
                        existingItem.CurrentComponentQuantity += GivenQuantity;
                    }
                    else
                    {
                        existingItem.CurrentComponentQuantity++;
                    }
                    
                    currentQuantity = existingItem.CurrentComponentQuantity;

                    if (currentQuantity > Math.Max(RequiredAmount, MaxAmount))
                    {
                        existingItem.CurrentComponentQuantity = Math.Max(RequiredAmount, MaxAmount);
                        currentQuantity = Math.Max(RequiredAmount, MaxAmount);
                    }
                }

                else
                {
                    Notification.NotificationLimitReached();
                }
            }
            else if (Math.Max(RequiredAmount, MaxAmount) > 0)
            {
                int quantity = MinAmount > 0 ? MinAmount : GivenQuantity;
                currentQuantity = quantity;

                if (GivenQuantity > Math.Max(RequiredAmount, MaxAmount))
                {
                    int maximumIs = 0;
                    Notification.NotificationMoreThanMaximum();
                    CalculateTotalComponent();

                    CurrentPackageComponentValidation currentPackageComponentValidation = CurrentPackageComponentValidation.currentPackageComponentValidations
                        .Find(v => v.SetGroupNoItemSelected == productComponentProduct.ProductComponentProductSetGroupNo);
                    CurrentComponentGroupItem currentComponentGroupItem = CurrentComponentGroupItem.CPGI
                    .Find(CG => CG.CurrentSetGroupNo == productComponentProduct.ProductComponentProductSetGroupNo);

                    if (currentPackageComponentValidation != null)
                    {
                        maximumIs = Math.Max(currentComponentGroupItem.CurrentSetGroupMaxQty, currentComponentGroupItem.CurrentSetGroupReq) - currentPackageComponentValidation.SetGroupNeedQuantityItemSelected;
                    }
                    else
                    {
                        maximumIs = Math.Max(currentComponentGroupItem.CurrentSetGroupMaxQty, currentComponentGroupItem.CurrentSetGroupReq);
                    }
                    quantity = maximumIs > 0 ? maximumIs : 1;
                    currentQuantity = quantity;
                }

                isMaxQuantity = IsMaxQuantity(productComponentProduct.ProductComponentProductSetGroupNo);

                if (!isMaxQuantity)
                {
                    SelectedComponentItems addNew = new SelectedComponentItems(
                    productComponentProduct.ProductComponentProductSetGroupNo,
                    productComponentProduct.ProductComponentProductID,
                    productComponentProduct.ProductComponentProductName,
                    productComponentProduct.ProductComponentProductPrice,
                    quantity
                    );
                }
                else
                {
                    Notification.NotificationLimitReached();
                }
            }

            CalculateTotalComponent();

            UpdateTextBlocks();
            UpdateCartUI();
            packageItemQuantity.Text = "0";

            isMaxQuantity = IsMaxQuantity(productComponentProduct.ProductComponentProductSetGroupNo);
            if (isMaxQuantity)
            {
                // Find the current group's index
                int currentIndex = CurrentComponentGroupItem.CPGI.FindIndex(g => g.CurrentSetGroupNo == productComponentProduct.ProductComponentProductSetGroupNo);

                // Check if the current index is valid and if there's a next group
                if (currentIndex != -1 && currentIndex < CurrentComponentGroupItem.CPGI.Count - 1)
                {
                    // Get the next group
                    CurrentComponentGroupItem nextGroup = CurrentComponentGroupItem.CPGI[currentIndex + 1];

                    // Perform operations with the next group
                    CurrentSetGroupNoSelected = nextGroup.CurrentSetGroupNo;
                    LoadProductComponent(nextGroup.CurrentPGroupID, nextGroup.CurrentSetGroupNo, nextGroup.CurrentSetGroupReq, nextGroup.CurrentSetGroupMinQty, nextGroup.CurrentSetGroupMaxQty);
                }
                else
                {
                    // Handle the case where there is no next group (e.g., the current group is the last one)
                    Console.WriteLine("No next group available.");
                }
            }

        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            // Create a list to store boolean values indicating if each group is maxed out
            List<bool> isAllMax = new List<bool>();

            // Check each item in the CurrentComponentGroupItem list
            foreach (var item in CurrentComponentGroupItem.CPGI)
            {
                if (item.CurrentSetGroupNo == 0)
                {
                    continue;
                }
                bool isMax = IsMaxQuantity(item.CurrentSetGroupNo);
                if (item.CurrentSetGroupReq != 0)
                {
                    isAllMax.Add(true);
                    continue;
                }
                isAllMax.Add(isMax); // Use Add method instead of add
            }

            if (!isAllMax.Contains(false))
            {
                SelectedItemsAdded();
                SendProductComponentToProcessing();
                CurrentComponentGroupItem.CPGI.Clear();
                SelectedComponentItems.SCI.Clear();
                CurrentPackageComponentValidation.currentPackageComponentValidations.Clear();

                modelProcessing.UpdateCartUI();
                IsConfirmed = true;
                // Close the window
                this.Close();
            }
            else
            {
                Notification.NotificationNeedRequirement();
            }

        }

        private void SendProductComponentToProcessing()
        {
            if (mainWindow.childItemsSelected != null && mainWindow.childItemsSelected.Any())
            {
                if (modelProcessing.cartProducts.ContainsKey(CartIndex))
                {
                    Product selectedProduct = modelProcessing.cartProducts[CartIndex];

                    foreach (var childItem in mainWindow.childItemsSelected)
                    {
                        Console.WriteLine($"ChildItems Selected {childItem.ChildName}");
                        selectedProduct.ChildItems.Add(childItem);
                    }
                    modelProcessing.UpdateCartUI();
                }
                else
                {
                    // Handle the case where product is not found
                    Console.WriteLine("Error: Product with ID {0} not found in cart.", ProductIDSelected);
                }
            }
        }

        private void SelectedItemsAdded()
        {
            foreach (var ProductSelected in SelectedComponentItems.SCI)
            {
                var CurrentPGroupID = ProductComponentProduct.productComponentProducts
                    .Where(xs => xs.ProductComponentProductID == ProductSelected.CurrentComponentID && xs.ProductComponentProductSetGroupNo == ProductSelected.CurrentComponentSetGroupNo)
                    .Select(xs => xs.ProductComponentProductPGroupID)
                    .FirstOrDefault();

                bool itemExists = mainWindow.childItemsSelected.Any(item => 
                           item.ChildId == ProductSelected.CurrentComponentID &&
                           item.ChildName == ProductSelected.CurrentComponentName &&
                           item.ChildPrice == ProductSelected.CurrentComponentPrice);

                if (!itemExists)
                {
                    mainWindow.childItemsSelected.Add(new ChildItem(
                        ProductSelected.CurrentComponentID,
                        ProductSelected.CurrentComponentName,
                        ProductSelected.CurrentComponentPrice,
                        ProductSelected.CurrentComponentQuantity,
                        true,
                        ProductSelected.CurrentComponentSetGroupNo,
                        CurrentPGroupID
                    ));
                }
                else
                {
                    var existingItems = mainWindow.childItemsSelected.FirstOrDefault(item =>
                        item.ChildName == ProductSelected.CurrentComponentName&&
                        item.ChildPrice == ProductSelected.CurrentComponentPrice );
                }
            }
        }

        private void UpdateCartUI()
        {
            try
            {
                // Update the ItemsSource of the ListView
                packageItemSelectedListView.ItemsSource = null;
                packageItemSelectedListView.ItemsSource = SelectedComponentItems.SCI;

                // Scroll to the last item in the ListView if there are items
                if (packageItemSelectedListView.Items.Count > 0)
                {
                    packageItemSelectedListView.ScrollIntoView(packageItemSelectedListView.Items[(packageItemSelectedListView.Items.Count - 1) - 1]);
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions if necessary
                Console.WriteLine($"Error in UpdateCartUI: {ex.Message}");
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            // Get the item associated with the button
            if (sender is Button button && button.Tag is SelectedComponentItems item)
            {
                // Remove the item from the SCI collection
                SelectedComponentItems.SCI.Remove(item);

                // Find and remove the corresponding validation from the CurrentPackageComponentValidation collection
                var validationToRemove = CurrentPackageComponentValidation.currentPackageComponentValidations
                    .FirstOrDefault(v => v.SetGroupNoItemSelected == item.CurrentComponentSetGroupNo);

                if (validationToRemove != null)
                {
                    validationToRemove.SetGroupNeedQuantityItemSelected -= item.CurrentComponentQuantity;
                }

                // Refresh the ListView
                packageItemSelectedListView.ItemsSource = null;
                packageItemSelectedListView.ItemsSource = SelectedComponentItems.SCI;
                
                if (IsMaxQuantity(item.CurrentComponentSetGroupNo))
                {
                    UpdateVisible(item.CurrentComponentSetGroupNo, true);
                }
                else
                {
                    UpdateVisible(item.CurrentComponentSetGroupNo, false);
                }

                UpdateTextBlocks();
            }
        }

        private bool IsMaxQuantity(int setGroupNo)
        {
            var maxGroupValue = CurrentComponentGroupItem.CPGI
                .Where(item => item.CurrentSetGroupNo == setGroupNo)
                .Select(item => Math.Max(item.CurrentSetGroupMaxQty, item.CurrentSetGroupReq))
                .DefaultIfEmpty(0) // This ensures there's a default value if the sequence is empty
                .Max();

            var validationValues = CurrentPackageComponentValidation.currentPackageComponentValidations
                .Where(v => v.SetGroupNoItemSelected == setGroupNo)
                .Select(v => v.SetGroupNeedQuantityItemSelected);

            

            if (!validationValues.Any())
            {
                return false;
            }

            var maxValidationValue = validationValues.Max();

            return maxValidationValue >= maxGroupValue;
        }

        public void CalculateTotalComponent()
        {
            // Group selected items by set group number and sum their quantities
            var groupedItems = SelectedComponentItems.SCI
                .GroupBy(item => item.CurrentComponentSetGroupNo)
                .Select(group => new
                {
                    SetGroupNo = group.Key,
                    TotalQuantity = group.Sum(item => item.CurrentComponentQuantity)
                });

            // Clear existing validations
            CurrentPackageComponentValidation.currentPackageComponentValidations.Clear();

            // Update validations based on grouped items
            foreach (var group in groupedItems)
            {
                CurrentPackageComponentValidation currentPackageComponentValidation = new CurrentPackageComponentValidation(group.SetGroupNo, group.TotalQuantity);
            }

            // Optionally print the current state of validations for debugging
            foreach (var validation in CurrentPackageComponentValidation.currentPackageComponentValidations)
            {
                var maxGroupValue = CurrentComponentGroupItem.CPGI
                    .Where(item => item.CurrentSetGroupNo == validation.SetGroupNoItemSelected)
                    .Select(item => Math.Max(item.CurrentSetGroupMaxQty, item.CurrentSetGroupReq))
                    .Max();

                if (validation.SetGroupNeedQuantityItemSelected == maxGroupValue)
                {
                    UpdateVisible(validation.SetGroupNoItemSelected);
                }
            }
        }

        private void UpdateTextBlocks()
        {
            foreach (var items in CurrentPackageComponentValidation.currentPackageComponentValidations)
            {
                string textBlockName = $"SetGroupNo{items.SetGroupNoItemSelected}";

                CurrentComponentGroupItem currentComponentGroupItem = CurrentComponentGroupItem.CPGI
                    .Find(CG => CG.CurrentSetGroupNo == items.SetGroupNoItemSelected);

                TextBlock targetTextBlock = GetTextBlockByName(textBlockName);
                if (targetTextBlock != null && currentComponentGroupItem != null)
                {
                    targetTextBlock.Text = currentComponentGroupItem.CurrentSetGroupReq == 0 ?
                        $"({currentComponentGroupItem.CurrentSetGroupMinQty} - {currentComponentGroupItem.CurrentSetGroupMaxQty}) : {items.SetGroupNeedQuantityItemSelected}" :
                        $"{currentComponentGroupItem.CurrentSetGroupReq.ToString()} : {items.SetGroupNeedQuantityItemSelected}";
                }
            }
        }

        private void UpdateVisible(int SetGroupNo, bool isMax = true)
        {
            string textBlockName = $"SetGroupNo{SetGroupNo}";
            TextBlock targetTextBlock = GetTextBlockByName(textBlockName);

            if (targetTextBlock != null && targetTextBlock.Parent is Border parentBorder && isMax)
            {
                parentBorder.Background = (Brush)Application.Current.FindResource("ErrorColor");
            }
            else if (targetTextBlock != null && targetTextBlock.Parent is Border parentBorders && !isMax) 
            {
                parentBorders.Background = (Brush)Application.Current.FindResource("AccentColor");
            }
        }

        private TextBlock GetTextBlockByName(string name)
        {
            return this.FindName(name) as TextBlock;
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
            if (ProductComponentGroupButtonStartIndex + ProductComponentGroupButtonTot < CurrentComponentGroupItem.CPGI.Count)
            {
                // Increment productButtonStartIndex by ProductButtonShiftAmount to shift the visible range downwards
                ProductComponentGroupButtonStartIndex = Math.Min(CurrentComponentGroupItem.CPGI.Count - ProductComponentGroupButtonTot, ProductComponentButtonStartIndex + ProductComponentGroupButtonShiftAmount);
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

        public void isReopenComponent(bool reOpen)
        {
            if (!reOpen)
            {
                mainWindow.childItemsSelected.Clear();
            }
        }

        private void Number_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                string buttonText = button.Content.ToString();
                string currentText = packageItemQuantity.Text;

                if (currentText == "0")
                {
                    // Replace the current "0" with the new number
                    packageItemQuantity.Text = buttonText;
                }
                else if (currentText.Length > 0 && buttonText == "0" && currentText[0] == '0')
                {
                    // Do nothing, we don't want to allow numbers with leading zero
                    return;
                }
                else
                {
                    packageItemQuantity.Text += buttonText;
                }
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            IsConfirmed = false; // Mark as not confirmed
            mainWindow.childItemsSelected.Clear();
            CurrentComponentGroupItem.CPGI.Clear();
            SelectedComponentItems.SCI.Clear();
            CurrentPackageComponentValidation.currentPackageComponentValidations.Clear();
            this.Close();
        }

        private void Enter_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(SelectedItemNameFromList))
            {
                SelectedComponentItems selectedComponent = SelectedComponentItems.SCI.FirstOrDefault(item => item.CurrentComponentName == SelectedItemNameFromList);

                if (selectedComponent != null)
                {
                    if (int.TryParse(packageItemQuantity.Text, out int enteredQuantity))
                    {
                        var validationCurrentQuantity = CurrentPackageComponentValidation.currentPackageComponentValidations
                            .Where(x => x.SetGroupNoItemSelected == selectedComponent.CurrentComponentSetGroupNo)
                            .Select(x => x.SetGroupNeedQuantityItemSelected)
                            .FirstOrDefault(); // Assuming you expect a single value

                        var maxSetGroupQuantity = CurrentComponentGroupItem.CPGI
                            .Where(y => y.CurrentSetGroupNo == selectedComponent.CurrentComponentSetGroupNo)
                            .Select(y => Math.Max(y.CurrentSetGroupReq, y.CurrentSetGroupMaxQty))
                            .FirstOrDefault(); // Assuming you expect a single value

                        int newValidation = validationCurrentQuantity - selectedComponent.CurrentComponentQuantity;
                        int newQuantity = maxSetGroupQuantity - newValidation;

                        CurrentPackageComponentValidation ReplaceNew = CurrentPackageComponentValidation.currentPackageComponentValidations
                               .Find(x => x.SetGroupNoItemSelected == selectedComponent.CurrentComponentSetGroupNo);

                        if (enteredQuantity <= newQuantity)
                        {
                            selectedComponent.CurrentComponentQuantity = enteredQuantity;
                            ReplaceNew.SetGroupNeedQuantityItemSelected = enteredQuantity + newValidation;
                        }
                        else
                        {
                            Notification.NotificationMoreThanMaximum();
                            selectedComponent.CurrentComponentQuantity = newQuantity;
                            ReplaceNew.SetGroupNeedQuantityItemSelected = newQuantity + newValidation;
                        }

                        if (IsMaxQuantity(selectedComponent.CurrentComponentSetGroupNo))
                        {
                            UpdateVisible(selectedComponent.CurrentComponentSetGroupNo, true);
                        }
                        else
                        {
                            UpdateVisible(selectedComponent.CurrentComponentSetGroupNo, false);
                        }

                        UpdateTextBlocks();
                        UpdateCartUI();
                        packageItemQuantity.Text = "0";
                        SelectedItemNameFromList = string.Empty;
                    }
                    else
                    {
                        // Handle invalid input for packageItemQuantity.Text
                        // For example, show an error message or revert to previous quantity
                        Console.WriteLine("Invalid quantity entered.");
                    }
                }
            }
        }

        private void Backspace_Click(object sender, RoutedEventArgs e)
        {
            string currentText = packageItemQuantity.Text;

            if (currentText.Length > 0)
            {
                currentText = currentText.Substring(0, currentText.Length - 1);

                if (currentText.Length == 0)
                {
                    packageItemQuantity.Text = "0";
                }
                else
                {
                    packageItemQuantity.Text = currentText;
                }
            }
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            packageItemQuantity.Text = "0";
        }

        private void Min_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Max_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentSetGroupNoSelected != null)
            {
                // Calculate the total quantity for the selected group
                var setGroupMain = CurrentComponentGroupItem.CPGI
                    .Where(x => x.CurrentSetGroupNo == CurrentSetGroupNoSelected)
                    .Select(x => Math.Max(x.CurrentSetGroupMaxQty, x.CurrentSetGroupReq))
                    .Sum();

                // Calculate the total validated quantity for the selected group
                var validationGroup = CurrentPackageComponentValidation.currentPackageComponentValidations
                    .Where(y => y.SetGroupNoItemSelected == CurrentSetGroupNoSelected)
                    .Select(y => y.SetGroupNeedQuantityItemSelected)
                    .Sum();

                // Calculate the available quantity
                int availableQuantity = setGroupMain - validationGroup;

                // Update the packageItemQuantity text
                packageItemQuantity.Text = availableQuantity.ToString();
            }
        }

        private void productQuantitySelectorText_TextChanged(object sender, TextChangedEventArgs e)
        {
        }

        private async void packageItemSelectedListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListView listView)
            {
                try
                {
                    if (listView.SelectedItem is SelectedComponentItems selectedItem)
                    {
                        string productName = selectedItem.CurrentComponentName;
                        int productQuantity = selectedItem.CurrentComponentQuantity;

                        // Find the SelectedComponentItems object in SelectedComponentItems.SCI
                        SelectedComponentItems selectedComponent = SelectedComponentItems.SCI.FirstOrDefault(item => item.CurrentComponentName == productName);

                        if (selectedComponent != null)
                        {

                            SelectedItemNameFromList = productName;

                            // Update the quantity text box
                            packageItemQuantity.Text = productQuantity.ToString();

                            // Update the quantity in the SelectedComponentItems object
                            selectedComponent.CurrentComponentQuantity = int.Parse(packageItemQuantity.Text);

                            // Optionally perform additional operations here based on the selected item
                            await Task.Delay(100); // Example asynchronous operation
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Handle exceptions if necessary
                    Console.WriteLine($"Error in selection changed event: {ex.Message}");
                }
            }
        }


    }
}