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
    using MySql.Data.MySqlClient;
    using System.Collections.Generic;

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

    public class UserSessions
    {
        public int StaffID { get; set; } = 0;
        public int StaffRoleID { get; set; } = 2;
        public string StaffFirstName { get; set; } = "DefaultForUpdate";
        public  string StaffLastName { get; set; } = "DefaultForUpdate";

        public void SetSessions()
        {
            CurrentSessions.SetNow(StaffID, StaffRoleID, StaffFirstName, StaffLastName);
        }

    }

    public class CurrentSessions
    {
        public static int StaffID { get; set; }
        public static int StaffRoleID { get; set; }
        public static string StaffFirstName { get; set; }
        public static string StaffLastName { get; set; }

        public static void SetNow(int staffID, int staffRoleID, string staffFirstName, string staffLastName)
        {
            StaffID = staffID;
            StaffRoleID = staffRoleID;
            StaffFirstName = staffFirstName;
            StaffLastName = staffLastName;
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

}
