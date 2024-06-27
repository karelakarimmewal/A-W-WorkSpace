using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TranQuik.Model;
using System.Windows.Media;
using System.IO;

namespace TranQuik.Pages
{
    /// <summary>
    /// Interaction logic for ProductModifier.xaml
    /// </summary>
    public partial class ProductModifier : Window
    {
        private MainWindow mainWindow;
        private ModelProcessing modelProcessing;
        private LocalDbConnector localDbConnector;

        private List<ModifierGroup> ProductModifierGroup;
        private List<ModifierMenu> ProductModifierMenu;

        private List<Button> ProductModifierGroupButtons = new List<Button>();
        private List<Button> ProductModifierMenuButtons = new List<Button>();

        private int ProductIDSelected;
        private int SelectedIndex;

        private int ProductComponentGroupButtonTot = 4;
        private int ProductComponentGroupButtonShiftAmount = 4;
        private int ProductComponentGroupButtonStartIndex = 0;
        private int ProductComponentGroupButtonEndtIndex = 0;

        private int ProductComponentButtonTot = 10;
        private int ProductComponentButtonShiftAmount = 10;
        private int ProductComponentButtonStartIndex = 0;
        private int ProductComponentButtonEndtIndex = 0;
        public ProductModifier(MainWindow mainWindow, ModelProcessing modelProcessing, int ProductID, int selectedIndex)
        {
            this.mainWindow = mainWindow;
            this.modelProcessing = modelProcessing;
            this.ProductIDSelected = ProductID;
            this.SelectedIndex = selectedIndex;
            this.localDbConnector = new LocalDbConnector();
            InitializeComponent();
            ProductModifierGroup = GetModifierGroups(mainWindow.SaleMode);
            ModifierGroupButton();
            UpdateVisibleProductGroupButtons();
            int modifierGroupID = int.Parse(ProductModifierGroup[0].ModifierGroupID);
            GetModifierMenus(modifierGroupID);
            CreateModifierMenusButton();
            UpdateVisibleProductModifierButtons();

        }

