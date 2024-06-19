using MySql.Data.MySqlClient;
using System;
using System.Data.SqlClient;

namespace TransQuik_Sync_App.Controller
{
    public static class ConnectionString
    {
        // Local Database Connection Settings (MySQL)
        public static string LocalDbServer { get; set; } = "localhost";
        public static int LocalDbPort { get; set; } = 3308;
        public static string LocalDbUser { get; set; } = "vtecPOS";
        public static string LocalDbPassword { get; set; } = "vtecpwnet";
        public static string LocalDbName { get; set; } = "vtectestaw";

        // Cloud Database Connection Settings (Microsoft SQL Server)
        public static string CloudDbServer { get; set; } = "202.43.164.33";
        public static string CloudDbUser { get; set; } = "dimas";
        public static string CloudDbPassword { get; set; } = "dimas";
        public static string CloudDbName { get; set; } = "VTEC_ID_HQ_DUMMY";

        public static MySqlConnection GetLocalDbConnection()
        {
            string connectionString = $"Server={ConnectionString.LocalDbServer};Port={ConnectionString.LocalDbPort};Database={ConnectionString.LocalDbName};Uid={ConnectionString.LocalDbUser};Pwd={ConnectionString.LocalDbPassword};";
            MySqlConnection connection = new MySqlConnection(connectionString);
            return connection;
        }

        // Method to establish a connection to the cloud SQL Server database
        public static SqlConnection GetCloudDbConnection()
        {
            string connectionString = $"Server={ConnectionString.CloudDbServer};Database={ConnectionString.CloudDbName};User Id={ConnectionString.CloudDbUser};Password={ConnectionString.CloudDbPassword};";
            SqlConnection connection = new SqlConnection(connectionString);
            return connection;
        }

        // Method to check the connection to the local MySQL database
        public static bool TestLocalDbConnection()
        {
            using (var connection = GetLocalDbConnection())
            {
                try
                {
                    connection.Open();
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error connecting to local database: {ex.Message}");
                    return false;
                }
            }
        }

        // Method to check the connection to the cloud SQL Server database
        public static bool TestCloudDbConnection()
        {
            using (var connection = GetCloudDbConnection())
            {
                try
                {
                    connection.Open();
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error connecting to cloud database: {ex.Message}");
                    return false;
                }
            }
        }
    }
}
