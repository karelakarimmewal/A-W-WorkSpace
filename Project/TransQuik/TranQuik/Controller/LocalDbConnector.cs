using MySql.Data.MySqlClient;
using System;
using TranQuik.Configuration;

public class LocalDbConnector
{
    public string ComputerReceiptHeader;
    public MySqlConnection GetMySqlConnection()
    {
        string connectionString = $"Server={DatabaseSettings.LocalDbServer};Port={DatabaseSettings.LocalDbPort};" +
                                   $"Database={DatabaseSettings.LocalDbName};Uid={DatabaseSettings.LocalDbUser};" +
                                   $"Pwd={DatabaseSettings.LocalDbPassword};";

        MySqlConnection connection = new MySqlConnection(connectionString);
        return connection;
    }

    public void RetrieveReceiptHeader(int computerId)
    {
        using (MySqlConnection connection = GetMySqlConnection())
        {
            string query = "SELECT ReceiptHeader FROM computername WHERE ComputerID = @ComputerID;";

            MySqlCommand cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@ComputerID", computerId);

            try
            {
                connection.Open();
                MySqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    ComputerReceiptHeader = reader["ReceiptHeader"].ToString();
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                // Handle any errors here
                Console.WriteLine(ex.Message);
                // You might want to log the error or throw an exception
            }
        }
    }
}
