using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Air3550
{
    public class LoggedIn
    {
        private User CurUser;
        public LoggedIn(string UserID)
        {
            CurUser = new User();
            CurUser.UserID = UserID;
            using (SqlConnection sqlConn = new SqlConnection("Server=34.162.94.248; Database=air3550; Uid=sqlserver; Password=123;"))
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

        public void UpdateCurUser()
        {
            // update CurUser
            using (SqlConnection sqlConn = new SqlConnection("Server=34.162.94.248; Database=air3550; Uid=sqlserver; Password=123;"))
            {
                sqlConn.Open();
                string queryString = $"SELECT IsManager, IsEngineer, FirstName, LastName, Phone, Birthday, PointsAvailable, PointsUsed, CreditCard, Address FROM Users WHERE Users.UserID = {CurUser.UserID}";
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
                        if (!reader.IsDBNull(8))
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
        // gets the accounting information for the accountants
        // need how many flights
        // Percentage of capacity for each flight
        // income per flight
        // income for the company
        public void AccountingInfo()
        {
            // Assuming monthly
            // get the month they want
            DateTime accountDate;
            do
            {
                Console.WriteLine("Please input a day of the month: mm/dd/yyyy");
                string? accountDateString = Console.ReadLine();
                if (!DateTime.TryParse(accountDateString, out accountDate))
                {
                    Console.WriteLine("Invalid date format. Please enter a valid date in mm/dd/yyyy format.");
                    continue;
                }
                break;
            } while (true);
            SqlDateTime sqlAccountingDate = new SqlDateTime(accountDate.Year, accountDate.Month, accountDate.Day);
            // given the date, find all the flights from that month and give the information
            using (SqlConnection sqlConn = new SqlConnection("Server=34.162.94.248; Database=air3550; Uid=sqlserver; Password=123;"))
            {
                Console.WriteLine("\n\n");
                Console.WriteLine("FlightID, FlightNumber, Percentage Full, Income Per Flight");
                Console.WriteLine("------------------------------------------------------------------------");
                sqlConn.Open();
                string queryString = $"SELECT Flights.FlightID, Flights.FlightNumber, (CAST((Planes.Seats - Flights.SeatsAvailable)AS FLOAT) / CAST((Planes.Seats) AS FLOAT)) * 100 AS PerFull, ((Planes.Seats - Flights.SeatsAvailable) * Flights.Price) AS IncomePerFlight" +
                    $"  FROM Flights" +
                    $"  JOIN Planes ON Flights.PlaneID = Planes.PlaneID" +
                    $"  WHERE DATEDIFF(MONTH, DepartureDateTime, '{sqlAccountingDate}') = 0";
                SqlCommand query = new SqlCommand(queryString, sqlConn);
                using (SqlDataReader reader = query.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Console.WriteLine($"{reader.GetInt32(0)}, {reader.GetInt32(1)}, {reader.GetDouble(2)}, {reader.GetSqlDecimal(3)}");
                    }
                }
                queryString = $"  SELECT Count(*), SUM(((Planes.Seats - Flights.SeatsAvailable) * Flights.Price)) " +
                    $"FROM Flights JOIN Planes On Flights.PlaneID = Planes.PlaneID " +
                    $"WHERE DATEDIFF(MONTH, DepartureDateTime, '{sqlAccountingDate}') = 0";
                query = new SqlCommand(queryString, sqlConn);
                using (SqlDataReader reader = query.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Console.WriteLine("\nSummary: Total Flights, Total Income");
                        Console.WriteLine($"{reader.GetInt32(0)}, {reader.GetDecimal(1)}");
                    }
                }
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
                Console.WriteLine("5. View Upcoming Flights");
                Console.WriteLine("6. Print Boarding Pass");
                Console.WriteLine("Q. Go Back\n");
                if(CurUser.IsManager)
                {
                    Console.WriteLine("M. Choose Planes");
                    Console.WriteLine("A. Accounting");
                }
                if(CurUser.IsEngineer)
                {
                    Console.WriteLine("E. Manage Flights");
                }
                string? input = Console.ReadLine();
                if (input == null | (input != "1" & input != "2" & input != "3" & input != "4" & input != "5" & input != "6" & input != "Q" & input != "M" & input != "E" & input!= "A"))
                {
                    Console.WriteLine("Please input a correct input");
                    continue;
                }
                Flights flight = new Flights(CurUser);
                UserOptions options = new UserOptions(CurUser);
                switch (input)
                {
                    case "1":
                        options.BookFlight(CurUser.UserID);
                        UpdateCurUser();
                        break;
                    case "Q":
                        return;
                    case "2":
                        Login.ChangeAccountSettings(CurUser.UserID);
                        UpdateCurUser();
                        break;
                    case "3":
                        options.DisplayFlightHistory(CurUser.UserID);
                        break;
                    case "4":
                        options.cancel();
                        UpdateCurUser();
                        break;
                    case "5":
                        options.DisplayUpcomingFlights(CurUser.UserID);
                        break;
                    case "6":
                        options.printBoardingPass(CurUser.UserID);
                        break;
                    case "M":
                        if(CurUser.IsManager) 
                        {                      
                            flight.ManagePlanes();
                        }
                        else
                        {
                            Console.WriteLine("Must be a Manager for this!");
                        }
                        break;
                    case "A":
                        if(CurUser.IsManager)
                        {
                            AccountingInfo();
                        }
                        else
                        {
                            Console.WriteLine("Must be a Manager for this!");
                        }
                        break;
                    case "E":
                        if( CurUser.IsEngineer)
                        {
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
