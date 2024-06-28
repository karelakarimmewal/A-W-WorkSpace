using MySql.Data.MySqlClient;
using Serilog;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using TranQuik.Configuration;
using TranQuik.Model;

namespace TranQuik.Controller
{
    public class SyncMethod
    {
        public int times = 50000;
        private MySqlConnection LocalConnection;
        private static readonly string logFilePath = "log.txt";
        private readonly Action<int> _updateProgress;
        private LocalDbConnector dbConnector;
        private CloudDbConnector cloudDbConnector;
        private Scheduler scheduler;
        private UserSessions userSessions1;
        public SyncMethod(Action<int> updateProgress = null)
        {
            dbConnector = new LocalDbConnector();
            cloudDbConnector = new CloudDbConnector();
            _updateProgress = updateProgress;
        }

        public async Task SyncDataAsync()
        {
            try
            {
                // Establish connection to the local database
                LocalConnection = dbConnector.GetMySqlConnection();
                await LocalConnection.OpenAsync();

                string deleteSqlQueryShopData = "TRUNCATE TABLE Shop_Data";
                bool ShopDataSuccess = await DeleteExistingRecordsAsync(deleteSqlQueryShopData);
                if (ShopDataSuccess)
                {
                    await SyncDataFromCloudAsyncShopData();
                }

                string deleteSqlQueryComputerDetails = "TRUNCATE TABLE ComputerName";
                bool favComputerDetailsSuccess = await DeleteExistingRecordsAsync(deleteSqlQueryComputerDetails);
                if (favComputerDetailsSuccess)
                {
                    await SyncDataFromCloudAsyncComputerDetails();
                }

                string deleteSqlQueryFavPageIndex = "TRUNCATE TABLE favoritepageindex";
                bool favProductPageSuccess = await DeleteExistingRecordsAsync(deleteSqlQueryFavPageIndex);
                if (favProductPageSuccess)
                {
                    await SyncDataFromCloudAsyncFavoritePageIndex();
                }

                string deleteSqlQueryFav = "TRUNCATE TABLE favoriteproducts";
                bool favProductSuccess = await DeleteExistingRecordsAsync(deleteSqlQueryFav);
                if (favProductSuccess)
                {
                    await SyncDataFromCloudAsyncFavoriteProducts();
                }

                string deleteSqlQueryProductComponent = "TRUNCATE TABLE productcomponent";
                bool productComponentSuccess = await DeleteExistingRecordsAsync(deleteSqlQueryProductComponent);
                if (productComponentSuccess)
                {
                    await SyncDataFromCloudAsyncProductComponent();
                }

                string deleteSqlQueryProductComponentGroup = "TRUNCATE TABLE productcomponentgroup";
                bool productComponentGroupSuccess = await DeleteExistingRecordsAsync(deleteSqlQueryProductComponentGroup);
                if (productComponentGroupSuccess)
                {
                    await SyncDataFromCloudAsyncProductComponentGroup();
                }

                string deleteSqlSaleMode = "TRUNCATE TABLE salemode ";
                bool saleModeSuccess = await DeleteExistingRecordsAsync(deleteSqlSaleMode);
                if (saleModeSuccess)
                {
                    await SyncDataFromCloudAsyncSaleMode();
                }
            }
            catch (Exception ex)
            {
                UpdateSessions(2, 0);
                Log.ForContext("LogType", "SyncLog").Information("An error occurred", ex);
            }
            finally
            {
                UpdateSessions(2, 2);
                times = 5000;
                // Close connection to the local database
                if (LocalConnection != null && LocalConnection.State == System.Data.ConnectionState.Open)
                {
                    Properties.Settings.Default._LastSync = DateTime.Now;
                    AppSettings.LastSync = Properties.Settings.Default._LastSync;
                    Properties.Settings.Default.Save();
                    //Config.SaveAppSettings();
                    await LocalConnection.CloseAsync();
                }
            }
        }

        private async Task SyncDataFromCloudAsyncShopData()
        {
            string cloudSqlQuery = @"
            SELECT [ShopID], [BrandID], [MerchantID], [ShopCode], [DocumentShopCode], [ShopName], [IsShop], [IsInv],
                   [HasSC], [IsSCBeforeDisc], [SCPercent], [VATCode], [VATType], [MasterShop], [MasterShopLink],
                   [ShowInReport], [ShopTypeID], [ShopCatID1], [ShopCatID2], [ShopCatID3], [ShopCatID4], [ShopCatID5],
                   [ShopCatID6], [ShopCatID7], [ShopCatID8], [ShopCatID9], [ShopCatID10], [OpenHour], [CloseHour],
                   [CompanyName], [CompanyAddress1], [CompanyAddress2], [CompanyCity], [CompanyProvince],
                   [DisplayCompanyProvinceLangID], [CompanyZipCode], [CompanyTelephone], [CompanyFax], [CompanyCountry],
                   [CompanyTaxID], [CompanyRegisterID], [BranchName], [BranchNo], [CompanyRegisterLocation],
                   [BranchNameInFullTaxReport], [AccountingCode], [DeliveryName], [DeliveryAddress1],
                   [DeliveryAddress2], [DeliveryCity], [DeliveryProvince], [DeliveryZipCode], [DeliveryTelephone],
                   [DeliveryFax], [IPAddress], [Addtional], [ProductLevelOrder], [SLOC], [PTTShopCode], [StartSaleDate],
                   [StartMonth], [StartYear], [Deleted], [ShopKey], [ReportToAP2]
            FROM [VTEC_ID_HQ_DUMMY].[dbo].[shop_data]
            WHERE [ShopID] = @ShopID";


            using (SqlConnection cloudConnection = cloudDbConnector.GetSqlConnection())
            {
                await cloudConnection.OpenAsync();

                using (SqlCommand cloudCommand = new SqlCommand(cloudSqlQuery, cloudConnection))
                {
                    cloudCommand.Parameters.AddWithValue("@ShopID", Properties.Settings.Default._AppID);

                    int totalRecords = 0;
                    using (SqlDataReader reader = await cloudCommand.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            totalRecords++;
                        }
                    }

                    if (totalRecords == 0) return;

                    using (SqlDataReader reader = await cloudCommand.ExecuteReaderAsync())
                    {
                        int processedRecords = 0;
                        while (await reader.ReadAsync())
                        {
                            await InsertIntoLocalDatabaseAsyncShopData(reader);
                            processedRecords++;
                            int progress = (processedRecords * 100) / totalRecords;
                            if (_updateProgress != null)
                            {
                                _updateProgress(progress);
                            }
                        }
                    }
                }
            }
        }

