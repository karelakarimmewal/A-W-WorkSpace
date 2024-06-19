using MySql.Data.MySqlClient;
using System;
using System.Data.SqlClient;

namespace TranQuik.Configuration
{
    internal static class DatabaseSettings
    {
        // Local Database Connection Settings (MySQL)
        public static string LocalDbServer { get; set; }
        public static int LocalDbPort { get; set; }
        public static string LocalDbUser { get; set; }
        public static string LocalDbPassword { get; set; }
        public static string LocalDbName { get; set; }

        // Cloud Database Connection Settings (Microsoft SQL Server)
        public static string CloudDbServer { get; set; }
        public static int CloudDbPort { get; set; }
        public static string CloudDbUser { get; set; }
        public static string CloudDbPassword { get; set; }
        public static string CloudDbName { get; set; }

        public static SqlConnection GetSqlConnection()
        {
            string portPart = CloudDbPort != 0 ? $",Port={CloudDbPort}" : string.Empty;
            string connectionString = $"Server={CloudDbServer}{portPart};" +
                                       $"Database={CloudDbName};" +
                                       $"User Id={CloudDbUser};" +
                                       $"Password={CloudDbPassword};";

            SqlConnection connection = new SqlConnection(connectionString);
            return connection;
        }

        public static MySqlConnection GetMySqlConnection()
        {
            string portPart = LocalDbPort != 0 ? $";Port={LocalDbPort}" : string.Empty;
            string connectionString = $"Server={LocalDbServer}{portPart};" +
                                       $"Database={LocalDbName};Uid={LocalDbUser};" +
                                       $"Pwd={LocalDbPassword};";

            Console.WriteLine($"Connection String: {connectionString}");
            MySqlConnection connection = new MySqlConnection(connectionString);
            return connection;
        }

    }
}
