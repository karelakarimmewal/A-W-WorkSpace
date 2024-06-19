using System;
using System.Data.Common;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace TranQuik.Controller
{
    public class SessionMethod
    {
        private readonly LocalDbConnector dbConnector;

        public SessionMethod()
        {
            dbConnector = new LocalDbConnector();
        }

        public async Task<DateTime> CheckThisOpenSession()
        {
            try
            {
                string query = @"
                                SELECT `OpenSessionDateTime`
                                FROM `session`
                                WHERE `ComputerName` = @ComputerName AND `ComputerID` = @ComputerID 
                                ORDER BY `SessionID` DESC
                                LIMIT 1;
                                ";
                using (MySqlConnection connection = dbConnector.GetMySqlConnection())
                {
                    await connection.OpenAsync();

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ComputerID", Properties.Settings.Default._ComputerID);
                        command.Parameters.AddWithValue("@ComputerName", Properties.Settings.Default._ComputerName);

                        object result = await command.ExecuteScalarAsync();

                        if (result != null)
                        {
                            return Convert.ToDateTime(result);
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("MySQL Error: " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }

            // If no session is found or an error occurred, return the default DateTime value
            return DateTime.MinValue;
        }

        public async Task<bool> CheckSessionConditionAsync(DateTime thisOpenSession)
        {
            try
            {
                string query = @"
                SELECT `SessionUpdateDate`
                FROM `session`
                WHERE `ComputerName` = @ComputerName
                ORDER BY `SessionID` DESC
                LIMIT 1;
        ";

                using (MySqlConnection connection = dbConnector.GetMySqlConnection())
                {
                    await connection.OpenAsync();

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ComputerName", "POS1");

                        object result = await command.ExecuteScalarAsync();

                        if (result != null)
                        {
                            DateTime updateDate = Convert.ToDateTime(result);

                            // Check if thisOpenSession is earlier than updateDate
                            if (thisOpenSession < updateDate)
                            {
                                return true; // Notify needed
                            }
                            else
                            {
                                return false; // No notify needed
                            }
                        }
                        else
                        {
                            return false; // No update date found, assume no need for notification
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("MySQL Error: " + ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return false;
            }
        }


    }
}
