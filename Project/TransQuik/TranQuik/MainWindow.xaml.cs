using Microsoft.CSharp.RuntimeBinder;
using MySql.Data.MySqlClient;
using Serilog;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using TranQuik.Configuration;
using TranQuik.Controller;
using TranQuik.Model;
using TranQuik.Pages;
using WpfScreenHelper;


namespace TranQuik
{
    public partial class MainWindow : Window
    {
        #region Cart Management

        List<int> payTypeIDsList = new List<int>();

        #endregion

        #region Database Settings

        private LocalDbConnector localDbConnector;
        public ModelProcessing modelProcessing;
        private TransactionStatus transactionStatus;
        public HeldCartManager heldCartManager;
        private SyncMethod syncMethod;


        #endregion

        #region User Interface Elements

        public List<Button> productGroupButtons = new List<Button>(); // List of buttons for product groups
        public List<Button> menuGroupButtons = new List<Button>(); // List of buttons for product groups
        private List<FilteredPayType> filteredPayTypes = new List<FilteredPayType>();
        public List<SaleMode> saleModes;
        public List<string> productGroupNames;
        public List<int> productGroupIds;
        public List<string> menuGroupNames;
        public List<int> menuGroupIds;

        #endregion

        #region Application State

        public int SaleMode = 0; // Sale mode indicator
        private int currentIndex = 0; // Current index state
        private int batchSize = 10; // Batch size for data operations
        public int startIndex = 0; // Start index for data display
        public int endIndex = 0;

        public int productGroupButtonStartIndex = 0; // Start index for product buttons
        public int ProductGroupButtonVisibleAmount = 8; // Initial visible button count
        public int ProductGroupButtonShiftAmount = 8; // Define the shift amount

        public int productButtonStartIndex = 0; // Start index for product buttons
        public int productButtonCount = 24; // Total count of product buttons
        public int ProductButtonShiftAmount = 24; // Define the shift amount

        public int subContentProductStartIndex = 0; // Start index for product buttons
        public int subContentProductButtonCount = 24; // Total count of product buttons
        public int subContentProductButtonShiftAmount = 24; // Define the shift amount

        public int MenuGroupButtonStartIndex = 0; // Start index for product buttons
        public int MenuGroupButtonButtonCount = 8; // Total count of product buttons
        public const int MenuGroupButtonShiftCount = 8; // Define the shift amount

        public int subContentButtonShiftAmount = 5; // Define the shift amount
        public int subContentStartIndex = 0; // Start index for product buttons
        public int subContentButtonCount = 5; // Total count of product buttons


        
        public bool isNew = true;

        private bool DevTest = Properties.Settings.Default._PrinterDevMode;
        private static string ReceiptHeader;
        public string PrefixText;

        #endregion

        #region Payment and Display Data

        private List<int> payTypeIDs = new List<int>(); // List of payment type IDs
        private List<string> displayNames = new List<string>(); // List of display names
        private List<bool> isAvailableList = new List<bool>(); // List of availability statuses
        public List<ChildItem> childItemsSelected = new List<ChildItem>();

        private SessionMethod sessionMethod;
        public SecondaryMonitor secondaryMonitor;
        private DispatcherTimer timer;

        #endregion

        #region Cart and Customer Management

        public Dictionary<DateTime, HeldCart> heldCarts = new Dictionary<DateTime, HeldCart>(); // Dictionary of held carts
        public int OrderID { get; set; }
        public int paxTotal { get; set; }
        public DateTime CustomerTime { get; set; }

        #endregion

        public MainWindow()
        {
            InitializeSettings();
            InitializeLocalDbConnector();
            InitializeHeldCartManager();
            PerformAutoSyncIfNeeded();
            RetrieveReceiptHeader();
            LoadHeldCarts();
            InitializeComponent();
            SetAnyDeskIDText();
            PositionWindowOnPrimaryScreen();
            SubscribeToLoadedEvent();
            InitializeModelProcessing();
            MaximizeWindowIfNeeded();
            SetVatNumberText();
            ShowSecondaryMonitorIfNeeded();
            UpdateChecker();
            
        }

        private void InitializeSettings()
        {
            Config.LoadAppSettings();
            Config.AnyDeskID();
            sessionMethod = new SessionMethod();
            syncMethod = new SyncMethod();
        }

        private async Task UpdateChecker()
        {
            
            if (Properties.Settings.Default._ComputerName != "POS1")
            {
                DateTime openSession = await sessionMethod.CheckThisOpenSession();

                bool isFound = false;
                while (!isFound)
                {
                    bool notifyNeeded = await sessionMethod.CheckSessionConditionAsync(openSession);
                    if (notifyNeeded)
                    {
                        NotificationPopup notificationPopup = new NotificationPopup("THERE ANY UPDATE IN DATABASE, PLEASE REOPEN, THIS TRANSACTION WILL BE IN HOLD CART !!!", false, this, true);
                        await syncMethod.CreateNewSessionInLocalDatabaseAsync(Properties.Settings.Default._ComputerID, Properties.Settings.Default._AppID, 2);
                        notificationPopup.Topmost = true;
                        notificationPopup.ShowDialog();
                    }
                }
                
            }
        }

        private void InitializeLocalDbConnector()
        {
            this.localDbConnector = new LocalDbConnector();
        }

        private void InitializeHeldCartManager()
        {
            this.heldCartManager = new HeldCartManager();
        }

        private async void PerformAutoSyncIfNeeded()
        {
            if (Properties.Settings.Default._AutoSync)
            {
                PerformDataSyncAsync();
            }
        }

        private void RetrieveReceiptHeader()
        {
            localDbConnector.RetrieveReceiptHeader(Properties.Settings.Default._ComputerID);
            ReceiptHeader = localDbConnector.ComputerReceiptHeader;
        }

        private void LoadHeldCarts()
        {
            heldCarts = heldCartManager.LoadHeldCarts();
        }

        private void SetAnyDeskIDText()
        {
            AnyDeskID.Text = Config.AnyDeskIDs != null ? Config.AnyDeskIDs.ToString() : "Open AnyDesk";
            
        }

        private void PositionWindowOnPrimaryScreen()
        {
            Screen[] screens = Screen.AllScreens.ToArray();

            if (screens.Length > 0)
            {
                Screen primaryScreen = screens[0];
                Rect bounds = primaryScreen.Bounds;

                this.WindowStartupLocation = WindowStartupLocation.Manual;
                this.Left = bounds.Left + (bounds.Width / 2.0) - (this.Width / 2.0);
                this.Top = bounds.Top + (bounds.Height / 2.0) - (this.Height / 2.0);
            }
        }

        private void SubscribeToLoadedEvent()
        {
            Loaded += WindowLoaded;
        }

        private void InitializeModelProcessing()
        {
            modelProcessing = new ModelProcessing(this);
        }

        private void MaximizeWindowIfNeeded()
        {
            Rect workingArea = SystemParameters.WorkArea;

            if (workingArea.Width <= 1038)
            {
                this.WindowState = WindowState.Maximized;
            }
        }

        private void SetVatNumberText()
        {
            VatNumber.Text = $"{modelProcessing.productVatText}";
        }

