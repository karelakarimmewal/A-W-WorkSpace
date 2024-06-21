using Material.Icons;
using MySql.Data.MySqlClient;
using Mysqlx.Session;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace TranQuik.Model
{
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
        public int ComputerID { get; set; }
        public string ComputerName { get; set; }
        public int ShopID { get; set; }
        public string ComputerType { get; set; }
        public string PayTypeList { get; set; }
        public string SaleModeList { get; set; }
        public string TableZoneList { get; set; }
        public string IPAddress { get; set; }
        public string RegistrationNumber { get; set; }
        public string ReceiptHeader { get; set; }
        public string FullTaxHeader { get; set; }
        public string DeviceCode { get; set; }
        public string KDSID { get; set; }
        public string KDSPrinters { get; set; }
        public string Description { get; set; }
        public string ProductGroupList { get; set; }
        public string FavoriteImagePageList { get; set; }
        public string FavoriteTextPageList { get; set; }
        public bool Deleted { get; set; }
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
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }

    public class ProductComponentProduct
    {
        public string ProductComponentProductCode { get; set; }
        public string ProductComponentProductName { get; set; }
        public decimal ProductComponentProductPrice { get; set; }
        public int ProductComponentProductQuantity { get; set; } = 0;
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
        public PaymentDetails(int paymentTypeID, string paymentTypeName, decimal paymentAmount, bool paymentIsActive = true)
        {
            PaymentTypeID = paymentTypeID;
            PaymentTypeName = paymentTypeName;
            PaymentAmount = paymentAmount;
            PaymentIsAcitve = paymentIsActive;
        }
    }

    public class Product
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal ProductPrice { get; set; }
        public string ProductButtonColor { get; set; }
        public int Quantity { get; set; } = 1; // Default quantity is 1
        public bool Status { get; set; } // Status property

        public List<ChildItem> ChildItems { get; set; } // List of child items for the product

        public bool HasChildItems()
        {
            return ChildItems != null && ChildItems.Count > 0;
        }

        public Product(int productId, string productName, decimal productPrice, string productButtonColor)
        {
            ProductId = productId;
            ProductName = productName;
            ProductPrice = productPrice;
            ProductButtonColor = productButtonColor;
            Status = true; // Default status is Active
            ChildItems = new List<ChildItem>(); // Initialize child items list
        }
    }

    public class ChildItem
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public bool Status { get; set; }
        public ChildItem(string name, decimal price, int quantity, bool status)
        {
            Name = name;
            Quantity = quantity;
            Price = price;
            Status = status;
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
        public int SaleModeID { get; set; }
        public string SaleModeName { get; set; }
        public string ReceiptHeaderText { get; set; }
        public string NotInPayTypeList { get; set; }
        public string PrefixText { get; set; }
        public string PrefixQueue { get; set; }
    }

    public class ModifierGroup
    {
        public string ModifierGroupID { get; set; }
        public string ModifierGroupCode { get; set; }
        public string ModifierName { get; set; }
    }

    public class ModifierMenu
    {
        public string ModifierMenuCode { get; set; }
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
