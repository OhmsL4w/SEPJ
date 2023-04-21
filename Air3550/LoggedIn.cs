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
            //using (SqlConnection sqlConn = new SqlConnection(connectionString: "Data Source=(localdb)\\ProjectModels;Initial Catalog=Air3550;Integrated Security=True;Encrypt=False;"))
            using (SqlConnection sqlConn = new SqlConnection("Data Source=(local);Database=Air3550;Integrated Security=true;"))
            {
                sqlConn.Open();
                string queryString = $"SELECT IsManager, IsEngineer, FirstName, LastName, Phone, Birthday, PointsAvailable, PointsUsed, CreditCard, Address FROM Users WHERE Users.UserID = {UserID}";
                SqlCommand query = new SqlCommand(queryString, sqlConn);
                using (SqlDataReader reader = query.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        CurUser.IsManager = reader.GetBoolean(0);
                        CurUser.IsEngineer = reader.GetBoolean(1);
                        CurUser.FirstName = reader.GetString(2);
                        CurUser.LastName = reader.GetString(3);
                        if (!reader.IsDBNull(4))
                        {
                            CurUser.Phone = reader.GetString(4);
                        }
                        if (!reader.IsDBNull(5))
                        {
                            CurUser.Birthday = reader.GetDateTime(5);
                        }
                        CurUser.PointsAvailable = reader.GetInt32(6);
                        CurUser.PointsUsed = reader.GetInt32(7);
                        if(!reader.IsDBNull(8))
                        {
                            CurUser.CreditCard = reader.GetString(8);
                        }
                        if (!reader.IsDBNull(9))
                        {
                            CurUser.Address = reader.GetString(9);
                        }
                    }
                }
                sqlConn.Close();
            }
        }

        public void LoggedInLoop()
        {
            while (true)
            {
                Console.WriteLine($"\nLogged In as: {CurUser.FirstName} {CurUser.LastName}");
                Console.WriteLine($"Currently has {CurUser.PointsAvailable} points\n");
                Console.WriteLine("Input an option to continue");
                Console.WriteLine("1. Book Flight");
                Console.WriteLine("2. Change Account Information");
                Console.WriteLine("3. View Past Flights");
                Console.WriteLine("4. Cancel Flights");
                Console.WriteLine("Q. Go Back");
                if(CurUser.IsManager)
                {
                    Console.WriteLine("M. Choose Planes");
                }
                if(CurUser.IsEngineer)
                {
                    Console.WriteLine("E. Manage Flights");
                }
                string? input = Console.ReadLine();
                if (input == null | (input != "1" & input != "2" & input != "3" & input != "4" & input != "Q" & input != "M" & input != "E"))
                {
                    Console.WriteLine("Please input a correct input");
                    continue;
                }
                switch (input)
                {
                    case "1":
                        Login.LoginMethod();
                        break;
                    case "Q":
                        return;
                    case "2":
                        Login.ChangeAccountSettings(CurUser.UserID);
                        break;
                    case "3":
                        Login.DisplayFlightHistory(CurUser.UserID);
                        break;
                    case "4":
                        //Flights.CancelFlights();
                        break;
                    case "M":
                        if(CurUser.IsManager) 
                        {
                            // Flights.ChoosePlanes();
                        }
                        else
                        {
                            Console.WriteLine("Must be a Manager for this!");
                        }
                        break;
                    case "E":
                        if( CurUser.IsEngineer)
                        {
                            Flights flight = new Flights(CurUser);
                            flight.ManageFlights();
                        }else
                        {
                            Console.WriteLine("Must be an Engineer for this!");
                        }
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
        public string? Address { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Phone { get; set; }
        public DateTime? Birthday { get; set; }
        public int? PointsAvailable { get; set; }
        public int? PointsUsed { get; set; }
        public string? CreditCard { get; set; }

    }
}