        private void ShowSecondaryMonitorIfNeeded()
        {
            if (Properties.Settings.Default._AppSecMonitor)
            {
                secondaryMonitor = new SecondaryMonitor(modelProcessing);
                secondaryMonitor.Topmost = true;
                secondaryMonitor.ShowInTaskbar = false;
                secondaryMonitor.Show();
            }
        }

        private async void PerformDataSyncAsync()
        {
            try
            {
                // Initialize and execute the SyncMethod
                SyncMethod syncMethod = new SyncMethod();
                await syncMethod.SyncDataAsync();

                // Update the last sync timestamp
                Properties.Settings.Default._LastSync = DateTime.Now;
                Properties.Settings.Default.Save(); // Save the settings if needed
            }
            catch (Exception ex)
            {
                // Log or handle any exceptions that occur
                MessageBox.Show($"An error occurred during data synchronization: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void SaleModeView()
        {
            SaleModePop saleModeWindow = new SaleModePop(this, this.modelProcessing, secondaryMonitor); // Pass reference to MainWindow
            saleModeWindow.Topmost = true;
            saleModeWindow.ShowInTaskbar = false;
            saleModeWindow.ShowDialog(); // Show SaleModePop window as modal
            productGroupButtons.Clear();
            startIndex = 0;
            endIndex = 0;
            GetPayTypeList((Properties.Settings.Default._ComputerID), SaleMode);
            ProductGroupLoad();
            modelProcessing.LoadProductDetails(51);
            
        }

        public void ModifierMenuView(int productID, int SelectedIndex)
        {
            if (modelProcessing != null)
            {
                ProductModifier productModifier = new ProductModifier(this, modelProcessing, productID, SelectedIndex);
                // Set TouchDialogMode to Full to enable proper touch handling
                productModifier.ShowInTaskbar = false;
                productModifier.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                productModifier.ResizeMode = ResizeMode.NoResize;
                productModifier.Topmost = true;
                productModifier.ShowDialog();
            }
            else
            {
                Console.WriteLine("Error: modelProcessing is null and cannot create MenuModifier.");
            }
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            SaleModeView();
        }

        public void ProductGroupLoad()
        {
            // Check if SaleMode is greater than zero
            if (SaleMode > 0)
            {
                try
                {
                    // Load product group names and IDs

                    modelProcessing.GetProductGroupNamesAndIds(out productGroupNames, out productGroupIds);
                    modelProcessing.GetMenuGroup(out menuGroupNames, out menuGroupIds);
                    modelProcessing.FavGroupButtonCreate(productGroupNames, productGroupIds);
                    modelProcessing.MenuGroupButtonCreate(menuGroupNames, menuGroupIds);
                    modelProcessing.UpdateVisibleButtons();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public void GroupClicked(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                productButtonStartIndex = 0;
                int productGroupId = Convert.ToInt32(button.Tag);
                MainContentProduct.Visibility = Visibility.Visible;
                PayementProcess.Visibility = Visibility.Collapsed;
                modelProcessing.LoadProductDetails(productGroupId);
            }
        }

        private void ButtonUp_Click(object sender, RoutedEventArgs e)
        {
            if (startIndex > 0)
            {
                startIndex = Math.Max(0, startIndex - ProductGroupButtonShiftAmount);
                modelProcessing.UpdateVisibleButtons();
            }
        }

        private void ButtonDown_Click(object sender, RoutedEventArgs e)
        {
            if (startIndex + ProductGroupButtonVisibleAmount <= productGroupButtons.Count)
            {
                startIndex = startIndex + ProductGroupButtonShiftAmount;
                modelProcessing.UpdateVisibleButtons();
            }
        }

        private void ScrollProductGroupsUp_Click(object sender, RoutedEventArgs e)
        {
            if (MainContentProduct.IsVisible)
            {
                if (productButtonStartIndex > 0)
                {
                    // Decrement productButtonStartIndex by ProductButtonShiftAmount to shift the visible range upwards
                    productButtonStartIndex = Math.Max(0, productButtonStartIndex - ProductButtonShiftAmount);
                    modelProcessing.UpdateVisibleProductButtons();
                }
            }
            else
            {
                if (subContentProductStartIndex > 0)
                {
                    // Decrement subContentProductStartIndex by subContentButtonShiftAmount to shift the visible range upwards
                    subContentProductStartIndex = Math.Max(0, subContentProductStartIndex - subContentProductButtonShiftAmount);
                    modelProcessing.UpdateProductSubcontent();
                }
            }
        }

        private void ScrollProductGroupsDown_Click(object sender, RoutedEventArgs e)
        {
            if (MainContentProduct.IsVisible)
            {
                if (productButtonStartIndex + productButtonCount <= MainContentProduct.Children.Count)
                {
                    // Increment productButtonStartIndex by ProductButtonShiftAmount to shift the visible range downwards
                    productButtonStartIndex = productButtonStartIndex + ProductButtonShiftAmount;
                    modelProcessing.UpdateVisibleProductButtons();
                }
            }
            else
            {
                if (subContentProductStartIndex + subContentProductButtonCount < MenuSubCategoryContentProduct.Children.Count)
                {
                    // Increment productButtonStartIndex by ProductButtonShiftAmount to shift the visible range downwards
                    subContentProductStartIndex = subContentProductStartIndex + subContentProductButtonShiftAmount;
                    modelProcessing.UpdateProductSubcontent();
                }
            }
            
        }

        private void prevContent_Click(object sender, RoutedEventArgs e)
        {
            if (subContentStartIndex > 0)
            {
                // Decrement productButtonStartIndex by ProductButtonShiftAmount to shift the visible range upwards
                subContentStartIndex = Math.Max(0, subContentStartIndex - subContentButtonShiftAmount);
                modelProcessing.updateVisibleContentButton();
            }
        }

        private void nextContent_Click(object sender, RoutedEventArgs e)
        {
            if (subContentStartIndex + subContentButtonCount < MenuSubCategoryContentButton.Children.Count)
            {
                // Increment productButtonStartIndex by ProductButtonShiftAmount to shift the visible range downwards
                subContentStartIndex = Math.Min(MenuSubCategoryContentButton.Children.Count - subContentButtonCount, subContentStartIndex + subContentButtonShiftAmount);
                modelProcessing.updateVisibleContentButton();
            }
        }

        private async void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListView listView && listView.SelectedIndex != -1)
            {
                try
                {
                    dynamic selectedItem = listView.SelectedItem;

                    if (selectedItem != null)
                    {
                        int lastIndex = listView.Items.Count - 1;
                        int selectedIndex = selectedItem.Index;
                        string productName = selectedItem.ProductName;
                        decimal productPrice = Convert.ToDecimal(selectedItem.ProductPrice);
                        int quantity = selectedItem.Quantity;

                        Product selectedProduct = new Product(selectedIndex, productName, productPrice, string.Empty)
                        {
                            Quantity = quantity
                        };

                        Product foundProduct = await Task.Run(() =>
                        {
                            if (modelProcessing.cartProducts.TryGetValue(selectedIndex, out Product product))
                            {
                                return product;
                            }
                            else
                            {
                                return null;
                            }
                        });

                        if (foundProduct != null)
                        {
                            if (foundProduct.ChildItems != null && foundProduct.ChildItems.Any())
                            {
                                foreach (var childItem in foundProduct.ChildItems)
                                {
                                    childItemsSelected.Add(childItem);
                                }
                            }
                            else
                            {
                                Console.WriteLine("No Child Items");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Product at index {selectedIndex} not found in cartProducts");
                        }

                        await Application.Current.Dispatcher.InvokeAsync(() =>
                        {
                            ModifierButton.Background = (Brush)Application.Current.FindResource("PrimaryButtonColor");
                            ModifierButton.IsEnabled = true;
                        });

                        bool isProductComponent = false;
                        if (foundProduct?.Status == true)
                        {
                            if (modelProcessing.cartProducts.TryGetValue(selectedIndex, out Product selectedProductst))
                            {
                                Product backupProduct = selectedProductst;
                                List<ChildItem> backupChild = new List<ChildItem>(childItemsSelected);

                                modelProcessing.CheckProductComponent(selectedProductst, out modelProcessing.componentGroups);
                                if (modelProcessing.componentGroups.Count > 1)
                                {
                                    string messageNotify = "Are You Sure To Reselect Product Package?";
                                    NotificationPopup notificationPopup = new NotificationPopup(messageNotify, true);
                                    notificationPopup.ShowDialog();

                                    if (notificationPopup.IsConfirmed)
                                    {
                                        await Application.Current.Dispatcher.InvokeAsync(() =>
                                        {
                                            selectedProductst.ChildItems.Clear(); // Clear child items of selectedProductst
                                            childItemsSelected.Clear(); // Clear child items of childItemsSelected
                                            Product ForComponent = new Product(selectedProductst.ProductId, productName, productPrice, string.Empty);
                                            ProductComponent productComponent = new ProductComponent(modelProcessing, ForComponent, this, SaleMode, selectedIndex, true);
                                            productComponent.ShowDialog();
                                            if (!productComponent.IsConfirmed)
                                            {
                                                // Restore the original values if the operation is not confirmed
                                                selectedProductst = backupProduct;
                                                childItemsSelected = backupChild;
                                            }
                                        });
                                        isProductComponent = true;
                                    }                                    
                                }

                                else
                                {
                                    ModifierMenuView(selectedProductst.ProductId, selectedIndex);
                                }
                            }
                        }

                        await Task.Run(() =>
                        {
                            if (childItemsSelected != null && childItemsSelected.Any())
                            {
                                foreach (var item in childItemsSelected)
                                {
                                    selectedProduct.ChildItems.Add(item);
                                }
                            }

                            bool productFound = false;
                            if (modelProcessing.cartProducts.TryGetValue(selectedIndex, out Product selectedProducts))
                            {
                                productFound = true;
                                selectedProducts.ChildItems = selectedProduct.ChildItems;
                            }

                            if (!productFound)
                            {
                                modelProcessing.cartProducts[selectedIndex] = selectedProduct;
                            }
                        });

                        await Application.Current.Dispatcher.InvokeAsync(() =>
                        {
                            modelProcessing.UpdateCartUI();
                        });
                    }
                    else
                    {
                        ModifierButton.Background = Brushes.SlateGray;
                    }
                    childItemsSelected.Clear();
                }
                catch (RuntimeBinderException ex)
                {
                    Console.WriteLine($"Error accessing properties: {ex.Message}");
                }
            }
        }

        private void shutDownTrigger(object sender, RoutedEventArgs e)
        {
            // Create and show the ShutDownPopup window
            ShutDownPopup shutDownPopup = new ShutDownPopup();
            shutDownPopup.Topmost = true;
            shutDownPopup.ShowDialog();
        }

        private int[] GetPayTypeList(int computerID, int saleModeID)
        {
            // Define SQL query to retrieve PayTypeList based on ComputerID
            string queryComputer = "SELECT PayTypeList FROM computername WHERE ComputerID = @ComputerID";
            string querySaleMode = "SELECT PayTypeList, NotInPayTypeList FROM saleMode WHERE SaleModeID = @SaleModeID";

            using (MySqlConnection connection = localDbConnector.GetMySqlConnection())
            {
                MySqlCommand commandComputer = new MySqlCommand(queryComputer, connection);
                commandComputer.Parameters.AddWithValue("@ComputerID", computerID);

                MySqlCommand commandSaleMode = new MySqlCommand(querySaleMode, connection);
                commandSaleMode.Parameters.AddWithValue("@SaleModeID", saleModeID);

                try
                {
                    connection.Open();
                    object resultComputer = commandComputer.ExecuteScalar();
                    MySqlDataReader readerSaleMode = commandSaleMode.ExecuteReader();

                    if (resultComputer != null && resultComputer != DBNull.Value)
                    {
                        string payTypeListString = resultComputer.ToString();
                        // Split the string into individual IDs and convert to integer array
                        string[] payTypeIDsArray = payTypeListString.Split(',');
                        foreach (string payTypeIDStr in payTypeIDsArray)
                        {
                            if (int.TryParse(payTypeIDStr, out int payTypeID))
                            {
                                payTypeIDsList.Add(payTypeID);
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("PayTypeList not found for the specified ComputerID.");
                    }

                    // Check if the SaleMode query returned results
                    if (readerSaleMode.Read())
                    {
                        string payTypeListSaleMode = readerSaleMode["PayTypeList"].ToString();
                        string notInPayTypeListSaleMode = readerSaleMode["NotInPayTypeList"].ToString();

                        if (!string.IsNullOrEmpty(payTypeListSaleMode))
                        {
                            // If PayTypeList is not null, use this list
                            payTypeIDsList.Clear(); // Clear the existing list
                            string[] payTypeIDsArray = payTypeListSaleMode.Split(',');
                            foreach (string payTypeIDStr in payTypeIDsArray)
                            {
                                if (int.TryParse(payTypeIDStr, out int payTypeID))
                                {
                                    payTypeIDsList.Add(payTypeID);
                                }
                            }
                        }
                        else if (!string.IsNullOrEmpty(notInPayTypeListSaleMode))
                        {
                            // If PayTypeList is null, use NotInPayTypeList to remove items
                            string[] notInPayTypeIDsArray = notInPayTypeListSaleMode.Split(',');
                            List<int> notInPayTypeIDsList = new List<int>();
                            foreach (string payTypeIDStr in notInPayTypeIDsArray)
                            {
                                if (int.TryParse(payTypeIDStr, out int payTypeID))
                                {
                                    notInPayTypeIDsList.Add(payTypeID);
                                }
                            }

                            // Remove items from payTypeIDsList that are in notInPayTypeIDsList
                            payTypeIDsList = payTypeIDsList.Except(notInPayTypeIDsList).ToList();
                        }
                    }
                    readerSaleMode.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error retrieving PayTypeList: {ex.Message}");
                }
            }

            return payTypeIDsList.ToArray();
        }

        private void AddButtonGridToPaymentMethod()
        {
            // Clear existing children in PaymentMethod
            PaymentMethod.Children.Clear();

            // Connect to MySQL database and retrieve data
            string query = "SELECT PayTypeID, DisplayName, IsAvailable FROM paytype WHERE isAvailable = 1 ORDER BY PayTypeOrdering ASC";

            using (MySqlConnection connection = localDbConnector.GetMySqlConnection())
            {
                MySqlCommand command = new MySqlCommand(query, connection);
                connection.Open();
                MySqlDataReader reader = command.ExecuteReader();

                // Populate lists with PayTypeID, DisplayName, and IsAvailable
                while (reader.Read())
                {
                    int payTypeID = reader.GetInt32(0); // PayTypeID (index 0)
                    string displayName = reader.GetString(1); // DisplayName (index 1)
                    bool isAvailable = reader.GetBoolean(2); // IsAvailable (index 2)

                    payTypeIDs.Add(payTypeID);
                    displayNames.Add(displayName);
                    isAvailableList.Add(isAvailable);
                }

                reader.Close();
            }

            // Display buttons for the current batch
            FilteredPayType();
            
        }

        private void FilteredPayType()
        {
            filteredPayTypes.Clear();
            filteredPayTypes = new List<FilteredPayType>();
            for (int index = 0; index < payTypeIDs.Count; index++)
            {
                int payTypeID = payTypeIDs[index];
                string displayName = displayNames[index];
                bool isAvailable = isAvailableList[index];
                if (payTypeIDsList.Contains(payTypeID))
                {
                    filteredPayTypes.Add(new FilteredPayType(payTypeID, displayName, isAvailable));
                }
            }
            DisplayCurrentBatch();
        }

        private void DisplayCurrentBatch()
        {
            // Clear existing children in the PaymentMethod grid
            PaymentMethod.Children.Clear();

            // Variable to track the current position in the grid
            int buttonCount = 0;
            int startIndex = currentIndex;
            int endIndex = Math.Min(currentIndex + batchSize, filteredPayTypes.Count);

            // Display filtered pay types
            for (int index = startIndex; index < endIndex; index++)
            {
                FilteredPayType filteredPayType = filteredPayTypes[index];
                int row = buttonCount / 5;
                int col = buttonCount % 5;

                // Determine the button's background color based on IsAvailable
                Brush backgroundColor = filteredPayType.IsAvailable ? Brushes.Azure : (Brush)FindResource("DisabledButtonColor");
                Brush foregroundColor = filteredPayType.IsAvailable ? Brushes.Black : (Brush)FindResource("FontColor");

                Button button = new Button
                {
                    FontSize = 12,
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(2),
                    Padding = new Thickness(10),
                    Background = backgroundColor,
                    Foreground = foregroundColor,
                    IsEnabled = filteredPayType.IsAvailable,
                    Content = filteredPayType.DisplayName,
                    Tag = filteredPayType // Set the PayTypeID as the Tag property of the button
                };


                // Create a text block for the button content
                TextBlock textBlock = new TextBlock
                {
                    Text = filteredPayType.DisplayName,
                    TextWrapping = TextWrapping.Wrap,
                    TextAlignment = TextAlignment.Center // Optionally, you can align the text
                };

                // Set the content of the button to the text block
                button.Content = textBlock;

                // Attach click event handler to the button
                button.Click += (sender, e) => PaymentTypeButton_Click(sender, e);

                // Apply DropShadowEffect to the button
                button.Effect = FindResource("DropShadowEffect") as DropShadowEffect;

                // Add the button to the PaymentMethod grid
                Grid.SetRow(button, row);
                Grid.SetColumn(button, col);
                PaymentMethod.Children.Add(button);

                // Increment the button count
                buttonCount++;
            }

            // Fill remaining spaces with blank buttons if less than batchSize items are displayed
            while (buttonCount < batchSize)
            {
                int row = buttonCount / 5;
                int col = buttonCount % 5;

                Button blankButton = new Button
                {
                    Content = string.Empty,
                    FontSize = 12,
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(2),
                    Padding = new Thickness(10),
                    Background = Brushes.Transparent,
                    Foreground = Brushes.Transparent,
                    IsEnabled = false
                };


                // Add the blank button to the PaymentMethod grid
                Grid.SetRow(blankButton, row);
                Grid.SetColumn(blankButton, col);
                PaymentMethod.Children.Add(blankButton);

                // Increment the button count
                buttonCount++;
            }

            // Add "Prev" button to the last row, first column
            Button prevButton = new Button
            {
                Content = "<-",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(2),
                Padding = new Thickness(10),
                Background = (Brush)FindResource("AccentColor"),
                Foreground = Brushes.White,
                IsEnabled = currentIndex > 0 // Enable only if there are previous items
            };
            prevButton.Click += PrevButton_Click;
            Grid.SetRow(prevButton, 3); // Adjust to fit layout
            Grid.SetColumn(prevButton, 0); // First column
            PaymentMethodNav.Children.Add(prevButton);

            // Add "Next" button to the last row, last column
            Button nextButton = new Button
            {
                Content = "->",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(2),
                Padding = new Thickness(10),
                Background = (Brush)FindResource("AccentColor"),
                Foreground = Brushes.White,
                IsEnabled = currentIndex + batchSize < filteredPayTypes.Count // Enable only if there are more items
            };
            nextButton.Effect = FindResource("DropShadowEffect") as DropShadowEffect;
            nextButton.Click += NextButton_Click;
            Grid.SetRow(nextButton, 3); // Adjust to fit layout
            Grid.SetColumn(nextButton, 4); // Last column
            PaymentMethodNav.Children.Add(nextButton);
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            currentIndex += batchSize;
            if (currentIndex >= filteredPayTypes.Count)
            {
                currentIndex = filteredPayTypes.Count - batchSize;
                if (currentIndex < 0) currentIndex = 0; // Ensure currentIndex doesn't go negative if there are fewer items than batchSize
            }
            DisplayCurrentBatch();
        }

        private void PrevButton_Click(object sender, RoutedEventArgs e)
        {
            currentIndex -= batchSize;
            if (currentIndex < 0)
                currentIndex = 0;

            DisplayCurrentBatch();
        }

        private void PaymentTypeButton_Click (object sender, RoutedEventArgs e)
        {
            Log.ForContext("LogType", "ApplicationLog").Information($"Pay Type Button Clicked");

            // Handle button click event here
            if (sender is Button button)
            {
                // Retrieve the PayType stored in the button's Tag property
                if (button.Tag is FilteredPayType filteredPayType)
                {
                    // Access the properties of filteredPayType
                    string displayName = filteredPayType.DisplayName;
                    int payTypeID = filteredPayType.PayTypeID;

                    // Display the DisplayName on the secondary monitor
                    if (secondaryMonitor != null)
                    {
                        secondaryMonitor.Payment.Text = displayName;
                    }

                    // Set the modelProcessing.isPaymentChange
                    modelProcessing.isPaymentChange = payTypeID.ToString();

                    // Handle specific actions based on PayTypeID
                    if (payTypeID == 1)
                    {
                        Button simulateButton = new Button();  // Create a new button instance (this could be any UI element)
                        RoutedEventArgs args = new RoutedEventArgs();  // Create new instance of RoutedEventArgs
                        payCashButton(simulateButton, args);
                    }
                    else if (payTypeID == 10011)
                    {
                        // Call QRIS method
                        modelProcessing.qrisProcess(payTypeID.ToString(), displayName);
                    } else if (payTypeID == 10009)
                    {
                        PaymentTypeWindow paymentTypeWindow = new PaymentTypeWindow(this, total.Text, payTypeID, displayName);
                        paymentTypeWindow.Topmost = true;
                        paymentTypeWindow.ShowDialog();
                    }
                    //MessageBox.Show($"Waiting For Payment Using: {displayName}\nPayTypeID: {payTypeID}");
                }
                else
                {
                    MessageBox.Show("Invalid button content", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public void HoldButton_Click(object sender, RoutedEventArgs e)
        {
            Log.ForContext("LogType", "ApplicationLog").Information($"Hold Button Clicked");

            // Create a deep copy of the current cart products
            Dictionary<int, Product> currentCartProducts = new Dictionary<int, Product>(modelProcessing.cartProducts);

            if (heldCarts.ContainsKey(CustomerTime))
            {
                // Retrieve the existing HeldCart for the given CustomerTime
                HeldCart existingHeldCart = heldCarts[CustomerTime];

                existingHeldCart.SalesModeName = salesModeText.Text;

                // Update the existing HeldCart with the new products
                existingHeldCart.CartProducts = currentCartProducts;

                // Notify the user that the cart has been updated
                Log.ForContext("LogType", "TransactionLog").Information($"Cart for Order ID: {existingHeldCart.CustomerId} at {existingHeldCart.TimeStamp} has been updated.");
            }
            else
            {
                // Create a new HeldCart instance
                HeldCart heldCart = new HeldCart(OrderID, CustomerTime, currentCartProducts, SaleMode, salesModeText.Text, PrefixText);

                // Add the held cart to the dictionary with the timestamp as the key
                heldCarts.Add(CustomerTime, heldCart);

                // Notify the user that the cart has been held
                Log.ForContext("LogType", "TransactionLog").Information($"Cart has been held for Order ID: {OrderID} at {CustomerTime}");
            }

            // Reset the UI
            heldCartManager.SaveHeldCarts(heldCarts);

            modelProcessing.ResetUI("Hold");
            DisplayHeldCartsInConsole();
        }

        private void DisplayHeldCarts()
        {
            Console.WriteLine("Held Carts:");
            foreach (var kvp in heldCarts)
            {
                Console.WriteLine($"Timestamp: {kvp.Key}");
                Console.WriteLine($"Order ID: {kvp.Value.CustomerId}");
                Console.WriteLine("Cart Products:");
                foreach (var product in kvp.Value.CartProducts.Values)
                {
                    Console.WriteLine($"- {product.ProductName}");
                }
                Console.WriteLine(); // Add a blank line between each held cart entry
            }
        }

        public void HoldBill(Dictionary<int, Product> cartProducts)
        {
            modelProcessing.cartProducts = cartProducts;

            // Update the cart UI
            modelProcessing.UpdateCartUI();
        }

        public void UpdateSecondayMonitor()
        {
            if (secondaryMonitor != null)
            {
                secondaryMonitor.UpdateCartUI();
            }
        }

        public void QrisDone(string transactionStatus, string PayTypeID, string PayTypeName)
        {
            if (secondaryMonitor != null)
            {
                switch (transactionStatus)
                {
                    case "Settlement":

                        secondaryMonitor.imageLoader.Source = null;
                        secondaryMonitor.imageLoader.Height = 0;
                        secondaryMonitor.imageLoader.Width = 0;
                        TransactionDone(PayTypeID, PayTypeName);
                        break;

                    case "Cancel":
                        secondaryMonitor.imageLoader.Source = null;
                        secondaryMonitor.imageLoader.Height = 0;
                        secondaryMonitor.imageLoader.Width = 0;
                        TransactionCanceled();
                        break;

                    case "Expire":
                        // Actions to perform when QRIS transaction has expired

                        // Add other actions as needed
                        break;

                    default:
                        // Handle unknown status or other statuses if needed
                        MessageBox.Show("Unknown Transaction Status!");
                        break;
                }
            }            
        }

        private void DisplayHeldCartsInConsole()
        {
            Console.WriteLine("Displaying Held Carts...");
            DisplayHeldCarts();
            Console.WriteLine("End of Held Carts");
        }

        private void ClearList_Click(object sender, RoutedEventArgs e)
        {
            string NotificationText = "ARE U SURE TO CLEAR LIST?";
            NotificationPopup notificationPopup = new NotificationPopup(NotificationText, true);

            notificationPopup.ShowDialog();

            if (notificationPopup.IsConfirmed)
            {
                // User clicked "Yes", proceed with clearing the list
                foreach (var product in modelProcessing.cartProducts.Values)
                {
                    product.Status = false; // Set the status of each product to false
                }

                // Update the cart UI to reflect the changes
                modelProcessing.UpdateCartUI();
            }
            else
            {
                // User clicked "Cancel", do nothing
            }
        }

        private void ModifierButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Number_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                string buttonText = button.Content.ToString();

                // Append the clicked number or dot to the displayed text
                if (buttonText == ".")
                {
                    // Check if dot is already present in the display text
                    if (!displayText.Text.Contains("."))
                    {
                        displayText.Text += buttonText; // Append dot if not already present
                    }
                }
                else
                {
                    // Append the clicked number to the display text
                    displayText.Text += buttonText;
                }

                // Update the displayed text with formatting (thousands separators)
                modelProcessing.Calculating();
                modelProcessing.UpdateFormattedDisplay();
            }
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            displayText.Text = "0";
            modelProcessing.Calculating();
        }

        private void Backspace_Click(object sender, RoutedEventArgs e)
        {
            if (displayText.Text.Length > 0)
                displayText.Text = displayText.Text.Substring(0, displayText.Text.Length - 1);
            modelProcessing.Calculating();
        }

        private void Enter_Click(object sender, RoutedEventArgs e)
        {
            string inputText = displayText.Text;

            // Parse the entered text into a double value
            if (double.TryParse(inputText, out double enteredAmount))
            {
                // Calculate the grand total value from the UI element
                if (double.TryParse(GrandTotalCalculator.Text, out double grandTotalValue))
                {
                    // Calculate the return amount
                    double returnAmount = enteredAmount - grandTotalValue;

                    if (returnAmount < 0)
                    {
                        // Display a message indicating insufficient funds
                        Log.ForContext("LogType", "TransactionLog").Information($"Cart for Order ID: {OrderID}. Insufficient funds. Please enter more money to proceed.");
                        MessageBox.Show("Insufficient funds. Please enter more money to proceed.");
                        return; // Exit the method without further processing
                    }
                    else
                    {
                        // Proceed with the rest of the processing
                        Log.ForContext("LogType", "TransactionLog").Information($"Cart for Order ID: {OrderID} Cash transaction Successfully return value is {returnAmount}");
                        modelProcessing.OrderTransactionFunction();
                        TransactionDone("0", "Cash");
                    }
                }
                else
                {
                    MessageBox.Show("Invalid grand total value. Please check the total amount.");
                }
            }
            else
            {
                MessageBox.Show("Invalid input. Please enter a valid number.");
            }
        }

        public static string RemovePipeCharacter(string input)
        {
            // Define the regex pattern to match the pipe character
            string pattern = @"\|";

            // Replace all occurrences of the pipe character with an empty string
            string result = Regex.Replace(input, pattern, string.Empty);

            return result;
        }

        public void Print(string PayTypeID, string PayTypeName, string ReceiptNumber)
        {
            
            // Get the base directory of the application
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            // Create a new directory named "ReceiptLogs" within the base directory
            string receiptLogsDirectory = Path.Combine(baseDirectory, "ReceiptLogs");
            if (!Directory.Exists(receiptLogsDirectory))
            {
                Directory.CreateDirectory(receiptLogsDirectory);
            }
            
            // Create a new PrintDocument
            var doc = new PrintDocument();
            PrintController printController = new StandardPrintController();

            // Attach the event handler for printing content
            doc.PrintPage += (sender, e) => ProvideContent(sender, e, PayTypeID, PayTypeName, ReceiptNumber);

            // Calculate the required height based on the content
            int requiredHeight = CalculateRequiredHeight(); // Implement this method to calculate required height

            // Convert millimeters to hundredths of an inch
            float widthInHundredthsOfInch = MillimetersToHundredthsOfInch(80); // 80mm to hundredths of an inch
            float heightInHundredthsOfInch = MillimetersToHundredthsOfInch(requiredHeight); // Convert required height to hundredths of an inch

            // Set the paper size to 80mm x requiredHeight
            var paperSize = new PaperSize("Receipt", (int)widthInHundredthsOfInch, (int)heightInHundredthsOfInch);
            doc.DefaultPageSettings.PaperSize = paperSize;

            // Set margins
            doc.DefaultPageSettings.Margins = new Margins(Properties.Settings.Default._PrinterMarginLeft, Properties.Settings.Default._PrinterMarginTop, Properties.Settings.Default._PrinterMarginRight, Properties.Settings.Default._PrinterMarginBottom); // Adjust margins in hundredths of an inch

            // Get the list of installed printers
            foreach (string printer in System.Drawing.Printing.PrinterSettings.InstalledPrinters)
            {
                if (printer.Contains(Properties.Settings.Default._PrinterName))
                {
                    // Set the EPSON TM-T82 Receipt printer as the printer name
                    doc.PrinterSettings.PrinterName = printer;
                    break;
                }
            }

            if (DevTest)
            {
                // Create a PrintPreviewDialog
                var printPreviewDialog = new System.Windows.Forms.PrintPreviewDialog();
                printPreviewDialog.Document = doc;

                //// Show the print preview dialog
                printPreviewDialog.ShowDialog();
            }
            
            // Set the file name and save location
            //doc.PrinterSettings.PrintToFile = true;
            //doc.PrinterSettings.PrintFileName = filePath;

            // PrintController must be set before printing
            doc.PrintController = printController;

            // Print the document
            try
            {
                if (DevTest)
                {
                    Console.Write("Printed");
                }
                else
                {
                    doc.Print();
                }

            }
            catch (Exception ex)
            {
                // Log any exceptions that occur during printing
                Console.WriteLine($"Printing failed: {ex.Message}");
                transactionStatus = new TransactionStatus("Failed", modelProcessing);
                transactionStatus.ShowDialog();
            }

            ReceiptNumber = RemovePipeCharacter(ReceiptNumber);
            string filePath = Path.Combine(receiptLogsDirectory, $"{ReceiptNumber}.pdf");
        }

        private float MillimetersToHundredthsOfInch(double millimeters)
        {
            return (float)(millimeters / 25.4f * 100.0f);
        }

        private float MillimetersToHundredthsOfInch(float millimeters)
        {
            return millimeters / 25.4f * 100.0f;
        }

        private int CalculateRequiredHeight()
        {
            // Calculate the number of lines
            int numberOfLines = GetNumberOfLines();
            int lineHeight = 4; // Assuming each line of text is 12 pixels high
            int margin = 40; // Add some margin for the top and bottom of the receipt
            // Calculate the required height
            return numberOfLines * lineHeight + margin;
        }

        private int GetNumberOfLines()
        {
            // Sample method to calculate the number of lines in the content
            // This will depend on how you are generating your content

            // For demonstration, let's assume each item and its details take up 2 lines
            int numberOfLines = 0;
            foreach (var item in modelProcessing.cartProducts.Values)
            {
                numberOfLines += 1; // Item name and details
                if (item.HasChildItems())
                {
                    foreach (var childItem in item.ChildItems)
                    {
                        numberOfLines += 1; // Child item details
                    }
                }
            }

            // Add extra lines for headers, footers, and any additional information
            numberOfLines += 16; // Example value

            return numberOfLines;
        }

        public void ProvideContent(object sender, PrintPageEventArgs e, string PayTypeID, string PayTypeName, string ReceiptNumber)
        {
            // Load settings from the file
            var settings = Config.LoadPrinterSettings();

            string businessAddress = settings.ContainsKey("BusinessAddress") ? settings["BusinessAddress"] : "Default Address";
            string businessPhone = settings.ContainsKey("BusinessPhone") ? settings["BusinessPhone"] : "Default Phone";
            string footerText1 = settings.ContainsKey("FooterText1") ? settings["FooterText1"] : "Default Footer Text 1";
            string footerText2 = settings.ContainsKey("FooterText2") ? settings["FooterText2"] : "Default Footer Text 2";
            string footerText3 = settings.ContainsKey("FooterText3") ? settings["FooterText3"] : "Default Footer Text 3";
            string footerText4 = settings.ContainsKey("FooterText4") ? settings["FooterText4"] : "Default Footer Text 4";
            string footerText5 = settings.ContainsKey("FooterText5") ? settings["FooterText5"] : "Default Footer Text 5";


            string receiptNumber = ReceiptNumber;
            int paperWidth = 576; // Width of the 80mm thermal paper in dots (typically 576 dots for 80mm)

            // Define column widths (total should be around 48 characters for 80mm paper)
            const int FIRST_COL_WIDTH = 22;  // Adjusted to fit within the 80mm paper width
            const int SECOND_COL_WIDTH = 2;  // Adjusted to fit within the 80mm paper width
            const int FOURTH_COL_WIDTH = 15; // Adjusted to fit within the 80mm paper width

            var sb = new StringBuilder();
            string LogoName = Properties.Settings.Default._PrinterLogo;
            // Load the image
            System.Drawing.Bitmap image = new System.Drawing.Bitmap($"Resource/Logo/{LogoName}");
            // Calculate the number of spaces needed to center the image
            int spacesCount = Math.Max(0, (paperWidth / 12 - image.Width) / 2);
            // Calculate the scaling factor based on the paper width and the image width
            float scalingFactor = (float)paperWidth / image.Width;

            // Calculate the scaled image dimensions
            int scaledWidth = 330;
            int scaledHeight = (int)(image.Height * scalingFactor);

            // Replace with your business name and details
            sb.AppendLine($"{" ".PadRight(spacesCount)}");
            // Calculate the X position to center the image horizontally
            int imageX = 0;

            // Calculate the Y position to place the image at the top of the receipt
            int headerHeight = 100; // Example: Replace with your actual header height
            int imageY = headerHeight - image.Height; // Adjust as needed based on your header height and image size

            // Draw the image on the Graphics object
            e.Graphics.DrawImage(image, new System.Drawing.Rectangle(imageX, 0, scaledWidth, 45));
            sb.AppendLine($"{" ".PadRight(spacesCount)}");
            sb.AppendLine($"{" ".PadRight(spacesCount)}");
            sb.AppendLine(new string('=', paperWidth / 12));
            sb.AppendLine($"Receipt Number : {receiptNumber}");
            sb.AppendLine($"Date           : {DateTime.Now}");
            sb.AppendLine($"Cashier        : {CurrentSessions.StaffFirstName} {CurrentSessions.StaffLastName}");
            sb.AppendLine(new string('=', paperWidth / 12));
            sb.AppendLine(businessAddress);
            sb.AppendLine($"Vat Reg. No. : {VatNumber.Text}");
            sb.AppendLine($"TEL          : {businessPhone}");
            sb.AppendLine($"Payment Type : {PayTypeName}");
            sb.AppendLine($"Sales Type   :{salesModeText.Text}");
            sb.AppendLine(new string('=', paperWidth / 12));
            sb.AppendLine();
            sb.AppendLine($"{"ITEM".PadRight(FIRST_COL_WIDTH)}{"QTY".PadLeft(SECOND_COL_WIDTH)}{"TOTAL".PadLeft(FOURTH_COL_WIDTH)}");
            sb.AppendLine(new string('-', paperWidth / 12));

            decimal total = 0; // Initialize total variable
            var receiptItems = modelProcessing.cartProducts.Values; // Use cartProducts instead of Order.orders

            foreach (var item in receiptItems)
            {
                if (!item.Status)
                {
                    continue;
                }

                string itemName = item.ProductName.Length > FIRST_COL_WIDTH ? item.ProductName.Substring(0, FIRST_COL_WIDTH - 1) : item.ProductName;

                sb.Append(itemName.PadRight(FIRST_COL_WIDTH));
                sb.Append(item.Quantity.ToString().PadLeft(SECOND_COL_WIDTH));
                sb.AppendLine((item.ProductPrice * item.Quantity).ToString("N0").PadLeft(FOURTH_COL_WIDTH));

                total += item.ProductPrice * item.Quantity; // Add item's amount to total

                // Check if the product has child items
                if (item.HasChildItems())
                {
                    foreach (var childItem in item.ChildItems)
                    {
                        string childItemName = childItem.Name.Length > FIRST_COL_WIDTH - 2 ? childItem.Name.Substring(0, FIRST_COL_WIDTH - 2) : childItem.Name;

                        sb.Append((" - " + childItemName).PadRight(FIRST_COL_WIDTH));
                        sb.Append(childItem.Quantity.ToString().PadLeft(SECOND_COL_WIDTH));
                        sb.AppendLine((childItem.Price * childItem.Quantity).ToString("N0").PadLeft(FOURTH_COL_WIDTH));

                        total += childItem.Price * childItem.Quantity; // Add child item's amount to total
                    }
                }
            }

            decimal vatAmount = total * modelProcessing.productVATPercent / 100;
            decimal grandTotal = total + vatAmount;

            decimal tendered = 0;
            if (PayTypeID == "0")
            {
                if (!decimal.TryParse(displayText.Text, out decimal parsedTendered))
                {
                    // Handle parsing error, maybe show a message to the user
                    return;
                }
                tendered = Math.Round(parsedTendered);
            }
            else
            {
                tendered = grandTotal;
            }
            decimal change = tendered - grandTotal;

            sb.AppendLine(new string('-', paperWidth / 12));
            sb.AppendLine($"{"Sub Total:".PadLeft(FIRST_COL_WIDTH + SECOND_COL_WIDTH)}{total.ToString("N0").PadLeft(FOURTH_COL_WIDTH)}");
            sb.AppendLine($"{$"{modelProcessing.productVatText}:".PadLeft(FIRST_COL_WIDTH + SECOND_COL_WIDTH)}{vatAmount.ToString("N0").PadLeft(FOURTH_COL_WIDTH)}");
            sb.AppendLine(new string('-', paperWidth / 12));
            sb.AppendLine($"{"Bill Total:".PadLeft(FIRST_COL_WIDTH + SECOND_COL_WIDTH)}{grandTotal.ToString("C0").PadLeft(FOURTH_COL_WIDTH)}");
            sb.AppendLine($"{"Cash:".PadLeft(FIRST_COL_WIDTH + SECOND_COL_WIDTH)}{tendered.ToString("C0").PadLeft(FOURTH_COL_WIDTH)}");
            sb.AppendLine($"{"Change:".PadLeft(FIRST_COL_WIDTH + SECOND_COL_WIDTH)}{change.ToString("C0").PadLeft(FOURTH_COL_WIDTH)}");
            sb.AppendLine(new string('=', paperWidth / 12));

            // Calculate the number of spaces needed to center the footer
            int footerSpacesCount = ((paperWidth / 12 - footerText1.Length) / 2) - 3;

            // Append the centered footer text to the StringBuilder
            sb.AppendLine($"{" ".PadRight(footerSpacesCount - 1)}{footerText1}");
            sb.AppendLine($"{" ".PadRight(footerSpacesCount - 4)}{footerText2}");
            sb.AppendLine($"{" ".PadRight(footerSpacesCount - 4)}{footerText3}");
            sb.AppendLine($"{" ".PadRight(footerSpacesCount - 8)}{footerText4}");
            sb.AppendLine($"{" ".PadRight(footerSpacesCount - 6)}{footerText5}");


            string printText = sb.ToString(); // Convert StringBuilder to string

            // Draw text on the Graphics object
            e.Graphics.DrawString(printText, new System.Drawing.Font(System.Drawing.FontFamily.GenericMonospace, 8, System.Drawing.FontStyle.Bold),
                                  new System.Drawing.SolidBrush(System.Drawing.Color.Black), 0, 0);
        }

        public static string GenerateReceiptNumber(string CustomerID)
        {
            string computerID = (Properties.Settings.Default._ComputerID).ToString();
            string kodeStore = ReceiptHeader;
            // Get the current date and time
            DateTime now = DateTime.Now;

            // Format the date as YYYYMMDD
            string datePart = now.ToString("MMyyyy");

            // Format the time as HHMMSS
            string timePart = now.ToString("HHmmss");

            // Concatenate all parts to form the receipt number
            string receiptNumber = $"{kodeStore}{datePart}/{int.Parse(CustomerID):D5}";

            return receiptNumber;
        }

        private void TransactionDone(string PayTypeID, string PayTypeName)
        {
            if (heldCarts.ContainsKey(CustomerTime))
            {
                heldCarts.Remove(CustomerTime);
                heldCartManager.SaveHeldCarts(heldCarts);
            }
            string ReceiptNumber = GenerateReceiptNumber(OrderID.ToString());

            string totalText = total.Text.Replace(".", ""); // Remove decimal points
            int totalValue = int.Parse(totalText); // Parse the string into an integer

            if (totalValue != 0) 
            {
                if (Properties.Settings.Default._PrinterStatus)
                {
                    Print(PayTypeID, PayTypeName, ReceiptNumber);
                }
                transactionStatus = new TransactionStatus("Success", modelProcessing);
                transactionStatus.ShowDialog();
            }
            else
            {
                transactionStatus = new TransactionStatus("Void", modelProcessing);
                transactionStatus.ShowDialog();
            }
        }

        private void TransactionCanceled()
        {
            transactionStatus = new TransactionStatus("Cancel", modelProcessing);
            transactionStatus.ShowDialog();
        }

        private void salesModeSee_Click(object sender, RoutedEventArgs e)
        {
            isNew = false;
            Log.ForContext("LogType", "TransactionLog").Information($"Order ID {OrderID} changed Sales Mode");
            SaleModeView();
        }

        public void UpdateQRCodeImage(byte[] imageData, string transactionQrUrl)
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
            secondaryMonitor.imageLoader.Width = 250;
            secondaryMonitor.imageLoader.Height = 250;
            secondaryMonitor.imageLoader.Stretch = Stretch.Fill;
            secondaryMonitor.imageLoader.Source = bitmapImage;

            // Optionally handle image click event to copy URL to clipboard
            secondaryMonitor.imageLoader.MouseLeftButtonDown += (sender, e) =>
            {
                Clipboard.SetText(transactionQrUrl);
            };
        }

        private void changeSalesMode_Click(object sender, RoutedEventArgs e)
        {
            // Toggle between SaleModeIDs 1 and 2
            if (SaleMode == 1)
            {
                SaleMode = 2;
            }
            else if (SaleMode == 2)
            {
                SaleMode = 1;
            }
            else
            {
                string NotificationText = "FORBIDDEN TO CHANGE SALE MODE FROM THIS!";
                NotificationPopup notificationPopup = new NotificationPopup(NotificationText, false);
                notificationPopup.CancelButton.Visibility = Visibility.Collapsed;
                notificationPopup.ShowDialog();
            }

            // Find the SaleMode object in the list based on SaleModeID
            SaleMode selectedSaleMode = saleModes.FirstOrDefault(mode => mode.SaleModeID == SaleMode);

            // Update your UI to display the SaleModeName of the selected SaleMode
            if (selectedSaleMode != null)
            {
                StatusCondition.Text = $"{selectedSaleMode.SaleModeName} ({paxTotal})";
                salesModeText.Text = selectedSaleMode.SaleModeName;
            }
        }

        private void favButton(object sender, RoutedEventArgs e)
        {
            HideMenuRelatedUI();
            ShowFavoriteProductUI();
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            LoadMenuContent();
            ShowMenuRelatedUI();
            HideFavoriteProductUI();
        }

        private void HideMenuRelatedUI()
        {
            MenuGroupButton.Visibility = Visibility.Collapsed;
            MenuSubCategoryContentProduct.Visibility = Visibility.Collapsed;
            MenuSubCategoryContent.Visibility = Visibility.Collapsed;
            MenuBorder.Visibility = Visibility.Collapsed;
        }

        private void ShowFavoriteProductUI()
        {
            ProductGroupName.Visibility = Visibility.Visible;
            MainContentProduct.Visibility = Visibility.Visible;
        }

        private void LoadMenuContent()
        {
            modelProcessing.MenuGroupButtonCreateSubContent(16);
            modelProcessing.LoadSubContentProduct(52);
            modelProcessing.updateVisibleContentButton();
        }

        private void ShowMenuRelatedUI()
        {
            MenuGroupButton.Visibility = Visibility.Visible;
            MenuSubCategoryContentProduct.Visibility = Visibility.Visible;
            MenuSubCategoryContent.Visibility = Visibility.Visible;
            MenuBorder.Visibility = Visibility.Visible;
        }

        private void HideFavoriteProductUI()
        {
            ProductGroupName.Visibility = Visibility.Collapsed;
            PayementProcess.Visibility = Visibility.Collapsed;
            MainContentProduct.Visibility = Visibility.Collapsed;
        }

        private void fastCash_Click(object sender, RoutedEventArgs e)
        {
            // Check if any product in cartProducts has status set to true
            if (modelProcessing.cartProducts.Values.Any(product => product.Status))
            {
                if (sender is Button button)
                {
                    if (button.Content is string buttonText)
                    {
                        // Get the existing text from displayText.Text
                        string currentText = displayText.Text;

                        // Try to parse the existing text into a decimal
                        if (decimal.TryParse(currentText, out decimal currentValue))
                        {
                            // Try to parse the buttonText into a decimal
                            if (decimal.TryParse(buttonText, out decimal buttonValue))
                            {
                                // Add the parsed buttonValue to the currentValue
                                decimal newValue = currentValue + buttonValue;

                                // Set the displayText.Text to the new value
                                displayText.Text = newValue.ToString();

                                // Call the payCashButton method
                                PayButton_Click(sender, e);

                                // Call the Calculating method of modelProcessing
                                modelProcessing.Calculating();
                            }
                        }
                    }
                }
            }
        }

        private void payCashButton(object sender, RoutedEventArgs e)
        {
            modelProcessing.isPaymentChange = "1";
            secondaryMonitor.Payment.Text = "Cash";
            ProcessPayment("Cash");
        }

        private void PayButton_Click(object sender, RoutedEventArgs e)
        {
            ProcessPayment("Selecting.....");
        }

        private void PaymentButton_Click(object sender, RoutedEventArgs e)
        {
            ProcessPayment("Selecting.....");
        }

        private void ProcessPayment(string paymentType)
        {
            MainContentProduct.Children.Clear();
            MenuSubCategoryContentProduct.Visibility = Visibility.Collapsed;
            MenuSubCategoryContent.Visibility = Visibility.Collapsed;
            MenuBorder.Visibility = Visibility.Collapsed;
            PayementProcess.Visibility = Visibility.Visible;
            MainContentProduct.Visibility = Visibility.Collapsed;
            CalculatorShowed.Visibility = Visibility.Visible;
            modelProcessing.Calculating();
            AddButtonGridToPaymentMethod();
            Log.ForContext("LogType", "TransactionLog").Information($"Cart for Order ID: {OrderID} Payment Process, {paymentType}");
        }

        private void OpenAnyDesk_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                
                string anyDeskPath = Config.anyDeskPath;
                if (anyDeskPath != null)
                {
                    var processInfo = new ProcessStartInfo
                    {
                        FileName = anyDeskPath,
                        UseShellExecute = true
                    };

                    string NotificationText = "ARE U SURE TO OPEN ANYDESK?";
                    NotificationPopup notificationPopup = new NotificationPopup(NotificationText, true);
                    notificationPopup.ShowDialog();
                    if (notificationPopup.IsConfirmed)
                    {
                        Process.Start(processInfo);
                    }                   
                }
                else
                {
                    MessageBox.Show("AnyDesk not found or AnyDesk ID could not be retrieved.");
                }
                Config.AnyDeskID();
                string anyDeskID = Config.AnyDeskIDs;
                AnyDeskID.Text = anyDeskID;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }
    }
}