        public List<ModifierGroup> GetModifierGroups(int saleMode)
        {
            List<ModifierGroup> modifierGroups = new List<ModifierGroup>();

            try
            {
                using (MySqlConnection connection = localDbConnector.GetMySqlConnection())
                {
                    connection.Open();

                    string sqlQuery = @"
                    SELECT DISTINCT PD.ProductDeptID, PD.ProductDeptCode, PD.ProductDeptName
                    FROM Products P
                    JOIN ProductPrice PP ON P.ProductID = PP.ProductID
                    JOIN ProductDept PD ON PD.ProductDeptID = P.ProductDeptID
                    JOIN ProductGroup PG ON P.ProductGroupID = PG.ProductGroupID
                    WHERE P.ProductActivate = 1 AND PG.ProductGroupID = 11 AND PP.SaleMode = @SaleMode";

                    using (MySqlCommand command = new MySqlCommand(sqlQuery, connection))
                    {
                        // Add parameter for SaleMode
                        command.Parameters.AddWithValue("@SaleMode", saleMode);

                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ModifierGroup modifierGroup = new ModifierGroup();
                                modifierGroup.ModifierGroupID = reader["ProductDeptID"].ToString();
                                modifierGroup.ModifierGroupCode = reader["ProductDeptCode"].ToString();
                                modifierGroup.ModifierName = reader["ProductDeptName"].ToString();
                                modifierGroups.Add(modifierGroup);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions here (e.g., log the error)
                Console.WriteLine("Error retrieving modifier groups: " + ex.Message);
            }

            return modifierGroups;
        }

        public void ModifierGroupButton()
        {
            ProductModifierGroupGrid.Children.Clear();
            ProductModifierGroupButtons.Clear(); // Clear the list of buttons

            // Define the total number of buttons required
            int totalButtonCount = ProductModifierGroup.Count; // Use ProductModifierGroup.Count instead of ProductComponentGroupButtonTot
                                                               // Create buttons for each component group
            for (int i = 0; i < totalButtonCount; i++) // Use totalButtonCount instead of ProductModifierGroup.Count
            {
                ModifierGroup modifierGroup = ProductModifierGroup[i];

                // Create a new button for each group
                Button button = new Button
                {
                    Content = modifierGroup.ModifierName,
                    Tag = modifierGroup,
                    FontWeight = FontWeights.Bold,
                    Background = (Brush)Application.Current.FindResource("PrimaryButtonColor"),
                    Foreground = (Brush)Application.Current.FindResource("PrimaryBackgroundColor"),
                    BorderThickness = new Thickness(0.8),
                };

                // Set button properties
                button.Click += ProductComponentGroupButtonsClicked;

                // Add the button to the grid
                int row = i / 4;
                int column = i % 4;
                Grid.SetRow(button, row);
                Grid.SetColumn(button, column);

                ProductModifierGroupGrid.Children.Add(button);
                ProductModifierGroupButtons.Add(button); // Add the button to the list of buttons
            }

            UpdateVisibleProductGroupButtons();
        }

        private void ProductComponentGroupButtonsClicked(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                ModifierGroup modifierGroup = button.Tag as ModifierGroup;
                if (modifierGroup != null)
                {
                    int modifierGroupID = int.Parse(modifierGroup.ModifierGroupID);
                    GetModifierMenus(modifierGroupID);
                    CreateModifierMenusButton();
                    UpdateVisibleProductModifierButtons();
                }
            }
        }

        private void GetModifierMenus(int modifierGroupID)
        {
            try
            {
                if (ProductModifierMenu == null)
                {
                    ProductModifierMenu = new List<ModifierMenu>(); // Ensure list is initialized
                }
                else
                {
                    ProductModifierMenu.Clear(); // Clear the list before adding new items
                }

                using (MySqlConnection connection = localDbConnector.GetMySqlConnection())
                {
                    connection.Open();

                    string sqlQuery = @"
                    SELECT PG.`ProductGroupCode`, P.`ProductDeptID`, PD.`ProductDeptCode`, PD.`ProductDeptName`, 
                           P.`ProductCode`, P.`ProductName`, P.`ProductName2`, PP.`ProductPrice`, PP.`SaleMode`
                    FROM Products P
                    JOIN ProductPrice PP ON P.`ProductID` = PP.`ProductID`
                    JOIN ProductDept PD ON PD.`ProductDeptID` = P.`ProductDeptID`
                    JOIN ProductGroup PG ON P.`ProductGroupID` = PG.`ProductGroupID`
                    WHERE P.`ProductActivate` = 1 AND PG.`ProductGroupID` = 11 AND PP.`SaleMode` = @SaleMode AND PD.`ProductDeptID` = @ModifierGroupID
                    ORDER BY P.`ProductName`";

                    using (MySqlCommand command = new MySqlCommand(sqlQuery, connection))
                    {
                        command.Parameters.AddWithValue("@SaleMode", mainWindow.SaleMode); // Specify the sale mode value
                        command.Parameters.AddWithValue("@ModifierGroupID", modifierGroupID);

                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ModifierMenu modifierMenu = new ModifierMenu();
                                modifierMenu.ModifierMenuCode = Convert.ToInt32(reader["ProductCode"]);
                                modifierMenu.ModifierMenuName = reader["ProductName"].ToString();
                                modifierMenu.ModifierMenuPrice = Convert.ToDecimal(reader["ProductPrice"]);
                                modifierMenu.ModifierMenuQuantity = 0; // Initialize quantity to zero
                                ProductModifierMenu.Add(modifierMenu);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error retrieving modifier menus: " + ex.Message);
            }
        }

        private void CreateModifierMenusButton()
        {
            // Clear existing buttons
            ProductModifierGrid.Children.Clear();

            // Calculate the total number of buttons required
            int totalButtonCount = ProductModifierMenu.Count;

            // Create buttons for each modifier menu
            for (int i = 0; i < totalButtonCount; i++)
            {
                ModifierMenu modifierMenu = ProductModifierMenu[i];

                // Create a new button for each modifier menu
                Button button = new Button
                {
                    Content = modifierMenu.ModifierMenuName,
                    Tag = modifierMenu,
                    FontWeight = FontWeights.Bold,
                    Background = (Brush)Application.Current.FindResource("AccentColor"),
                    Foreground = (Brush)Application.Current.FindResource("PrimaryBackgroundColor"),
                    BorderThickness = new Thickness(0.8),
                };

                // Set button properties
                button.Click += ProductComponentMenuButtonsClicked;

                // Calculate row and column indices for the grid
                int row = i / 5; // Assuming 5 columns per row
                int column = i % 5;
                Grid.SetRow(button, row);
                Grid.SetColumn(button, column);

                // Add the button to the grid
                ProductModifierGrid.Children.Add(button);
            }
        }

        private void ProductComponentMenuButtonsClicked(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            ModifierMenu modifierMenu = button?.Tag as ModifierMenu;

            if (modifierMenu != null)
            {
                int currentQuantity = 0;
                int maxQuantity = modelProcessing.cartProducts.Values
                                    .Where(product => product.ProductId == ProductIDSelected)
                                    .Select(product => product.Quantity)
                                    .DefaultIfEmpty(1) // Handle the case where no matching product is found
                                    .Max();

                // Check if a ChildItem corresponding to the ModifierMenu already exists
                ChildItem existingItem = mainWindow.childItemsSelected.FirstOrDefault(item =>
                    item.ChildName == modifierMenu.ModifierMenuName &&
                    item.ChildPrice == modifierMenu.ModifierMenuPrice);

                if (existingItem != null)
                {
                    currentQuantity = existingItem.ChildQuantity;
                    existingItem.ChildQuantity++;

                    if (existingItem.ChildQuantity > maxQuantity)
                    {
                        existingItem.ChildQuantity = maxQuantity;
                    }
                }
                else
                {
                    // Initialize the quantity to 1 on the first click
                    modifierMenu.ModifierMenuQuantity = 1;

                    ChildItem childItem = new ChildItem(
                        modifierMenu.ModifierMenuCode,
                        modifierMenu.ModifierMenuName,
                        modifierMenu.ModifierMenuPrice,
                        modifierMenu.ModifierMenuQuantity,
                        true, 0,0 // Assuming StatusBar is a property of ChildItem
                    );

                    // Add the ChildItem to the mainWindow's childItemsSelected collection
                    mainWindow.childItemsSelected.Add(childItem);

                    currentQuantity = 1;
                }

                // Update UI or perform any other necessary actions based on the currentQuantity
                UpdateButtonVisualState(button, true, currentQuantity); // Pass the currentQuantity
            }
        }

        private void UpdateButtonVisualState(Button button, bool isSelected, int quantity)
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

            // Get the existing button content
            string buttonText = button.Content.ToString();

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
        }

        public void UpdateVisibleProductGroupButtons()
        {
            int col = 4;
            modelProcessing.UpdateVisibleProductButtons(ProductModifierGroupGrid, ProductComponentGroupButtonStartIndex, ProductComponentGroupButtonTot, NextComponentGroup_Button, col);
        }

        public void UpdateVisibleProductModifierButtons()
        {
            int col = 5;
            modelProcessing.UpdateVisibleProductButtons(ProductModifierGrid, ProductComponentButtonStartIndex, ProductComponentButtonShiftAmount, NextComponentButton, col);
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
                    }
                    else
                    {
                        // If the new quantity is less than 1, reset to the last valid quantity
                        quantityDisplay.Text = "1";
                        selectedProduct.Quantity = 1;
                    }
                }
                modelProcessing.UpdateCartUI();

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
            if (ProductComponentGroupButtonStartIndex + ProductComponentGroupButtonTot < CurrentComponentGroupItem.CPGI.Count)
            {
                // Increment productButtonStartIndex by ProductButtonShiftAmount to shift the visible range downwards
                ProductComponentGroupButtonStartIndex = Math.Min(CurrentComponentGroupItem.CPGI.Count - ProductComponentGroupButtonTot, ProductComponentButtonStartIndex + ProductComponentGroupButtonShiftAmount);
                UpdateVisibleProductGroupButtons();
            }

        }

