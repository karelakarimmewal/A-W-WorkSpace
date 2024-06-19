using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TranQuik.Configuration;
using Microsoft.Win32;

namespace TransQuikConfiguration.Pages
{
    /// <summary>
    /// Interaction logic for SecondaryMonitors.xaml
    /// </summary>
    public partial class SecondaryMonitors : Page
    {
        private bool monitorIsActive;
        private int monitorLoop;
        private int monitorBorderStyle;
        private string monitorPath;
        private int currentVideoIndex = 0;

        public SecondaryMonitors()
        {
            InitializeComponent();
            ParamsRead();
        }

        private void ParamsRead()
        {
            PopulateBorderType();
            PopulateIsActive();
            PopulateLoopingType();
        }

        private void PopulateBorderType()
        {
            comboBorderType.Items.Add(new ComboBoxItem { Content = "Borderless", Tag = "0" });
            comboBorderType.Items.Add(new ComboBoxItem { Content = "Bordered", Tag = "1" });
            
            // Set the selected item based on AppSecMonitorBorder
            comboBorderType.SelectedIndex = AppSettings.AppSecMonitorBorder;
        }

        private void PopulateLoopingType()
        {
            comboLoopingType.Items.Add(new ComboBoxItem { Content = "Loop Once", Tag = "0" });
            comboLoopingType.Items.Add(new ComboBoxItem { Content = "Loop All", Tag = "1" });

            // Set the selected item based on AppSecMonitorLoop
            comboLoopingType.SelectedIndex = AppSettings.AppSecMonitorLoop;
        }

        private void PopulateIsActive()
        {
            comboIsActive.Items.Add(new ComboBoxItem { Content = "Active Monitor", Tag = "1" });
            comboIsActive.Items.Add(new ComboBoxItem { Content = "Deactive Monitor", Tag = "0" });

            // Set the selected item based on AppSecMonitor
            comboIsActive.SelectedIndex = AppSettings.AppSecMonitor ? 0 : 1;
        }

        private void comboBorderType_selectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboBorderType.SelectedItem != null)
            {
                // Retrieve the selected ComboBoxItem
                ComboBoxItem selectedItem = (ComboBoxItem)comboBorderType.SelectedItem;

                // Get the tag of the selected item
                string selectedTag = selectedItem.Tag.ToString();

                // Update AppSettings.AppFontFamily with the tag value
                monitorBorderStyle = int.Parse(selectedTag); 
                AppSettings.AppSecMonitorBorder = monitorBorderStyle;

                if (selectedTag == "1")
                {
                    Bordererd.Visibility = Visibility.Visible;
                }
                else
                {
                    Bordererd.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void comboLoopingType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboLoopingType.SelectedItem != null)
            {
                // Retrieve the selected ComboBoxItem
                ComboBoxItem selectedItem = (ComboBoxItem)comboLoopingType.SelectedItem;

                // Get the tag of the selected item
                string selectedTag = selectedItem.Tag.ToString();

                // Update AppSettings.AppFontFamily with the tag value
                AppSettings.AppSecMonitorLoop = int.Parse(selectedTag);

                monitorLoop = int.Parse(selectedTag);
            }
        }

