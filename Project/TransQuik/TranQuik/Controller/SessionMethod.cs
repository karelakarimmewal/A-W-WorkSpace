using MySql.Data.MySqlClient;
using System;
using System.Threading.Tasks;
using TranQuik.Model;

namespace TranQuik.Controller
{
    public class SessionMethod
    {
        private readonly LocalDbConnector dbConnector;

        public SessionMethod()
        {
            dbConnector = new LocalDbConnector();
        }

        public static DateTime CheckThisOpenSession()
        {
            DateTime results = UserSessions.Current_OpenSessionDate;
            return results;
        }

        public async Task<bool> CheckSessionConditionAsync(DateTime thisOpenSession)
        {
            try
            {
                string query = @"
                SELECT `SyncLastUpdate`
                FROM `log_lastsync`
                WHERE ExportImport = 2
                ORDER BY `SyncLastUpdate` DESC
                LIMIT 1;
        ";

                using (MySqlConnection connection = dbConnector.GetMySqlConnection())
                {
                    await connection.OpenAsync();

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
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
                Console.WriteLine("MySQL Error");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error");
                return false;
            }
        }
    }
}
