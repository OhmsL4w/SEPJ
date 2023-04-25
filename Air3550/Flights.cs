using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using System.Reflection.PortableExecutable;
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

        public void ManagePlanes()
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
                            $" \nPrice: {reader.GetDecimal(4)}  \tDeparture Date: {reader.GetDateTime(5).ToString("d")}  Departure Time: {reader.GetDateTime(5).ToString("T")} " +
                            $"\n\t\t   Arrival Date: {reader.GetDateTime(6).ToString("d")}    Arrival Time: {reader.GetDateTime(6).ToString("T")}\n");
                    }
                }
                while (true)
                {
                    Console.WriteLine("Input an option to continue");
                    Console.WriteLine("1. Add Planes");
                    Console.WriteLine("2. Assign plane");
                    Console.WriteLine("Q. Go Back");
                    string? input = Console.ReadLine();
                    if (input == null | (input != "1" & input != "2" & input != "Q"))
                    {
                        Console.WriteLine("Please input a correct input");
                        continue;
                    }
                    switch (input)
                    {
                        case "1":
                            AddPlanes();
                            break;
                        case "2":
                            ChoosePlanes();
                            break;
                        case "Q":
                            sqlConn.Close();
                            return;
                    }
                }
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
                            $" \nPrice: {reader.GetDecimal(4)}  \tDeparture Date: {reader.GetDateTime(5).ToString("d")}  Departure Time: {reader.GetDateTime(5).ToString("T")} " +
                            $"\n\t\t   Arrival Date: {reader.GetDateTime(6).ToString("d")}    Arrival Time: {reader.GetDateTime(6).ToString("T")}\n");
                    }
                }
                while (true)
                {
                    Console.WriteLine("Input an option to continue");
                    Console.WriteLine("1. Update Flight");
                    Console.WriteLine("2. Add Flight");
                    Console.WriteLine("3. Delete Flight");
                    Console.WriteLine("4. Add Planes");
                    // Console.WriteLine("4. Display All Flight");
                    Console.WriteLine("Q. Go Back");
                    string? input = Console.ReadLine();
                    if (input == null | (input != "1" & input != "2" & input != "3" & input != "4" & input != "Q"))
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
                        case "4":
                            AddPlanes();
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
                                    Console.WriteLine($"Successfully updated flight: {flight}\n");
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
                                    Console.WriteLine($"Successfully updated flight: {flight}\n");
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
                DateTime endDate = DateTime.Now.AddMonths(1);
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
            Console.WriteLine("Flights added for the next 6 months!\n");
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
                    string queryString = $"SELECT TOP 1 IsCard FROM Transactions WHERE FlightID = '{flight}' ORDER BY TransactionID DESC";
                    SqlCommand query = new SqlCommand(queryString, sqlConn);
                    SqlDataReader reader = query.ExecuteReader();
                    bool isCard = false;
                    if (reader.Read())
                    {
                        isCard = reader.GetBoolean(0);
                    }
                    reader.Close();
                    sqlConn.Close();

                    RefundFlight(flight, isCard);

                    sqlConn.Open();
                    //  queryString = $"DELETE Flights WHERE FlightID = {flight}";
                    query = new SqlCommand(queryString, sqlConn);
                    int rows = query.ExecuteNonQuery();
                    if (rows > 0)
                    {
                        Console.WriteLine("Successfully deleted the flight!\n");
                    }
                    sqlConn.Close();
                }
            }
            else
            {
                Console.WriteLine("Input a Flight Number");
                string? flightNumber = Console.ReadLine();
                while (flightNumber == null)
                {
                    Console.WriteLine("Please input a Flight Number");
                    flightNumber = Console.ReadLine();
                }

                using (SqlConnection sqlConn = new SqlConnection("Server=34.162.94.248; Database=air3550; Uid=sqlserver; Password=123;"))
                {
                    // get all flight IDs for that flight number
                    sqlConn.Open();
                    string queryString = $"SELECT FlightID FROM Flights WHERE FlightNumber = {flightNumber}";
                    SqlCommand query = new SqlCommand(queryString, sqlConn);
                    SqlDataReader reader = query.ExecuteReader();
                    List<string?> flightIDs = new List<string?>();
                    while (reader.Read())
                    {
                        flightIDs.Add(reader.GetValue(0).ToString());
                    }
                    reader.Close();

                    // delete all flights with that flight number
                    // queryString = $"DELETE Flights WHERE FlightNumber = {flightNumber}";
                    query = new SqlCommand(queryString, sqlConn);
                    int rows = query.ExecuteNonQuery();
                    if (rows > 0)
                    {
                        Console.WriteLine($"Successfully deleted {rows} flights with flight number {flightNumber}!\n");
                    }
                    // call Refund(string FlightID, bool IsCard) on all flight IDs for that flight number
                    foreach (string? flightiD in flightIDs)
                    {
                        // get the IsCard value from the transactions table
                        queryString = $"SELECT IsCard FROM Transactions WHERE FlightID = {flightiD}";
                        query = new SqlCommand(queryString, sqlConn);
                        reader = query.ExecuteReader();
                        bool isCard = false;
                        if (reader.Read())
                        {
                            isCard = reader.GetBoolean(0);
                        }
                        reader.Close();
                        RefundFlight(flightiD, isCard);
                    }
                    sqlConn.Close();
                }
            }
        }
        public static void RefundFlight(string flightID, bool isCard)
        {
            using (SqlConnection sqlConn = new SqlConnection("Server=34.162.94.248; Database=air3550; Uid=sqlserver; Password=123;"))
            {
                sqlConn.Open();
                // Get the user ID and payment amount for the transaction associated with this flight
                string transactionQueryString = $"SELECT UserID, AmountCharged, IsRefunded FROM Transactions WHERE FlightID = {flightID}";
                SqlCommand transactionQuery = new SqlCommand(transactionQueryString, sqlConn);
                SqlDataReader transactionReader = transactionQuery.ExecuteReader();
                int userID = -1;
                decimal paymentAmount = -1;
                bool? isRefunded = null;
                if (transactionReader.Read())
                {
                    userID = (int)transactionReader["UserID"];
                    paymentAmount = (int)transactionReader["AmountCharged"];
                    if (!transactionReader.IsDBNull(transactionReader.GetOrdinal("IsRefunded")))
                    {
                        isRefunded = (bool)transactionReader["IsRefunded"];
                    }
                }
                transactionReader.Close();
                if (userID != -1 && paymentAmount != -1 && isRefunded != true)
                {
                    // Refund the payment to the user
                    if (isCard)
                    {
                        string refundQueryString = $"UPDATE Users SET CreditCard = 'CreditCard' + '{paymentAmount}' WHERE UserID = {userID}";
                        SqlCommand refundQuery = new SqlCommand(refundQueryString, sqlConn);
                        refundQuery.ExecuteNonQuery();
                    }
                    else
                    {
                        string refundQueryString = $"UPDATE Users SET PointsAvailable = PointsAvailable + {paymentAmount} WHERE UserID = {userID}";
                        SqlCommand refundQuery = new SqlCommand(refundQueryString, sqlConn);
                        refundQuery.ExecuteNonQuery();
                    }
                    // Mark the transaction as refunded
                    string markAsRefundedQueryString = $"UPDATE Transactions SET IsRefunded = 1 WHERE FlightID = '{flightID}' AND UserID = '{userID}'";
                    SqlCommand markAsRefundedQuery = new SqlCommand(markAsRefundedQueryString, sqlConn);
                    markAsRefundedQuery.ExecuteNonQuery();
                    Console.WriteLine($"Flight {flightID} has been canceled and a refund of ${paymentAmount} has been issued to user {userID}\n");
                }
                else if (isRefunded == true)
                {
                    Console.WriteLine($"The transaction associated with flight {flightID} has already been canceled and refunded.\n");
                }
                else
                {
                    Console.WriteLine($"No transaction found for flight {flightID}\n");
                }
                sqlConn.Close();
            }
        }
        public static void cancel()
        {
            Console.WriteLine("Please enter the flight ID to be cancelled: ");
            string? flightiD = Console.ReadLine();
            while (flightiD == null)
            {
                Console.WriteLine("Please input a Flight ID");
                flightiD = Console.ReadLine();
            }
            // Check if flightID exists in Transactions table
            bool flightExists = false;
            bool isCard = false;
            string? userID = "";
            using (SqlConnection sqlConn = new SqlConnection("Server=34.162.94.248; Database=air3550; Uid=sqlserver; Password=123;"))
            {
                sqlConn.Open();
                string queryString = $"SELECT UserID, IsCard FROM Transactions WHERE FlightID = '{flightiD}'";
                SqlCommand query = new SqlCommand(queryString, sqlConn);
                using (SqlDataReader reader = query.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        userID = reader.GetValue(0).ToString();
                        isCard = reader.GetBoolean(1);
                        flightExists = true;
                    }
                }
                sqlConn.Close();
            }
            if (flightExists)
            {
                RefundFlight(flightiD, isCard);
                // Mark flight as refunded in Transactions table
                using (SqlConnection sqlConn = new SqlConnection("Server=34.162.94.248; Database=air3550; Uid=sqlserver; Password=123;"))
                {
                    sqlConn.Open();
                    string updateString = $"UPDATE Transactions SET IsRefunded = 1 WHERE FlightID = '{flightiD}' AND UserID = '{userID}'";
                    SqlCommand update = new SqlCommand(updateString, sqlConn);
                    update.ExecuteNonQuery();
                    sqlConn.Close();
                }
            }
            else
            {
                Console.WriteLine("Sorry, the specified flight ID does not exist.");
            }
        }
        public void AddPlanes()
        {
            Console.WriteLine("Input Plane Model:");
            string? model = Console.ReadLine();

            int seats;
            Console.WriteLine("Input Number of Seats:");
            while (!int.TryParse(Console.ReadLine(), out seats))
            {
                Console.WriteLine("Please enter a valid number");
            }
            using (SqlConnection sqlConn = new SqlConnection("Server=34.162.94.248; Database=air3550; Uid=sqlserver; Password=123;"))
            {
                sqlConn.Open();

                string queryString = $"INSERT INTO Planes (Model, Seats) VALUES ('{model}', {seats})";
                SqlCommand query = new SqlCommand(queryString, sqlConn);
                int rows = query.ExecuteNonQuery();
                if (rows > 0)
                {
                    Console.WriteLine("Successfully added the plane!\n");
                }
                sqlConn.Close();
            }
        }
        public static void ChoosePlanes()
        {
            // Prompt user to enter flight number
            Console.WriteLine("Enter flight number: ");
            string? flightNumber = Console.ReadLine();
            int existingPlaneId = 0;
            int existingSeatsAvailable = 0;
            bool hasExistingPlane = false;

            using (SqlConnection sqlConn = new SqlConnection("Server=34.162.94.248; Database=air3550; Uid=sqlserver; Password=123;"))
            {
                sqlConn.Open();
                SqlCommand cmd = new SqlCommand("SELECT PlaneID, SeatsAvailable FROM Flights WHERE FlightNumber = @flightNumber", sqlConn);
                cmd.Parameters.AddWithValue("@flightNumber", flightNumber);

                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    existingPlaneId = Convert.ToInt32(reader["PlaneID"]);
                    existingSeatsAvailable = Convert.ToInt32(reader["SeatsAvailable"]);
                    hasExistingPlane = true;
                }
                reader.Close(); // Close the SqlDataReader object

                // Retrieve plane options from database
                DataTable planesTable = new DataTable();
                string planesQuery = "SELECT PlaneID, Model, Seats FROM Planes";
                using (SqlCommand command = new SqlCommand(planesQuery, sqlConn))
                {
                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    adapter.Fill(planesTable);
                }

                // Display plane options
                Console.WriteLine("Available planes:");
                foreach (DataRow row in planesTable.Rows)
                {
                    Console.WriteLine($"{row["PlaneID"]}: {row["Model"]} ({row["Seats"]} seats)");
                }

                // Prompt user to select a plane
                Console.WriteLine("Enter plane ID: ");
                string? planeId = Console.ReadLine();

                // Retrieve selected plane's seats
                int seats = 0;
                string seatsQuery = $"SELECT Seats FROM Planes WHERE PlaneID = '{planeId}'";
                using (SqlCommand command = new SqlCommand(seatsQuery, sqlConn))
                {
                    object result = command.ExecuteScalar();
                    if (result != null)
                    {
                        seats = Convert.ToInt32(result);
                    }
                }
                if (hasExistingPlane)
                {
                    Console.WriteLine($"\nFlight {flightNumber} already has a plane assigned to it:");
                    Console.WriteLine($"Plane ID: {existingPlaneId}, Seats available: {existingSeatsAvailable}");
                    Console.Write("\nDo you want to update the plane assignment? (Y/N): ");
                    string? answer = Console.ReadLine();
                    while (answer == null)
                    {
                        Console.WriteLine("Please input a Flight ID");
                        answer = Console.ReadLine();
                    }
                    if (answer.Equals("Y", StringComparison.OrdinalIgnoreCase))
                    {
                        string updateQuery = $"UPDATE Flights SET SeatsAvailable = {seats}, PlaneID = {planeId} WHERE FlightNumber = '{flightNumber}'";
                        SqlCommand command = new SqlCommand(updateQuery, sqlConn);
                        int rows = command.ExecuteNonQuery();
                        if (rows > 0)
                        {
                            Console.WriteLine($"Plane assignment for flight {flightNumber} updated.\n");
                        }
                        else
                        {
                            Console.WriteLine($"Error updating plane assignment for flight {flightNumber}.\n");
                        }
                        sqlConn.Close();

                    }
                    else
                    {
                        Console.WriteLine($"Update was declined. No changes were made for flight {flightNumber}.\n");
                    }
                }

                else
                {
                    // Update flight's SeatsAvailable column
                    string updateQuery = $"UPDATE Flights SET SeatsAvailable = {seats}, PlaneID = {planeId} WHERE FlightNumber = '{flightNumber}'";
                    using (SqlCommand command = new SqlCommand(updateQuery, sqlConn))
                    {
                        command.ExecuteNonQuery();
                    }
                    Console.WriteLine("Plane assigned successfully!\n");
                }
                sqlConn.Close();              
            }
        }
    }
}