        private async Task SyncDataFromCloudAsyncComputerDetails()
        {
            string cloudSqlQuery = @"
                SELECT ComputerID, ComputerName, ShopID, ComputerType, PayTypeList, SaleModeList, TableZoneList,
                       IPAddress, RegistrationNumber, ReceiptHeader, FullTaxHeader, DeviceCode, 
                       KDSID, KDSPrinters, Description, ProductGroupList, 
                       FavoriteImagePageList, FavoriteTextPageList, Deleted 
                FROM computername
                WHERE ShopID = @ShopID";

            using (SqlConnection cloudConnection = cloudDbConnector.GetSqlConnection())
            {
                await cloudConnection.OpenAsync();

                using (SqlCommand cloudCommand = new SqlCommand(cloudSqlQuery, cloudConnection))
                {
                    cloudCommand.Parameters.AddWithValue("@ShopID", Properties.Settings.Default._AppID);

                    int totalRecords = 0;
                    using (SqlDataReader reader = await cloudCommand.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            totalRecords++;
                        }
                    }

                    if (totalRecords == 0) return;

                    using (SqlDataReader reader = await cloudCommand.ExecuteReaderAsync())
                    {
                        int processedRecords = 0;
                        while (await reader.ReadAsync())
                        {
                            await UpdateLocalDatabaseAsync(reader);
                            processedRecords++;
                            int progress = (processedRecords * 100) / totalRecords;
                            if (_updateProgress != null)
                            {
                                _updateProgress(progress);
                            }
                        }
                    }
                }
            }
        }

        private async Task SyncDataFromCloudAsyncFavoritePageIndex()
        {
            string cloudSqlQuery = @"
                SELECT [TemplateID], [PageIndex], [PageName], [PageOrder], [ButtonColorCode], [ButtonColorHexCode], [PageType]
                FROM [VTEC_ID_HQ].[dbo].[favoritepageindextemplate]
                WHERE TemplateID = 2
                ORDER BY PageOrder ASC";

            using (SqlConnection cloudConnection = cloudDbConnector.GetSqlConnection())
            {
                await cloudConnection.OpenAsync();

                using (SqlCommand cloudCommand = new SqlCommand(cloudSqlQuery, cloudConnection))
                {
                    int totalRecords = 0;
                    using (SqlDataReader reader = await cloudCommand.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            totalRecords++;
                        }
                    }

                    if (totalRecords == 0) return;

                    using (SqlDataReader reader = await cloudCommand.ExecuteReaderAsync())
                    {
                        int processedRecords = 0;
                        while (await reader.ReadAsync())
                        {
                            await InsertIntoLocalDatabaseAsyncFavoritePageIndex(reader);
                            processedRecords++;
                            int progress = (processedRecords * 100) / totalRecords;
                            if (_updateProgress != null)
                            {
                                _updateProgress(progress);

                            }
                        }
                    }
                }
            }
        }

        private async Task SyncDataFromCloudAsyncFavoriteProducts()
        {
            string cloudSqlQuery = @"
                SELECT [ProductID], FSL.ShopID, FSL.[TemplateID], [PageIndex], [ProductCode], [ButtonOrder], [ButtonColorCode],
                       [ButtonColorHexCode], [ImageFileName], [PageType]
                FROM [VTEC_ID_HQ_DUMMY].[dbo].[favoriteproductstemplate] FPT
                JOIN favoriteShopLink FSL ON FPT.TemplateID = FSL.TemplateID
                WHERE FSL.ShopID = @ShopID
                ORDER BY ButtonOrder";

            using (SqlConnection cloudConnection = cloudDbConnector.GetSqlConnection())
            {
                await cloudConnection.OpenAsync();

                using (SqlCommand cloudCommand = new SqlCommand(cloudSqlQuery, cloudConnection))
                {
                    cloudCommand.Parameters.AddWithValue("@ShopID", Properties.Settings.Default._AppID);
                    int totalRecords = 0;
                    using (SqlDataReader reader = await cloudCommand.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            totalRecords++;
                        }
                    }

                    if (totalRecords == 0) return;

                    using (SqlDataReader reader = await cloudCommand.ExecuteReaderAsync())
                    {
                        int processedRecords = 0;
                        while (await reader.ReadAsync())
                        {
                            await InsertIntoLocalDatabaseAsyncFavoriteProducts(reader);
                            processedRecords++;
                            int progress = (processedRecords * 100) / totalRecords;
                            if (_updateProgress != null)
                            {
                                _updateProgress(progress);
                            }
                        }
                    }
                }
            }
        }

        private async Task SyncDataFromCloudAsyncProductComponent()
        {
            string cloudSqlQuery = @"
        SELECT [PGroupID], [ProductID], [SaleMode], [MaterialID], [MaterialAmount], [QtyRatio], [UnitSmallID], [ShowOnOrder], [DataType], [FlexibleProductPrice], [FlexibleProductIncludePrice], [DiscountAmount], [DiscountPercent], [Ordering], [AddingFromBranch]
        FROM [VTEC_ID_HQ_DUMMY].[dbo].[productcomponent]";

            using (SqlConnection cloudConnection = cloudDbConnector.GetSqlConnection())
            {
                await cloudConnection.OpenAsync();

                using (SqlCommand cloudCommand = new SqlCommand(cloudSqlQuery, cloudConnection))
                {
                    int totalRecords = 0;
                    using (SqlDataReader reader = await cloudCommand.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            totalRecords++;
                        }
                    }

                    if (totalRecords == 0) return;

                    using (SqlDataReader reader = await cloudCommand.ExecuteReaderAsync())
                    {
                        int processedRecords = 0;
                        while (await reader.ReadAsync())
                        {
                            await InsertIntoLocalDatabaseAsyncProductComponent(reader);
                            processedRecords++;
                            int progress = (processedRecords * 100) / totalRecords;
                            if (_updateProgress != null)
                            {
                                _updateProgress(progress);
                            }
                        }
                    }
                }
            }
        }

        private async Task SyncDataFromCloudAsyncProductComponentGroup()
        {
            string cloudSqlQuery = @"
                        SELECT [PGroupID], [PGroupTypeID], [ProductID], [SaleMode], [StartDate], [EndDate], [SetGroupNo], [SetGroupName], [RequireAddAmountForProduct], [MinQty], [MaxQty], [AddingFromBranch], [IsDefault], [PackageTypeID], [PromotionID], [VoucherHeaderID], [StaffRoleID], [SetGroupText], [SetGroupDesp]
                        FROM [VTEC_ID_HQ_DUMMY].[dbo].[productcomponentgroup]";

            using (SqlConnection cloudConnection = cloudDbConnector.GetSqlConnection())
            {
                await cloudConnection.OpenAsync();

                using (SqlCommand cloudCommand = new SqlCommand(cloudSqlQuery, cloudConnection))
                {
                    int totalRecords = 0;
                    using (SqlDataReader reader = await cloudCommand.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            
                            totalRecords++;
                        }
                    }

                    if (totalRecords == 0) return;

                    using (SqlDataReader reader = await cloudCommand.ExecuteReaderAsync())
                    {
                        int processedRecords = 0;
                        while (await reader.ReadAsync())
                        {
                            await InsertIntoLocalDatabaseAsyncProductComponentGroup(reader);
                            processedRecords++;
                            int progress = (processedRecords * 100) / totalRecords;
                            if (_updateProgress != null)
                            {
                                _updateProgress(progress);
                            }
                        }
                    }
                }
            }
        }

        private async Task SyncDataFromCloudAsyncSaleMode()
        {
            string cloudSqlQuery = @"
        SELECT [SaleModeID]
              ,[SaleModeName]
              ,[Deleted]
              ,[PositionPrefix]
              ,[PrefixText]
              ,[PrefixTextPrinting]
              ,[PrefixQueue]
              ,[ReceiptHeaderText]
              ,[HasServiceCharge]
              ,[PayTypeList]
              ,[NOTinPayTypeList]
              ,[IsDefault]
              ,[PriceAtHeader]
              ,[PrefixHeaderText]
              ,[AutoAddProduct]
              ,[KDSHeaderColor]
              ,[SaleModeTypeID]
              ,[NoPrintCopy]
          FROM [VTEC_ID_HQ_DUMMY].[dbo].[salemode]";

            using (SqlConnection cloudConnection = cloudDbConnector.GetSqlConnection())
            {
                await cloudConnection.OpenAsync();

                using (SqlCommand cloudCommand = new SqlCommand(cloudSqlQuery, cloudConnection))
                {
                    int totalRecords = 0;
                    using (SqlDataReader reader = await cloudCommand.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            totalRecords++;
                        }
                    }

                    if (totalRecords == 0) return;

                    using (SqlDataReader reader = await cloudCommand.ExecuteReaderAsync())
                    {
                        int processedRecords = 0;
                        while (await reader.ReadAsync())
                        {
                            await InsertIntoLocalDatabaseAsyncSaleMode(reader);
                            processedRecords++;
                            int progress = (processedRecords * 100) / totalRecords;
                            _updateProgress?.Invoke(progress);
                        }
                    }
                }
            }
        }

        private async Task<bool> DeleteExistingRecordsAsync(string deleteSqlQuery)
        {
            try
            {
                using (MySqlConnection connection = dbConnector.GetMySqlConnection())
                {
                    await connection.OpenAsync();

                    using (MySqlCommand deleteCommand = new MySqlCommand(deleteSqlQuery, connection))
                    {
                        int rowsAffected = await deleteCommand.ExecuteNonQueryAsync();
                        Log.ForContext("LogType", "SyncLog").Information($"Deleted {rowsAffected} records for query: {deleteSqlQuery}");
                        //Log($"Deleted {rowsAffected} records for query: {deleteSqlQuery}");
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.ForContext("LogType", "SyncLog").Information($"Error deleting existing records", ex);
                //LogError("Error deleting existing records", ex);
                return false;
            }
        }

        private async Task InsertIntoLocalDatabaseAsyncShopData(SqlDataReader reader)
        {
            /* Table: shop_data */
            int shopID = (int)reader["ShopID"];
            int brandID = (int)reader["BrandID"];
            int merchantID = (int)reader["MerchantID"];
            string shopCode = reader["ShopCode"].ToString();
            string shopKey = reader["ShopKey"].ToString();
            string documentShopCode = reader["DocumentShopCode"].ToString();
            string shopName = reader["ShopName"].ToString();
            byte isShop = (byte)reader["IsShop"];
            byte isInv = (byte)reader["IsInv"];
            byte hasSC = (byte)reader["HasSC"];
            byte isSCBeforeDisc = (byte)reader["IsSCBeforeDisc"];
            decimal scPercent = (decimal)reader["SCPercent"];
            string vatCode = reader["VATCode"].ToString();
            byte vatType = (byte)reader["VATType"];
            short masterShop = (short)reader["MasterShop"];
            short masterShopLink = (short)reader["MasterShopLink"];
            byte showInReport = (byte)reader["ShowInReport"];
            short shopTypeID = (short)reader["ShopTypeID"];
            short shopCatID1 = (short)reader["ShopCatID1"];
            short shopCatID2 = (short)reader["ShopCatID2"];
            short shopCatID3 = (short)reader["ShopCatID3"];
            short shopCatID4 = (short)reader["ShopCatID4"];
            short shopCatID5 = (short)reader["ShopCatID5"];
            short shopCatID6 = (short)reader["ShopCatID6"];
            short shopCatID7 = (short)reader["ShopCatID7"];
            short shopCatID8 = (short)reader["ShopCatID8"];
            short shopCatID9 = (short)reader["ShopCatID9"];
            short shopCatID10 = (short)reader["ShopCatID10"];
            DateTime? openHour = reader.IsDBNull(reader.GetOrdinal("OpenHour")) ? (DateTime?)null : (DateTime)reader["OpenHour"];
            DateTime? closeHour = reader.IsDBNull(reader.GetOrdinal("CloseHour")) ? (DateTime?)null : (DateTime)reader["CloseHour"];
            string companyName = reader["CompanyName"].ToString();
            string companyAddress1 = reader["CompanyAddress1"].ToString();
            string companyAddress2 = reader["CompanyAddress2"].ToString();
            string companyCity = reader["CompanyCity"].ToString();
            int? companyProvince = reader.IsDBNull(reader.GetOrdinal("CompanyProvince")) ? (int?)null : (int)reader["CompanyProvince"];
            byte displayCompanyProvinceLangID = (byte)reader["DisplayCompanyProvinceLangID"];
            string companyZipCode = reader["CompanyZipCode"].ToString();
            string companyTelephone = reader["CompanyTelephone"].ToString();
            string companyFax = reader["CompanyFax"].ToString();
            string companyCountry = reader["CompanyCountry"].ToString();
            string companyTaxID = reader["CompanyTaxID"].ToString();
            string companyRegisterID = reader["CompanyRegisterID"].ToString();
            string branchName = reader["BranchName"].ToString();
            string branchNo = reader["BranchNo"].ToString();
            byte? companyRegisterLocation = reader.IsDBNull(reader.GetOrdinal("CompanyRegisterLocation")) ? (byte?)null : (byte)reader["CompanyRegisterLocation"];
            string branchNameInFullTaxReport = reader["BranchNameInFullTaxReport"].ToString();
            string accountingCode = reader["AccountingCode"].ToString();
            string taxPOSID = null;
            string deliveryName = reader["DeliveryName"].ToString();
            string deliveryAddress1 = reader["DeliveryAddress1"].ToString();
            string deliveryAddress2 = reader["DeliveryAddress2"].ToString();
            string deliveryCity = reader["DeliveryCity"].ToString();
            string deliveryProvince = reader["DeliveryProvince"].ToString();
            string deliveryZipCode = reader["DeliveryZipCode"].ToString();
            string deliveryTelephone = reader["DeliveryTelephone"].ToString();
            string deliveryFax = reader["DeliveryFax"].ToString();
            string ipAddress = reader["IPAddress"].ToString();
            string addtional = reader["Addtional"].ToString();
            int productLevelOrder = (int)reader["ProductLevelOrder"];
            string sloc = reader["SLOC"].ToString();
            string pttShopCode = reader["PTTShopCode"].ToString();
            DateTime? startSaleDate = reader.IsDBNull(reader.GetOrdinal("StartSaleDate")) ? (DateTime?)null : (DateTime)reader["StartSaleDate"];
            short? startMonth = reader.IsDBNull(reader.GetOrdinal("StartMonth")) ? (short?)null : (short)reader["StartMonth"];
            short? startYear = reader.IsDBNull(reader.GetOrdinal("StartYear")) ? (short?)null : (short)reader["StartYear"];
            byte deleted = (byte)reader["Deleted"];



            string localSqlQuery = @"
        INSERT INTO shop_data (
            ShopID, BrandID, MerchantID, ShopCode, ShopKey, DocumentShopCode, ShopName, IsShop, IsInv,
            HasSC, IsSCBeforeDisc, SCPercent, VATCode, VATType, MasterShop, MasterShopLink, ShowInReport,
            ShopTypeID, ShopCatID1, ShopCatID2, ShopCatID3, ShopCatID4, ShopCatID5, ShopCatID6, ShopCatID7,
            ShopCatID8, ShopCatID9, ShopCatID10, OpenHour, CloseHour, CompanyName, CompanyAddress1, CompanyAddress2,
            CompanyCity, CompanyProvince, DisplayCompanyProvinceLangID, CompanyZipCode, CompanyTelephone, CompanyFax,
            CompanyCountry, CompanyTaxID, CompanyRegisterID, BranchName, BranchNo, CompanyRegisterLocation, BranchNameInFullTaxReport,
            AccountingCode, TaxPOSID, DeliveryName, DeliveryAddress1, DeliveryAddress2, DeliveryCity,  DeliveryProvince, DeliveryZipCode, DeliveryTelephone, DeliveryFax, IPAddress, Addtional, ProductLevelOrder,
            SLOC, PTTShopCode, StartSaleDate, StartMonth, StartYear, Deleted)
        VALUES (
            @ShopID, @BrandID, @MerchantID, @ShopCode, @ShopKey, @DocumentShopCode, @ShopName, @IsShop, @IsInv,
            @HasSC, @IsSCBeforeDisc, @SCPercent, @VATCode, @VATType, @MasterShop, @MasterShopLink, @ShowInReport,
            @ShopTypeID, @ShopCatID1, @ShopCatID2, @ShopCatID3, @ShopCatID4, @ShopCatID5, @ShopCatID6, @ShopCatID7,
            @ShopCatID8, @ShopCatID9, @ShopCatID10, @OpenHour, @CloseHour, @CompanyName, @CompanyAddress1, @CompanyAddress2,
            @CompanyCity, @CompanyProvince, @DisplayCompanyProvinceLangID, @CompanyZipCode, @CompanyTelephone, @CompanyFax,
            @CompanyCountry, @CompanyTaxID, @CompanyRegisterID, @BranchName, @BranchNo, @CompanyRegisterLocation, @BranchNameInFullTaxReport,
            @AccountingCode, @TaxPOSID, @DeliveryName, @DeliveryAddress1, @DeliveryAddress2, @DeliveryCity, @DeliveryProvince,
            @DeliveryZipCode, @DeliveryTelephone, @DeliveryFax, @IPAddress, @Addtional, @ProductLevelOrder,
            @SLOC, @PTTShopCode, @StartSaleDate, @StartMonth, @StartYear, @Deleted)
        ON DUPLICATE KEY UPDATE
            ShopCode = VALUES(ShopCode), ShopKey = VALUES(ShopKey), DocumentShopCode = VALUES(DocumentShopCode),
            ShopName = VALUES(ShopName), IsShop = VALUES(IsShop), IsInv = VALUES(IsInv), HasSC = VALUES(HasSC),
            IsSCBeforeDisc = VALUES(IsSCBeforeDisc), SCPercent = VALUES(SCPercent), VATCode = VALUES(VATCode),
            VATType = VALUES(VATType), MasterShop = VALUES(MasterShop), MasterShopLink = VALUES(MasterShopLink),
            ShowInReport = VALUES(ShowInReport), ShopTypeID = VALUES(ShopTypeID), ShopCatID1 = VALUES(ShopCatID1),
            ShopCatID2 = VALUES(ShopCatID2), ShopCatID3 = VALUES(ShopCatID3), ShopCatID4 = VALUES(ShopCatID4),
            ShopCatID5 = VALUES(ShopCatID5), ShopCatID6 = VALUES(ShopCatID6), ShopCatID7 = VALUES(ShopCatID7),
            ShopCatID8 = VALUES(ShopCatID8), ShopCatID9 = VALUES(ShopCatID9), ShopCatID10 = VALUES(ShopCatID10),
            OpenHour = VALUES(OpenHour), CloseHour = VALUES(CloseHour), CompanyName = VALUES(CompanyName),
            CompanyAddress1 = VALUES(CompanyAddress1), CompanyAddress2 = VALUES(CompanyAddress2),
            CompanyCity = VALUES(CompanyCity), CompanyProvince = VALUES(CompanyProvince),
            DisplayCompanyProvinceLangID = VALUES(DisplayCompanyProvinceLangID), CompanyZipCode = VALUES(CompanyZipCode),
            CompanyTelephone = VALUES(CompanyTelephone), CompanyFax = VALUES(CompanyFax), CompanyCountry = VALUES(CompanyCountry),
            CompanyTaxID = VALUES(CompanyTaxID), CompanyRegisterID = VALUES(CompanyRegisterID),
            BranchName = VALUES(BranchName), BranchNo = VALUES(BranchNo), CompanyRegisterLocation = VALUES(CompanyRegisterLocation),
            BranchNameInFullTaxReport = VALUES(BranchNameInFullTaxReport), AccountingCode = VALUES(AccountingCode),
            TaxPOSID = VALUES(TaxPOSID), DeliveryName = VALUES(DeliveryName), DeliveryAddress1 = VALUES(DeliveryAddress1),
            DeliveryAddress2 = VALUES(DeliveryAddress2), DeliveryCity = VALUES(DeliveryCity), DeliveryProvince = VALUES(DeliveryProvince),
            DeliveryZipCode = VALUES(DeliveryZipCode), DeliveryTelephone = VALUES(DeliveryTelephone), DeliveryFax = VALUES(DeliveryFax),
            IPAddress = VALUES(IPAddress), Addtional = VALUES(Addtional), ProductLevelOrder = VALUES(ProductLevelOrder),
            SLOC = VALUES(SLOC), PTTShopCode = VALUES(PTTShopCode), StartSaleDate = VALUES(StartSaleDate),
            StartMonth = VALUES(StartMonth), StartYear = VALUES(StartYear), Deleted = VALUES(Deleted)";

            await UpdateDatabaseAsync(
            LocalConnection,
            localSqlQuery,
            new MySqlParameter("@ShopID", shopID),
            new MySqlParameter("@BrandID", brandID),
            new MySqlParameter("@MerchantID", merchantID),
            new MySqlParameter("@ShopCode", shopCode),
            new MySqlParameter("@ShopKey", shopKey),
            new MySqlParameter("@DocumentShopCode", documentShopCode),
            new MySqlParameter("@ShopName", shopName),
            new MySqlParameter("@IsShop", isShop),
            new MySqlParameter("@IsInv", isInv),
            new MySqlParameter("@HasSC", hasSC),
            new MySqlParameter("@IsSCBeforeDisc", isSCBeforeDisc),
            new MySqlParameter("@SCPercent", scPercent),
            new MySqlParameter("@VATCode", vatCode),
            new MySqlParameter("@VATType", vatType),
            new MySqlParameter("@MasterShop", masterShop),
            new MySqlParameter("@MasterShopLink", masterShopLink),
            new MySqlParameter("@ShowInReport", showInReport),
            new MySqlParameter("@ShopTypeID", shopTypeID),
            new MySqlParameter("@ShopCatID1", shopCatID1),
            new MySqlParameter("@ShopCatID2", shopCatID2),
            new MySqlParameter("@ShopCatID3", shopCatID3),
            new MySqlParameter("@ShopCatID4", shopCatID4),
            new MySqlParameter("@ShopCatID5", shopCatID5),
            new MySqlParameter("@ShopCatID6", shopCatID6),
            new MySqlParameter("@ShopCatID7", shopCatID7),
            new MySqlParameter("@ShopCatID8", shopCatID8),
            new MySqlParameter("@ShopCatID9", shopCatID9),
            new MySqlParameter("@ShopCatID10", shopCatID10),
            new MySqlParameter("@OpenHour", openHour ?? (object)DBNull.Value),
            new MySqlParameter("@CloseHour", closeHour ?? (object)DBNull.Value),
            new MySqlParameter("@CompanyName", companyName),
            new MySqlParameter("@CompanyAddress1", companyAddress1),
            new MySqlParameter("@CompanyAddress2", companyAddress2),
            new MySqlParameter("@CompanyCity", companyCity),
            new MySqlParameter("@CompanyProvince", companyProvince ?? (object)DBNull.Value),
            new MySqlParameter("@DisplayCompanyProvinceLangID", displayCompanyProvinceLangID),
            new MySqlParameter("@CompanyZipCode", companyZipCode),
            new MySqlParameter("@CompanyTelephone", companyTelephone),
            new MySqlParameter("@CompanyFax", companyFax),
            new MySqlParameter("@CompanyCountry", companyCountry),
            new MySqlParameter("@CompanyTaxID", companyTaxID),
            new MySqlParameter("@CompanyRegisterID", companyRegisterID),
            new MySqlParameter("@BranchName", branchName),
            new MySqlParameter("@BranchNo", branchNo),
            new MySqlParameter("@CompanyRegisterLocation", companyRegisterLocation ?? (object)DBNull.Value),
            new MySqlParameter("@BranchNameInFullTaxReport", branchNameInFullTaxReport),
            new MySqlParameter("@AccountingCode", accountingCode),
            new MySqlParameter("@TaxPOSID", taxPOSID),
            new MySqlParameter("@DeliveryName", deliveryName),
            new MySqlParameter("@DeliveryAddress1", deliveryAddress1),
            new MySqlParameter("@DeliveryAddress2", deliveryAddress2),
            new MySqlParameter("@DeliveryCity", deliveryCity),
            new MySqlParameter("@DeliveryProvince", deliveryProvince),
            new MySqlParameter("@DeliveryZipCode", deliveryZipCode),
            new MySqlParameter("@DeliveryTelephone", deliveryTelephone),
            new MySqlParameter("@DeliveryFax", deliveryFax),
            new MySqlParameter("@IPAddress", ipAddress),
            new MySqlParameter("@Addtional", addtional),
            new MySqlParameter("@ProductLevelOrder", productLevelOrder),
            new MySqlParameter("@SLOC", sloc),
            new MySqlParameter("@PTTShopCode", pttShopCode),
            new MySqlParameter("@StartSaleDate", startSaleDate ?? (object)DBNull.Value),
            new MySqlParameter("@StartMonth", startMonth ?? (object)DBNull.Value),
            new MySqlParameter("@StartYear", startYear ?? (object)DBNull.Value),
            new MySqlParameter("@Deleted", deleted));
        }

        private async Task InsertIntoLocalDatabaseAsyncFavoritePageIndex(SqlDataReader reader)
        {
            int shopID = int.Parse(Properties.Settings.Default._AppID);
            int computerType = 0;
            int pageIndex = (int)reader["PageIndex"];
            string pageName = reader["PageName"].ToString();
            int pageOrder = (int)reader["PageOrder"];
            string buttonColorCode = reader["ButtonColorCode"].ToString();
            string buttonColorHexCode = reader["ButtonColorHexCode"].ToString();
            string pageType = reader["PageType"].ToString();

            string localSqlQuery = @"
                INSERT INTO favoritepageindex (ShopID, ComputerType, PageIndex, PageName, PageOrder, ButtonColorCode, ButtonColorHexCode, PageType)
                VALUES (@ShopID, @ComputerType, @PageIndex, @PageName, @PageOrder, @ButtonColorCode, @ButtonColorHexCode, @PageType)
                ON DUPLICATE KEY UPDATE
                PageName = VALUES(PageName),
                PageOrder = VALUES(PageOrder),
                ButtonColorCode = VALUES(ButtonColorCode),
                ButtonColorHexCode = VALUES(ButtonColorHexCode),
                PageType = VALUES(PageType)";

            await UpdateDatabaseAsync(
                LocalConnection,
                localSqlQuery,
                new MySqlParameter("@ShopID", shopID),
                new MySqlParameter("@ComputerType", computerType),
                new MySqlParameter("@PageIndex", pageIndex),
                new MySqlParameter("@PageName", pageName),
                new MySqlParameter("@PageOrder", pageOrder),
                new MySqlParameter("@ButtonColorCode", buttonColorCode),
                new MySqlParameter("@ButtonColorHexCode", buttonColorHexCode),
                new MySqlParameter("@PageType", pageType)
            );
        }

        private async Task InsertIntoLocalDatabaseAsyncSaleMode(SqlDataReader reader)
        {
            byte saleModeID = reader.GetByte(reader.GetOrdinal("SaleModeID"));
            string saleModeName = reader["SaleModeName"].ToString();
            byte deleted = reader.GetByte(reader.GetOrdinal("Deleted"));
            byte positionPrefix = reader.GetByte(reader.GetOrdinal("PositionPrefix"));
            string prefixText = reader["PrefixText"].ToString();
            string prefixTextPrinting = reader["PrefixTextPrinting"].ToString();
            string prefixQueue = reader["PrefixQueue"].ToString();
            string receiptHeaderText = reader["ReceiptHeaderText"].ToString();
            byte hasServiceCharge = reader.GetByte(reader.GetOrdinal("HasServiceCharge"));
            string payTypeList = reader["PayTypeList"].ToString();
            string notInPayTypeList = reader["NOTinPayTypeList"].ToString();
            byte isDefault = reader.GetByte(reader.GetOrdinal("IsDefault"));
            byte priceAtHeader = reader.GetByte(reader.GetOrdinal("PriceAtHeader"));
            string prefixHeaderText = reader["PrefixHeaderText"].ToString();
            string autoAddProduct = reader["AutoAddProduct"].ToString();
            string kdsHeaderColor = reader["KDSHeaderColor"].ToString();
            short saleModeTypeID = reader.GetInt16(reader.GetOrdinal("SaleModeTypeID"));
            byte noPrintCopy = reader.GetByte(reader.GetOrdinal("NoPrintCopy"));
            int ordering = 0; // Default value as the field does not exist in the cloud table

            string localSqlQuery = @"
            INSERT INTO salemode (SaleModeID, SaleModeName, Deleted, PositionPrefix, PrefixText, PrefixTextPrinting, PrefixQueue, ReceiptHeaderText, HasServiceCharge, PayTypeList, NOTinPayTypeList, IsDefault, PriceAtHeader, PrefixHeaderText, AutoAddProduct, KDSHeaderColor, SaleModeTypeID, NoPrintCopy, Ordering)
            VALUES (@SaleModeID, @SaleModeName, @Deleted, @PositionPrefix, @PrefixText, @PrefixTextPrinting, @PrefixQueue, @ReceiptHeaderText, @HasServiceCharge, @PayTypeList, @NOTinPayTypeList, @IsDefault, @PriceAtHeader, @PrefixHeaderText, @AutoAddProduct, @KDSHeaderColor, @SaleModeTypeID, @NoPrintCopy, @Ordering)
            ON DUPLICATE KEY UPDATE
            SaleModeName = VALUES(SaleModeName),
            Deleted = VALUES(Deleted),
            PositionPrefix = VALUES(PositionPrefix),
            PrefixText = VALUES(PrefixText),
            PrefixTextPrinting = VALUES(PrefixTextPrinting),
            PrefixQueue = VALUES(PrefixQueue),
            ReceiptHeaderText = VALUES(ReceiptHeaderText),
            HasServiceCharge = VALUES(HasServiceCharge),
            PayTypeList = VALUES(PayTypeList),
            NOTinPayTypeList = VALUES(NOTinPayTypeList),
            IsDefault = VALUES(IsDefault),
            PriceAtHeader = VALUES(PriceAtHeader),
            PrefixHeaderText = VALUES(PrefixHeaderText),
            AutoAddProduct = VALUES(AutoAddProduct),
            KDSHeaderColor = VALUES(KDSHeaderColor),
            SaleModeTypeID = VALUES(SaleModeTypeID),
            NoPrintCopy = VALUES(NoPrintCopy),
            Ordering = VALUES(Ordering)";

            await UpdateDatabaseAsync(
                LocalConnection,
                localSqlQuery,
                new MySqlParameter("@SaleModeID", saleModeID),
                new MySqlParameter("@SaleModeName", saleModeName),
                new MySqlParameter("@Deleted", deleted),
                new MySqlParameter("@PositionPrefix", positionPrefix),
                new MySqlParameter("@PrefixText", prefixText),
                new MySqlParameter("@PrefixTextPrinting", prefixTextPrinting),
                new MySqlParameter("@PrefixQueue", prefixQueue),
                new MySqlParameter("@ReceiptHeaderText", receiptHeaderText),
                new MySqlParameter("@HasServiceCharge", hasServiceCharge),
                new MySqlParameter("@PayTypeList", payTypeList),
                new MySqlParameter("@NOTinPayTypeList", notInPayTypeList),
                new MySqlParameter("@IsDefault", isDefault),
                new MySqlParameter("@PriceAtHeader", priceAtHeader),
                new MySqlParameter("@PrefixHeaderText", prefixHeaderText),
                new MySqlParameter("@AutoAddProduct", autoAddProduct),
                new MySqlParameter("@KDSHeaderColor", kdsHeaderColor),
                new MySqlParameter("@SaleModeTypeID", saleModeTypeID),
                new MySqlParameter("@NoPrintCopy", noPrintCopy),
                new MySqlParameter("@Ordering", ordering)
            );
        }

        private async Task InsertIntoLocalDatabaseAsyncFavoriteProducts(SqlDataReader reader)
        {
            int productID = (int)reader["ProductID"];
            int shopID = (int)reader["ShopID"];
            string productCode = reader["ProductCode"].ToString();
            string pageType = reader["PageType"].ToString();
            int pageIndex = (int)reader["PageIndex"];
            int buttonOrder = (int)reader["ButtonOrder"];
            string buttonColorCode = reader["ButtonColorCode"].ToString();
            string buttonColorHexCode = reader["ButtonColorHexCode"].ToString();
            string imageFileName = reader["ImageFileName"].ToString();

            string localSqlQuery = @"
                INSERT INTO favoriteproducts
                (ProductID, ShopID, ProductCode, PageType, PageIndex, ButtonOrder, ButtonColorCode, ButtonColorHexCode, ImageFileName)
                VALUES
                (@ProductID, @ShopID, @ProductCode, @PageType, @PageIndex, @ButtonOrder, @ButtonColorCode, @ButtonColorHexCode, @ImageFileName)
                ON DUPLICATE KEY UPDATE
                ProductCode = VALUES(ProductCode),
                PageType = VALUES(PageType),
                PageIndex = VALUES(PageIndex),
                ButtonOrder = VALUES(ButtonOrder),
                ButtonColorCode = VALUES(ButtonColorCode),
                ButtonColorHexCode = VALUES(ButtonColorHexCode),
                ImageFileName = VALUES(ImageFileName)";

            await UpdateDatabaseAsync(
                LocalConnection,
                localSqlQuery,
                new MySqlParameter("@ProductID", productID),
                new MySqlParameter("@ShopID", shopID),
                new MySqlParameter("@ProductCode", productCode),
                new MySqlParameter("@PageType", pageType),
                new MySqlParameter("@PageIndex", pageIndex),
                new MySqlParameter("@ButtonOrder", buttonOrder),
                new MySqlParameter("@ButtonColorCode", buttonColorCode),
                new MySqlParameter("@ButtonColorHexCode", buttonColorHexCode),
                new MySqlParameter("@ImageFileName", imageFileName)
            );
        }

        private async Task InsertIntoLocalDatabaseAsyncProductComponent(SqlDataReader reader)
        {
            int pGroupID = (int)reader["PGroupID"];
            int productID = (int)reader["ProductID"];
            byte saleMode = (byte)reader["SaleMode"];
            int materialID = (int)reader["MaterialID"];
            decimal materialAmount = (decimal)reader["MaterialAmount"];
            decimal qtyRatio = (decimal)reader["QtyRatio"];
            int unitSmallID = (int)reader["UnitSmallID"];
            byte showOnOrder = (byte)reader["ShowOnOrder"];
            byte dataType = (byte)reader["DataType"];
            decimal flexibleProductPrice = (decimal)reader["FlexibleProductPrice"];
            byte flexibleProductIncludePrice = (byte)reader["FlexibleProductIncludePrice"];
            decimal discountAmount = (decimal)reader["DiscountAmount"];
            decimal discountPercent = (decimal)reader["DiscountPercent"];
            short ordering = (short)reader["Ordering"];
            int addingFromBranch = (int)reader["AddingFromBranch"];

            string localSqlQuery = @"
        INSERT INTO productcomponent (PGroupID, ProductID, SaleMode, MaterialID, MaterialAmount, QtyRatio, UnitSmallID, ShowOnOrder, DataType, FlexibleProductPrice, FlexibleProductIncludePrice, DiscountAmount, DiscountPercent, Ordering, AddingFromBranch)
        VALUES (@PGroupID, @ProductID, @SaleMode, @MaterialID, @MaterialAmount, @QtyRatio, @UnitSmallID, @ShowOnOrder, @DataType, @FlexibleProductPrice, @FlexibleProductIncludePrice, @DiscountAmount, @DiscountPercent, @Ordering, @AddingFromBranch)
        ON DUPLICATE KEY UPDATE
        MaterialAmount = VALUES(MaterialAmount),
        QtyRatio = VALUES(QtyRatio),
        UnitSmallID = VALUES(UnitSmallID),
        ShowOnOrder = VALUES(ShowOnOrder),
        DataType = VALUES(DataType),
        FlexibleProductPrice = VALUES(FlexibleProductPrice),
        FlexibleProductIncludePrice = VALUES(FlexibleProductIncludePrice),
        DiscountAmount = VALUES(DiscountAmount),
        DiscountPercent = VALUES(DiscountPercent),
        Ordering = VALUES(Ordering),
        AddingFromBranch = VALUES(AddingFromBranch)";

            await UpdateDatabaseAsync(
                LocalConnection,
                localSqlQuery,
                new MySqlParameter("@PGroupID", pGroupID),
                new MySqlParameter("@ProductID", productID),
                new MySqlParameter("@SaleMode", saleMode),
                new MySqlParameter("@MaterialID", materialID),
                new MySqlParameter("@MaterialAmount", materialAmount),
                new MySqlParameter("@QtyRatio", qtyRatio),
                new MySqlParameter("@UnitSmallID", unitSmallID),
                new MySqlParameter("@ShowOnOrder", showOnOrder),
                new MySqlParameter("@DataType", dataType),
                new MySqlParameter("@FlexibleProductPrice", flexibleProductPrice),
                new MySqlParameter("@FlexibleProductIncludePrice", flexibleProductIncludePrice),
                new MySqlParameter("@DiscountAmount", discountAmount),
                new MySqlParameter("@DiscountPercent", discountPercent),
                new MySqlParameter("@Ordering", ordering),
                new MySqlParameter("@AddingFromBranch", addingFromBranch)
            );
        }

        private async Task InsertIntoLocalDatabaseAsyncProductComponentGroup(SqlDataReader reader)
        {
            int pGroupID = (int)reader["PGroupID"];
            short pGroupTypeID = (short)reader["PGroupTypeID"];
            int productID = (int)reader["ProductID"];
            byte saleMode = (byte)reader["SaleMode"];
            DateTime startDate = (DateTime)reader["StartDate"];
            DateTime endDate = (DateTime)reader["EndDate"];
            short setGroupNo = (short)reader["SetGroupNo"]; // Assuming it's smallint in the database
            string setGroupName = reader["SetGroupName"] != DBNull.Value ? reader["SetGroupName"].ToString() : null;
            byte requireAddAmountForProduct = (byte)reader["RequireAddAmountForProduct"];
            int minQty = (int)reader["MinQty"];
            int maxQty = (int)reader["MaxQty"];
            int addingFromBranch = (int)reader["AddingFromBranch"];
            byte isDefault = (byte)reader["IsDefault"];
            short calPrice = 9999; // Assuming it's smallint in the database
            int packageTypeID = (int)reader["PackageTypeID"];
            int promotionID = (int)reader["PromotionID"];
            int voucherHeaderID = (int)reader["VoucherHeaderID"];
            int staffRoleID = (int)reader["StaffRoleID"];
            string setGroupText = reader["SetGroupText"] != DBNull.Value ? reader["SetGroupText"].ToString() : null;
            int expireType = 0;
            DateTime? expireDate = null; // Initialize as null
            int expireAfterDay = 0;
            int expireAfterMonth = 0;
            int expireAfterYear = 0;

            string localSqlQuery = @"
            INSERT INTO productcomponentgroup (
                PGroupID, PGroupTypeID, ProductID, SaleMode, StartDate, EndDate, SetGroupNo, SetGroupName, RequireAddAmountForProduct, MinQty, MaxQty, AddingFromBranch, IsDefault, CalPrice, PackageTypeID, PromotionID, VoucherHeaderID, StaffRoleID, SetGroupText, ExpireType, ExpireDate, ExpireAfterDay, ExpireAfterMonth, ExpireAfterYear
            ) VALUES (
                @PGroupID, @PGroupTypeID, @ProductID, @SaleMode, @StartDate, @EndDate, @SetGroupNo, @SetGroupName, @RequireAddAmountForProduct, @MinQty, @MaxQty, @AddingFromBranch, @IsDefault, @CalPrice, @PackageTypeID, @PromotionID, @VoucherHeaderID, @StaffRoleID, @SetGroupText, @ExpireType, @ExpireDate, @ExpireAfterDay, @ExpireAfterMonth, @ExpireAfterYear
            )";

            await UpdateDatabaseAsync(
                LocalConnection,
                localSqlQuery,
                new MySqlParameter("@PGroupID", pGroupID),
                new MySqlParameter("@PGroupTypeID", pGroupTypeID),
                new MySqlParameter("@ProductID", productID),
                new MySqlParameter("@SaleMode", saleMode),
                new MySqlParameter("@StartDate", startDate),
                new MySqlParameter("@EndDate", endDate),
                new MySqlParameter("@SetGroupNo", setGroupNo),
                new MySqlParameter("@SetGroupName", setGroupName ?? (object)DBNull.Value),
                new MySqlParameter("@RequireAddAmountForProduct", requireAddAmountForProduct),
                new MySqlParameter("@MinQty", minQty),
                new MySqlParameter("@MaxQty", maxQty),
                new MySqlParameter("@AddingFromBranch", addingFromBranch),
                new MySqlParameter("@IsDefault", isDefault),
                new MySqlParameter("@CalPrice", calPrice),
                new MySqlParameter("@PackageTypeID", packageTypeID),
                new MySqlParameter("@PromotionID", promotionID),
                new MySqlParameter("@VoucherHeaderID", voucherHeaderID),
                new MySqlParameter("@StaffRoleID", staffRoleID),
                new MySqlParameter("@SetGroupText", setGroupText ?? (object)DBNull.Value),
                new MySqlParameter("@ExpireType", expireType),
                new MySqlParameter("@ExpireDate", expireDate ?? (object)DBNull.Value), // Handle nullable date
                new MySqlParameter("@ExpireAfterDay", expireAfterDay),
                new MySqlParameter("@ExpireAfterMonth", expireAfterMonth),
                new MySqlParameter("@ExpireAfterYear", expireAfterYear)
            );
        }

        private async Task UpdateLocalDatabaseAsync(SqlDataReader reader)
        {
            int computerID = (int)reader["ComputerID"];
            string computerName = reader["ComputerName"].ToString();
            int shopID = (int)reader["ShopID"];
            string computerType = reader["ComputerType"].ToString();
            string payTypeList = reader["PayTypeList"].ToString();
            string saleModeList = reader["SaleModeList"].ToString();
            string tableZoneList = reader["TableZoneList"].ToString();
            string ipAddress = reader["IPAddress"].ToString();
            string registrationNumber = reader["RegistrationNumber"].ToString();
            string receiptHeader = reader["ReceiptHeader"].ToString();
            string fullTaxHeader = reader["FullTaxHeader"].ToString();
            string deviceCode = reader["DeviceCode"].ToString();
            string kdsID = reader["KDSID"].ToString();
            string kdsPrinters = reader["KDSPrinters"].ToString();
            string description = reader["Description"].ToString();
            string productGroupList = reader["ProductGroupList"].ToString();
            string favoriteImagePageList = reader["FavoriteImagePageList"].ToString();
            string favoriteTextPageList = reader["FavoriteTextPageList"].ToString();

            string query = @"
                INSERT INTO ComputerName 
                (ComputerID, ComputerName, ShopID, ComputerType, PayTypeList, SaleModeList, TableZoneList, IPAddress,
                 RegistrationNumber, ReceiptHeader, FullTaxHeader, DeviceCode, KDSID, KDSPrinters, Description, 
                 ProductGroupList, FavoriteImagePageList, FavoriteTextPageList)
                VALUES
                (@ComputerID, @ComputerName, @ShopID, @ComputerType, @PayTypeList, @SaleModeList, @TableZoneList, @IPAddress,
                 @RegistrationNumber, @ReceiptHeader, @FullTaxHeader, @DeviceCode, @KDSID, @KDSPrinters, @Description, 
                 @ProductGroupList, @FavoriteImagePageList, @FavoriteTextPageList)
                ON DUPLICATE KEY UPDATE
                ComputerName = VALUES(ComputerName), 
                ShopID = VALUES(ShopID), 
                ComputerType = VALUES(ComputerType),
                PayTypeList = VALUES(PayTypeList),
                SaleModeList = VALUES(SaleModeList),
                TableZoneList = VALUES(TableZoneList),
                IPAddress = VALUES(IPAddress),
                RegistrationNumber = VALUES(RegistrationNumber),
                ReceiptHeader = VALUES(ReceiptHeader),
                FullTaxHeader = VALUES(FullTaxHeader),
                DeviceCode = VALUES(DeviceCode),
                KDSID = VALUES(KDSID),
                KDSPrinters = VALUES(KDSPrinters),
                Description = VALUES(Description),
                ProductGroupList = VALUES(ProductGroupList),
                FavoriteImagePageList = VALUES(FavoriteImagePageList),
                FavoriteTextPageList = VALUES(FavoriteTextPageList)";

            int rowsAffected = await UpdateDatabaseAsync(
                LocalConnection,
                query,
                new MySqlParameter("@ComputerID", computerID),
                new MySqlParameter("@ComputerName", computerName),
                new MySqlParameter("@ShopID", shopID),
                new MySqlParameter("@ComputerType", computerType),
                new MySqlParameter("@PayTypeList", payTypeList),
                new MySqlParameter("@SaleModeList", saleModeList),
                new MySqlParameter("@TableZoneList", tableZoneList),
                new MySqlParameter("@IPAddress", ipAddress),
                new MySqlParameter("@RegistrationNumber", registrationNumber),
                new MySqlParameter("@ReceiptHeader", receiptHeader),
                new MySqlParameter("@FullTaxHeader", fullTaxHeader),
                new MySqlParameter("@DeviceCode", deviceCode),
                new MySqlParameter("@KDSID", kdsID),
                new MySqlParameter("@KDSPrinters", kdsPrinters),
                new MySqlParameter("@Description", description),
                new MySqlParameter("@ProductGroupList", productGroupList),
                new MySqlParameter("@FavoriteImagePageList", favoriteImagePageList),
                new MySqlParameter("@FavoriteTextPageList", favoriteTextPageList)
            );

            Log.ForContext("LogType", "SyncLog").Information($"Rows updated in LocalDb for ComputerID {computerID}: {rowsAffected}");
            //Log($"Rows updated in LocalDb for ComputerID {computerID}: {rowsAffected}");
        }

        public async Task<(bool isOpen, bool isNew)> CheckUserSessions(UserSessions userSessions)
        {
            try
            {
                // Create a new instance of LocalDbConnector to get a MySqlConnection
                LocalDbConnector localDbConnector = new LocalDbConnector();

                // Define your SQL query to find the close session with the maximum session ID for a given computer ID
                string query = @"
                SELECT SessionID, OpenStaffID, ComputerID
                FROM session 
                WHERE ComputerID = @ComputerID AND CloseStaffID = 0
                ORDER BY SessionID DESC 
                LIMIT 1";

                using (MySqlConnection connection = localDbConnector.GetMySqlConnection())
                {
                    await connection.OpenAsync();

                    // Create a MySqlCommand object with the query and connection
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        // Add parameters to the command
                        command.Parameters.AddWithValue("@ComputerID", Properties.Settings.Default._ComputerID);
                        bool isNew;
                        // Execute the query and get the result
                        using (MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                int computerID = reader.GetInt32("ComputerID");
                                int SessionIDss = reader.GetInt32("SessionID");

                                SessionsID.SessionIDs = SessionIDss;
                                
                                // Compare the computerID with the expectedComputerID
                                if (computerID == Properties.Settings.Default._ComputerID)
                                {
                                    Console.WriteLine("Computer ID matches the expected value. Setting result to false.");
                                    isNew = false;
                                    return (false, isNew );
                                }
                                else
                                {
                                    isNew = false;
                                    Console.WriteLine("Computer ID does not match the expected value. Setting result to true.");
                                    return (true, isNew);
                                }
                            }
                            else
                            {
                                // No result found
                                Console.WriteLine("No session found for the given OpenStaffID with CloseStaffID = 0.");
                                isNew = true;
                                return (false, isNew);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions
                Console.WriteLine("Error: " + ex.Message);
                
                return (false, false);
            }
        }

        public async Task CreateNewSessionInLocalDatabaseAsync(int computerID, string shopID, int Sessions, UserSessions userSessions = null)
        {
            //SESSIONS 0 = NEW, 1 = OPEN, 2 = CLOSE, 3 = UPDATE
            if (userSessions != null)
            {
                userSessions1 = userSessions;
            }
            if (Sessions == 1)
            {
                await NewSessions();
            } 
            else if (Sessions == 2)
            {
                await CloseSessions();
            } 
        }

        public async Task NewSessions()
        {
            try
            {
                string getLastSessionIdQuery = "SELECT MAX(SessionID) FROM session";
                int newSessionID = 0;
                string ComName = Properties.Settings.Default._ComputerName;

                using (MySqlConnection connection = dbConnector.GetMySqlConnection())
                {
                    await connection.OpenAsync();

                    using (MySqlCommand getLastSessionIdCommand = new MySqlCommand(getLastSessionIdQuery, connection))
                    {
                        object result = await getLastSessionIdCommand.ExecuteScalarAsync();
                        newSessionID = result != DBNull.Value ? Convert.ToInt32(result) : 0;
                        newSessionID++;
                        SessionsID.SessionIDs = newSessionID;
                    }
                }

                string NewSessionsQuery = @"
                INSERT INTO session 
                (SessionID, ComputerID, SessionKey, ComputerName, OpenStaffID, OpenStaff, CloseStaffID, CloseStaff, OpenSessionDateTime, 
                 CloseSessionDateTime, SessionDate, OpenSessionAmount, CashAmount, CashInAmount, CashOutAmount, DropCashAmount, 
                 CloseSessionAmount, CashShortOver, SessionUpdateDate, ShopID, IsEndDaySession)
                VALUES
                (@SessionID, @ComputerID, @SessionKey, @ComputerName, @OpenStaffID, @OpenStaff, @CloseStaffID, @CloseStaff, @OpenSessionDateTime, 
                 @CloseSessionDateTime, @SessionDate, @OpenSessionAmount, @CashAmount, @CashInAmount, @CashOutAmount, @DropCashAmount, 
                 @CloseSessionAmount, @CashShortOver, @SessionUpdateDate, @ShopID, @IsEndDaySession)";
                using (MySqlConnection connection = dbConnector.GetMySqlConnection())
                {
                    await connection.OpenAsync();

                    using (MySqlCommand command = new MySqlCommand(NewSessionsQuery, connection))
                    {
                        command.Parameters.AddWithValue("@SessionID", newSessionID);
                        command.Parameters.AddWithValue("@ComputerID", Properties.Settings.Default._ComputerID);
                        command.Parameters.AddWithValue("@SessionKey", $"{newSessionID}:{Properties.Settings.Default._ComputerID}"); // Default value
                        command.Parameters.AddWithValue("@ComputerName", ComName); // Default value
                        command.Parameters.AddWithValue("@OpenStaffID", UserSessions.Current_StaffID); // Default value
                        command.Parameters.AddWithValue("@OpenStaff", UserSessions.Current_StaffFirstName); // Default value
                        command.Parameters.AddWithValue("@CloseStaffID", 0); // Default value
                        command.Parameters.AddWithValue("@CloseStaff", string.Empty); // Default value
                        command.Parameters.AddWithValue("@OpenSessionDateTime", DateTime.Now);
                        command.Parameters.AddWithValue("@CloseSessionDateTime", null); // Set close session time to null
                        command.Parameters.AddWithValue("@SessionDate", DateTime.Now); // Default to current date
                        command.Parameters.AddWithValue("@OpenSessionAmount", PosSession.POS_SESSION_OPENCASH); // Default value
                        command.Parameters.AddWithValue("@CashAmount", 0.0m); // Default value
                        command.Parameters.AddWithValue("@CashInAmount", 0.0m); // Default value
                        command.Parameters.AddWithValue("@CashOutAmount", 0.0m); // Default value
                        command.Parameters.AddWithValue("@DropCashAmount", 0.0m); // Default value
                        command.Parameters.AddWithValue("@CloseSessionAmount", 0.0m); // Default value
                        command.Parameters.AddWithValue("@CashShortOver", 0.0m); // Default value
                        command.Parameters.AddWithValue("@SessionUpdateDate", DBNull.Value); // Default to current date
                        command.Parameters.AddWithValue("@ShopID", Properties.Settings.Default._AppID);
                        command.Parameters.AddWithValue("@IsEndDaySession", 0);

                        int rowsAffected = await command.ExecuteNonQueryAsync();
                        Log.ForContext("LogType", "SyncLog").Information($"New session created with SessionID {newSessionID} and ComputerID {Properties.Settings.Default._ComputerID}. Rows affected: {rowsAffected}");
                    }
                }
            }
            catch (MySqlException ex)
            {
                // Handle MySQL exceptions
                Console.WriteLine("MySQL Error: " + ex.Message);
            }
            catch (Exception ex)
            {
                // Handle any other exceptions
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        public async Task CloseSessions()
        {
            try
            {
                int computerID = Properties.Settings.Default._ComputerID;

                string getLastSessionIdQuery = "SELECT MAX(SessionID) FROM session WHERE ComputerID = @ComputerID AND CloseStaffID = 0";
                int newSessionID = 0;
                string ComName = Properties.Settings.Default._ComputerName;

                using (MySqlConnection connection = dbConnector.GetMySqlConnection())
                {
                    await connection.OpenAsync();

                    using (MySqlCommand getLastSessionIdCommand = new MySqlCommand(getLastSessionIdQuery, connection))
                    {
                        getLastSessionIdCommand.Parameters.AddWithValue("@ComputerID", computerID);
                        object result = await getLastSessionIdCommand.ExecuteScalarAsync();
                        newSessionID = result != DBNull.Value ? Convert.ToInt32(result) : 0;
                    }
                }

                string CloseSessionsQuery = @"
                UPDATE session 
                SET CloseSessionAmount = @CloseSessionAmount, CloseSessionDateTime = @CloseSessionDateTime,  CloseStaffID = @CloseStaffID, CloseStaff = @CloseStaff
                WHERE SessionID = @SessionID";
                using (MySqlConnection connection = dbConnector.GetMySqlConnection())
                {
                    await connection.OpenAsync();

                    using (MySqlCommand command = new MySqlCommand(CloseSessionsQuery, connection))
                    {
                        command.Parameters.AddWithValue("@CloseSessionAmount", PosSession.POS_SESSION_CLOSECASH);
                        command.Parameters.AddWithValue("@CloseSessionDateTime", DateTime.Now); // Set close session time to null
                        command.Parameters.AddWithValue("@CloseStaffID", UserSessions.Current_StaffID.ToString()) ; // Set close session time to null
                        command.Parameters.AddWithValue("@CloseStaff", UserSessions.Current_StaffFirstName.ToString()) ; // Set close session time to null
                        command.Parameters.AddWithValue("@SessionID", newSessionID);

                        int rowsAffected = await command.ExecuteNonQueryAsync();
                        Log.ForContext("LogType", "SyncLog").Information($"Close session created with SessionID {newSessionID} and ComputerID {computerID}. Rows affected: {rowsAffected}");
                    }
                }
            }
            catch (MySqlException ex)
            {
                // Handle MySQL exceptions
                Console.WriteLine("MySQL Error: " + ex.Message);
            }
            catch (Exception ex)
            {
                // Handle any other exceptions
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        public async Task UpdateSessions(int ExportImport, int SyncTypeID)
        {
            try
            {
                string shopID = Properties.Settings.Default._AppID;
                int computerID = Properties.Settings.Default._ComputerID;
                int exportImport = ExportImport;
                int syncTypeID = SyncTypeID;
                // Define your SQL query to insert into log_lastsync
                string UpdateSessionsQuery = @"
                INSERT INTO log_lastsync 
                (ShopID, ComputerID, ExportImport, SyncTypeID, SyncLastUpdate) 
                VALUES 
                (@ShopID, @ComputerID, @ExportImport, @SyncTypeID, @SyncLastUpdate)";

                using (MySqlConnection connection = dbConnector.GetMySqlConnection())
                {
                    await connection.OpenAsync();

                    using (MySqlCommand command = new MySqlCommand(UpdateSessionsQuery, connection))
                    {
                        // Add parameters to the command
                        command.Parameters.AddWithValue("@ShopID", shopID);
                        command.Parameters.AddWithValue("@ComputerID", computerID);
                        command.Parameters.AddWithValue("@ExportImport", exportImport);
                        command.Parameters.AddWithValue("@SyncTypeID", syncTypeID);
                        command.Parameters.AddWithValue("@SyncLastUpdate", DateTime.Now);

                        int rowsAffected = await command.ExecuteNonQueryAsync();
                        Log.ForContext("LogType", "SyncLog").Information($"Update session created with ShopID {shopID} and ComputerID {computerID}. Rows affected: {rowsAffected}");
                    }
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("MySQL Error: " + ex.Message);
                Log.ForContext("LogType", "SyncLog").Error("MySQL Error: {0}", ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                Log.ForContext("LogType", "SyncLog").Error("Error: {0}", ex.Message);
            }
        }

        private async Task<int> UpdateDatabaseAsync(MySqlConnection connection, string sqlQuery, params MySqlParameter[] parameters)
        {
            int rowsAffected = 0;
            try
            {
                using (MySqlCommand command = new MySqlCommand(sqlQuery, connection))
                {
                    command.Parameters.AddRange(parameters);
                    rowsAffected = await command.ExecuteNonQueryAsync();
                    Log.ForContext("LogType", "SyncLog").Information($"Rows affected: {rowsAffected}");
                    //Log($"Rows affected: {rowsAffected}");
                }
            }
            catch (Exception ex)
            {
                Log.ForContext("LogType", "SyncLog").Information("Error updating database", ex);
                //LogError("Error updating database", ex);
            }
            return rowsAffected;
        }
    }
}
