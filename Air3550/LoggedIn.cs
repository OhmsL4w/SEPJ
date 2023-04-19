using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Air3550
{
    public class LoggedIn
    {
        private User CurUser;
        public LoggedIn(string UserID)
        {
            CurUser = new User();
            CurUser.UserID = UserID;
            using (SqlConnection sqlConn = new SqlConnection("Data Source=(local);Database=Air3550;Integrated Security=true;"))
            {
                sqlConn.Open();
                string queryString = "SELECT IsManager, IsEngineer, FirstName, LastName, Phone, Birthday, PointsAvailable, PointsUsed, CreditCard FROM Users WHERE Users.UserID =" + "\'" + UserID + "\'";
                SqlCommand query = new SqlCommand(queryString, sqlConn);
                using (SqlDataReader reader = query.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        CurUser.IsManager = reader.GetBoolean(0);
                        CurUser.IsEngineer = reader.GetBoolean(1);
                        CurUser.FirstName = reader.GetString(2);
                        CurUser.LastName = reader.GetString(3);
                        CurUser.Phone = reader.GetString(4);
                        CurUser.Birthday = reader.GetSqlDateTime(5);
                        CurUser.PointsAvailable = reader.GetInt32(6);
                        CurUser.CreditCard = reader.GetString(7);
                    }
                }
                sqlConn.Close();
            }
        }

        public void LoggedInLoop()
        {
            while (true)
            {
                Console.WriteLine($"Logged In as: {CurUser.FirstName} {CurUser.LastName}");
                Console.WriteLine($"Currently has {CurUser.PointsAvailable}\n");
                Console.WriteLine("Input a number to continue");
                Console.WriteLine("1. Book Flight");
                Console.WriteLine("2. Change Account Information");
                Console.WriteLine("3. View Past Flights");
                string? input = Console.ReadLine();
                if (input == null | (input != "1" & input != "2" & input != "3"))
                {
                    Console.WriteLine("Please input a correct input");
                    continue;
                }
                switch (input)
                {
                    case "1":
                        Login.LoginMethod();
                        break;
                    case "2":
                        return;
                    case "3":
                        Login.CreateAccount();
                        break;
                }
            }
        }

    }
    public class User
    {
        public string UserID { get; set; } = "";
        public bool IsManager { get; set; } = false;
        public bool IsEngineer { get; set; } = false;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Phone { get; set; }
        public SqlDateTime? Birthday { get; set; }
        public int? PointsAvailable { get; set; }
        public int? PointsUsed { get; set; }
        public string? CreditCard { get; set; }

    }
}
