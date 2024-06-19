using Serilog;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using TranQuik.Model;
using WpfScreenHelper;

namespace TranQuik.Pages
{
    public partial class SecondaryMonitor : Window
    {
        // This Windows
        private ModelProcessing modelProcessing;

        // Some Variable
        private int currentVideoIndex = 0;
        public Array screens;
        public SecondaryMonitor(ModelProcessing modelProcessing)
        {
            modelProcessing.UpdateSecondaryMonitor(this);
            InitializeComponent();
            BorderCheck();
            InitializeScreenPosition();
            this.modelProcessing = modelProcessing;
        }

        private void InitializeScreenPosition()
        {
            Screen[] screens = Screen.AllScreens.ToArray();

            if (screens.Length > 1)
            {
                Screen secondScreen = screens[1]; // Index 1 for the second monitor
                // Get the resolution (width and height) of the second screen
                int secondScreenWidth = (int)secondScreen.Bounds.Width;
                int secondScreenHeight = (int)secondScreen.Bounds.Height;


                // Calculate the center position of the second screen
                int secondScreenCenterX = (int)(secondScreen.Bounds.Left + (secondScreenWidth / 2));
                int secondScreenCenterY = (int)(secondScreen.Bounds.Top + (secondScreenHeight / 2));

                Left = secondScreenCenterX - (Width / 2);
                Top = secondScreenCenterY - (Height / 2);
                PlayVideos();
            }
            else
            {
                Log.ForContext("LogType", "ApplicationLog").Information($"Second Monitor didn't display properly, Error {screens.Length}");
            }
        }

        private void PlayVideos()
        {
            string videoDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resource/Video");
            string specificVideoName = Properties.Settings.Default._AppSecMonitorUrl;
            string specificVideoPath = Path.Combine(videoDirectory, specificVideoName);

            if (Properties.Settings.Default._AppSecMonitorLoop == 1)
            {
                PlayAllVideosInDirectory(videoDirectory);
            }
            else
            {
                PlaySpecificVideo(specificVideoPath);
            }
        }

        private void PlayAllVideosInDirectory(string videoDirectory)
        {
            string[] videoFiles = Directory.GetFiles(videoDirectory, "*.mp4");

            if (videoFiles.Length > 0)
            {
                MediaPlayer.Stretch = Stretch.Uniform;
                MediaPlayer.LoadedBehavior = MediaState.Manual;

                MediaPlayer.MediaEnded += (sender, e) =>
                {
                    currentVideoIndex = (currentVideoIndex + 1) % videoFiles.Length;
                    MediaPlayer.Source = new Uri(videoFiles[currentVideoIndex]);
                    MediaPlayer.Play();
                };

                currentVideoIndex = 0;
                MediaPlayer.Source = new Uri(videoFiles[currentVideoIndex]);
                MediaPlayer.Play();
            }
            else
            {
                Log.ForContext("LogType", "ApplicationLog").Warning("No video files found in the Video directory.");
            }
        }