        private void comboIsActive_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboBorderType.SelectedItem != null)
            {
                // Retrieve the selected ComboBoxItem
                ComboBoxItem selectedItem = (ComboBoxItem)comboIsActive.SelectedItem;

                // Get the tag of the selected item
                string selectedTag = selectedItem.Tag.ToString();

                // Update AppSettings.AppFontFamily with the tag value
                monitorIsActive = selectedTag == "1";

                if (selectedTag == "1")
                {
                    secMonitorPreview.Visibility = Visibility.Visible;
                }
                else
                {
                    secMonitorPreview.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void previewButton_Click(object sender, RoutedEventArgs e)
        {
            string baseDirectory = System.IO.Path.GetDirectoryName(Config.ConfigFilePath);
            string relativePath = @"..\Resource\Video\";

            string combinedPath = System.IO.Path.Combine(baseDirectory, relativePath, SecondaryMonitorUrl.Text);
            string normalizedPath = System.IO.Path.GetFullPath(combinedPath);

            if (monitorLoop == 1)
            {
                // Play all videos in the specified directory
                PlayAllVideosInDirectory(normalizedPath);
            }
            else
            {
                // Play the specific video
                PlaySpecificVideo(normalizedPath);
            }
        }

        private void PlayAllVideosInDirectory(string path)
        {
            string directory = path;

            // Check if the path is a file
            if (File.Exists(path))
            {
                // Get the directory from the file path
                directory = Path.GetDirectoryName(path);
            }

            if (Directory.Exists(directory))
            {
                monitorPath = directory;
                string[] videoFiles = Directory.GetFiles(directory, "*.mp4");

                if (videoFiles.Length > 0)
                {
                    // Set up a MediaElement to play videos
                    preview.Stretch = Stretch.Fill;
                    preview.LoadedBehavior = MediaState.Manual;

                    // Handle the MediaEnded event to loop the videos
                    preview.MediaEnded += (mediaSender, mediaArgs) =>
                    {
                        currentVideoIndex = (currentVideoIndex + 1) % videoFiles.Length;
                        string nextVideoPath = videoFiles[currentVideoIndex];
                        preview.Source = new Uri(nextVideoPath);
                        preview.Play();
                    };

                    // Start playing the first video
                    currentVideoIndex = 0;
                    preview.Source = new Uri(videoFiles[currentVideoIndex]);
                    preview.Play();
                }
                else
                {
                    MessageBox.Show("No video files found in the specified directory.", "File Not Found", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show($"The specified directory '{directory}' does not exist.", "Directory Not Found", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PlaySpecificVideo(string videoPath)
        {
            string directory = Path.GetDirectoryName(videoPath);
            string defaultVideoPath = Path.Combine(directory, "default.mp4");

            // Check if the specified video file exists
            if (!File.Exists(videoPath))
            {
                MessageBox.Show($"The specified video file '{videoPath}' does not exist. Playing default video instead.", "File Not Found", MessageBoxButton.OK, MessageBoxImage.Warning);
                videoPath = defaultVideoPath;
            }

            if (File.Exists(videoPath))
            {
                monitorPath = videoPath;

                // Set up a MediaElement to play the specific video
                preview.Stretch = Stretch.Fill;
                preview.LoadedBehavior = MediaState.Manual;

                // Handle the MediaEnded event to replay the video
                preview.MediaEnded += (mediaSender, mediaArgs) =>
                {
                    preview.Source = new Uri(videoPath);
                    preview.Play();
                };

                // Set the source of the MediaElement to the specific video and start playing
                preview.Source = new Uri(videoPath);
                preview.Play();
            }
            else
            {
                MessageBox.Show($"The default video file '{defaultVideoPath}' does not exist either.", "File Not Found", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenPathDialog_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog
            OpenFileDialog openFileDialog = new OpenFileDialog();

            // Set initial directory to the combined path
            string baseDirectory = System.IO.Path.GetDirectoryName(Config.ConfigFilePath);
            string relativePath = @"..\Resource\Video\";
            string combinedPath = System.IO.Path.Combine(baseDirectory, relativePath);

            // Log the combined path to check if it's correct
            Console.WriteLine($"Combined Path: {combinedPath}");
           
            // Check if the combined path exists
            if (System.IO.Directory.Exists(combinedPath))
            {
                // Set filter for file extension and default file extension
                openFileDialog.Filter = "MP4 files (*.mp4)|*.mp4|All files (*.*)|*.*";

                // Display OpenFileDialog by calling ShowDialog method
                bool? result = openFileDialog.ShowDialog();

                // Get the selected file name and display in a TextBox
                if (result == true)
                {
                    // Open document
                    string filename = openFileDialog.FileName;
                    // Assuming you have a TextBox named 'TextBoxFilePath'
                    SecondaryMonitorUrl.Text = filename;
                    monitorPath = filename;
                }
            }
            else
            {
                // Display error message with the combined path
                MessageBox.Show($"The directory does not exist: {combinedPath}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void ApplySecMonitor()
        {
            AppSettings.AppSecMonitor = monitorIsActive;
            AppSettings.AppSecMonitorBorder = monitorBorderStyle;
            AppSettings.AppSecMonitorLoop = monitorLoop;
            AppSettings.AppSecMonitorUrl = Path.GetFileName(monitorPath);
        }
    }
}
