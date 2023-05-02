using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                    Console.WriteLine("Q. Go Back\n");
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
                    // Console.WriteLine("4. Display All Flight");
                    Console.WriteLine("Q. Go Back\n");
                    string? input = Console.ReadLine();
                    if (input == null | (input != "1" & input != "2" & input != "3" & input != "Q"))
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
                        Console.WriteLine("2. Departure Date Time");
                        Console.WriteLine("3. Arrival Date Time");
                        Console.WriteLine("Q. Quit");
                        input = Console.ReadLine();
                        if (input == null | (input != "1" & input != "2" & input != "3" & input != "Q"))
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
                                qs = $"UPDATE Flights SET DepartureDateTime = \'{sqlndd}\' WHERE FlightID = {flight}";
                                q = new SqlCommand(qs, sqlConn);
                                rows = q.ExecuteNonQuery();
                                if (rows > 0)
                                {
                                    Console.WriteLine($"Successfully updated flight: {flight}");
                                }
                                break;
                            case "3":
                                Console.WriteLine("Input a new Arrival Date (mm/dd/yyyy HH:MM:SS)");
                                string? nads = Console.ReadLine();
                                DateTime nad;
                                while (true)
                                {
                                    if (!DateTime.TryParse(nads, out nad))
                                    {
                                        Console.WriteLine("Could not parse the date and time!");
                                        Console.WriteLine("Input a correct Date and time in the format given no parenthesis please");
                                    }
                                    else
                                    {
                                        break;
                                    }
                                    Console.WriteLine("Input a new Arrival Date (mm/dd/yyyy HH:MM:SS)");
                                    nads = Console.ReadLine();
                                }
                                SqlDateTime sqlnad = new SqlDateTime(nad.Year, nad.Month, nad.Day, nad.Hour, nad.Minute, nad.Second);
                                qs = $"UPDATE Flights SET ArrivalDateTime = \'{sqlnad}\' WHERE FlightID = {flight}";
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
                        Console.WriteLine("2. Departure Date Time");
                        Console.WriteLine("3. Arrival Date Time");
                        Console.WriteLine("Q. Quit");
                        input = Console.ReadLine();
                        if (input == null | (input != "1" & input != "2" & input != "3" & input != "Q"))
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
                                qs = $"UPDATE Flights SET DepartureDateTime = \'{sqlndd}\' WHERE FlightNumber = {flight}";
                                q = new SqlCommand(qs, sqlConn);
                                rows = q.ExecuteNonQuery();
                                if (rows > 0)
                                {
                                    Console.WriteLine($"Successfully updated flight: {flight}\n");
                                }
                                break;
                            case "3":
                                Console.WriteLine("Input a new Arrival Date (mm/dd/yyyy HH:MM:SS)");
                                string? nads = Console.ReadLine();
                                DateTime nad;
                                while (true)
                                {
                                    if (!DateTime.TryParse(nads, out nad))
                                    {
                                        Console.WriteLine("Could not parse the date and time!");
                                        Console.WriteLine("Input a correct Date and time in the format given no parenthesis please");
                                    }
                                    else
                                    {
                                        break;
                                    }
                                    Console.WriteLine("Input a new Arrival Date (mm/dd/yyyy HH:MM:SS)");
                                    nads = Console.ReadLine();
                                }
                                SqlDateTime sqlnad = new SqlDateTime(nad.Year, nad.Month, nad.Day, nad.Hour, nad.Minute, nad.Second);
                                qs = $"UPDATE Flights SET ArrivalDateTime = \'{sqlnad}\' WHERE FlightNumber = {flight}";
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
                    airports.Close();
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
                    // connection flights
                    SqlDateTime sqlConArrDate = new SqlDateTime();
                    SqlDateTime sqlConDeptDate = new SqlDateTime();
                    if(con != null)
                    {
                        sqlConArrDate = new SqlDateTime(conArrDate.Year, conArrDate.Month, conArrDate.Day, conArrDate.Hour, conArrDate.Minute, conArrDate.Second);
                        sqlConDeptDate = new SqlDateTime(conDeptDate.Year, conDeptDate.Month, conDeptDate.Day, conDeptDate.Hour, conDeptDate.Minute, conDeptDate.Second);
                    }
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
                    string queryString = $"INSERT INTO Flights (FlightNumber, OriginCity, DestinationCity, Price, DepartureDateTime, ArrivalDateTime)" +
                    $"VALUES ({nfn}, \'{noa}\', \'{nda}\', {np}, \'{sqlDepartureDate}\', \'{sqlArrivalDate}\')";
                    if(con != null)
                    {
                        queryString = $"INSERT INTO Flights (FlightNumber, OriginCity, DestinationCity, Price, DepartureDateTime, ArrivalDateTime, FirstConFlight, SecondConFlight, ConCity)" +
                        $"VALUES ({nfn}, \'{noa}\', \'{nda}\', {np}, \'{sqlDepartureDate}\', \'{sqlArrivalDate}\', {conFlightID1}, {conFlightID2}, \'{con}\')";
                    }
                    SqlCommand query = new SqlCommand(queryString, sqlConn);
                    int rows = query.ExecuteNonQuery();

                    // add these flights every week
                    departureDate = departureDate.AddDays(7);
                    arrivalDate = arrivalDate.AddDays(7);
                    conArrDate = conArrDate.AddDays(7);
                    conDeptDate = conDeptDate.AddDays(7);
                }
                Console.WriteLine("Flights added for the next 6 months!");
                sqlConn.Close();
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
                string? flightString = Console.ReadLine();
                int flight;
                while (!Int32.TryParse(flightString, out flight))
                {
                    Console.WriteLine("Please input a Flight ID");
                    flightString = Console.ReadLine();
                }
                using (SqlConnection sqlConn = new SqlConnection("Server=34.162.94.248; Database=air3550; Uid=sqlserver; Password=123;"))
                {
                    // get value of isCard                   
                    //string queryString = $"SELECT TOP 1 IsCard FROM Transactions WHERE FlightID = '{flight}' ORDER BY TransactionID DESC";
                    //SqlCommand query = new SqlCommand(queryString, sqlConn);
                    //SqlDataReader reader = query.ExecuteReader();
                    //bool isCard = false;
                    //if (reader.Read())
                    //{
                    //    isCard = reader.GetBoolean(0);
                    //}
                    //reader.Close();
                    //sqlConn.Close();

                    //check flight's date and only refund to flight after the date of delete
                    sqlConn.Open();
                    string queryString = $"SELECT * FROM Flights WHERE FlightID = '{flight}'";
                    SqlCommand query = new SqlCommand(queryString, sqlConn);
                    SqlDataReader reader = query.ExecuteReader();
                    if (reader.Read())
                    {
                        DateTime departureTime = (DateTime)reader["DepartureDateTime"];
                        DateTime currentTime = DateTime.Now;
                        if (currentTime < departureTime)
                        {
                            RefundFlight(flight);
                        }
                    }
                    reader.Close();
                    sqlConn.Close();

                    sqlConn.Open();
                    queryString = $"DELETE Flights WHERE FlightID = {flight}";
                    query = new SqlCommand(queryString, sqlConn);
                    int rows = query.ExecuteNonQuery();
                    if (rows > 0)
                    {
                        Console.WriteLine("Successfully deleted the flight!\n");
                    }
                    sqlConn.Close();
                }
            }
            else{
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
                    List<int> flightIDs = new List<int>();
                    while (reader.Read())
                    {
                        flightIDs.Add(reader.GetInt32(0));
                    }
                    reader.Close();
                    sqlConn.Close();

                    // call Refund(int FlightID) on all flight IDs for that flight number
                    foreach (int flightiD in flightIDs)
                    {
                        //check flight's date and only refund to flight after the date of delete
                        sqlConn.Open();
                        queryString = $"SELECT * FROM Flights WHERE FlightID = '{flightiD}'";
                        query = new SqlCommand(queryString, sqlConn);
                        reader = query.ExecuteReader();
                        if (reader.Read())
                        {
                            DateTime departureTime = (DateTime)reader["DepartureDateTime"];
                            DateTime currentTime = DateTime.Now;
                            if (currentTime < departureTime)
                            {
                                RefundFlight(flightiD);
                            }
                        }
                        reader.Close();
                        sqlConn.Close();                                        
                    }

                    // delete all flights with that flight number
                    sqlConn.Open();
                    queryString = $"DELETE Flights WHERE FlightNumber = {flightNumber}";
                    query = new SqlCommand(queryString, sqlConn);
                    int rows = query.ExecuteNonQuery();
                    if (rows > 0)
                    {
                        Console.WriteLine($"Successfully deleted {rows} flights with flight number {flightNumber}!\n");
                    }
                    sqlConn.Close();
                }
            }
        }
        public void RefundFlight(int flightID)
        {
            using (SqlConnection sqlConn = new SqlConnection("Server=34.162.94.248; Database=air3550; Uid=sqlserver; Password=123;"))
            {
                sqlConn.Open();
                // Get the user ID and payment amount for the transaction associated with this flight
                string transactionQueryString = $"SELECT UserID, AmountCharged, IsRefunded, IsCard, TransactionID FROM Transactions WHERE FlightID = {flightID}";
                SqlCommand transactionQuery = new SqlCommand(transactionQueryString, sqlConn);
                SqlDataReader transactionReader = transactionQuery.ExecuteReader();
                while (transactionReader.Read())
                {
                    int userID = (int)transactionReader["UserID"];
                    decimal paymentAmount = (int)transactionReader["AmountCharged"];
                    int transactionID = (int)transactionReader["TransactionID"];
                    bool isRefunded = transactionReader.GetBoolean(2);
                    bool isCard = transactionReader.GetBoolean(3);
                    if (userID != -1 && paymentAmount != -1 && isRefunded != true && transactionID != -1)
                    {
                        // Refund the payment to the user
                        if (isCard)
                        {
                            // deletes the Card Charge from our table, this is how to "refund card"
                            string refundCardQueryString = $"DELETE FROM CardCharges WHERE UserID = {userID} AND TransactionID = {transactionID}";
                            SqlCommand refundCardQuery = new SqlCommand(refundCardQueryString, sqlConn);
                            refundCardQuery.ExecuteNonQuery();
                            Console.WriteLine($"Flight {flightID} has been canceled and a refund of ${paymentAmount} has been issued to user {userID} at original payment");                     
                        }
                       
                        else
                        {
                            string refundQueryString = $"UPDATE Users SET PointsAvailable = PointsAvailable + {paymentAmount} WHERE UserID = {userID}";
                            SqlCommand refundQuery = new SqlCommand(refundQueryString, sqlConn);
                            refundQuery.ExecuteNonQuery();
                            Console.WriteLine($"Flight {flightID} has been canceled and a refund of ${paymentAmount}, points has been issued to user {userID}'s account\n");
                        }
                        // Mark the transaction as refunded
                        string markAsRefundedQueryString = $"UPDATE Transactions SET IsRefunded = 1 WHERE FlightID = '{flightID}' AND UserID = '{userID}'";
                        SqlCommand markAsRefundedQuery = new SqlCommand(markAsRefundedQueryString, sqlConn);
                        markAsRefundedQuery.ExecuteNonQuery();
transactionReader.Close();
                    }

                    else if (isRefunded == true)
                    {
                        Console.WriteLine($"The transaction associated with flight {flightID} has already been canceled and refunded.\n");
                    }

                    else
                    {
                        Console.WriteLine($"No transaction found for flight {flightID}\n");
                    }
                }
                
                sqlConn.Close();
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
        public void ChoosePlanes()
        {
            Console.WriteLine("Enter flight number: ");
            string? flightNumber = Console.ReadLine();
            int? existingPlaneId = 0;
            int? existingSeatsAvailable = 0;
            bool hasExistingPlane = false;

            using (SqlConnection sqlConn = new SqlConnection("Server=34.162.94.248; Database=air3550; Uid=sqlserver; Password=123;"))
            {
                sqlConn.Open();
                SqlCommand cmd = new SqlCommand("SELECT PlaneID, SeatsAvailable FROM Flights WHERE FlightNumber = @flightNumber", sqlConn);
                cmd.Parameters.AddWithValue("@flightNumber", flightNumber);

                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    if (reader["PlaneID"] == DBNull.Value)
                    {
                        hasExistingPlane = false;
                    }
                    else
                    {
                        existingPlaneId = Convert.ToInt32(reader["PlaneID"]);
                        existingSeatsAvailable = Convert.ToInt32(reader["SeatsAvailable"]);
                        hasExistingPlane = true;
                    }
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
                    Console.WriteLine($"Plane ID: {row["PlaneID"]}, Model: {row["Model"]} ({row["Seats"]} seats)");
                }
                Console.WriteLine("\nEnter plane ID: ");
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
                    Console.Write("\nDo you want to update the plane assignment? (Y/N): \n");
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

