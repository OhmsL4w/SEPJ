using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Air3550
{
    public class Flights
    {
        private User CurUser;
        public Flights(User user)
        {
            CurUser = user;
        }

        public void FlightManifest(DateTime date, int flightNumber)
        {
            int flightID = 0;
            // find the flight from the information given
            using (SqlConnection sqlConn = new SqlConnection("Server=34.162.94.248; Database=air3550; Uid=sqlserver; Password=123;"))
            {
                // first get the flight ID for that flight
                sqlConn.Open();
                string queryString = $"SELECT FlightID FROM Flights WHERE  flightNumber = {flightNumber} AND DepartureDateTime = \'{date}\'";
                SqlCommand query = new SqlCommand(queryString, sqlConn);
                using (SqlDataReader reader = query.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        flightID = reader.GetInt32(0);
                    }
                }
                string queryStringManifests = $"SELECT Users.FirstName + ' ' + Users.Lastname as Name FROM Transactions JOIN Users ON Transactions.UserID = Users.UserID WHERE FlightId = {flightID}";
                SqlCommand queryManifest = new SqlCommand(queryStringManifests, sqlConn);
                List<string> names = new List<string>();
                using (SqlDataReader reader = queryManifest.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        names.Add(reader.GetString(0));
                    }
                }
                sqlConn.Close();
            }
        }
        public void ManageFlights()
        {
            using (SqlConnection sqlConn = new SqlConnection("Server=34.162.94.248; Database=air3550; Uid=sqlserver; Password=123;"))
            {
                // Get all of the flights from today and later
                sqlConn.Open();
                string queryString = $"SELECT FlightID, FlightNumber, OriginCity, DestinationCity, Price, DepartureDateTime, ArrivalDateTime FROM Flights WHERE DepartureDateTime >= GETDATE()";
                SqlCommand query = new SqlCommand(queryString, sqlConn);
                using (SqlDataReader reader = query.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Console.WriteLine($"FlightID:{reader.GetInt32(0)}  FlightNumber: {reader.GetInt32(1)}  Origin Airport: {reader.GetString(2)}  Destination Aiport: {reader.GetString(3)}" +
                            $" \nPrice: {reader.GetDecimal(4)} \t Departure Date: {reader.GetDateTime(5).ToString("d")}  Departure Time: {reader.GetDateTime(5).ToString("T")} " +
                            $"\n\t\t   Arrival Date: {reader.GetDateTime(6).ToString("d")}    Arrival Time: {reader.GetDateTime(6).ToString("T")}");
                    }
                }
                while (true)
                {
                    Console.WriteLine("Input an option to continue");
                    Console.WriteLine("1. Update Flight");
                    Console.WriteLine("2. Add Flight");
                    Console.WriteLine("3. Delete Flight");
                 // Console.WriteLine("4. Display All Flight");
                    Console.WriteLine("Q. Go Back");
                    string? input = Console.ReadLine();
                    if (input == null | (input != "1" & input != "2" & input != "3"  &input != "Q"))
                    {
                        Console.WriteLine("Please input a correct input");
                        continue;
                    }
                    switch (input)
                    {
                        case "1":
                            UpdateFlight();
                            break;
                        case "2":
                            AddFlight();
                            break;
                        case "3":
                            DeleteFlight();                         
                            break;
                       /* case "4":
                            DisplayAllFlights();
                            break;*/
                        case "Q":
                            sqlConn.Close();
                            return;
                    }
                }
            }
        }