        private void PlaySpecificVideo(string specificVideoPath)
        {
            if (File.Exists(specificVideoPath))
            {
                MediaPlayer.Stretch = Stretch.Uniform;
                MediaPlayer.LoadedBehavior = MediaState.Manual;

                MediaPlayer.MediaEnded += (sender, e) =>
                {
                    MediaPlayer.Source = new Uri(specificVideoPath);
                    MediaPlayer.Play();
                };

                MediaPlayer.Source = new Uri(specificVideoPath);
                MediaPlayer.Play();
            }
            else
            {
                MessageBox.Show($"The video file '{Path.GetFileName(specificVideoPath)}' was not found in the directory '{Path.GetDirectoryName(specificVideoPath)}'.", "File Not Found", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BorderCheck()
        {
            int BorderSettings = Properties.Settings.Default._AppSecMonitorBorder;
            if (BorderSettings == 0)
            {
                Bordererd.Visibility = Visibility.Collapsed;
            }
            else if (BorderSettings == 1)
            {
                Bordererd.Visibility = Visibility.Visible;
            }
        }

        public void UpdateCartUI()
        {
            // Clear existing items in the ListView
            bool hasItemsInCart = modelProcessing.cartProducts.Any();
            bool hasItemsActive = modelProcessing.cartProducts.Values.Any(Product => Product.Status);
            if (!hasItemsInCart)
            {
                hasItemsActive = false;
            }
            // Set the visibility of PaymentDetail based on the presence of items in CartProducts
            PaymentDetail.Visibility = hasItemsActive ? Visibility.Visible : Visibility.Collapsed;
            MediaPlayer.Stretch = hasItemsActive? Stretch.Uniform : Stretch.Uniform;

            cartPanelSecondary.Items.Clear();
            decimal totalPrice = 0;

            // Create a GridView
            GridView gridView = new GridView();

            // Define a style for the header columns
            Style headerColumnStyle = new Style(typeof(GridViewColumnHeader));
            headerColumnStyle.Setters.Add(new Setter(GridViewColumnHeader.BackgroundProperty, (Brush)Application.Current.FindResource("FontColor"))); // Set background color
            headerColumnStyle.Setters.Add(new Setter(GridViewColumnHeader.ForegroundProperty, Brushes.LightYellow)); // Set foreground color
            headerColumnStyle.Setters.Add(new Setter(GridViewColumnHeader.FontWeightProperty, FontWeights.Bold)); // Set font weight
            headerColumnStyle.Setters.Add(new Setter(GridViewColumnHeader.HorizontalContentAlignmentProperty, HorizontalAlignment.Center)); // Horizontally center the header content

            Style cellContentStyle = new Style(typeof(TextBlock));
            cellContentStyle.Setters.Add(new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Center)); // Horizontally center the cell content
            cellContentStyle.Setters.Add(new Setter(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Center)); // Horizontally center the cell content

            // Define column headers
            var indexColumnTemplate = new DataTemplate(typeof(TextBlock));
            var indexTextBlockFactory = new FrameworkElementFactory(typeof(TextBlock));
            indexTextBlockFactory.SetBinding(TextBlock.TextProperty, new Binding("Index"));
            indexTextBlockFactory.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Center); // Center the text in cells
            indexTextBlockFactory.SetValue(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Center); // Center the text in cells
            indexColumnTemplate.VisualTree = indexTextBlockFactory;

            var quantityColumnTemplate = new DataTemplate(typeof(TextBlock));
            var quantityTextBlockFactory = new FrameworkElementFactory(typeof(TextBlock));
            quantityTextBlockFactory.SetBinding(TextBlock.TextProperty, new Binding("Quantity"));
            quantityTextBlockFactory.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Center); // Center the text in cells
            quantityTextBlockFactory.SetValue(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Center); // Center the text in cells
            quantityColumnTemplate.VisualTree = quantityTextBlockFactory;

            gridView.Columns.Add(new GridViewColumn { Header = "#", DisplayMemberBinding = new Binding("Index"), Width = 20, HeaderContainerStyle = headerColumnStyle, CellTemplate = indexColumnTemplate });
            gridView.Columns.Add(new GridViewColumn { Header = "Product", DisplayMemberBinding = new Binding("ProductName"), Width = 80, HeaderContainerStyle = headerColumnStyle });
            gridView.Columns.Add(new GridViewColumn { Header = "Price", DisplayMemberBinding = new Binding("ProductPrice"), Width = 65, HeaderContainerStyle = headerColumnStyle });
            gridView.Columns.Add(new GridViewColumn { Header = "Qty", DisplayMemberBinding = new Binding("Quantity"), Width = 25, HeaderContainerStyle = headerColumnStyle, CellTemplate = quantityColumnTemplate });
            gridView.Columns.Add(new GridViewColumn { Header = "Total", DisplayMemberBinding = new Binding("TotalPrice"), Width = 80, HeaderContainerStyle = headerColumnStyle });

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
            cartPanelSecondary.ItemContainerStyle = listViewItemStyle;

            // Create a DataTemplate for the Action column
            DataTemplate actionCellTemplate = new DataTemplate();
            FrameworkElementFactory stackPanelFactory = new FrameworkElementFactory(typeof(StackPanel));
            stackPanelFactory.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);

            // Set the stack panel as the visual tree of the DataTemplate
            actionCellTemplate.VisualTree = stackPanelFactory;

            // Set the ListView's View to the created GridView
            cartPanelSecondary.View = gridView;

            int index = 1;

            // Loop through each product in the cart and add corresponding UI elements
            foreach (Product product in modelProcessing.cartProducts.Values)
            {
                decimal totalProductPrice = product.ProductPrice * product.Quantity; // Total price for the product
                TextDecorationCollection textDecorations = new TextDecorationCollection();
                // Determine background and foreground colors based on product status
                Brush rowBackground = product.Status ? Brushes.Transparent : Brushes.Red;
                Brush rowForeground = product.Status ? Brushes.Black : Brushes.White; // Foreground color for canceled (false) products
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
                else
                {
                    continue;
                }
                // Add item to ListView
                cartPanelSecondary.Items.Add(new
                {
                    Index = index,
                    ProductName = product.ProductName,
                    ProductPrice = product.ProductPrice.ToString("#,0"), // Format ProductPrice without currency symbol
                    Quantity = product.Quantity,
                    TotalPrice = totalProductPrice.ToString("#,0"),
                    ProductId = product.ProductId,
                });
                if (product.ChildItems != null && product.ChildItems.Any())
                {
                    foreach (ChildItem childItem in product.ChildItems)
                    {
                        // Add each child item to the ListView
                        cartPanelSecondary.Items.Add(new
                        {
                            Index = "-", // Indent child items with a dash for visual separation
                            ProductName = childItem.Name,
                            ProductPrice = childItem.Price.ToString("#,0"),
                            Quantity = childItem.Quantity,
                            Background = Brushes.LightGray, // Inherit parent's background color
                            Foreground = rowForeground // Inherit parent's foreground color
                        });
                    }
                }
                index++;
            }
            priceTextBlock.Text = $"{totalPrice:C0}";
            taxTextBlock.Text = $"{(totalPrice * modelProcessing.productVATPercent / 100).ToString("#,0")}";
            finalPriceTextBlock.Text = $"{totalPrice + (totalPrice * modelProcessing.productVATPercent / 100):C0}";

            if (cartPanelSecondary.Items.Count > 0)
            {
                // Scroll into the last item
                cartPanelSecondary.ScrollIntoView(cartPanelSecondary.Items[cartPanelSecondary.Items.Count - 1]);
            }
        }
    }
}
