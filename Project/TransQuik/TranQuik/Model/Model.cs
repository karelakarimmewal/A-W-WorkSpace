using Material.Icons;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace TranQuik.Model
{
    using Grpc.Core;
    using MySql.Data.MySqlClient;
    using System.Collections.Generic;
    using System.Drawing.Printing;
    using System.Linq;
    using TranQuik.Configuration;
    using TranQuik.Pages;

    public class ProductQuantityClass
    {
        private MainWindow mainWindow;
        public static int ProductQTY { get; private set; }

        public static void ProductQuantityGetted(MainWindow mainWindow)
        {
            mainWindow = mainWindow;
            int totalQuantity = int.Parse(mainWindow.productQuantitySelectorText.Text);
            if (totalQuantity <= 0)
            {
                ProductQTY = 1;
            }
            else
            {
                ProductQTY = totalQuantity;
            }
        }
    }

    public class CurrentComponentGroupItem
    {
        public static List<CurrentComponentGroupItem> CPGI = new List<CurrentComponentGroupItem>();

        public int CurrentSetGroupNo { get; private set; }
        public int CurrentSetGroupReq { get; private set; }
        public int CurrentSetGroupMinQty { get; private set; }
        public int CurrentSetGroupMaxQty { get; private set; }

        public CurrentComponentGroupItem(int currentSetGroupNo,int currentSetGroupReq,int currentSetGroupMinQty,int currentSetGroupMaxQty)
        {
            CurrentSetGroupNo = currentSetGroupNo;
            CurrentSetGroupReq = currentSetGroupReq;
            CurrentSetGroupMinQty = currentSetGroupMinQty;
            CurrentSetGroupMinQty = currentSetGroupMaxQty;
        }

    }

    public class SelectedComponentItems
    {
        public static List<SelectedComponentItems> SCI = new List<SelectedComponentItems>();

        public int CurrentComponentSetGroupNo { get; set; }
        public int  CurrentComponentID { get; set; }
        public string CurrentComponentName { get; set; }
        public decimal CurrentComponentPrice { get; set; }
        public int CurrentComponentQuantity { get; set; }

        public SelectedComponentItems(int currentComponentSetGroupNo, int currentComponentID, string currentComponentName, decimal currentComponentPrice, int currentComponentQuantity)
        {
            CurrentComponentSetGroupNo = currentComponentSetGroupNo;
            CurrentComponentID = currentComponentID;
            CurrentComponentName = currentComponentName;
            CurrentComponentPrice = currentComponentPrice;
            CurrentComponentQuantity = currentComponentQuantity;
        }
    }

    public class ReportProductPrice
    {
        public static List<ReportProductPrice> reportProductPrices = new List<ReportProductPrice>();

        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public decimal ProductPrice { get; set; }
        public string PrefixText { get; set; }
        public int SaleMode { get; set; } // Assuming SaleMode is an int

        public ReportProductPrice(int productID,string productName, decimal productPrice, int saleMode, string prefixText)
        {
            ProductID = productID;
            ProductName = productName;
            ProductPrice = productPrice;
            SaleMode = saleMode;
            PrefixText = prefixText;
        }

        public static void PopulateReports(LocalDbConnector localDbConnector, int ProductGroup, int ProductDept, int SaleMode, string DatePicker)
        {
            string baseQuery = "SELECT P.ProductID, P.ProductName, PP.ProductPrice, SM.PrefixText, PP.SaleMode " +
                "FROM products P " +
                "JOIN productprice PP ON P.ProductID = PP.ProductID " +
                "JOIN SaleMode SM ON PP.SaleMode = SM.SaleModeID";

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
                baseQuery += " ORDER BY P.ProductName, PP.SaleMode ASC;"; // Ensure space before ORDER
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
                                int productID = reader.GetInt32("ProductID");
                                string productName = reader.GetString("ProductName");
                                decimal productPrice = reader.GetDecimal("ProductPrice");
                                int saleMode = reader.GetInt32("SaleMode");
                                string prefixText = reader.GetString("PrefixText");

                                ReportProductPrice report = new ReportProductPrice(productID, productName, productPrice, saleMode, prefixText);
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

    public class Notification
    {
        public static void NotificationNotPermitted()
        {
            NotificationPopup notificationPopup = new NotificationPopup("USER ROLE NOT PERMITTED", false);
            notificationPopup.ShowDialog();
        }

        public static void NotificationLoginSuccess()
        {
            NotificationPopup notificationPopup = new NotificationPopup("LOGIN SUCCESSFULL", false);
            notificationPopup.ShowDialog();
        }

        public static void NotificationLoginFailed()
        {
            NotificationPopup notificationPopup = new NotificationPopup("LOGIN NAME OR PASSWORD IS INVALID", false);
            notificationPopup.ShowDialog();
        }

        public static void NotificationLoginAnotherUserIsActivate()
        {
            NotificationPopup notificationPopup = new NotificationPopup("CANT LOGIN BECAUSE THIS USER IS USED IN ANOTHER SESSION / NOT CLOSED", false);
            notificationPopup.ShowDialog();
        }

        public static void NotificationTransactionMustBeGreaterThanZero()
        {
            NotificationPopup notificationPopup = new NotificationPopup("TRANSACTION AMOUNT MUST BE GREATER THAN ZERO", false);
            notificationPopup.ShowDialog();
        }

        public static void NotificationFeatureNotAvailable()
        {
            NotificationPopup notificationPopup = new NotificationPopup("FOR NOW, THIS FEATURE IS NOT AVAILABLE YET", false);
            notificationPopup.ShowDialog();
        }
    }

    public class ShopData
    {
        public static List<ShopData> shopDatas = new List<ShopData>();

        public int ShopId { get; set; }
        public int BrandID { get; set; }
        public int MerchantID { get; set; }
        public string ShopCode { get; set; }
        public string ShopKey { get; set; }
        public string ShopName { get; set; }
        public int IsShop { get; set; }
        public int IsInv { get; set; }
        public string VATCode { get; set; }
        public int VATType { get; set; }
        public int MasterShop { get; set; }
        public int MasterShopLink { get; set; }
        public DateTime OpenHour { get; set; }
        public DateTime CloseHour { get; set; }
        public string CompanyName { get; set; }

        public ShopData(int shopId, int brandId, int merchantId, string shopCode, string shopKey, string shopName,
                        int isShop, int isInv, string vatCode, int vatType, int masterShop, int masterShopLink,
                        DateTime openHour, DateTime closeHour, string companyName)
        {
            ShopId = shopId; BrandID = brandId;
            MerchantID = merchantId; ShopCode = shopCode;
            ShopKey = shopKey; ShopName = shopName;
            IsShop = isShop; IsInv = isInv;
            VATCode = vatCode; VATType = vatType;
            MasterShop = masterShop; MasterShopLink = masterShopLink;
            OpenHour = openHour; CloseHour = closeHour;
            CompanyName = companyName;
        }

        public static void PopulateShopData(LocalDbConnector localDbConnector)
        {
            string queryShopData = "SELECT ShopID, BrandID, MerchantID, ShopCode, ShopKey, ShopName, IsShop, " +
                                   "IsInv, VATCode, VATType, MasterShop, MasterShopLink, OpenHour, CloseHour, " +
                                   "CompanyName FROM shop_data  ";

            try
            {
                shopDatas.Clear();
                using (MySqlConnection connection = localDbConnector.GetMySqlConnection())
                {
                    connection.Open();
                    using (MySqlCommand command = new MySqlCommand(queryShopData, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {

                            while (reader.Read())
                            {
                                int shopId = reader.GetInt32("ShopID");
                                int brandId = reader.GetInt32("BrandID");
                                int merchantId = reader.GetInt32("MerchantID");
                                string shopCode = reader.GetString("ShopCode");
                                string shopKey = reader.GetString("ShopKey");
                                string shopName = reader.GetString("ShopName");
                                int isShop = reader.GetInt32("IsShop");
                                int isInv = reader.GetInt32("IsInv");
                                string vatCode = reader.GetString("VATCode");
                                int vatType = reader.GetInt32("VATType");
                                int masterShop = reader.GetInt32("MasterShop");
                                int masterShopLink = reader.GetInt32("MasterShopLink");
                                DateTime openHour = reader.GetDateTime("OpenHour");
                                DateTime closeHour = reader.GetDateTime("CloseHour");
                                string companyName = reader.GetString("CompanyName");

                                ShopData shopData = new ShopData(shopId, brandId, merchantId, shopCode, shopKey,
                                                                 shopName, isShop, isInv, vatCode, vatType, masterShop,
                                                                 masterShopLink, openHour, closeHour, companyName);
                                shopDatas.Add(shopData);
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

    public class ProductGroupPopulate
    {
        public static List<ProductGroupPopulate> productGroupPopulates = new List<ProductGroupPopulate>();

        // Removed the instance variable for LocalDbConnector

        public int ProductGroupID
        {
            get; private set;
        }
        public int ShopID
        {
            get; private set;
        }
        public string ProductGroupCode
        {
            get; private set;
        }
        public string ProductGroupName
        {
            get; private set;
        }
        public bool ProductGroupActivate
        {
            get; private set;
        }

        public ProductGroupPopulate(int productGroupID, int shopID, string productGroupCode, string productGroupName, bool productGroupActivate)
        {
            ProductGroupID = productGroupID;
            ShopID = shopID;
            ProductGroupCode = productGroupCode;
            ProductGroupName = productGroupName;
            ProductGroupActivate = productGroupActivate;
        }

        public static void PopulateProductGroup(LocalDbConnector localDbConnector)
        {
            string queryProductGroup = "SELECT ProductGroupID, ShopID, ProductGroupCode, ProductGroupName, ProductGroupActivate FROM productgroup ORDER BY ProductGroupID ASC"; // Adjust the table name as needed

            try
            {
                productGroupPopulates.Clear();
                using (MySqlConnection connection = localDbConnector.GetMySqlConnection())
                {
                    connection.Open();
                    using (MySqlCommand command = new MySqlCommand(queryProductGroup, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int productGroupID = reader.GetInt32("ProductGroupID");
                                int shopID = reader.GetInt32("ShopID");
                                string productGroupCode = reader.GetString("ProductGroupCode");
                                string productGroupName = reader.GetString("ProductGroupName");
                                bool productGroupActivate = reader.GetBoolean("ProductGroupActivate");

                                ProductGroupPopulate productGroup = new ProductGroupPopulate(productGroupID, shopID, productGroupCode, productGroupName, productGroupActivate);
                                productGroupPopulates.Add(productGroup);
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

    public class ProductDeptPopulate
    {
        public static List<ProductDeptPopulate> productDeptPopulates = new List<ProductDeptPopulate>();

        public int ProductDeptID 
        { 
            get; set; 
        }
        public int ProductGroupID
        {
            get; set;
        }
        public int ShopID
        {
            get; set;
        }
        public string ProductDeptCode
        {
            get; set;
        }
        public string ProductDeptName
        {
            get; set;
        }
        public bool ProductDeptActivate
        {
            get; set;
        }
        public ProductDeptPopulate(int productDeptID, int productGroupID, int shopID, string productDeptCode, string productDeptName, bool productDeptActivate)
        {
            ProductDeptID = productDeptID;
            ProductGroupID = productGroupID;
            ShopID = shopID;
            ProductDeptCode = productDeptCode;
            ProductDeptName = productDeptName;
            ProductDeptActivate = productDeptActivate;
        }

        public static void PopulateProductDept(LocalDbConnector localDbConnector)
        {
            // Ensure the query handles multiple ProductGroupIDs correctly
            string queryProductGroup = "SELECT ProductDeptID, ProductGroupID, ShopID, ProductDeptCode, ProductDeptName, ProductDeptActivate FROM productdept " +
                "ORDER BY ProductDeptID ASC";

            try
            {
                productDeptPopulates.Clear();
                using (MySqlConnection connection = localDbConnector.GetMySqlConnection())
                {
                    connection.Open();
                    using (MySqlCommand command = new MySqlCommand(queryProductGroup, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int productDeptID = reader.GetInt32("ProductDeptID");
                                int productGroupID = reader.GetInt32("ProductGroupID");
                                int shopID = reader.GetInt32("ShopID");
                                string productDeptCode = reader.GetString("ProductDeptCode");
                                string productDeptName = reader.GetString("ProductDeptName");
                                bool productDeptActivate = reader.GetBoolean("ProductDeptActivate");

                                ProductDeptPopulate productDeptPopulate = new ProductDeptPopulate(productDeptID, productGroupID, shopID, productDeptCode, productDeptName, productDeptActivate);
                                productDeptPopulates.Add(productDeptPopulate);
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

    public class TimerForScheduler
    {
        public int Times { get; set; } = 5000;
        
    }

    public class SessionsID
    {
        public static int SessionIDs { get; set; }
    }

    public class UserAuth
    {
        private readonly LocalDbConnector dbConnector;

        public UserAuth()
        {
            dbConnector = new LocalDbConnector();
        }

        public async Task<(bool isLogged, int staffID, int staffRoleID, string staffFirstName, string staffLastName)> AuthenticateUserAsync(string userName, string userPassword)
        {
            try
            {
                // SQL query to authenticate the user and retrieve their details
                string query = @"
                SELECT StaffID, StaffRoleID, StaffFirstName, StaffLastName 
                FROM staffs 
                WHERE StaffLogin = @StaffLogin ";

                using (MySqlConnection connection = dbConnector.GetMySqlConnection())
                {
                    await connection.OpenAsync();

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        // Add parameters to prevent SQL injection
                        command.Parameters.AddWithValue("@StaffLogin", userName);
                        //command.Parameters.AddWithValue("@StaffPassword", userPassword);

                        using (MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                return (
                                    isLogged: true,
                                    staffID: reader.GetInt32("StaffID"),
                                    staffRoleID: reader.GetInt32("StaffRoleID"),
                                    staffFirstName: reader.GetString("StaffFirstName"),
                                    staffLastName: reader.GetString("StaffLastName")
                                );
                            }
                            else
                            {
                                // If no user is found, return a tuple with false and default values
                                return (isLogged: false, staffID: 0, staffRoleID: 0, staffFirstName: null, staffLastName: null);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions
                Console.WriteLine("Error: " + ex.Message);
                return (isLogged: false, staffID: 0, staffRoleID: 0, staffFirstName: null, staffLastName: null);
            }
        }
    }

    public class UserSessions
    {
        public int Authentication_StaffID { get; set; }
        public int Authentication_StaffRoleID { get; set; }
        public string Authentication_StaffFirstName { get; set; }
        public string Authentication_StaffLastName { get; set; }
        public DateTime Authentication_OpenSessionDate { get; set; }


        public static int Current_StaffID{ get; set; }
        public static int Current_StaffRoleID { get; set; }
        public static string Current_StaffFirstName { get; set; } 
        public static string Current_StaffLastName { get; set; }
        public static DateTime Current_OpenSessionDate { get; set; }

        public void CurrentSession(int StaffID, int StaffRoleID,string StaffFirstName, string StaffLastName, DateTime OpenSessionDate)
        {
            Current_StaffID = StaffID;
            Current_StaffRoleID = StaffRoleID;
            Current_StaffFirstName = StaffFirstName;
            Current_StaffLastName = StaffLastName;
            Current_OpenSessionDate = OpenSessionDate;
        }

        public void AuthenticationSession(int StaffID, int StaffRoleID, string StaffFirstName, string StaffLastName)
        {
            Authentication_StaffID = StaffID;
            Authentication_StaffRoleID = StaffRoleID;
            Authentication_StaffFirstName = StaffFirstName;
            Authentication_StaffLastName = StaffLastName;
            Authentication_OpenSessionDate = DateTime.Now;
        }

    }

    public class ComputerAccessData
    {
        public static List<ComputerAccessData> ComputerAccessDatas = new List<ComputerAccessData>();

        public int ComputerID { get; set; }
        public string ComputerName { get; set; }
        public int ShopID { get; set; }
        public int ComputerType { get; set; }
        public string PayTypeList { get; set; }
        public string SaleModeList { get; set; }
        public string TableZoneList { get; set; }
        public string IPAddress { get; set; }
        public string RegistrationNumber { get; set; }
        public string ReceiptHeader { get; set; }
        public string FullTaxHeader { get; set; }
        public string DeviceCode { get; set; }
        public byte KDSID { get; set; }
        public string KDSPrinters { get; set; }
        public string Description { get; set; }
        public string ProductGroupList { get; set; }
        public string FavoriteImagePageList { get; set; }
        public string FavoriteTextPageList { get; set; }
        public bool Deleted { get; set; }

        public ComputerAccessData(int computerID, string computerName, int shopID, int computerType, string payTypeList, string saleModeList, string tableZoneList, string ipAddress, string registrationNumber, string receiptHeader, string fullTaxHeader, string deviceCode, byte kDSID, string kDSPrinters, string description, string productGroupList, string favoriteImagePageList, string favoriteTextPageList, bool deleted)
        {
            ComputerID = computerID;
            ComputerName = computerName;
            ShopID = shopID;
            ComputerType = computerType;
            PayTypeList = payTypeList;
            SaleModeList = saleModeList;
            TableZoneList = tableZoneList;
            IPAddress = ipAddress;
            RegistrationNumber = registrationNumber;
            ReceiptHeader = receiptHeader;
            FullTaxHeader = fullTaxHeader;
            DeviceCode = deviceCode;
            KDSID = kDSID;
            KDSPrinters = kDSPrinters;
            Description = description;
            ProductGroupList = productGroupList;
            FavoriteImagePageList = favoriteImagePageList;
            FavoriteTextPageList = favoriteTextPageList;
            Deleted = deleted;
        }

        public static void PopulateComputerAccessData(LocalDbConnector localDbConnector)
        {
            string queryComputerAccess = "SELECT ComputerID, ComputerName, ShopID, ComputerType, PayTypeList, SaleModeList, TableZoneList, IPAddress, RegistrationNumber, ReceiptHeader, FullTaxHeader, DeviceCode, KDSID, KDSPrinters, Description, ProductGroupList, FavoriteImagePageList, FavoriteTextPageList, Deleted FROM computername ORDER BY ComputerID ASC";

            try
            {
                ComputerAccessDatas.Clear();
                using (MySqlConnection connection = localDbConnector.GetMySqlConnection())
                {
                    connection.Open();
                    using (MySqlCommand command = new MySqlCommand(queryComputerAccess, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int computerID = reader.GetInt32("ComputerID");
                                string computerName = reader.GetString("ComputerName");
                                int shopID = reader.GetInt32("ShopID");
                                int computerType = reader.GetInt32("ComputerType");
                                string payTypeList = reader.GetString("PayTypeList");
                                string saleModeList = reader.GetString("SaleModeList");
                                string tableZoneList = reader.GetString("TableZoneList");
                                string ipAddress = reader.GetString("IPAddress");
                                string registrationNumber = reader.GetString("RegistrationNumber");
                                string receiptHeader = reader.GetString("ReceiptHeader");
                                string fullTaxHeader = reader.GetString("FullTaxHeader");
                                string deviceCode = reader.GetString("DeviceCode");
                                byte kDSID = reader.GetByte("KDSID");
                                string kDSPrinters = reader.GetString("KDSPrinters");
                                string description = reader.GetString("Description");
                                string productGroupList = reader.GetString("ProductGroupList");
                                string favoriteImagePageList = reader.GetString("FavoriteImagePageList");
                                string favoriteTextPageList = reader.GetString("FavoriteTextPageList");
                                bool deleted = reader.GetBoolean("Deleted");

                                ComputerAccessData computerAccessData = new ComputerAccessData(computerID, computerName, shopID, computerType, payTypeList, saleModeList, tableZoneList, ipAddress, registrationNumber, receiptHeader, fullTaxHeader, deviceCode, kDSID, kDSPrinters, description, productGroupList, favoriteImagePageList, favoriteTextPageList, deleted);
                                ComputerAccessDatas.Add(computerAccessData);
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

    public class FilteredPayType
    {
        public int PayTypeID { get; set; }
        public string DisplayName { get; set; }
        public bool IsAvailable { get; set; }

        public FilteredPayType(int payTypeID, string displayName, bool isAvailable)
        {
            PayTypeID = payTypeID;
            DisplayName = displayName;
            IsAvailable = isAvailable;
        }
    }

    public class ProductComponentGroup
    {
        public int PGroupID { get; set; }
        public int ProductID { get; set; }
        public string SaleMode { get; set; }
        public string SetGroupName { get; set; }
        public int SetGroupNo { get; set; }
        public int RequireAddAmountForProduct { get; set; }
        public int MinQty { get; set; }
        public int MaxQty { get; set; }
    }

    public class ProductComponentSelectedItems
    {
        public int ID { get ; set ; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int SetGroupNo { get; set; }
        public int Quantity { get; set; }
    }

    public class ProductComponentProduct
    {
        public int ProductComponentProductPGroupID { get; set; }
        public int ProductComponentProductID { get; set; }
        public string ProductComponentProductName { get; set; }
        public decimal ProductComponentProductPrice { get; set; }
        public int ProductComponentProductSetGroupNo { get; set; }
        public int ProductComponentProductQuantity { get; set; } = 0;

    }

    public static class ReportManager
    {
        public static Dictionary<string, PackIconKind> GetReportManagerItem()
        {
            return new Dictionary<string, PackIconKind>
        {
            { "End Day Report", PackIconKind.CalendarAlert },
            { "Session Report", PackIconKind.PersonAlert },
            { "Receipt Report", PackIconKind.Receipt },
            { "Sales By Prod Report", PackIconKind.SaleBox},
            { "Product Hourly Report", PackIconKind.ClockAlert},
            { "Product Price Report", PackIconKind.DocumentSign },
            { "Sales Type Report", PackIconKind.FileDocumentAlert },

        };
        }
    }

    public static class UtilityManager
    {
        public static Dictionary<string, PackIconKind> GetUtilityManagerItem()
        {
            return new Dictionary<string, PackIconKind>
        {
            { "Edit Initial Cash", PackIconKind.HandCoin },
            { "Manual Drop Cash", PackIconKind.CashChargeback },
            { "Open Cash Drawer", PackIconKind.SlotMachine },
            { "Void Receipt", PackIconKind.FileAlert},
            { "Cash Drawer", PackIconKind.SlotMachineOutline},
            { "Import Master Data", PackIconKind.DatabaseAdd },
            { "Clear Sales Data", PackIconKind.TrashEmpty },
        };
        }
    }

    public class Customer
    {
        public int CustomerId { get; private set; }
        public DateTime Time { get; private set; }

        public Customer(DateTime time)
        {
            Time = time;
        }

        public async Task<int> LoadLastCustomerData()
        {
            LocalDbConnector dbConnector = new LocalDbConnector();

            try
            {
                string getLastSessionIdQuery = "SELECT MAX(NoCustomer) FROM ordertransaction";

                int newCustomerID;

                using (MySqlConnection connection = dbConnector.GetMySqlConnection())
                {
                    await connection.OpenAsync();

                    using (MySqlCommand getLastSessionIdCommand = new MySqlCommand(getLastSessionIdQuery, connection))
                    {
                        object result = await getLastSessionIdCommand.ExecuteScalarAsync();

                        newCustomerID = result != DBNull.Value ? Convert.ToInt32(result) + 1 : 1;

                        CustomerId = newCustomerID;
                    }
                }

                return newCustomerID; // Return the generated transaction ID
            }
            catch (MySqlException ex)
            {
                // Handle MySQL exceptions
                Console.WriteLine("MySQL Error: " + ex.Message);
                throw; // Rethrow the exception for higher-level handling
            }
            catch (Exception ex)
            {
                // Handle any other exceptions
                Console.WriteLine("Error: " + ex.Message);
                throw; // Rethrow the exception for higher-level handling
            }
        }

        //private void SaveLastCustomerData()
        //{
        //    var data = new { LastCustomerId = lastCustomerId, LastCreationDate = lastCreationDate };
        //    string jsonData = JsonConvert.SerializeObject(data);

        //    // Encrypt the data before writing to file
        //    byte[] encryptedData = EncryptStringToBytes_Aes(jsonData, key, iv);

        //    Directory.CreateDirectory(directoryPath);
        //    File.WriteAllBytes(filePath, encryptedData);
        //}

        //private void LoadLastCustomerData()
        //{
        //    if (File.Exists(filePath))
        //    {
        //        byte[] encryptedData = File.ReadAllBytes(filePath);

        //        // Decrypt the data after reading from file
        //        string decryptedData = DecryptStringFromBytes_Aes(encryptedData, key, iv);

        //        var data = JsonConvert.DeserializeObject<dynamic>(decryptedData);
        //        lastCustomerId = data.LastCustomerId;
        //        lastCreationDate = data.LastCreationDate;
        //    }
        //}

        //private static byte[] EncryptStringToBytes_Aes(string plainText, byte[] key, byte[] iv)
        //{
        //    if (plainText == null || plainText.Length <= 0)
        //        throw new ArgumentNullException(nameof(plainText));
        //    if (key == null || key.Length <= 0)
        //        throw new ArgumentNullException(nameof(key));
        //    if (iv == null || iv.Length <= 0)
        //        throw new ArgumentNullException(nameof(iv));

        //    using (Aes aesAlg = Aes.Create())
        //    {
        //        aesAlg.Key = key;
        //        aesAlg.IV = iv;

        //        ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

        //        using (MemoryStream msEncrypt = new MemoryStream())
        //        {
        //            using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
        //            {
        //                using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
        //                {
        //                    swEncrypt.Write(plainText);
        //                }
        //                return msEncrypt.ToArray();
        //            }
        //        }
        //    }
        //}

        //private static string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] key, byte[] iv)
        //{
        //    if (cipherText == null || cipherText.Length <= 0)
        //        throw new ArgumentNullException(nameof(cipherText));
        //    if (key == null || key.Length <= 0)
        //        throw new ArgumentNullException(nameof(key));
        //    if (iv == null || iv.Length <= 0)
        //        throw new ArgumentNullException(nameof(iv));

        //    using (Aes aesAlg = Aes.Create())
        //    {
        //        aesAlg.Key = key;
        //        aesAlg.IV = iv;

        //        ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

        //        using (MemoryStream msDecrypt = new MemoryStream(cipherText))
        //        {
        //            using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
        //            {
        //                using (StreamReader srDecrypt = new StreamReader(csDecrypt))
        //                {
        //                    return srDecrypt.ReadToEnd();
        //                }
        //            }
        //        }
        //    }
        //}
    }

    public class CurrentTransaction
    {
        public static decimal NeedToPay {get; private set; }
        public static decimal TaxAmount { get; private set; }
        public CurrentTransaction(decimal needToPay, decimal taxAmount)
        {
            NeedToPay = needToPay;
            TaxAmount = taxAmount;
        }
    }

    public class CustomerTransactionDetail
    {
        public static int PaymentID { get; private set; }
        public static decimal CustomerPayAmount { get; private set; }

        public static void setCustomerTransactionID(int paymentID, decimal customerPayAmount)
        {
            paymentID = paymentID;
            CustomerPayAmount = customerPayAmount;
        }
    }

    public class PaymentDetails
    {
        public int PaymentTypeID { get; private set; }
        public string PaymentTypeName { get; private set; }
        public decimal PaymentAmount { get; private set; }
        public bool PaymentIsAcitve { get; set; }
        public string PaymentPayRemarks{ get; private set; }
        public PaymentDetails(int paymentTypeID, string paymentTypeName, decimal paymentAmount, bool paymentIsActive = true, string paymentPayRemarks = null)
        {
            PaymentTypeID = paymentTypeID;
            PaymentTypeName = paymentTypeName;
            PaymentAmount = paymentAmount;
            PaymentIsAcitve = paymentIsActive;
            PaymentPayRemarks = paymentPayRemarks;
        }
    }

    public class SettQuantity
    {
        public static void SettQuantityCTOR(Product product)
        {
            QuantitySelector quantitySelector = new QuantitySelector(product);
            quantitySelector.ShowDialog();
        }
    }

    public class Product
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal ProductPrice { get; set; }
        public int ProductComponentLevel { get; set; }
        public string ProductButtonColor { get; set; }
        public int Quantity { get; set; } = 1; // Default quantity is 1
        public bool Status { get; set; } // Status property

        public DateTime dateTime { get; private set; }


        public List<ChildItem> ChildItems { get; set; } // List of child items for the product

        public bool HasChildItems()
        {
            return ChildItems != null && ChildItems.Count > 0;
        }

        public Product(int productId, string productName, decimal productPrice, string productButtonColor, int productComponentLevel = 1)
        {
            ProductId = productId;
            ProductName = productName;
            ProductPrice = productPrice;
            ProductComponentLevel = productComponentLevel;
            ProductButtonColor = productButtonColor;
            Status = true; // Default status is Active
            ChildItems = new List<ChildItem>(); // Initialize child items list
            dateTime = DateTime.Now;
        }
    }

    public class ChildItem
    {
        public int ChildId { get; set; } 
        public string ChildName { get; set; }
        public decimal ChildPrice { get; set; }
        public int ChildSetGroupNo { get; set; }
        public int ChildPGroupID { get; set; }
        public int ChildQuantity { get; set; }
        public bool ChildStatus { get; set; }
        public DateTime dateTime { get; set; }
        public ChildItem(int childID, string childName, decimal childPrice, int childQuantity, bool childStatus, int childSetGroupNo, int childPGroupID)
        {
            ChildId = childID;
            ChildName = childName;
            ChildQuantity = childQuantity;
            ChildPrice = childPrice;
            ChildStatus = childStatus;
            ChildSetGroupNo = childSetGroupNo;
            ChildPGroupID = childPGroupID;
            dateTime = DateTime.Now;
        }
    }

    public class Cart
    {
        public static int lastCartId ; // Static field to track the last used CartID
        private static DateTime lastCreationDate; // Static field to track the date of the last cart creation
        private static readonly string directoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
        private static readonly string cartFilePath = Path.Combine(directoryPath, "c.json"); // File to store cart data
        private static readonly string idFilePath = Path.Combine(directoryPath, "last_cart_id.txt"); // File to store lastCartId
        private static readonly byte[] key = Encoding.UTF8.GetBytes("0123456789abcdef0123456789abcdef"); // 32 bytes for AES-256
        private static readonly byte[] iv = Encoding.UTF8.GetBytes("abcdef9876543210"); // 16 bytes for AES
        private static bool encryptionEnabled = false; // Flag to enable/disable encryption

        public int CartID { get; set; }
        public int CustomerID { get; set; }
        public List<Product> Products { get; set; }
        public int SaleModes { get; set; }
        public string TStatus { get; set; }

        public Cart(int customerID, List<Product> products, int saleModes, bool isReset, string tStatus)
        {
            CustomerID = customerID;
            Products = products;
            SaleModes = saleModes;
            TStatus = tStatus;

            LoadDataFromFile(); // Load the lastCartId from file
            Console.WriteLine(lastCreationDate);
            // Incremental approach for the first cart of the day
            

            if (isReset)
            {
                // Reset the isReset flag
                isReset = false;

                // Increment the lastCartId to create a new CartID
                lastCartId++;
                // Create a new Cart instance with the new CartID
                CartID = lastCartId;
                SaveDataToFile(); // Save lastCartId to file
            }
            else
            {
                if (lastCreationDate.Date != DateTime.Today)
                {
                    lastCartId = 1;
                    lastCreationDate = DateTime.Today;
                }
                CartID = lastCartId;
                SaveLastCartData(); // Save last cart data to file
            }
        }

        // Method to load the lastCartId from file
        private static void LoadDataFromFile()
        {
            if (File.Exists(idFilePath))
            {
                string[] lines = File.ReadAllLines(idFilePath);
                if (lines.Length >= 2)
                {
                    if (int.TryParse(lines[0], out int id))
                    {
                        lastCartId = id;
                    }
                    if (DateTime.TryParse(lines[1], out DateTime date))
                    {
                        lastCreationDate = date;
                    }
                }
            }
        }

        // Method to save the lastCartId to file
        private static void SaveDataToFile()
        {
            try
            {
                Directory.CreateDirectory(directoryPath);
                string[] lines = { lastCartId.ToString(), DateTime.Now.ToString() };
                File.WriteAllLines(idFilePath, lines);
            }
            catch (Exception ex)
            {
                // Handle any exceptions that might occur during file write
                // For example:
                Console.WriteLine("Error saving data to file: " + ex.Message);
            }
        }

        private void SaveLastCartData()
        {
            var data = new
            {
                CartID,
                CustomerID,
                Products,
                SaleModes,
                TStatus
            };

            string jsonData = JsonConvert.SerializeObject(data) + Environment.NewLine;

            if (encryptionEnabled)
            {
                // Encrypt the data before writing to file
                byte[] encryptedData = EncryptStringToBytes_Aes(jsonData, key, iv);

                Directory.CreateDirectory(directoryPath);

                // Use FileStream with FileMode.Append to append data to the file
                using (var fileStream = new FileStream(cartFilePath, FileMode.Append))
                {
                    fileStream.Write(encryptedData, 0, encryptedData.Length);
                }
            }
            else
            {
                // Save data without encryption
                File.AppendAllText(cartFilePath, jsonData);
            }
        }

        // Method to load last cart data from file
        private void LoadLastCartData()
        {
            if (File.Exists(cartFilePath))
            {
                byte[] encryptedData = File.ReadAllBytes(cartFilePath);

                if (encryptionEnabled)
                {
                    // Decrypt the data after reading from file
                    string decryptedData = DecryptStringFromBytes_Aes(encryptedData, key, iv);

                    var data = JsonConvert.DeserializeObject<dynamic>(decryptedData);
                    lastCartId = data.CartID;
                    lastCreationDate = DateTime.Today; // Assuming there's no date stored in the encrypted data
                }
                else
                {
                    // Read data without decryption
                    string jsonData = File.ReadAllText(cartFilePath);
                    var data = JsonConvert.DeserializeObject<dynamic>(jsonData);
                    lastCartId = data.CartID;
                    lastCreationDate = DateTime.Today; // Assuming there's no date stored in the unencrypted data
                }
            }
        }

        // Methods for encryption and decryption
        private static byte[] EncryptStringToBytes_Aes(string plainText, byte[] key, byte[] iv)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                        return msEncrypt.ToArray();
                    }
                }
            }
        }

        private static string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] key, byte[] iv)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
        }

        public static void ToggleEncryption(bool enableEncryption)
        {
            encryptionEnabled = enableEncryption;
        }
    }

    public class SaleMode
    {
        public static List<SaleMode> populateSaleModeList = new List<SaleMode>();

        public int SaleModeID
        {
            get; set;
        }
        public string SaleModeName
        {
            get; set;
        }
        public string PrefixText
        {
            get; set;
        }
        public string PrefixQueue
        {
            get; set;
        }
        public string ReceiptHeaderText
        {
            get; set;
        }
        public string PayTypeList
        {
            get; set;
        }
        public string NotInPayTypeList
        {
            get; set;
        }

        public SaleMode(int saleModeID, string saleModeName, string prefixText, string prefixQueue, string receiptHeaderText, string payTypeList, string notInPayTypeList)
        {
            SaleModeID = saleModeID;
            SaleModeName = saleModeName;
            PrefixText = prefixText;
            PrefixQueue = prefixQueue;
            ReceiptHeaderText = receiptHeaderText;
            PayTypeList = payTypeList;
            NotInPayTypeList = notInPayTypeList;
        }

        public static void PopulateSaleMode(LocalDbConnector localDbConnector)
        {
            string querySaleMode = "SELECT SaleModeID, SaleModeName, PrefixText, PrefixQueue, ReceiptHeaderText, PayTypeList, NotInPayTypeList FROM salemode ORDER BY SaleModeID ASC"; // Adjust the table name as needed

            try
            {
                populateSaleModeList.Clear();
                using (MySqlConnection connection = localDbConnector.GetMySqlConnection())
                {
                    connection.Open();
                    using (MySqlCommand command = new MySqlCommand(querySaleMode, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int saleModeID = reader.GetInt32("SaleModeID");
                                string saleModeName = reader.GetString("SaleModeName");
                                string prefixText = reader.GetString("PrefixText");
                                string prefixQueue = reader.GetString("PrefixQueue");
                                string receiptHeaderText = reader.GetString("ReceiptHeaderText");
                                string payTypeList = reader.GetString("PayTypeList");
                                string notInPayTypeList = reader.GetString("NotInPayTypeList");

                                SaleMode saleMode = new SaleMode(saleModeID, saleModeName, prefixText, prefixQueue, receiptHeaderText, payTypeList, notInPayTypeList);
                                populateSaleModeList.Add(saleMode);
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

    public class ModifierGroup
    {
        public string ModifierGroupID { get; set; }
        public string ModifierGroupCode { get; set; }
        public string ModifierName { get; set; }
    }

    public class ModifierMenu
    {
        public int ModifierMenuCode { get; set; }
        public string ModifierMenuName { get; set; }
        public decimal ModifierMenuPrice { get; set; }
        public int ModifierMenuQuantity { get; set; } = 0;
    }

    public class HeldCart
    {
        public int CustomerId { get; set; }
        public DateTime TimeStamp { get; set; }
        public Dictionary<int, Product> CartProducts { get; set; } // Change the type here
        public int SalesMode { get; set; } // New property for SalesMode
        public string SalesModeName { get; set; }
        public string PrefixText { get; set; }

        public HeldCart(int customerId, DateTime timeStamp, Dictionary<int, Product> cartProducts, int salesMode, string salesModeName, string prefixText)
        {
            CustomerId = customerId;
            TimeStamp = timeStamp;
            CartProducts = cartProducts;
            SalesMode = salesMode; // Assign SalesMode
            SalesModeName = salesModeName;
            PrefixText = prefixText;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Customer: {CustomerId}");
            sb.AppendLine($"TimeStamp: {TimeStamp}");
            sb.AppendLine($"SalesMode: {SalesMode}"); // Include SalesMode in the string representation
            sb.AppendLine($"SalesModeName: {SalesModeName}"); // Include SalesMode in the string representation
            sb.AppendLine($"PrefixText: {PrefixText}"); // Include SalesMode in the string representation
            sb.AppendLine("CartProduct:");
            foreach (var product in CartProducts.Values)
            {
                sb.AppendLine($"{product.ProductName}");
            }
            return sb.ToString();
        }
    }

    public class SaleModeIconMapper
    {
        // Dictionary to store SaleModeID to MaterialIconKind and Brush mappings
        private Dictionary<int, (MaterialIconKind, Brush)> iconMappings = new Dictionary<int, (MaterialIconKind, Brush)>
    {
        { 2, (MaterialIconKind.Motorbike, (Brush)Application.Current.FindResource("AccentColor")) },             // Example color: Red
        { 9, (MaterialIconKind.Car, (Brush)Application.Current.FindResource("SuccessColor")) },                   // Example color: Blue
        { 1, (MaterialIconKind.Restaurant, (Brush)Application.Current.FindResource("PrimaryButtonColor")) },           // Example color: Green
        { 3, (MaterialIconKind.PackageDelivered, (Brush)Application.Current.FindResource("ErrorColor")) },    // Example color: Orange
        { 10, (MaterialIconKind.Shopping,(Brush)Application.Current.FindResource("ButtonEnabledColor1")) },           // Example color: Purple
        { 11, (MaterialIconKind.Shopping, (Brush)Application.Current.FindResource("ButtonEnabledColor1")) },           // Example color: Yellow
        { 12, (MaterialIconKind.Shopping,(Brush)Application.Current.FindResource("ButtonEnabledColor1")) },             // Example color: Cyan
        { 13, (MaterialIconKind.Shopping, (Brush)Application.Current.FindResource("ButtonEnabledColor1")) },           // Example color: Magenta
        { 14, (MaterialIconKind.Shopping, (Brush)Application.Current.FindResource("ButtonEnabledColor1")) }             // Example color: Brown
    };

        // Method to retrieve the MaterialIconKind for a given SaleModeID
        public MaterialIconKind GetIconForSaleMode(int saleModeID)
        {
            // Check if the SaleModeID exists in the iconMappings dictionary
            if (iconMappings.ContainsKey(saleModeID))
            {
                return iconMappings[saleModeID].Item1; // Return the MaterialIconKind
            }
            else
            {
                // Default to MaterialIconKind.Restaurant if SaleModeID is not found
                return MaterialIconKind.Restaurant;
            }
        }

        // Method to retrieve the Brush (color) for a given SaleModeID
        public Brush GetColorForSaleMode(int saleModeID)
        {
            // Check if the SaleModeID exists in the iconMappings dictionary
            if (iconMappings.ContainsKey(saleModeID))
            {
                return iconMappings[saleModeID].Item2; // Return the Brush (color)
            }
            else
            {
                // Default color (if SaleModeID is not found, return a fallback color)
                return Brushes.Orange; // Example fallback color: Black
            }
        }
    }

    public class StaffRoleManager
    {
        public static List <StaffRoleManager> staffRoleManagers = new List <StaffRoleManager> ();
        public static int StaffRoleID { get; private set; }
        public static int PermissionItemID { get; private set; }
        public static int PermissionItemNameID { get; private set; }
        public static string PermissionItemName { get; private set; }

        public StaffRoleManager(int staffRoleID, int permissionItemID, int permissionItemNameID, string permissionItemName)
        {
            StaffRoleID = staffRoleID;
            PermissionItemID = permissionItemID;
            PermissionItemNameID = permissionItemNameID;
            PermissionItemName = permissionItemName;
        }

        public static void SetStaffRoleManager(LocalDbConnector localDbConnector)
        {
            string rolemanagerquery = "SELECT SP.StaffRoleID, SP.PermissionItemID, PIN.PermissionItemNameID, PIN.PermissionItemName FROM staffpermission SP " +
                "JOIN permissionitemname PIN " +
                "ON SP.`PermissionItemID` = PIN.`PermissionItemID` " +
                "WHERE LangID = 1 " +
                "ORDER BY StaffRoleID, SP.PermissionItemID ASC";
            try
            {
                staffRoleManagers.Clear();
                using (MySqlConnection connection = localDbConnector.GetMySqlConnection())
                {
                    connection.Open();
                    using (MySqlCommand command = new MySqlCommand(rolemanagerquery, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int staffRoleID = reader.GetInt32("staffRoleID");
                                int permissionItemID = reader.GetInt32("permissionItemID");
                                int permissionItemNameID = reader.GetInt32("permissionItemNameID");
                                string permissionItemName = reader.GetString("permissionItemName");

                                StaffRoleManager staffRoleManager = new StaffRoleManager(staffRoleID, permissionItemID, permissionItemNameID, permissionItemName);
                                staffRoleManagers.Add(staffRoleManager);
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

    public class ThermalPrinter
    {
        private int PaperWidth { get; set; }
        private int PaperHeight { get; set; } // New property for dynamic paper height

        private List<ReportProductPrice> reportProductPrices; // Updated to instance variable

        public ThermalPrinter()
        {
            PaperWidth = 576;
            PaperHeight = 0; // Initialize with zero height
            reportProductPrices = ReportProductPrice.reportProductPrices; // Initialize the list
        }

        public void TemplateReceipt(string receiptNumber)
        {
            var doc = new PrintDocument();
            PrintController printController = new StandardPrintController();
            doc.PrintController = printController;

            doc.PrintPage += (sender, e) =>
            {
                StringBuilder sb = new StringBuilder();
                PaperHeight = ProvideReceiptContent(sb, receiptNumber);
                DrawContent(e.Graphics, sb.ToString());
            };

            if (Properties.Settings.Default._PrinterDevMode)
            {
                // Create a PrintPreviewDialog
                var printPreviewDialog = new System.Windows.Forms.PrintPreviewDialog();
                printPreviewDialog.Document = doc;

                //// Show the print preview dialog
                printPreviewDialog.ShowDialog();
            }
            else
            {
                doc.Print();
            }
        }

        public void TemplateReport(string reportTitle)
        {
            var doc = new PrintDocument();
            PrintController printController = new StandardPrintController();
            doc.PrintController = printController;

            doc.PrintPage += (sender, e) =>
            {
                StringBuilder sb = new StringBuilder();
                PaperHeight = ProvideReportContent(sb, reportTitle);
                DrawContent(e.Graphics, sb.ToString());
            };

            if (Properties.Settings.Default._PrinterDevMode)
            {
                // Create a PrintPreviewDialog
                var printPreviewDialog = new System.Windows.Forms.PrintPreviewDialog();
                printPreviewDialog.Document = doc;

                //// Show the print preview dialog
                printPreviewDialog.ShowDialog();
            }
            else
            {
                doc.Print();
            }
        }

        private int ProvideReceiptContent(StringBuilder sb, string receiptNumber)
        {
            var settings = Config.LoadPrinterSettings();
            string businessAddress = settings.ContainsKey("BusinessAddress") ? settings["BusinessAddress"] : "Default Address";
            string businessPhone = settings.ContainsKey("BusinessPhone") ? settings["BusinessPhone"] : "Default Phone";

            sb.AppendLine($"{"=".PadRight(PaperWidth / 12, '=')}");
            sb.AppendLine($"Receipt Number : {receiptNumber}");
            sb.AppendLine($"Date           : {DateTime.Now}");
            sb.AppendLine($"Cashier        : {UserSessions.Current_StaffFirstName} {UserSessions.Current_StaffLastName}");
            sb.AppendLine($"Shop Name      : {Properties.Settings.Default._ShopName}");
            sb.AppendLine($"=".PadRight(PaperWidth / 12, '='));
            sb.AppendLine();

            AddReceiptItems(sb);

            sb.AppendLine($"=".PadRight(PaperWidth / 12, '='));
            sb.AppendLine($"{"Total:".PadRight(20)}{CalculateTotal().ToString("C")}");

            // Calculate height based on number of lines
            int numLines = sb.ToString().Split(new[] { Environment.NewLine }, StringSplitOptions.None).Length;
            return numLines * 14; // Assuming 14 pixels per line height
        }

        private void AddReceiptItems(StringBuilder sb)
        {
            const int FIRST_COL_WIDTH = 22;
            const int SECOND_COL_WIDTH = 2;
            const int FOURTH_COL_WIDTH = 15;

            foreach (var item in reportProductPrices)
            {
                string itemName = item.ProductName.Length > FIRST_COL_WIDTH + SECOND_COL_WIDTH ? item.ProductName.Substring(0, FIRST_COL_WIDTH + SECOND_COL_WIDTH - 1) : item.ProductName;
                sb.AppendLine($"{itemName.PadRight(FIRST_COL_WIDTH + SECOND_COL_WIDTH)}{item.ProductPrice.ToString("C").PadLeft(FOURTH_COL_WIDTH)}");
            }
        }

        private decimal CalculateTotal()
        {
            return reportProductPrices.Sum(item => item.ProductPrice);
        }

        private int ProvideReportContent(StringBuilder sb, string reportTitle)
        {
            sb.AppendLine($"{"=".PadRight(PaperWidth / 12, '=')}");
            sb.AppendLine($"Report         : {reportTitle}");
            sb.AppendLine($"Date           : {DateTime.Now}");
            sb.AppendLine($"Session        : {UserSessions.Current_StaffFirstName} {UserSessions.Current_StaffLastName}");
            sb.AppendLine($"Shop Name      : {Properties.Settings.Default._ShopName}");
            sb.AppendLine($"=".PadRight(PaperWidth / 12, '='));
            sb.AppendLine();

            AddReportItems(sb);

            sb.AppendLine($"=".PadRight(PaperWidth / 12, '='));

            // Calculate height based on number of lines
            int numLines = sb.ToString().Split(new[] { Environment.NewLine }, StringSplitOptions.None).Length;
            return numLines * 14; // Assuming 14 pixels per line height
        }

        private void AddReportItems(StringBuilder sb)
        {
            const int FIRST_COL_WIDTH = 22;
            const int FOURTH_COL_WIDTH = 15;

            foreach (var item in reportProductPrices)
            {
                sb.AppendLine($"{item.ProductName.PadRight(FIRST_COL_WIDTH)}{item.ProductPrice.ToString("C").PadLeft(FOURTH_COL_WIDTH)}");
            }
        }

        private void DrawContent(System.Drawing.Graphics graphics, string content)
        {
            using (var stringFormat = new System.Drawing.StringFormat())
            {
                System.Drawing.RectangleF rect = new System.Drawing.RectangleF(0, 0, PaperWidth, PaperHeight);
                graphics.DrawString(content, new System.Drawing.Font(System.Drawing.FontFamily.GenericMonospace, 8, System.Drawing.FontStyle.Bold),
                                    new System.Drawing.SolidBrush(System.Drawing.Color.Black), rect, stringFormat);
            }
        }
    }


}