        private void PrevComponentGroup_Click(object sender, RoutedEventArgs e)
        {
            if (ProductComponentGroupButtonStartIndex > 0)
            {
                // Decrement productButtonStartIndex by ProductButtonShiftAmount to shift the visible range upwards
                ProductComponentGroupButtonStartIndex = Math.Max(0, ProductComponentGroupButtonStartIndex - ProductComponentGroupButtonShiftAmount);
                UpdateVisibleProductGroupButtons();
            }

        }

        private void NextComponent_Click(object sender, RoutedEventArgs e)
        {

            if (ProductComponentButtonStartIndex + ProductComponentButtonTot < ProductModifierMenuButtons.Count)
            {
                // Increment productButtonStartIndex by ProductButtonShiftAmount to shift the visible range downwards
                ProductComponentButtonStartIndex = Math.Min(ProductModifierMenuButtons.Count - ProductComponentButtonTot, ProductComponentButtonStartIndex + ProductComponentButtonShiftAmount);
                UpdateVisibleProductModifierButtons();
            }
        }

        private void PrevComponent_Click(object sender, RoutedEventArgs e)
        {
            if (ProductComponentButtonStartIndex > 0)
            {

                // Decrement productButtonStartIndex by ProductButtonShiftAmount to shift the visible range upwards
                ProductComponentButtonStartIndex = Math.Max(0, ProductComponentButtonStartIndex - ProductComponentButtonShiftAmount);
                UpdateVisibleProductModifierButtons();
            }
        }

        private void HandleDeleteItemClick(object sender, RoutedEventArgs e)
        {
            if (modelProcessing == null)
            {
                return;
            }
            // Retrieve the product details using the ProductSelectedIndex
            if (modelProcessing.cartProducts.TryGetValue(SelectedIndex, out Product selectedProduct))
            {
                selectedProduct.Status = false;
                Console.WriteLine($"This is product Status Setter {selectedProduct.Status}");
                modelProcessing.UpdateCartUI();
                this.Close();
            }
        }

        private void addOnSave_Click(object sender, RoutedEventArgs e)
        {

            this.Close();
        }

        private void addOnReset_Click(object sender, RoutedEventArgs e)
        {
            // Find the product with the specified product ID
            Product product = modelProcessing.cartProducts.Values.FirstOrDefault(p => p.ProductId == ProductIDSelected);

            // If the product is found, remove its associated child items from mainWindow.childItemsSelected
            if (product != null)
            {
                // Iterate over the child items and remove those associated with the product
                product.ChildItems.Clear();
                mainWindow.childItemsSelected.Clear();
            }

            modelProcessing.UpdateCartUI();

            // Close the MenuModifier window
            this.Close();
        }
    }
}
