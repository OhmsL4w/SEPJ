using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SqlTypes;
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
                while(true)
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
            while(true)
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
                while(flight == null)
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
                                while(true)
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
            }else
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
            Console.WriteLine("Input a Flight Number");
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
                Console.WriteLine("Input a Flight Number");
                nfns = Console.ReadLine();
            }
            string? noa;
            using (SqlConnection sqlConn = new SqlConnection("Server=34.162.94.248; Database=air3550; Uid=sqlserver; Password=123;"))
            {
                sqlConn.Open();
                do
                {
                    Console.WriteLine("Input a Origin Airport");
                    noa = Console.ReadLine();
                    string queryString = $"SELECT * FROM Airports WHERE Code=\'{noa}\'";
                    SqlCommand query = new SqlCommand(queryString, sqlConn);
                    SqlDataReader airports = query.ExecuteReader();
                    if (airports.HasRows)
                    {
                        break;
                    }
                } while (true);
                sqlConn.Close();
            }
            string? nda;
            using (SqlConnection sqlConn = new SqlConnection("Server=34.162.94.248; Database=air3550; Uid=sqlserver; Password=123;"))
            {
                sqlConn.Open();
                do
                {
                    Console.WriteLine("Input a Destination Airport");
                    nda = Console.ReadLine();
                    string queryString = $"SELECT * FROM Airports WHERE Code=\'{nda}\'";
                    SqlCommand query = new SqlCommand(queryString, sqlConn);
                    SqlDataReader airports = query.ExecuteReader();
                    if (airports.HasRows)
                    {
                        break;
                    }
                } while (true);
                sqlConn.Close();
            }

            Console.WriteLine("Input a Departure Date (mm/dd/yyyy HH:MM:SS)");
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
                Console.WriteLine("Input a Departure Date (mm/dd/yyyy HH:MM:SS)");
                ndds = Console.ReadLine();
            }
            SqlDateTime sqlndd = new SqlDateTime(ndd.Year, ndd.Month, ndd.Day, ndd.Hour, ndd.Minute, ndd.Second);
            
            Console.WriteLine("Input an Arrival Date and Time (mm/dd/yyyy HH:MM:SS)");
            string? nads = Console.ReadLine();
            DateTime nad;
            while (true)
            {
                if (!DateTime.TryParse(nads, out nad))
                {
                    Console.WriteLine("Could not parse the date and time!");
                    Console.WriteLine("Input a correct Date and time in the format given no parenthesis please");
                }
                else if(nad < ndd)
                {
                    Console.WriteLine("You have to input an arrival time after the departure date!");
                }
                else
                {
                    break;
                }
                Console.WriteLine("Input an Arrival Date (mm/dd/yyyy HH:MM:SS)");
                nads = Console.ReadLine();
            }

            using (SqlConnection sqlConn = new SqlConnection("Server=34.162.94.248; Database=air3550; Uid=sqlserver; Password=123;"))
            {
                sqlConn.Open();
                // first find the distance of the two airports to be able to get the price
                string distanceQuery = $"SELECT Distance FROM Distances WHERE (AirportA = \'{noa}\' AND AirportB = \'{nda}\') OR (AirportA = \'{nda}\' AND AirportB = \'{noa}\')";
                SqlCommand distanceCommand = new SqlCommand(distanceQuery, sqlConn);
                SqlDataReader distanceReader = distanceCommand.ExecuteReader();
                if (!distanceReader.HasRows)
                {
                    Console.WriteLine("Could not find airports distances!");
                    return;
                }
                List<int> distances = new List<int>();
                while(distanceReader.Read())
                {
                    distances.Add(distanceReader.GetInt32(0));
                }
                distanceReader.Close();
                decimal np = 50M;
                // the only flight that needs a connection is anything to LAX, first must go to MSP or DEN
                string? con = null;
                decimal npc1 = new();
                decimal npc2 = new();
                DateTime conDeptDate = new();
                DateTime conArrDate = new();
                if (distances.ElementAt(0) > 1550)
                {
                    // needs a connection flight
                    string connectionQuery = $"SELECT Distance FROM Distances WHERE (AirportA = \'DEN\' AND AirportB = \'{nda}\') OR (AirportA = \'{nda}\' AND AirportB = \'DEN\') OR (AirportA = 'DEN' AND AirportB = '{noa}') OR (AirportA = '{noa}' AND AirportB = 'DEN')";
                    SqlCommand connectionCommand = new SqlCommand(connectionQuery, sqlConn);
                    SqlDataReader connectionReader = connectionCommand.ExecuteReader();
                    if (!connectionReader.HasRows)
                    {
                        Console.WriteLine("Could not find airports distances!");
                        return;
                    }
                    bool allowDEN = true;
                    while (connectionReader.Read())
                    {
                        distances.Add(connectionReader.GetInt32(0));
                    }
                    connectionReader.Close();
                    for (int i = 1; i < distances.Count; i++)
                    {
                        if (distances.ElementAt(i) > 1550)
                        {
                            allowDEN = false;
                        }
                    }

                    if (allowDEN)
                    {
                        do
                        {
                            Console.WriteLine("Choose a connection airport, MSP or DEN");
                            con = Console.ReadLine();
                            if (con == "MSP" | con == "DEN") break;
                        } while (true);
                    }
                    else
                    {
                        Console.WriteLine("Must have MSP be connection to the flight since it is too long");
                        con = "MSP";
                    }
                    connectionQuery = $"SELECT Distance FROM Distances WHERE (AirportA = \'MSP\' AND AirportB = \'{nda}\') OR (AirportA = \'{nda}\' AND AirportB = \'MSP\') OR (AirportA = 'MSP' AND AirportB = '{noa}') OR (AirportA = '{noa}' AND AirportB = 'MSP')";
                    connectionCommand = new SqlCommand(connectionQuery, sqlConn);
                    connectionReader = connectionCommand.ExecuteReader();
                    if (!connectionReader.HasRows)
                    {
                        Console.WriteLine("Could not find airports distances!");
                        return;
                    }
                    while (connectionReader.Read())
                    {
                        distances.Add(connectionReader.GetInt32(0));
                    }
                    connectionReader.Close();
                    decimal firstConPrice;
                    decimal secondConPrice;
                    if (con == "DEN")
                    {
                        firstConPrice = new decimal(distances.ElementAt(1) * 0.12);
                        npc1 = firstConPrice;
                        secondConPrice = new decimal(distances.ElementAt(2) * 0.12);
                        npc2 = secondConPrice;
                    }
                    else
                    {
                        firstConPrice = new decimal(distances.ElementAt(3) * 0.12);
                        npc1 = firstConPrice;
                        secondConPrice = new decimal(distances.ElementAt(4) * 0.12);
                        npc2 = secondConPrice;
                    }
                    np = decimal.Add(np, firstConPrice);
                    np = decimal.Add(np, secondConPrice);
                    npc1 = decimal.Add(npc1, new decimal(8));
                    npc2 = decimal.Add(npc2, new decimal(8));
                    np = decimal.Add(np, new decimal(8)); // 8 dollar segment fee for first flight
                    np = decimal.Add(np, new decimal(8)); // 8 dollar segment fee for connection flight

                    // connection flight dates and times
                    Console.WriteLine("Input an Arrival Date and Time (mm/dd/yyyy HH:MM:SS) for the first flight");
                    string? cads = Console.ReadLine();
                    while (true)
                    {
                        if (!DateTime.TryParse(cads, out conArrDate))
                        {
                            Console.WriteLine("Could not parse the date and time!");
                            Console.WriteLine("Input a correct Date and time in the format given no paranthesis please");
                        }
                        else
                        {
                            break;
                        }
                        Console.WriteLine("Input an Arrival Date (mm/dd/yyyy HH:MM:SS) for the first flight");
                        nads = Console.ReadLine();
                    }
                    Console.WriteLine("Input a Departure Date and Time (mm/dd/yyyy HH:MM:SS) for the second flight");
                    string? cdds = Console.ReadLine();
                    while (true)
                    {
                        if (!DateTime.TryParse(cdds, out conDeptDate))
                        {
                            Console.WriteLine("Could not parse the date and time!");
                            Console.WriteLine("Input a correct Date and time in the format given no paranthesis please");
                        }
                        else if((conDeptDate - conArrDate).TotalMinutes < 40)
                        {
                            Console.WriteLine("Connecting flights must at least be 40 minutes after arrival of the first flight!");
                        }
                        else
                        {
                            break;
                        }
                        Console.WriteLine("Input a Departure Date (mm/dd/yyyy HH:MM:SS) for the second flight");
                        nads = Console.ReadLine();
                    }
                    // determine the new arrival time
                    Console.WriteLine("Input an Arrival Date and Time (mm/dd/yyyy HH:MM:SS) for the entire trip");
                    nads = Console.ReadLine();
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
                        Console.WriteLine("Input an Arrival Date (mm/dd/yyyy HH:MM:SS) for the entire trip");
                        nads = Console.ReadLine();
                    }
                }
                else
                {
                    // just a normal flight with a distance
                    decimal milesAdded = new decimal(distances.ElementAt(0) * 0.12);
                    np = decimal.Add(np, milesAdded);
                    np = decimal.Add(np, new decimal(8)); // 8 dollar segment fee
                }
                // check the timing of the flights
                if (ndd.Hour < 8 & ndd.Hour > 5)
                {
                    // off-peak discount
                    np = decimal.Multiply(np, new decimal(.90));
                }
                else if (ndd.Hour < 5)
                {
                    // red-eye discount
                    np = decimal.Multiply(np, new decimal(.80));
                }
                // check the timing of the flights
                if (con != null & conArrDate.Hour < 8 & conArrDate.Hour > 5)
                {
                    // off-peak discount
                    npc1 = decimal.Multiply(np, new decimal(.90));
                }
                else if (con != null & conArrDate.Hour < 5)
                {
                    // red-eye discount
                    np = decimal.Multiply(np, new decimal(.80));
                }
                if (con != null & conDeptDate.Hour < 8 & conDeptDate.Hour > 5)
                {
                    // off-peak discount
                    npc2 = decimal.Multiply(np, new decimal(.90));
                }
                else if (con != null & conDeptDate.Hour < 5)
                {
                    // red-eye discount
                    np = decimal.Multiply(np, new decimal(.80));
                }
                DateTime endDate = DateTime.Now.AddMonths(7);
                DateTime departureDate = ndd;
                DateTime arrivalDate = nad;
                while (departureDate <= endDate)
                {

                    SqlDateTime sqlDepartureDate = new SqlDateTime(departureDate.Year, departureDate.Month, departureDate.Day, departureDate.Hour, departureDate.Minute, departureDate.Second);
                    SqlDateTime sqlArrivalDate = new SqlDateTime(arrivalDate.Year, arrivalDate.Month, arrivalDate.Day, arrivalDate.Hour, arrivalDate.Minute, arrivalDate.Second);
                    SqlDateTime sqlConArrDate = new SqlDateTime(conArrDate.Year, conArrDate.Month, conArrDate.Day, conArrDate.Hour, conArrDate.Minute, conArrDate.Second);
                    SqlDateTime sqlConDeptDate = new SqlDateTime(conDeptDate.Year, conDeptDate.Month, conDeptDate.Day, conDeptDate.Hour, conDeptDate.Minute, conDeptDate.Second);
                    int? conFlightID1 = null;
                    int? conFlightID2 = null;
                    if (con != null) // has a connection flight so create the connection first then link them to the main flight
                    {
                        // first flight
                        string conQueryString = $"INSERT INTO Flights (FlightNumber, OriginCity, DestinationCity, Price, DepartureDateTime, ArrivalDateTime) OUTPUT INSERTED.FlightID " +
                        $"VALUES ({nfn + 1}, \'{noa}\', \'{con}\', {npc1}, \'{sqlDepartureDate}\', \'{sqlConArrDate}\')";
                        SqlCommand conQuery = new SqlCommand(conQueryString, sqlConn);
                        conFlightID1 = (int)conQuery.ExecuteScalar();
                        // second flight
                        conQueryString = $"INSERT INTO Flights (FlightNumber, OriginCity, DestinationCity, Price, DepartureDateTime, ArrivalDateTime) OUTPUT INSERTED.FlightID " +
                        $"VALUES ({nfn + 2}, \'{con}\', \'{nda}\', {npc2}, \'{sqlConDeptDate}\', \'{sqlArrivalDate}\')";
                        conQuery = new SqlCommand(conQueryString, sqlConn);
                        conFlightID2 = (int)conQuery.ExecuteScalar();
                    }
                    // input the connected flight 
                    string queryString = $"INSERT INTO Flights (FlightNumber, OriginCity, DestinationCity, Price, DepartureDateTime, ArrivalDateTime, FirstConFlight, SecondConFlight)" +
                    $"VALUES ({nfn}, \'{noa}\', \'{nda}\', {np}, \'{sqlDepartureDate}\', \'{sqlArrivalDate}\', {conFlightID1}, {conFlightID2})";
                    SqlCommand query = new SqlCommand(queryString, sqlConn);
                    int rows = query.ExecuteNonQuery();

                    // add these flights every week
                    departureDate = departureDate.AddDays(7);
                    arrivalDate = arrivalDate.AddDays(7);
                    conArrDate = conArrDate.AddDays(7);
                    conDeptDate = conDeptDate.AddDays(7);
                }
                Console.WriteLine("Flights added for the next 6 months!");

            }
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
            }else
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
    }
}
