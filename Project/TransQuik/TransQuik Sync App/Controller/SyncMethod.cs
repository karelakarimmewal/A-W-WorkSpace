using MySql.Data.MySqlClient;
using System.Data.SqlClient;
using System.IO;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace TransQuik_Sync_App.Controller
{
    public class SyncMethod
    {
        private MySqlConnection LocalConnection;
        private static readonly string logFilePath = "log.txt";
        private readonly Action<int> _updateProgress;

        public SyncMethod(Action<int> updateProgress)
        {
            _updateProgress = updateProgress;
        }

        public async Task SyncDataAsync()
        {
            try
            {
                // Establish connection to the local database
                LocalConnection = ConnectionString.GetLocalDbConnection();
                await LocalConnection.OpenAsync();


                string deleteSqlQueryComputerDetails = "TRUNCATE TABLE ComputerName";
                bool FavComputerDetailsSuccess = await DeleteExistingRecordsAsync(deleteSqlQueryComputerDetails);
                if (FavComputerDetailsSuccess)
                {
                    await SyncDataFromCloudAsyncComputerDetails();
                }

                string deleteSqlQueryFavPageIndex = "TRUNCATE TABLE favoritepageindex";
                bool FavProductPageSuccess = await DeleteExistingRecordsAsync(deleteSqlQueryFavPageIndex);
                if (FavProductPageSuccess)
                {
                    await SyncDataFromCloudAsyncFavoritePageIndex();
                }

                string deleteSqlQueryFav = "TRUNCATE TABLE favoriteproducts";
                bool FavProductSuccess = await DeleteExistingRecordsAsync(deleteSqlQueryFav);
                if (FavProductSuccess)
                {
                    await SyncDataFromCloudAsyncFavoriteProducts();
                }
                
            }
            catch (Exception ex)
            {
                Log($"An error occurred: {ex.Message}");
            }
            finally
            {
                // Close connection to the local database
                await LocalConnection.CloseAsync();
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

            using (SqlConnection cloudConnection = ConnectionString.GetCloudDbConnection())
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
                            _updateProgress(progress);
                        }
                    }
                }
            }
        }

        private async Task SyncDataFromCloudAsyncFavoritePageIndex()
        {
            string cloudSqlQuery = @"
                SELECT [TemplateID]
                      ,[PageIndex]
                      ,[PageName]
                      ,[PageOrder]
                      ,[ButtonColorCode]
                      ,[ButtonColorHexCode]
                      ,[PageType]
                FROM [VTEC_ID_HQ].[dbo].[favoritepageindextemplate] 
                WHERE TemplateID = 2 
                ORDER BY PageOrder ASC";

            using (SqlConnection cloudConnection = ConnectionString.GetCloudDbConnection())
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
                            _updateProgress(progress);
                        }
                    }
                }
            }
        }

        private async Task SyncDataFromCloudAsyncFavoriteProducts()
        {
            string cloudSqlQuery = @"
                                    SELECT
                                        [ProductID],
                                        FSL.ShopID,
                                        FSL.[TemplateID],
                                        [PageIndex],
                                        [ProductCode],
                                        [ButtonOrder],
                                        [ButtonColorCode],
                                        [ButtonColorHexCode],
                                        [ImageFileName],
                                        [PageType]
                                    FROM [VTEC_ID_HQ_DUMMY].[dbo].[favoriteproductstemplate] FPT 
                                    JOIN favoriteShopLink FSL ON FPT.TemplateID = FSL.TemplateID  
                                    WHERE FSL.ShopID = 18 
                                    ORDER BY ButtonOrder";

            using (SqlConnection cloudConnection = ConnectionString.GetCloudDbConnection())
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
                            _updateProgress(progress);
                        }
                    }
                }
            }
        }

        private async Task<bool> DeleteExistingRecordsAsync(string deleteSqlQuery)
        {
            try
            {
                using (MySqlConnection connection = ConnectionString.GetLocalDbConnection())
                {
                    await connection.OpenAsync();

                    using (MySqlCommand deleteCommand = new MySqlCommand(deleteSqlQuery, connection))
                    {
                        int rowsAffected = await deleteCommand.ExecuteNonQueryAsync();
                        Log($"Deleted {rowsAffected} records for query: {deleteSqlQuery}");
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"Error deleting existing records: {ex.Message}", true);
                return false;
            }
        }

        private async Task InsertIntoLocalDatabaseAsyncFavoritePageIndex(SqlDataReader reader)
        {
            int ShopID = 18;
            int ComputerType = 0;
            int pageIndex = (int)reader["PageIndex"];
            string pageName = reader["PageName"].ToString();
            int pageOrder = (int)reader["PageOrder"];
            string buttonColorCode = reader["ButtonColorCode"].ToString();
            string buttonColorHexCode = reader["ButtonColorHexCode"].ToString();
            string pageType = reader["PageType"].ToString();

            string localSqlQuery = @"
                INSERT INTO favoritepageindex
                (ShopID,ComputerType, PageIndex, PageName, PageOrder, ButtonColorCode, ButtonColorHexCode, PageType)
                VALUES
                (@ShopID, @ComputerType, @PageIndex, @PageName, @PageOrder, @ButtonColorCode, @ButtonColorHexCode, @PageType)
                ON DUPLICATE KEY UPDATE
                PageName = VALUES(PageName),
                PageOrder = VALUES(PageOrder),
                ButtonColorCode = VALUES(ButtonColorCode),
                ButtonColorHexCode = VALUES(ButtonColorHexCode),
                PageType = VALUES(PageType)";

            await UpdateDatabaseAsync(
                LocalConnection,
                localSqlQuery,
                new MySqlParameter("@ShopID", ShopID),
                new MySqlParameter("@ComputerType", ComputerType),
                new MySqlParameter("@PageIndex", pageIndex),
                new MySqlParameter("@PageName", pageName),
                new MySqlParameter("@PageOrder", pageOrder),
                new MySqlParameter("@ButtonColorCode", buttonColorCode),
                new MySqlParameter("@ButtonColorHexCode", buttonColorHexCode),
                new MySqlParameter("@PageType", pageType)
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

            // Construct the SQL query for insert or update
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

            // Execute the query
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

            // Log the result
            Log($"Rows updated in LocalDb for ComputerID {computerID}: {rowsAffected}");
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

                    // Log the number of rows affected after execution
                    Log($"Rows affected: {rowsAffected}");
                }
            }
            catch (Exception ex)
            {
                LogError("Error updating database", ex);
            }
            return rowsAffected;
        }

        private void Log(string message)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(logFilePath, true))
                {
                    writer.WriteLine($"{DateTime.Now}: {message}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error writing to log file: {ex.Message}", "Logging Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LogError(string errorMessage, Exception ex)
        {
            Log($"{errorMessage}: {ex.Message}");
        }


        private void Log(string message, bool isError = false)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(logFilePath, true))
                {
                    writer.WriteLine($"{DateTime.Now}: {(isError ? "ERROR" : "INFO")}: {message}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error writing to log file: {ex.Message}", "Logging Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}
