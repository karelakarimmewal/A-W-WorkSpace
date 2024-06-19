using MySql.Data.MySqlClient;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using TranQuik.Configuration;

namespace TranQuik.Controller
{
    public class SyncMethod
    {
        private MySqlConnection LocalConnection;
        private static readonly string logFilePath = "log.txt";
        private readonly Action<int> _updateProgress;

        public SyncMethod(Action<int> updateProgress = null)
        {
            _updateProgress = updateProgress;
        }

        public async Task SyncDataAsync()
        {
            try
            {
                // Establish connection to the local database
                LocalConnection = DatabaseSettings.GetMySqlConnection();
                await LocalConnection.OpenAsync();

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

                //string deleteSqlQueryProductComponentGroup = "TRUNCATE TABLE productcomponentgroup";
                //bool productComponentGroupSuccess = await DeleteExistingRecordsAsync(deleteSqlQueryProductComponentGroup);
                //if (productComponentGroupSuccess)
                //{
                //    await SyncDataFromCloudAsyncProductComponentGroup();
                //}

                string deleteSqlSaleMode = "TRUNCATE TABLE salemode ";
                bool saleModeSuccess = await DeleteExistingRecordsAsync(deleteSqlSaleMode);
                if (saleModeSuccess)
                {
                    await SyncDataFromCloudAsyncSaleMode();
                }
            }
            catch (Exception ex)
            {
                //Log.ForContext("LogType", "SyncLog").Information("An error occurred", ex);
                //LogError("An error occurred", ex);
            }
            finally
            {
                // Close connection to the local database
                if (LocalConnection != null && LocalConnection.State == System.Data.ConnectionState.Open)
                {
                    TransQuikConfiguration.Properties.Settings.Default._LastSync = DateTime.Now;
                    AppSettings.LastSync = TransQuikConfiguration.Properties.Settings.Default._LastSync;
                    TransQuikConfiguration.Properties.Settings.Default.Save();
                    Config.SaveAppSettings();
                    await LocalConnection.CloseAsync();
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
                WHERE ShopID = 18";

            using (SqlConnection cloudConnection = DatabaseSettings.GetSqlConnection())
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

            using (SqlConnection cloudConnection = DatabaseSettings.GetSqlConnection())
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
                WHERE FSL.ShopID = 18
                ORDER BY ButtonOrder";

            using (SqlConnection cloudConnection = DatabaseSettings.GetSqlConnection())
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

            using (SqlConnection cloudConnection = DatabaseSettings.GetSqlConnection())
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

            using (SqlConnection cloudConnection = DatabaseSettings.GetSqlConnection())
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

            using (SqlConnection cloudConnection = DatabaseSettings.GetSqlConnection())
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
                using (MySqlConnection connection = DatabaseSettings.GetMySqlConnection())
                {
                    await connection.OpenAsync();

                    using (MySqlCommand deleteCommand = new MySqlCommand(deleteSqlQuery, connection))
                    {
                        int rowsAffected = await deleteCommand.ExecuteNonQueryAsync();
                        //Log.ForContext("LogType", "SyncLog").Information($"Deleted {rowsAffected} records for query: {deleteSqlQuery}");
                        //Log($"Deleted {rowsAffected} records for query: {deleteSqlQuery}");
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                //Log.ForContext("LogType", "SyncLog").Information($"Error deleting existing records", ex);
                //LogError("Error deleting existing records", ex);
                return false;
            }
        }

        private async Task InsertIntoLocalDatabaseAsyncFavoritePageIndex(SqlDataReader reader)
        {
            int shopID = 18;
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
            DateTime? startDate = reader["StartDate"] as DateTime?;
            DateTime? endDate = reader["EndDate"] as DateTime?;
            int setGroupNo = (int)reader["SetGroupNo"];
            string setGroupName = reader["SetGroupName"] != DBNull.Value ? reader["SetGroupName"].ToString() : null;
            byte requireAddAmountForProduct = (byte)reader["RequireAddAmountForProduct"];
            int minQty = (int)reader["MinQty"];
            int maxQty = (int)reader["MaxQty"];
            int addingFromBranch = (int)reader["AddingFromBranch"];
            byte isDefault = (byte)reader["IsDefault"];
            short calPrice = (short)reader["CalPrice"];
            int packageTypeID = (int)reader["PackageTypeID"];
            int promotionID = (int)reader["PromotionID"];
            int voucherHeaderID = (int)reader["VoucherHeaderID"];
            int staffRoleID = (int)reader["StaffRoleID"];
            string setGroupText = reader["SetGroupText"] != DBNull.Value ? reader["SetGroupText"].ToString() : null;

            string localSqlQuery = @"
    INSERT INTO productcomponentgroup (
        PGroupID, PGroupTypeID, ProductID, SaleMode, StartDate, EndDate, SetGroupNo, SetGroupName, RequireAddAmountForProduct, MinQty, MaxQty, AddingFromBranch, IsDefault, CalPrice, PackageTypeID, PromotionID, VoucherHeaderID, StaffRoleID, SetGroupText, ExpireType, ExpireDate, ExpireAfterDay, ExpireAfterMonth, ExpireAfterYear
    ) VALUES (
        @PGroupID, @PGroupTypeID, @ProductID, @SaleMode, @StartDate, @EndDate, @SetGroupNo, @SetGroupName, @RequireAddAmountForProduct, @MinQty, @MaxQty, @AddingFromBranch, @IsDefault, @CalPrice, @PackageTypeID, @PromotionID, @VoucherHeaderID, @StaffRoleID, @SetGroupText, 0, NULL, 0, 0, 0
    ) ON DUPLICATE KEY UPDATE
        PGroupTypeID = VALUES(PGroupTypeID),
        StartDate = VALUES(StartDate),
        EndDate = VALUES(EndDate),
        SetGroupNo = VALUES(SetGroupNo),
        SetGroupName = VALUES(SetGroupName),
        RequireAddAmountForProduct = VALUES(RequireAddAmountForProduct),
        MinQty = VALUES(MinQty),
        MaxQty = VALUES(MaxQty),
        AddingFromBranch = VALUES(AddingFromBranch),
        IsDefault = VALUES(IsDefault),
        CalPrice = VALUES(CalPrice),
        PackageTypeID = VALUES(PackageTypeID),
        PromotionID = VALUES(PromotionID),
        VoucherHeaderID = VALUES(VoucherHeaderID),
        StaffRoleID = VALUES(StaffRoleID),
        SetGroupText = VALUES(SetGroupText),
        ExpireType = VALUES(ExpireType),
        ExpireDate = VALUES(ExpireDate),
        ExpireAfterDay = VALUES(ExpireAfterDay),
        ExpireAfterMonth = VALUES(ExpireAfterMonth),
        ExpireAfterYear = VALUES(ExpireAfterYear)";

            await UpdateDatabaseAsync(
                LocalConnection,
                localSqlQuery,
                new MySqlParameter("@PGroupID", pGroupID),
                new MySqlParameter("@PGroupTypeID", pGroupTypeID),
                new MySqlParameter("@ProductID", productID),
                new MySqlParameter("@SaleMode", saleMode),
                new MySqlParameter("@StartDate", startDate.HasValue ? (object)startDate.Value : DBNull.Value),
                new MySqlParameter("@EndDate", endDate.HasValue ? (object)endDate.Value : DBNull.Value),
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
                new MySqlParameter("@SetGroupText", setGroupText ?? (object)DBNull.Value)
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

            //Log.ForContext("LogType", "SyncLog").Information($"Rows updated in LocalDb for ComputerID {computerID}: {rowsAffected}");
            //Log($"Rows updated in LocalDb for ComputerID {computerID}: {rowsAffected}");
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
                    //Log.ForContext("LogType", "SyncLog").Information($"Rows affected: {rowsAffected}");
                    //Log($"Rows affected: {rowsAffected}");
                }
            }
            catch (Exception ex)
            {
                //Log.ForContext("LogType", "SyncLog").Information("Error updating database", ex);
                //LogError("Error updating database", ex);
            }
            return rowsAffected;
        }
    }
}