/*        public void DisplayAllFlights()
        {
            using (SqlConnection sqlConn = new SqlConnection("Server=34.162.94.248; Database=air3550; Uid=sqlserver; Password=123;"))
            {
                sqlConn.Open();
                string queryString = "SELECT * FROM Flights";
                SqlCommand query = new SqlCommand(queryString, sqlConn);
                SqlDataReader reader = query.ExecuteReader();

                Console.WriteLine("Flights:");

                while (reader.Read())
                {
                    int flightNumber = reader.GetInt32(1);
                    string originCity = reader.GetString(2);
                    string destinationCity = reader.GetString(3);
                    decimal price = reader.GetDecimal(4);
                    DateTime departureDateTime = reader.GetDateTime(5);
                    DateTime arrivalDateTime = reader.GetDateTime(6);

                    Console.WriteLine($"Flight Number: {flightNumber}, Origin City: {originCity}, Destination City: {destinationCity}, Price: {price}, Departure Time: {departureDateTime}, Arrival Time: {arrivalDateTime}");
                }
                sqlConn.Close();
            }
        }*/

        public void UpdateFlight()
        {
            bool flightID = false;
            while (true)
            {
                Console.WriteLine("Would you like to update a single flight, or all flights with a flight number?");
                Console.WriteLine("1. Single Flight (Flight ID)");
                Console.WriteLine("2. All Flights (Flight Number)");
                Console.WriteLine("Q. Quit");
                string? input = Console.ReadLine();
                if (input == null | (input != "1" & input != "2" & input != "Q"))
                {
                    Console.WriteLine("Please input a correct input");
                    continue;
                }
                if (input == "1") flightID = true;
                if (input == "Q") return;
                break;
            }
            if (flightID)
            {
                Console.WriteLine("Input a Flight ID");
                string? flight = Console.ReadLine();
                while (flight == null)
                {
                    Console.WriteLine("Please input a Flight ID");
                    flight = Console.ReadLine();
                }
                using (SqlConnection sqlConn = new SqlConnection("Server=34.162.94.248; Database=air3550; Uid=sqlserver; Password=123;"))
                {
                    sqlConn.Open();
                    string queryString = $"SELECT FlightID, FlightNumber, OriginCity, DestinationCity, Price, DepartureDateTime, ArrivalDateTime FROM Flights WHERE FlightID = {flight}";
                    SqlCommand query = new SqlCommand(queryString, sqlConn);
                    using (SqlDataReader reader = query.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Console.WriteLine($"FlightID:{reader.GetInt32(0)}  FlightNumber: {reader.GetInt32(1)}  Origin Airport: {reader.GetString(2)}  Destination Aiport: {reader.GetString(3)}  Price: {reader.GetDecimal(4)}  Departure Date: {reader.GetDateTime(5).ToString("d")}  Departure Time: {reader.GetDateTime(6).ToString("T")}  Arrival Date: {reader.GetDateTime(5).ToString("d")}  Arrival Time: {reader.GetDateTime(6).ToString("T")}");
                        }
                    }
                    string? input;
                    while (true)
                    {
                        Console.WriteLine("What would you like to change?");
                        Console.WriteLine("1. Flight Number");
                        Console.WriteLine("2. Origin Airport");
                        Console.WriteLine("3. Destination Airport");
                        Console.WriteLine("4. Price");
                        Console.WriteLine("5. Departure Date Time");
                        Console.WriteLine("6. Arrival Date Time");
                        Console.WriteLine("Q. Quit");
                        input = Console.ReadLine();
                        if (input == null | (input != "1" & input != "2" & input != "3" & input != "4" & input != "5" & input != "6" & input != "Q"))
                        {
                            Console.WriteLine("Please input a correct input");
                            continue;
                        }
                        switch (input)
                        {
                            case "1":
                                Console.WriteLine("Input a new Flight Number");
                                string? nfns = Console.ReadLine();
                                int nfn;
                                while (true)
                                {
                                    if (!int.TryParse(nfns, out nfn))
                                    {
                                        Console.WriteLine("Could not parse the integer!");
                                        Console.WriteLine("Input a correct flight number please");
                                    }
                                    else
                                    {
                                        break;
                                    }
                                    Console.WriteLine("Input a new Flight Number");
                                    nfns = Console.ReadLine();
                                }
                                string qs = $"UPDATE Flights SET FlightNumber = {nfn} WHERE FlightID = {flight}";
                                SqlCommand q = new SqlCommand(qs, sqlConn);
                                int rows = q.ExecuteNonQuery();
                                if (rows > 0)
                                {
                                    Console.WriteLine($"Successfully updated flight: {flight}");
                                }
                                break;
                            case "2":
                                Console.WriteLine("Input a new Origin Airport");
                                string? noa = Console.ReadLine();
                                while (noa == null | noa?.Length != 3)
                                {
                                    Console.WriteLine("Input a correct airport code please");

                                    Console.WriteLine("Input a new Airport Code");
                                    noa = Console.ReadLine();
                                }
                                qs = $"UPDATE Flights SET OriginCity = \'{noa}\' WHERE FlightID = {flight}";
                                q = new SqlCommand(qs, sqlConn);
                                rows = q.ExecuteNonQuery();
                                if (rows > 0)
                                {
                                    Console.WriteLine($"Successfully updated flight: {flight}");
                                }
                                break;
                            case "3":
                                Console.WriteLine("Input a new Destination Airport");
                                string? nda = Console.ReadLine();
                                while (nda == null | nda?.Length != 3)
                                {
                                    Console.WriteLine("Input a correct airport code please");

                                    Console.WriteLine("Input a new Airport Code");
                                    nda = Console.ReadLine();
                                }
                                qs = $"UPDATE Flights SET DestinationCity = \'{nda}\' WHERE FlightID = {flight}";
                                q = new SqlCommand(qs, sqlConn);
                                rows = q.ExecuteNonQuery();
                                if (rows > 0)
                                {
                                    Console.WriteLine($"Successfully updated flight: {flight}");
                                }
                                break;
                            case "4":
                                Console.WriteLine("Input a new Price (xx.xx)");
                                string? nps = Console.ReadLine();
                                decimal np;
                                while (true)
                                {
                                    if (!decimal.TryParse(nps, out np))
                                    {
                                        Console.WriteLine("Could not parse the decimal!");
                                        Console.WriteLine("Input a correct Price please");
                                    }
                                    else
                                    {
                                        break;
                                    }
                                    Console.WriteLine("Input a new Price (xx.xx)");
                                    nps = Console.ReadLine();
                                }
                                qs = $"UPDATE Flights SET Price = {np} WHERE FlightID = {flight}";
                                q = new SqlCommand(qs, sqlConn);
                                rows = q.ExecuteNonQuery();
                                if (rows > 0)
                                {
                                    Console.WriteLine($"Successfully updated flight: {flight}");
                                }
                                break;
                            case "5":
                                Console.WriteLine("Input a new Departure Date (mm/dd/yyyy HH:MM:SS)");
                                string? ndds = Console.ReadLine();
                                DateTime ndd;
                                while (true)
                                {
                                    if (!DateTime.TryParse(ndds, out ndd))
                                    {
                                        Console.WriteLine("Could not parse the date and time!");
                                        Console.WriteLine("Input a correct Date and time in the format given no paranthesis please");
                                    }
                                    else
                                    {
                                        break;
                                    }
                                    Console.WriteLine("Input a new Departure Date (mm/dd/yyyy HH:MM:SS)");
                                    nps = Console.ReadLine();
                                }
                                SqlDateTime sqlndd = new SqlDateTime(ndd.Year, ndd.Month, ndd.Day, ndd.Hour, ndd.Minute, ndd.Second);
                                qs = $"UPDATE Flights SET DepartureDateTime = \'{sqlndd}\' WHERE FlightID = {flight}";
                                q = new SqlCommand(qs, sqlConn);
                                rows = q.ExecuteNonQuery();
                                if (rows > 0)
                                {
                                    Console.WriteLine($"Successfully updated flight: {flight}");
                                }
                                break;
                            case "6":
                                Console.WriteLine("Input a new Departure Date (mm/dd/yyyy HH:MM:SS)");
                                string? nads = Console.ReadLine();
                                DateTime nad;
                                while (true)
                                {
                                    if (!DateTime.TryParse(nads, out nad))
                                    {
                                        Console.WriteLine("Could not parse the date and time!");
                                        Console.WriteLine("Input a correct Date and time in the format given no paranthesis please");
                                    }
                                    else
                                    {
                                        break;
                                    }
                                    Console.WriteLine("Input a new Departure Date (mm/dd/yyyy HH:MM:SS)");
                                    nps = Console.ReadLine();
                                }
                                SqlDateTime sqlnad = new SqlDateTime(nad.Year, nad.Month, nad.Day, nad.Hour, nad.Minute, nad.Second);
                                qs = $"UPDATE Flights SET DepartureDateTime = \'{sqlnad}\' WHERE FlightID = {flight}";
                                q = new SqlCommand(qs, sqlConn);
                                rows = q.ExecuteNonQuery();
                                if (rows > 0)
                                {
                                    Console.WriteLine($"Successfully updated flight: {flight}");
                                }
                                break;
                            case "Q":
                                sqlConn.Close();
                                return;
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("Input a Flight Number");
                string? flight = Console.ReadLine();
                while (flight == null)
                {
                    Console.WriteLine("Please input a Flight Number");
                    flight = Console.ReadLine();
                }
                using (SqlConnection sqlConn = new SqlConnection("Server=34.162.94.248; Database=air3550; Uid=sqlserver; Password=123;"))
                {
                    sqlConn.Open();
                    string queryString = $"SELECT FlightID, FlightNumber, OriginCity, DestinationCity, Price, DepartureDateTime, ArrivalDateTime FROM Flights WHERE FlightID = {flight}";
                    SqlCommand query = new SqlCommand(queryString, sqlConn);
                    using (SqlDataReader reader = query.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Console.WriteLine($"FlightID:{reader.GetInt32(0)}  FlightNumber: {reader.GetInt32(1)}  Origin Airport: {reader.GetString(2)}  Destination Aiport: {reader.GetString(3)}  Price: {reader.GetDecimal(4)}  Departure Date: {reader.GetDateTime(5).ToString("d")}  Departure Time: {reader.GetDateTime(6).ToString("T")}  Arrival Date: {reader.GetDateTime(5).ToString("d")}  Arrival Time: {reader.GetDateTime(6).ToString("T")}");
                        }
                    }
                    string? input;
                    while (true)
                    {
                        Console.WriteLine("What would you like to change?");
                        Console.WriteLine("1. Flight Number");
                        Console.WriteLine("2. Origin Airport");
                        Console.WriteLine("3. Destination Airport");
                        Console.WriteLine("4. Price");
                        Console.WriteLine("5. Departure Date Time");
                        Console.WriteLine("6. Arrival Date Time");
                        Console.WriteLine("Q. Quit");
                        input = Console.ReadLine();
                        if (input == null | (input != "1" & input != "2" & input != "3" & input != "4" & input != "5" & input != "6" & input != "Q"))
                        {
                            Console.WriteLine("Please input a correct input");
                            continue;
                        }
                        switch (input)
                        {
                            case "1":
                                Console.WriteLine("Input a new Flight Number");
                                string? nfns = Console.ReadLine();
                                int nfn;
                                while (true)
                                {
                                    if (!int.TryParse(nfns, out nfn))
                                    {
                                        Console.WriteLine("Could not parse the integer!");
                                        Console.WriteLine("Input a correct flight number please");
                                    }
                                    else
                                    {
                                        break;
                                    }
                                    Console.WriteLine("Input a new Flight Number");
                                    nfns = Console.ReadLine();
                                }
                                string qs = $"UPDATE Flights SET FlightNumber = {nfn} WHERE FlightNumber = {flight}";
                                SqlCommand q = new SqlCommand(qs, sqlConn);
                                int rows = q.ExecuteNonQuery();
                                if (rows > 0)
                                {
                                    Console.WriteLine($"Successfully updated flight: {flight}");
                                }
                                break;
                            case "2":
                                Console.WriteLine("Input a new Origin Airport");
                                string? noa = Console.ReadLine();
                                while (noa == null | noa?.Length != 3)
                                {
                                    Console.WriteLine("Input a correct airport code please");

                                    Console.WriteLine("Input a new Airport Code");
                                    noa = Console.ReadLine();
                                }
                                qs = $"UPDATE Flights SET OriginCity = \'{noa}\' WHERE FlightNumber = {flight}";
                                q = new SqlCommand(qs, sqlConn);
                                rows = q.ExecuteNonQuery();
                                if (rows > 0)
                                {
                                    Console.WriteLine($"Successfully updated flight: {flight}");
                                }
                                break;
                            case "3":
                                Console.WriteLine("Input a new Destination Airport");
                                string? nda = Console.ReadLine();
                                while (nda == null | nda?.Length != 3)
                                {
                                    Console.WriteLine("Input a correct airport code please");

                                    Console.WriteLine("Input a new Airport Code");
                                    nda = Console.ReadLine();
                                }
                                qs = $"UPDATE Flights SET DestinationCity = \'{nda}\' WHERE FlightNumber = {flight}";
                                q = new SqlCommand(qs, sqlConn);
                                rows = q.ExecuteNonQuery();
                                if (rows > 0)
                                {
                                    Console.WriteLine($"Successfully updated flight: {flight}");
                                }
                                break;
                            case "4":
                                Console.WriteLine("Input a new Price (xx.xx)");
                                string? nps = Console.ReadLine();
                                decimal np;
                                while (true)
                                {
                                    if (!decimal.TryParse(nps, out np))
                                    {
                                        Console.WriteLine("Could not parse the decimal!");
                                        Console.WriteLine("Input a correct Price please");
                                    }
                                    else
                                    {
                                        break;
                                    }
                                    Console.WriteLine("Input a new Price (xx.xx)");
                                    nps = Console.ReadLine();
                                }
                                qs = $"UPDATE Flights SET Price = {np} WHERE FlightNumber = {flight}";
                                q = new SqlCommand(qs, sqlConn);
                                rows = q.ExecuteNonQuery();
                                if (rows > 0)
                                {
                                    Console.WriteLine($"Successfully updated flight: {flight}");
                                }
                                break;
                            case "5":
                                Console.WriteLine("Input a new Departure Date (mm/dd/yyyy HH:MM:SS)");
                                string? ndds = Console.ReadLine();
                                DateTime ndd;
                                while (true)
                                {
                                    if (!DateTime.TryParse(ndds, out ndd))
                                    {
                                        Console.WriteLine("Could not parse the date and time!");
                                        Console.WriteLine("Input a correct Date and time in the format given no paranthesis please");
                                    }
                                    else
                                    {
                                        break;
                                    }
                                    Console.WriteLine("Input a new Departure Date (mm/dd/yyyy HH:MM:SS)");
                                    nps = Console.ReadLine();
                                }
                                SqlDateTime sqlndd = new SqlDateTime(ndd.Year, ndd.Month, ndd.Day, ndd.Hour, ndd.Minute, ndd.Second);
                                qs = $"UPDATE Flights SET DepartureDateTime = \'{sqlndd}\' WHERE FlightNumber = {flight}";
                                q = new SqlCommand(qs, sqlConn);
                                rows = q.ExecuteNonQuery();
                                if (rows > 0)
                                {
                                    Console.WriteLine($"Successfully updated flight: {flight}");
                                }
                                break;
                            case "6":
                                Console.WriteLine("Input a new Departure Date (mm/dd/yyyy HH:MM:SS)");
                                string? nads = Console.ReadLine();
                                DateTime nad;
                                while (true)
                                {
                                    if (!DateTime.TryParse(nads, out nad))
                                    {
                                        Console.WriteLine("Could not parse the date and time!");
                                        Console.WriteLine("Input a correct Date and time in the format given no paranthesis please");
                                    }
                                    else
                                    {
                                        break;
                                    }
                                    Console.WriteLine("Input a new Departure Date (mm/dd/yyyy HH:MM:SS)");
                                    nps = Console.ReadLine();
                                }
                                SqlDateTime sqlnad = new SqlDateTime(nad.Year, nad.Month, nad.Day, nad.Hour, nad.Minute, nad.Second);
                                qs = $"UPDATE Flights SET DepartureDateTime = \'{sqlnad}\' WHERE FlightNumber = {flight}";
                                q = new SqlCommand(qs, sqlConn);
                                rows = q.ExecuteNonQuery();
                                if (rows > 0)
                                {
                                    Console.WriteLine($"Successfully updated flight: {flight}");
                                }
                                break;
                            case "Q":
                                sqlConn.Close();
                                return;
                        }
                    }
                }
            }
        }
        public void AddFlight()
        {
            Console.WriteLine("Please enter all of the information");
            Console.WriteLine("Input a new Flight Number");
            string? nfns = Console.ReadLine();
            int nfn;
            while (true)
            {
                if (!int.TryParse(nfns, out nfn))
                {
                    Console.WriteLine("Could not parse the integer!");
                    Console.WriteLine("Input a correct flight number please");
                }
                else
                {
                    break;
                }
                Console.WriteLine("Input a new Flight Number");
                nfns = Console.ReadLine();
            }

            Console.WriteLine("Input a new Origin Airport");
            string? noa = Console.ReadLine();
            while (noa == null | noa?.Length != 3)
            {
                Console.WriteLine("Input a correct airport code please");

                Console.WriteLine("Input a new Airport Code");
                noa = Console.ReadLine();
            }

            Console.WriteLine("Input a new Destination Airport");
            string? nda = Console.ReadLine();
            while (nda == null | nda?.Length != 3)
            {
                Console.WriteLine("Input a correct airport code please");

                Console.WriteLine("Input a new Airport Code");
                nda = Console.ReadLine();
            }

            Console.WriteLine("Input a new Price (xx.xx)");
            string? nps = Console.ReadLine();
            decimal np;
            while (true)
            {
                if (!decimal.TryParse(nps, out np))
                {
                    Console.WriteLine("Could not parse the decimal!");
                    Console.WriteLine("Input a correct Price please");
                }
                else
                {
                    break;
                }
                Console.WriteLine("Input a new Price (xx.xx)");
                nps = Console.ReadLine();
            }

            Console.WriteLine("Input a new Departure Date (mm/dd/yyyy HH:MM:SS)");
            string? ndds = Console.ReadLine();
            DateTime ndd;
            while (true)
            {
                if (!DateTime.TryParse(ndds, out ndd))
                {
                    Console.WriteLine("Could not parse the date and time!");
                    Console.WriteLine("Input a correct Date and time in the format given no paranthesis please");
                }
                else
                {
                    break;
                }
                Console.WriteLine("Input a new Departure Date (mm/dd/yyyy HH:MM:SS)");
                ndds = Console.ReadLine();
            }
            SqlDateTime sqlndd = new SqlDateTime(ndd.Year, ndd.Month, ndd.Day, ndd.Hour, ndd.Minute, ndd.Second);

            Console.WriteLine("Input a new Arrival Date and Time (mm/dd/yyyy HH:MM:SS)");
            string? nads = Console.ReadLine();
            DateTime nad;
            while (true)
            {
                if (!DateTime.TryParse(nads, out nad))
                {
                    Console.WriteLine("Could not parse the date and time!");
                    Console.WriteLine("Input a correct Date and time in the format given no paranthesis please");
                }
                else
                {
                    break;
                }
                Console.WriteLine("Input a new Departure Date (mm/dd/yyyy HH:MM:SS)");
                nads = Console.ReadLine();
            }

            SqlDateTime sqlnad = new SqlDateTime(nad.Year, nad.Month, nad.Day, nad.Hour, nad.Minute, nad.Second);
            using (SqlConnection sqlConn = new SqlConnection("Server=34.162.94.248; Database=air3550; Uid=sqlserver; Password=123;"))
                { 
                        sqlConn.Open();
                DateTime endDate = DateTime.Now.AddMonths(7);
                DateTime departureDate = ndd;
                while (departureDate <= endDate)
                {
                    departureDate = departureDate.AddDays(7);

                    SqlDateTime sqlDepartureDate = new SqlDateTime(departureDate.Year, departureDate.Month, departureDate.Day, departureDate.Hour, departureDate.Minute, departureDate.Second);
                    SqlDateTime sqlArrivalDate = new SqlDateTime(departureDate.Year, departureDate.Month, departureDate.Day, nad.Hour, nad.Minute, nad.Second);

                    string queryString = $"INSERT INTO Flights (FlightNumber, OriginCity, DestinationCity, Price, DepartureDateTime, ArrivalDateTime)" +
                    $"VALUES ({nfn}, \'{noa}\', \'{nda}\', {np}, \'{sqlDepartureDate}\', \'{sqlArrivalDate}\')";                             
                        SqlCommand query = new SqlCommand(queryString, sqlConn);
                        int rows = query.ExecuteNonQuery();                      
                    }  
                sqlConn.Close();
                }
                Console.WriteLine("Flights added for the next 6 months!");                  
        }

        public void DeleteFlight()
        {
            bool flightID = false;
            while (true)
            {
                Console.WriteLine("Would you like to delete a single flight, or all flights with a flight number?");
                Console.WriteLine("1. Single Flight (Flight ID)");
                Console.WriteLine("2. All Flights (Flight Number)");
                Console.WriteLine("Q. Quit");
                string? input = Console.ReadLine();
                if (input == null | (input != "1" & input != "2" & input != "Q"))
                {
                    Console.WriteLine("Please input a correct input");
                    continue;
                }
                if (input == "1") flightID = true;
                if (input == "Q") return;
                break;
            }
            if (flightID)
            {
                Console.WriteLine("Input a Flight ID");
                string? flight = Console.ReadLine();
                while (flight == null)
                {
                    Console.WriteLine("Please input a Flight ID");
                    flight = Console.ReadLine();
                }
                using (SqlConnection sqlConn = new SqlConnection("Server=34.162.94.248; Database=air3550; Uid=sqlserver; Password=123;"))
                {
                    // first get the flight ID for that flight
                    sqlConn.Open();
                    string queryString = $"DELETE Flights WHERE FlightID = {flight}";
                    SqlCommand query = new SqlCommand(queryString, sqlConn);
                    int rows = query.ExecuteNonQuery();
                    if (rows > 0)
                    {
                        Console.WriteLine("Successfully deleted the flight!");
                    }
                    sqlConn.Close();
                }
            }
            else
            {
                Console.WriteLine("Input a Flight Number");
                string? flight = Console.ReadLine();
                while (flight == null)
                {
                    Console.WriteLine("Please input a Flight Number");
                    flight = Console.ReadLine();
                }
                using (SqlConnection sqlConn = new SqlConnection("Server=34.162.94.248; Database=air3550; Uid=sqlserver; Password=123;"))
                {
                    // first get the flight ID for that flight
                    sqlConn.Open();
                    string queryString = $"DELETE Flights WHERE FlightNumber = {flight}";
                    SqlCommand query = new SqlCommand(queryString, sqlConn);
                    int rows = query.ExecuteNonQuery();
                    if (rows > 0)
                    {
                        Console.WriteLine($"Successfully deleted the flight! Deleted: {rows}");
                    }
                    sqlConn.Close();
                }
            }
        }
        public void BookFlight(string username)
        {
            string? OriginAirport, DestinationAirport, DDate, ADate, checkFlightDets, paymentMethod, checkPaymentDets,CCardUpdate, CCard = null,newCCard;
            
            int rows = 0, isCard = 0 , addPoints = 0;
            do
            {
                Console.WriteLine("Please enter an Origin Airport");
                OriginAirport = Console.ReadLine();
            } while (OriginAirport == null);
            do
            {
                Console.WriteLine("Please enter a Destination Airport");
                DestinationAirport = Console.ReadLine();
            } while (DestinationAirport == null);
            DateTime DepartDate;

            do
            {
                Console.WriteLine("Please enter a Departure Date mm/dd/yyyy");
                DDate = Console.ReadLine();

                if (!DateTime.TryParse(DDate, out  DepartDate))
                {
                    Console.WriteLine("Invalid date format. Please enter a valid date in mm/dd/yyyy format.");
                    continue;
                }

                if (DepartDate < DateTime.Today)
                {
                    Console.WriteLine("Departure date cannot be in the past. Please enter a future date.");
                    continue;
                }
                break;

            } while (true);

            DateTime ArriveDate;
            do
            {
                Console.WriteLine("Please enter an Arrival Date mm/dd/yyyy");
                ADate = Console.ReadLine();

                if (!DateTime.TryParse(ADate, out  ArriveDate))
                {
                    Console.WriteLine("Invalid date format. Please enter a valid date in mm/dd/yyyy format.");
                    continue;
                }

                if (ArriveDate < DateTime.Today)
                {
                    Console.WriteLine("Arrival date cannot be in the past. Please enter a future date.");
                    continue;
                }
                break;

            } while (true);
            using (SqlConnection sqlConn = new SqlConnection("Server=34.162.94.248; Database=air3550; Uid=sqlserver; Password=123;"))
            {
                string OrgCity = null;
                string DesCity = null;
                int flightID = 0 ;
                //string tempPrice;
                DateTime DepartDets = default(DateTime);
                DateTime AriveDets = default(DateTime);
                decimal price = 0;
                //checking if we have a flight that the user is requesting 
                // checkFlightDets = $"SELECT * FROM Flights WHERE OriginCity = @OriginCity AND DestinationCity = @DestinationCity ";//AND DepartureDateTime = @DepartureDate AND ArrivalDateTime = @ArrivalDate";
                checkFlightDets = $"SELECT * FROM Flights WHERE OriginCity = @OriginCity AND DestinationCity = @DestinationCity AND CONVERT(date, DepartureDateTime) = CONVERT(date, @DepartureDate) AND CONVERT(date, ArrivalDateTime) = CONVERT(date, @ArrivalDate)";
                sqlConn.Open();
                using (SqlCommand FlightDets = new SqlCommand(checkFlightDets, sqlConn))
                {
                    FlightDets.Parameters.AddWithValue("@OriginCity", OriginAirport);
                    FlightDets.Parameters.AddWithValue("@DestinationCity", DestinationAirport);
                    FlightDets.Parameters.AddWithValue("@DepartureDate", DepartDate);
                    FlightDets.Parameters.AddWithValue("@ArrivalDate", ArriveDate);
                    
                    SqlDataReader reader = FlightDets.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            // Retrieve the flight information from the reader
                            
                            OrgCity = (string)reader["OriginCity"];
                            DesCity = (string)reader["DestinationCity"];
                            DepartDets = reader.GetDateTime(reader.GetOrdinal("DepartureDateTime"));
                            AriveDets = reader.GetDateTime(reader.GetOrdinal("ArrivalDateTime"));
                            flightID = (int)reader["FlightID"];
                            price = (decimal)reader["Price"];
                            //Double.TryParse(price, out price);
                            Console.WriteLine($"Flight found from {OrgCity} at {DepartDets.ToString()} to {DesCity} at {AriveDets.ToString()} at Price: ${price}");
                        }
                        reader.Close();
                        addPoints = (int)price * 100;
                        do
                        {
                            Console.WriteLine("Please select a Payment Method\n1. Credit Card\n2. Points");
                            paymentMethod = Console.ReadLine();
                        } while (paymentMethod == null | (paymentMethod != "1" & paymentMethod != "2"));

                        checkPaymentDets = $"SELECT * FROM Users WHERE UserID = @UserId"; // Need this to check if the user had a credit card on file or not 
                        using (SqlCommand PointsPayment = new SqlCommand(checkPaymentDets, sqlConn))
                        {
                            PointsPayment.Parameters.AddWithValue("@UserID", username);

                            using (SqlDataReader paymenreader = PointsPayment.ExecuteReader())
                            {
                                paymenreader.Read();
                                if (paymenreader.HasRows)
                                {
                                    bool notEnoughPoints = false;
                                    if (paymentMethod == "2")
                                    {

                                        int points = (int)paymenreader["PointsAvailable"];                                       
                                        if (points >= addPoints)
                                        {
                                            paymenreader.Close();
                                           Console.WriteLine("You have Enough points");

                                           string? usingPoints = $"UPDATE Users SET PointsAvailable = PointsAvailable - @pointsToReduce,PointsUsed = PointsUsed + @pointsToAdd WHERE UserID = @UserId";
                                            using (SqlCommand updatePoints = new SqlCommand(usingPoints, sqlConn))
                                            {
                                                updatePoints.Parameters.AddWithValue("@pointsToReduce", addPoints);
                                                updatePoints.Parameters.AddWithValue("@pointsToAdd", addPoints);
                                                updatePoints.Parameters.AddWithValue("@UserId", username);
                                                rows = updatePoints.ExecuteNonQuery();
                                                if (rows > 0)
                                                {
                                                    Console.WriteLine("Successfully updated points");
                                                }
                                                else
                                                {
                                                    Console.WriteLine("Unsuccessful point update.");
                                                }
                                            }
                                        }
                                        else
                                        {
                                            notEnoughPoints = true;
                                            Console.WriteLine("Insufficient points please use a credit card");
                                        }

                                    }
                                    if (notEnoughPoints == true)
                                    {

                                        CCard = paymenreader.IsDBNull(paymenreader.GetOrdinal("CreditCard")) ? null : (string)paymenreader["CreditCard"];
                                        string CCsave = null;
                                        if (CCard != null)
                                        {

                                            Console.WriteLine("Would you like to use the Card you have saved for your account?\n1.Y\n2.N");
                                            CCsave = Console.ReadLine();
                                            do
                                            {
                                                if (CCsave == "2")
                                                {
                                                    //ask user for new card and see if they want to save it to their account
                                                    do
                                                    {
                                                        Console.WriteLine("Enter new Credit Card");
                                                        newCCard = Console.ReadLine();
                                                    } while (newCCard == null || newCCard.Length != 16);
                                                }
                                                else break;
                                            } while (CCsave == null || CCsave != "1" || CCsave != "2");
                                        }
                                        else
                                        {
                                            //ask user for new credit card number
                                            Console.WriteLine("You do not have a credit card saved in your account");
                                            do
                                            {
                                                Console.WriteLine("Enter new Credit Card");
                                                newCCard = Console.ReadLine();
                                            } while (newCCard == null || newCCard.Length != 16);
                                            Console.WriteLine("Would you like to save this Card to your account?\n1.Y\n2.N");
                                            CCsave = Console.ReadLine();
                                            if (CCsave == "1")
                                            {
                                                paymenreader.Close();
                                                CCardUpdate = $"UPDATE Users SET CreditCard = @CreditCard WHERE UserID = @UserId";
                                                using (SqlCommand queryUpdateCC = new SqlCommand(CCardUpdate, sqlConn))
                                                {
                                                    queryUpdateCC.Parameters.AddWithValue("@CreditCard", newCCard);
                                                    queryUpdateCC.Parameters.AddWithValue("@UserId", username);
                                                    rows = queryUpdateCC.ExecuteNonQuery();
                                                    if (rows > 0)
                                                    {
                                                        Console.WriteLine("Successfully changed Credit Card");
                                                    }
                                                    else
                                                    {
                                                        Console.WriteLine("Unsuccessful Credit Card change.");
                                                    }
                                                }
                                            }
                                        }
                                        //show reciept
                                        paymenreader.Close();
                                        
                                        string pointUpdate = $"UPDATE Users SET PointsAvailable = PointsAvailable + @Points WHERE UserID = @UserId";
                                        using (SqlCommand queryUpdatePoints = new SqlCommand(pointUpdate, sqlConn))
                                        {
                                            queryUpdatePoints.Parameters.AddWithValue("@Points", addPoints);
                                            queryUpdatePoints.Parameters.AddWithValue("@UserId", username);
                                            rows = queryUpdatePoints.ExecuteNonQuery();
                                            if (rows > 0)
                                            {
                                                Console.WriteLine("Successfully added points");
                                            }
                                            else
                                            {
                                                Console.WriteLine("couldn't add points");
                                            }
                                        }

                                    }
                                    string Transac = $"INSERT INTO Transactions (AmountCharged, IsCard, UserID, FlightID, IsComplete) VALUES ( @AmountCharged, @IsCard, @UserId, @FlightID)";
                                    using (SqlCommand addTransaction = new SqlCommand(Transac, sqlConn))
                                    {
                                        addTransaction.Parameters.AddWithValue("@AmountCharged", price);
                                        if (paymentMethod == "1" || notEnoughPoints == true)
                                        {
                                            isCard = 1;
                                        }
                                        else
                                        {
                                            isCard = 0;
                                        }
                                        addTransaction.Parameters.AddWithValue("@IsCard", isCard);
                                        addTransaction.Parameters.AddWithValue("@UserId", username);
                                        addTransaction.Parameters.AddWithValue("@FlightID", flightID);
                                       
                                        rows = addTransaction.ExecuteNonQuery();
                                        if (rows > 0)
                                        {
                                            Console.WriteLine("Successfully added Transaction");
                                        }
                                        else
                                        {
                                            Console.WriteLine("couldn't add transaction");
                                        }
                                    }
                                    string forReciept = $"SELECT * FROM Users WHERE UserID = @UserID";
                                    using (SqlCommand Reciept = new SqlCommand(forReciept, sqlConn))
                                    {

                                        Reciept.Parameters.AddWithValue("@UserId", username);

                                        using (SqlDataReader recieptmaker = Reciept.ExecuteReader())
                                        {
                                            if (rows > 0)
                                            {
                                                Console.WriteLine("Here is your Reciept");
                                            }
                                            else
                                            {
                                                Console.WriteLine("couldn't find user"); //bad case where system glitches
                                            }
                                            recieptmaker.Read();
                                            int pointsAvial = (int)recieptmaker["PointsAvailable"];
                                            int pointsUsed = (int)recieptmaker["PointsUsed"];
                                            string name = (string)recieptmaker["FirstName"];
                                            name = name + " " +(string)recieptmaker["LastName"];
                                            //need to add logic for if credit card was used or not 
                                            Console.WriteLine($"Flight booked from {OrgCity} at {DepartDets.ToString()} to {DesCity} at {AriveDets.ToString()} at Price: ${price}");
                                            if(paymentMethod == "1" || notEnoughPoints == true)
                                            {
                                                Console.WriteLine($"The Payment Method Used was Credit Card: {CCard}");
                                                Console.WriteLine($"Points rewarded for the booking: {addPoints}, Total points in Account:{pointsAvial}");
                                            }
                                            else
                                            {
                                                Console.WriteLine($"The Payment Method Used was Points. Price of the Flight in points {addPoints}, Total points after purchase in Account {pointsAvial}, Total points used {pointsUsed}");
                                            }
                                            Console.WriteLine($"Thank You for booking with us {name}. ");

                                            recieptmaker.Close();
                                        }   
                                    }

                                }
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("No flights found for the specified Airports or dates.");
                    }
                    
                    
                }
                
                sqlConn.Close();
            }
            return;
        }
    }
}
