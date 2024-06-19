using MySql.Data.MySqlClient;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using TranQuik.Configuration;
using TranQuik.Model;

namespace TranQuik.Controller
{
    public class GET_CloudSyncHQ
    {
        private LocalDbConnector _localDbConnector;
        public GET_CloudSyncHQ() 
        {
            this._localDbConnector = new LocalDbConnector();
        }

        public List<ComputerAccessData> GetComputerAccessData(int shopID)
        {
            Log.ForContext("LogType", "SyncLog").Information("Getting Data From HQ Started");

            List<ComputerAccessData> dataList = new List<ComputerAccessData>();

            string connectionString = $"Server={DatabaseSettings.CloudDbServer},{DatabaseSettings.CloudDbPort};" +
                                      $"Database={DatabaseSettings.CloudDbName};User Id={DatabaseSettings.CloudDbUser};" +
                                      $"Password={DatabaseSettings.CloudDbPassword};";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = @"SELECT ComputerID, ComputerName, ShopID, ComputerType, PayTypeList, SaleModeList, TableZoneList,
                                    IPAddress, RegistrationNumber, ReceiptHeader, FullTaxHeader, DeviceCode, 
                                    KDSID, KDSPrinters, Description, ProductGroupList, 
                                    FavoriteImagePageList, FavoriteTextPageList, Deleted 
                             FROM computername
                             WHERE ShopID = @ShopID";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ShopID", shopID);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ComputerAccessData data = new ComputerAccessData
                                {
                                    ComputerID = reader.GetInt32(reader.GetOrdinal("ComputerID")),
                                    ComputerName = reader.GetString(reader.GetOrdinal("ComputerName")),
                                    ShopID = reader.GetInt32(reader.GetOrdinal("ShopID")),
                                    ComputerType = reader.GetString(reader.GetOrdinal("ComputerType")),
                                    PayTypeList = reader.GetString(reader.GetOrdinal("PayTypeList")),
                                    SaleModeList = reader.GetString(reader.GetOrdinal("SaleModeList")),
                                    TableZoneList = reader.GetString(reader.GetOrdinal("TableZoneList")),
                                    IPAddress = reader.GetString(reader.GetOrdinal("IPAddress")),
                                    RegistrationNumber = reader.GetString(reader.GetOrdinal("RegistrationNumber")),
                                    ReceiptHeader = reader.GetString(reader.GetOrdinal("ReceiptHeader")),
                                    FullTaxHeader = reader.GetString(reader.GetOrdinal("FullTaxHeader")),
                                    DeviceCode = reader.GetString(reader.GetOrdinal("DeviceCode")),
                                    KDSID = reader.GetString(reader.GetOrdinal("KDSID")),
                                    KDSPrinters = reader.GetString(reader.GetOrdinal("KDSPrinters")),
                                    Description = reader.GetString(reader.GetOrdinal("Description")),
                                    ProductGroupList = reader.GetString(reader.GetOrdinal("ProductGroupList")),
                                    FavoriteImagePageList = reader.GetString(reader.GetOrdinal("FavoriteImagePageList")),
                                    FavoriteTextPageList = reader.GetString(reader.GetOrdinal("FavoriteTextPageList")),
                                    Deleted = reader.GetBoolean(reader.GetOrdinal("Deleted"))
                                };
                                dataList.Add(data);
                                Console.WriteLine("Count 1");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error fetching data from HQ.");
                }
            }

            Log.ForContext("LogType", "SyncLog").Information("Getting Data From HQ Stopped");
            return dataList;
        }



        public void SyncDataToLocalDatabase(DataTable dataTable)
        {
            Log.ForContext("LogType", "SyncLog").Information("Update Data From HQ To Local Database Started");
            using (MySqlConnection connection = _localDbConnector.GetMySqlConnection())
            {
                try
                {
                    connection.Open();

                    foreach (DataRow row in dataTable.Rows)
                    {
                        string query = @"INSERT INTO computername (ComputerID, ComputerName, ShopID, ComputerType, PayTypeList, SaleModeList, TableZoneList,
                                                                IPAddress, RegistrationNumber, ReceiptHeader, FullTaxHeader, DeviceCode, 
                                                                KDSID, KDSPrinters, Description, ProductGroupList, 
                                                                FavoriteImagePageList, FavoriteTextPageList, Deleted)
                                         VALUES (@ComputerID, @ComputerName, @ShopID, @ComputerType, @PayTypeList, @SaleModeList, @TableZoneList,
                                                 @IPAddress, @RegistrationNumber, @ReceiptHeader, @FullTaxHeader, @DeviceCode, 
                                                 @KDSID, @KDSPrinters, @Description, @ProductGroupList, 
                                                 @FavoriteImagePageList, @FavoriteTextPageList, @Deleted)
                                         ON DUPLICATE KEY UPDATE
                                             ComputerName = @ComputerName,
                                             ShopID = @ShopID,
                                             ComputerType = @ComputerType,
                                             PayTypeList = @PayTypeList,
                                             SaleModeList = @SaleModeList,
                                             TableZoneList = @TableZoneList,
                                             IPAddress = @IPAddress,
                                             RegistrationNumber = @RegistrationNumber,
                                             ReceiptHeader = @ReceiptHeader,
                                             FullTaxHeader = @FullTaxHeader,
                                             DeviceCode = @DeviceCode,
                                             KDSID = @KDSID,
                                             KDSPrinters = @KDSPrinters,
                                             Description = @Description,
                                             ProductGroupList = @ProductGroupList,
                                             FavoriteImagePageList = @FavoriteImagePageList,
                                             FavoriteTextPageList = @FavoriteTextPageList,
                                             Deleted = @Deleted";

                        using (MySqlCommand cmd = new MySqlCommand(query, connection))
                        {
                            cmd.Parameters.AddWithValue("@ComputerID", row["ComputerID"]);
                            cmd.Parameters.AddWithValue("@ComputerName", row["ComputerName"]);
                            cmd.Parameters.AddWithValue("@ShopID", row["ShopID"]);
                            cmd.Parameters.AddWithValue("@ComputerType", row["ComputerType"]);
                            cmd.Parameters.AddWithValue("@PayTypeList", row["PayTypeList"]);
                            cmd.Parameters.AddWithValue("@SaleModeList", row["SaleModeList"]);
                            cmd.Parameters.AddWithValue("@TableZoneList", row["TableZoneList"]);
                            cmd.Parameters.AddWithValue("@IPAddress", row["IPAddress"]);
                            cmd.Parameters.AddWithValue("@RegistrationNumber", row["RegistrationNumber"]);
                            cmd.Parameters.AddWithValue("@ReceiptHeader", row["ReceiptHeader"]);
                            cmd.Parameters.AddWithValue("@FullTaxHeader", row["FullTaxHeader"]);
                            cmd.Parameters.AddWithValue("@DeviceCode", row["DeviceCode"]);
                            cmd.Parameters.AddWithValue("@KDSID", row["KDSID"]);
                            cmd.Parameters.AddWithValue("@KDSPrinters", row["KDSPrinters"]);
                            cmd.Parameters.AddWithValue("@Description", row["Description"]);
                            cmd.Parameters.AddWithValue("@ProductGroupList", row["ProductGroupList"]);
                            cmd.Parameters.AddWithValue("@FavoriteImagePageList", row["FavoriteImagePageList"]);
                            cmd.Parameters.AddWithValue("@FavoriteTextPageList", row["FavoriteTextPageList"]);
                            cmd.Parameters.AddWithValue("@Deleted", row["Deleted"]);

                            int affectedRows = cmd.ExecuteNonQuery();
                            Log.ForContext("LogType", "SyncLog").Information($"Synced data for ComputerID: {row["ComputerID"]}, ShopID: {row["ShopID"]}",
                                                                            row["ComputerID"], row["ShopID"], affectedRows);

                        }
                    }
                    Log.ForContext("LogType", "SyncLog").Information("Update Data From HQ To Local Database Started");
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error syncing data to local database.");
                }
            }
        }
    }
}
