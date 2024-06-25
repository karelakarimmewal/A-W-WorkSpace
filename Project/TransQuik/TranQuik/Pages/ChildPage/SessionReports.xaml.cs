using MySql.Data.MySqlClient;
using System;
using System.Globalization;
using System.Runtime.InteropServices.ComTypes;
using System.Windows;
using System.Windows.Controls;
using TranQuik.Model;

namespace TranQuik.Pages.ChildPage
{
    public partial class SessionReports : Page
    {
        private LocalDbConnector _connector;
        public SessionReports()
        {
            InitializeComponent();
            _connector = new LocalDbConnector();
            DefineFirst();
            
        }

        private void PopulateComputerAccess()
        {
            selectPOS.Items.Add(new ComboBoxItem
            {
                Content = "--All POS--",
                Tag = 0,
            });
            foreach (var items in ComputerAccessData.ComputerAccessDatas)
            {
                if (items.ComputerType == 0)
                {
                    selectPOS.Items.Add(new ComboBoxItem {
                    Content = items.ComputerName,
                    Tag = items.ComputerID,
                    });
                }
            }
            selectPOS.SelectedIndex = 0;
        }

        private void DefineFirst()
        {
            ComputerAccessData.PopulateComputerAccessData(_connector);
            startDatePicker.Text = DateTime.Now.AddDays(-7).ToString("MM/dd/yyyy");
            endDatePicker.Text = DateTime.Now.ToString();
            PopulateComputerAccess();
            var shop18 = ShopData.shopDatas.Find(shop => shop.ShopId == 18);
            if (shop18 != null)
            {
                sessionReportsShopName.Text = shop18.ShopName;
            }
        }

        private void showSessionReports_Click(object sender, RoutedEventArgs e)
        {
            // SQL query to retrieve data
            string query = "SELECT SessionDate, OpenSessionDateTime, CloseSessionDateTime, ComputerName, CloseStaff FROM session WHERE 1=1"; // Initial query

            // Check if Start Date is available
            bool isStartDateAvailable = !string.IsNullOrEmpty(startDatePicker.Text);
            if (isStartDateAvailable)
            {
                // Extracting the date part from the text
                string dateString = startDatePicker.Text.Split(' ')[0];

                // Parse the extracted date string into a DateTime object
                if (DateTime.TryParseExact(dateString, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime startDate))
                {
                    // Update the query to include the start date
                    query += $" AND SessionDate >= '{startDate.ToString("yyyy-MM-dd")}'";
                }
                else
                {
                    // Handle invalid date format
                    MessageBox.Show("Invalid start date format!");
                }
            }

            // Check if End Date is available
            bool isEndDateAvailable = !string.IsNullOrEmpty(endDatePicker.Text);
            if (isEndDateAvailable)
            {
                string dateString = endDatePicker.Text.Split(' ')[0];
                // Parse the end date string and set time to 23:59:59
                if (DateTime.TryParseExact(endDatePicker.Text, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime endDate))
                {
                    // Update the query to include the end date
                    query += $" AND SessionDate <= '{endDate.ToString("yyyy-MM-dd")}'";
                }
                else
                {
                    // Handle invalid date format
                    MessageBox.Show("Invalid end date format!");
                    return; // Exit the method if invalid date format
                }
            }

            // Check if Search Close Staff is available
            bool isSearchCloseStaffAvailable = !string.IsNullOrEmpty(searchCloseName.Text);
            if (isSearchCloseStaffAvailable)
            {
                query += $" AND CloseStaff LIKE '%{searchCloseName.Text}%'";
            }

            // Check if Selected POS is available
            bool isSelectedPosAvailable = selectPOS.SelectedIndex != -1;
            if (isSelectedPosAvailable)
            {
                ComboBoxItem selectedPosItem = (ComboBoxItem)selectPOS.SelectedItem;
                int computerID = Convert.ToInt32(selectedPosItem.Tag);
                if (computerID == 0)
                {

                }
                else
                {
                    query += $" AND ComputerID = {computerID}";
                }
                
            }

            query += " ORDER BY OpenSessionDateTime DESC";
            try
            {
                using (MySqlConnection connection = _connector.GetMySqlConnection())
                {
                    connection.Open();
                    // Create MySqlCommand object
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        // Create a reader to retrieve data from the database
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            // Clear existing items in ListView
                            sessionReportListView.Items.Clear();
                            int index = 0;
                            // Read each row from the result set
                            while (reader.Read())
                            {
                                index++;
                                // Retrieve values from the reader
                                DateTime? sessionDate = null;
                                if (!reader.IsDBNull(reader.GetOrdinal("SessionDate")))
                                    sessionDate = reader.GetDateTime("SessionDate");

                                DateTime? openSessionDateTime = null;
                                if (!reader.IsDBNull(reader.GetOrdinal("OpenSessionDateTime")))
                                    openSessionDateTime = reader.GetDateTime("OpenSessionDateTime");

                                string closeSessionDateTime = "STILL OPEN";
                                if (!reader.IsDBNull(reader.GetOrdinal("CloseSessionDateTime")))
                                    closeSessionDateTime = reader.GetDateTime("CloseSessionDateTime").ToString("yyyy-MM-dd HH:mm:ss");

                                string computerName = reader.IsDBNull(reader.GetOrdinal("ComputerName")) ? null : reader.GetString("ComputerName");
                                string closeStaff = reader.IsDBNull(reader.GetOrdinal("CloseStaff")) ? null : reader.GetString("CloseStaff");

                                string formattedSessionDate = sessionDate.HasValue ? sessionDate.Value.ToString("yyyy-MM-dd") : "";
                                string formattedOpenSessionDate = openSessionDateTime.HasValue ? openSessionDateTime.Value.ToString("yyyy-MM-dd HH:mm:ss") : "";

                                // Add the values to ListView
                                sessionReportListView.Items.Add(new
                                {
                                    Index = index,
                                    SessionDate = formattedSessionDate,
                                    OpenSessionDateTime = formattedOpenSessionDate,
                                    CloseSessionDateTime = closeSessionDateTime,
                                    ComputerName = computerName,
                                    CloseStaff = closeStaff
                                });;
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                MessageBox.Show("An error occurred: " + ex.Message);
            }
        }



    }
}
