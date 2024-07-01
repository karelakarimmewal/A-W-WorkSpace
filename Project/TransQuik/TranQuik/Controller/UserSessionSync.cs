using System.Data.SqlClient;
using System;
using TranQuik.Model;
using MySql.Data.MySqlClient;

namespace TranQuik.Controller
{
    public class StaffLoginLogOutTime
    {
        public static int LogInOutID { get; set; }
        public static int StaffID { get; set; }
        public static string Staff_FirstName { get; set; }
        public static string Staff_LastName { get; set; }
        public static int Staff_ComputerID { get; set; }
        public static int Staff_ProgramTypeID { get; set; }
        public static bool CanLogin {get; set;}

        private LocalDbConnector LocalDbConnector = new LocalDbConnector();

        public StaffLoginLogOutTime()
        {
            StaffID = UserSessions.Current_StaffID;
            Staff_FirstName = UserSessions.Current_StaffFirstName;
            Staff_LastName = UserSessions.Current_StaffLastName;
            Staff_ComputerID = Properties.Settings.Default._ComputerID;
            var ProgramTypeID = ComputerAccessData.ComputerAccessDatas
                .Find(v => v.ComputerID == Staff_ComputerID);
            Staff_ProgramTypeID = 0;
        }

        public void CreateStaffSession()
        {
            using (MySqlConnection conn = LocalDbConnector.GetMySqlConnection())
            {
                conn.Open();

                // Check if there is an open session
                string checkQuery = "SELECT LogInOutID, ComputerID FROM staffloginouttime " +
                                    "WHERE StaffID = @StaffID AND LogOutTime IS NULL LIMIT 1";

                using (MySqlCommand checkCmd = new MySqlCommand(checkQuery, conn))
                {
                    checkCmd.Parameters.AddWithValue("@StaffID", StaffID);

                    using (MySqlDataReader reader = checkCmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int existingLogInOutID = reader.GetInt32("LogInOutID");
                            int existingComputerID = reader.GetInt32("ComputerID");

                            if (existingComputerID != Staff_ComputerID)
                            {
                                CanLogin = false;
                                Notification.NotificationLoginAnotherUserIsActivate();
                                return;
                            }
                            CanLogin = true;
                            LogInOutID = existingLogInOutID;
                            return;
                        }
                    }
                }

                // If no open session, create a new one
                string query = "INSERT INTO staffloginouttime (StaffID, ComputerID, LogInTime, LogOutTime, ProgramTypeID) " +
                               "VALUES (@StaffID, @ComputerID, @LogInTime, NULL, @ProgramTypeID); " +
                               "SELECT LAST_INSERT_ID();";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@StaffID", StaffID);
                    cmd.Parameters.AddWithValue("@ComputerID", Staff_ComputerID);
                    cmd.Parameters.AddWithValue("@LogInTime", DateTime.Now);
                    cmd.Parameters.AddWithValue("@ProgramTypeID", Staff_ProgramTypeID);

                    object result = cmd.ExecuteScalar();
                    int logInOutID = Convert.ToInt32(result);

                    LogInOutID = logInOutID;
                    Console.WriteLine("New session created with LogInOutID: " + LogInOutID);
                }
            }
        }

        public void CloseStaffSession()
        {
            using (MySqlConnection conn = LocalDbConnector.GetMySqlConnection())
            {
                string query = "UPDATE staffloginouttime SET LogOutTime = @LogOutTime " +
                               "WHERE LogInOutID = @LogInOutID AND StaffID = @StaffID AND ComputerID = @ComputerID AND ProgramTypeID = @ProgramTypeID AND LogOutTime IS NULL";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@LogInOutID", LogInOutID);
                    cmd.Parameters.AddWithValue("@LogOutTime", DateTime.Now);
                    cmd.Parameters.AddWithValue("@StaffID", StaffID);
                    cmd.Parameters.AddWithValue("@ComputerID", Staff_ComputerID);
                    cmd.Parameters.AddWithValue("@ProgramTypeID", Staff_ProgramTypeID);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
