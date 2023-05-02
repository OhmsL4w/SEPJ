using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Air3550
{
    public class UserOptions
    {
        private User CurUser;
        public UserOptions(User user)
        {
            CurUser = user;
        }
        public void BookFlight(string username)
        {
            string? OriginAirport, DestinationAirport, DDate, ADate, checkFlightDets, paymentMethod, checkPaymentDets, CCardUpdate, CCard = null, newCCard = null;
            int rows = 0, isCard = 0, addPoints = 0, roundTrip = -1;
            do
            {
                Console.WriteLine("Is this flight round trip?\n1. Yes\n2. No\nQ. Quit");
                string? roundTripString = Console.ReadLine();
                if (roundTripString == "Q") return;
                Int32.TryParse(roundTripString, out roundTrip);
            } while (roundTrip == -1);
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
            if (roundTrip == 2) // this is not a round trip flight
            {
                DateTime DepartDate;
                do
                {
                    Console.WriteLine("Please enter a Departure Date mm/dd/yyyy");
                    DDate = Console.ReadLine();

                    if (!DateTime.TryParse(DDate, out DepartDate))
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

                using (SqlConnection sqlConn = new SqlConnection("Server=34.162.94.248; Database=air3550; Uid=sqlserver; Password=123;"))
                {
                    string OrgCity = null, DesCity = null, conCity = null;
                    int flightID = 0, firstCon = -1, SecCon = -1, selectFlight = -1, seats = -1;
                    DateTime DepartDets = default(DateTime);
                    DateTime AriveDets = default(DateTime);
                    decimal price = 0;
                    bool fullConnectedFlight = false;

                    //checking if we have a flight that the user is requesting 
                    checkFlightDets = $"SELECT * FROM Flights WHERE OriginCity = @OriginCity AND DestinationCity = @DestinationCity AND CONVERT(date, DepartureDateTime) = CONVERT(date, @DepartureDate)";
                    sqlConn.Open();
                    using (SqlCommand FlightDets = new SqlCommand(checkFlightDets, sqlConn))
                    {
                        FlightDets.Parameters.AddWithValue("@OriginCity", OriginAirport);
                        FlightDets.Parameters.AddWithValue("@DestinationCity", DestinationAirport);
                        FlightDets.Parameters.AddWithValue("@DepartureDate", DepartDate);
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
                                firstCon = reader.IsDBNull(reader.GetOrdinal("FirstConFlight")) ? -1 : (int)reader["FirstConFlight"];
                                SecCon = reader.IsDBNull(reader.GetOrdinal("SecondConFlight")) ? -1 : (int)reader["SecondConFlight"];
                                conCity = reader.IsDBNull(reader.GetOrdinal("ConCity")) ? null : (string)reader["ConCity"];


                                if (firstCon != -1 && SecCon != -1 && conCity != null) // flight has connections 
                                {
                                    Console.WriteLine($"Flight ID {flightID} found from {OrgCity} at {DepartDets.ToString()} to {DesCity} at {AriveDets.ToString()} with a connection at {conCity} for Price: ${price}");

                                }
                                else
                                {
                                    Console.WriteLine($"Flight ID {flightID} found from {OrgCity} at {DepartDets.ToString()} to {DesCity} at {AriveDets.ToString()} for Price: ${price}");
                                }

                            }
                            reader.Close();

                            // add which flight they want to select based on flight ID
                            while (true)
                            {
                                Console.WriteLine("Please select flight by entering its Flight ID");
                                if (int.TryParse(Console.ReadLine(), out selectFlight))
                                {
                                    break;
                                }
                            }

                            string flightInfoQuery = $"SELECT * FROM Flights WHERE FlightID = @flightId";
                            using (SqlCommand flightInfo = new SqlCommand(flightInfoQuery, sqlConn)) // setting all the varibles to the flight user selects
                            {
                                flightInfo.Parameters.AddWithValue("@flightID", selectFlight);
                                SqlDataReader fInfo = flightInfo.ExecuteReader();
                                fInfo.Read();
                                if (!fInfo.HasRows)
                                {
                                    Console.WriteLine("No flights found with this flight ID please try booking again");
                                    BookFlight(username);
                                    return;
                                }
                                // Retrieve the flight information from the reader
                                OrgCity = (string)fInfo["OriginCity"];
                                DesCity = (string)fInfo["DestinationCity"];
                                DepartDets = fInfo.GetDateTime(fInfo.GetOrdinal("DepartureDateTime"));
                                AriveDets = fInfo.GetDateTime(fInfo.GetOrdinal("ArrivalDateTime"));
                                flightID = (int)fInfo["FlightID"];
                                price = (decimal)fInfo["Price"];
                                seats = fInfo.IsDBNull(fInfo.GetOrdinal("SeatsAvailable")) ? -1 : (int)fInfo["SeatsAvailable"];
                                firstCon = fInfo.IsDBNull(fInfo.GetOrdinal("FirstConFlight")) ? -1 : (int)fInfo["FirstConFlight"];
                                SecCon = fInfo.IsDBNull(fInfo.GetOrdinal("SecondConFlight")) ? -1 : (int)fInfo["SecondConFlight"];
                                fInfo.Close();
                                if (firstCon != -1 && SecCon != -1) // flight has connections 
                                {
                                    Console.WriteLine($"Selected Flight has a Connection:");
                                    string connectionQuery = $"SELECT * FROM FLights WHERE FlightID = (SELECT FirstConFlight FROM Flights WHERE FlightID = @flightId) " +
                                                                "UNION " +
                                                                "SELECT * FROM Flights WHERE FlightID = (SELECT SecondConFlight FROM Flights WHERE FlightID = @flightId)";
                                    using (SqlCommand connections = new SqlCommand(connectionQuery, sqlConn))
                                    {
                                        connections.Parameters.AddWithValue("@flightId", flightID);
                                        SqlDataReader con = connections.ExecuteReader();
                                        if (con.HasRows)
                                        {
                                            while (con.Read())
                                            {
                                                int conFID = (int)con["FlightID"];
                                                string conOrgCity = (string)con["OriginCity"];
                                                string conDesCity = (string)con["DestinationCity"];
                                                DateTime conDepart = con.GetDateTime(con.GetOrdinal("DepartureDateTime"));
                                                DateTime conArrive = con.GetDateTime(con.GetOrdinal("ArrivalDateTime"));
                                                Console.WriteLine($"Flight ID {conFID} from {conOrgCity} at {conDepart.ToString()} to {conDesCity} at {conArrive.ToString()}");
                                                int conSeats = con.GetInt32(con.GetOrdinal("SeatsAvailable"));
                                                if (conSeats <= 0)
                                                {
                                                    fullConnectedFlight = true;
                                                }
                                            }
                                        }
                                        con.Close();
                                    }

                                }
                            }
                            addPoints = (int)(price * 100);
                            // if there are no seats and its not a connected flight
                            // or if the connected flight is full
                            if ((seats <= 0 & firstCon == -1 & SecCon == -1) || fullConnectedFlight)
                            {
                                Console.WriteLine("Flight is Fully Booked");
                                return;
                            }


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
                                        int points = (int)paymenreader["PointsAvailable"];
                                        CCard = paymenreader.IsDBNull(paymenreader.GetOrdinal("CreditCard")) ? null : (string)paymenreader["CreditCard"];
                                        paymenreader.Close();
                                        if (paymentMethod == "2")
                                        {
                                            if (points >= addPoints)
                                            {
                                                Console.WriteLine("You have Enough points");
                                                string? usingPoints = $"UPDATE Users SET PointsAvailable = PointsAvailable - @pointsToReduce,PointsUsed = PointsUsed + @pointsToAdd WHERE UserID = @UserId";
                                                using (SqlCommand updatePoints = new SqlCommand(usingPoints, sqlConn))
                                                {
                                                    updatePoints.Parameters.AddWithValue("@pointsToReduce", addPoints);
                                                    updatePoints.Parameters.AddWithValue("@pointsToAdd", addPoints);
                                                    updatePoints.Parameters.AddWithValue("@UserId", username);
                                                    rows = updatePoints.ExecuteNonQuery();
                                                    if (rows == 0)
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
                                        if (notEnoughPoints == true || paymentMethod == "1")
                                        {
                                            string? CCsave = null;
                                            if (CCard != null)
                                            {
                                                Console.WriteLine("Would you like to use the Card you have saved for your account?\n1.Yes\n2.No");

                                                do
                                                {
                                                    CCsave = Console.ReadLine();
                                                    if (CCsave == "2")
                                                    {
                                                        //ask user for new card and see if they want to save it to their account
                                                        do
                                                        {
                                                            Console.WriteLine("Enter new Credit Card");
                                                            newCCard = Console.ReadLine();
                                                        } while (newCCard == null || newCCard.Length != 16);
                                                        string? saveNew;
                                                        do
                                                        {
                                                            Console.WriteLine("Would you like to save this card to your account?");
                                                            Console.WriteLine("1. Yes");
                                                            Console.WriteLine("2. No");
                                                            saveNew = Console.ReadLine();
                                                        } while (saveNew == null | (saveNew != "1" && saveNew != "2"));
                                                        if (saveNew == "1")
                                                        {
                                                            CCardUpdate = $"UPDATE Users SET CreditCard = @CreditCard WHERE UserID = @UserId";
                                                            using (SqlCommand queryUpdateCC = new SqlCommand(CCardUpdate, sqlConn))
                                                            {
                                                                queryUpdateCC.Parameters.AddWithValue("@CreditCard", newCCard);
                                                                queryUpdateCC.Parameters.AddWithValue("@UserId", username);
                                                                rows = queryUpdateCC.ExecuteNonQuery();
                                                                if (rows == 0)
                                                                {
                                                                    Console.WriteLine("Unsuccessful Credit Card change.");
                                                                }
                                                            }
                                                        }
                                                    }
                                                    else if (CCsave == "1") break;
                                                } while (CCsave == null | (CCsave != "1" && CCsave != "2"));
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
                                                    CCardUpdate = $"UPDATE Users SET CreditCard = @CreditCard WHERE UserID = @UserId";
                                                    using (SqlCommand queryUpdateCC = new SqlCommand(CCardUpdate, sqlConn))
                                                    {
                                                        queryUpdateCC.Parameters.AddWithValue("@CreditCard", newCCard);
                                                        queryUpdateCC.Parameters.AddWithValue("@UserId", username);
                                                        rows = queryUpdateCC.ExecuteNonQuery();
                                                        if (rows == 0)
                                                        {
                                                            Console.WriteLine("Unsuccessful Credit Card change.");
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                        string subSeat = $"UPDATE Flights SET SeatsAvailable = SeatsAvailable - 1 WHERE FlightID = {flightID}";
                                        if (firstCon != -1 && SecCon != -1)
                                        {
                                            subSeat = $"UPDATE Flights SET SeatsAvailable = SeatsAvailable - 1 WHERE FlightID = {firstCon} " +
                                                      $"UPDATE Flights SET SeatsAvailable = SeatsAvailable - 1 WHERE FlightID = {SecCon} ";
                                        }
                                        using (SqlCommand querySubSeat = new SqlCommand(subSeat, sqlConn))
                                        {
                                            rows = querySubSeat.ExecuteNonQuery();
                                            if (rows == 0)
                                            {
                                                Console.WriteLine("couldn't subtract # of seats");
                                            }
                                        }
                                        string pointUpdate = $"UPDATE Users SET PointsAvailable = PointsAvailable + @Points WHERE UserID = @UserId";
                                        using (SqlCommand queryUpdatePoints = new SqlCommand(pointUpdate, sqlConn))
                                        {
                                            queryUpdatePoints.Parameters.AddWithValue("@Points", addPoints);
                                            queryUpdatePoints.Parameters.AddWithValue("@UserId", username);
                                            rows = queryUpdatePoints.ExecuteNonQuery();
                                            if (rows == 0)
                                            {
                                                Console.WriteLine("couldn't add points");
                                            }
                                        }
                                        string Transac = $"INSERT INTO Transactions (AmountCharged, IsCard, UserID, FlightID) OUTPUT INSERTED.TransactionID VALUES ( @AmountCharged, @IsCard, @UserId, @FlightID)";
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
                                            int transactionID = (int)addTransaction.ExecuteScalar();
                                            if (isCard == 1)
                                            {
                                                string cardCharge = $"INSERT INTO CardCharges (FirstName, LastName, UserID, AmountCharged, CardNumber, TransactionID) OUTPUT INSERTED.TransactionID VALUES ( @firstName, @lastName, @UserId, @AmountCharged, @cardNumber, @transactionID)";
                                                using (SqlCommand addCardCharge = new SqlCommand(cardCharge, sqlConn))
                                                {
                                                    addCardCharge.Parameters.AddWithValue("@firstName", CurUser.FirstName);
                                                    addCardCharge.Parameters.AddWithValue("@lastName", CurUser.LastName);
                                                    addCardCharge.Parameters.AddWithValue("@UserId", CurUser.UserID);
                                                    addCardCharge.Parameters.AddWithValue("@AmountCharged", price);
                                                    if (newCCard != null)
                                                    {
                                                        addCardCharge.Parameters.AddWithValue("@cardNumber", newCCard);
                                                    }
                                                    else
                                                    {
                                                        addCardCharge.Parameters.AddWithValue("@cardNumber", CCard);
                                                    }
                                                    addCardCharge.Parameters.AddWithValue("@transactionID", transactionID);
                                                    rows = addCardCharge.ExecuteNonQuery();
                                                    if (rows == 0)
                                                    {
                                                        Console.WriteLine("Couldn't create CardCharge");
                                                    }

                                                }
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
                                                name = name + " " + (string)recieptmaker["LastName"];
                                                Console.WriteLine($"Flight booked from {OrgCity} at {DepartDets.ToString()} to {DesCity} at {AriveDets.ToString()} at Price: ${price}");
                                                if (paymentMethod == "1" || notEnoughPoints == true)
                                                {
                                                    string card = newCCard == null ? CCard : newCCard;
                                                    Console.WriteLine($"The Payment Method Used was Credit Card: {card}");
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
            else // round trip flight
            {
                DateTime DepartDate;
                DateTime rtDepartDate;
                do
                {
                    Console.WriteLine("Please enter a Departure Date for the first flight mm/dd/yyyy");
                    DDate = Console.ReadLine();

                    if (!DateTime.TryParse(DDate, out DepartDate))
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

                do
                {
                    Console.WriteLine("Please enter a Departure Date for the return flight mm/dd/yyyy");
                    DDate = Console.ReadLine();

                    if (!DateTime.TryParse(DDate, out rtDepartDate))
                    {
                        Console.WriteLine("Invalid date format. Please enter a valid date in mm/dd/yyyy format.");
                        continue;
                    }

                    if (rtDepartDate < DateTime.Today)
                    {
                        Console.WriteLine("Departure date cannot be in the past. Please enter a future date.");
                        continue;
                    }
                    break;

                } while (true);

                using (SqlConnection sqlConn = new SqlConnection("Server=34.162.94.248; Database=air3550; Uid=sqlserver; Password=123;"))
                {
                    // all the same but doubled, all the ones with a 1 at the end is for the return flight
                    string OrgCity = null, DesCity = null, conCity = null;
                    string OrgCity1 = null, DesCity1 = null, conCity1 = null;
                    int flightID = 0, firstCon = -1, SecCon = -1, selectFlight = -1, seats = -1;
                    int flightID1 = 0, firstCon1 = -1, SecCon1 = -1, selectFlight1 = -1, seats1 = -1;
                    DateTime DepartDets = default(DateTime);
                    DateTime DepartDets1 = default(DateTime);
                    DateTime AriveDets = default(DateTime);
                    DateTime AriveDets1 = default(DateTime);
                    decimal price = 0;
                    decimal price1 = 0;
                    bool fullConnectedFlight = false;
                    bool fullConnectedFlight1 = false;

                    // checking if we have a flight that the user is requesting 
                    checkFlightDets = $"SELECT * FROM Flights WHERE OriginCity = @OriginCity AND DestinationCity = @DestinationCity AND CONVERT(date, DepartureDateTime) = CONVERT(date, @DepartureDate)";
                    sqlConn.Open();
                    using (SqlCommand FlightDets = new SqlCommand(checkFlightDets, sqlConn))
                    {
                        FlightDets.Parameters.AddWithValue("@OriginCity", OriginAirport);
                        FlightDets.Parameters.AddWithValue("@DestinationCity", DestinationAirport);
                        FlightDets.Parameters.AddWithValue("@DepartureDate", DepartDate);
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
                                firstCon = reader.IsDBNull(reader.GetOrdinal("FirstConFlight")) ? -1 : (int)reader["FirstConFlight"];
                                SecCon = reader.IsDBNull(reader.GetOrdinal("SecondConFlight")) ? -1 : (int)reader["SecondConFlight"];
                                conCity = reader.IsDBNull(reader.GetOrdinal("ConCity")) ? null : (string)reader["ConCity"];
                                int eachFlightSeat = reader.GetInt32(reader.GetOrdinal("SeatsAvailable"));

                                if (firstCon != -1 && SecCon != -1 && conCity != null) // flight has connections, can not check the seats yet
                                {
                                    Console.WriteLine($"Flight ID {flightID} found from {OrgCity} at {DepartDets.ToString()} to {DesCity} at {AriveDets.ToString()} with a connection at {conCity} for Price: ${price}");

                                }
                                else
                                {
                                    if (eachFlightSeat > 0) // only show it if there are seats
                                    {
                                        Console.WriteLine($"Flight ID {flightID} found from {OrgCity} at {DepartDets.ToString()} to {DesCity} at {AriveDets.ToString()} for Price: ${price}");
                                    }
                                }

                            }
                            reader.Close();
                            // add which flight they want to select based on flight ID
                            while (true)
                            {
                                Console.WriteLine("Please select flight by entering its Flight ID");
                                if (int.TryParse(Console.ReadLine(), out selectFlight))
                                {
                                    break;
                                }
                            }

                            string flightInfoQuery = $"SELECT * FROM Flights WHERE FlightID = @flightId";
                            using (SqlCommand flightInfo = new SqlCommand(flightInfoQuery, sqlConn)) // setting all the varibles to the flight user selects
                            {
                                flightInfo.Parameters.AddWithValue("@flightID", selectFlight);
                                SqlDataReader fInfo = flightInfo.ExecuteReader();
                                fInfo.Read();
                                if (!fInfo.HasRows)
                                {
                                    Console.WriteLine("No flights found with this flight ID please try booking again");
                                    BookFlight(username);
                                    return;
                                }
                                // Retrieve the flight information from the reader
                                OrgCity = (string)fInfo["OriginCity"];
                                DesCity = (string)fInfo["DestinationCity"];
                                DepartDets = fInfo.GetDateTime(fInfo.GetOrdinal("DepartureDateTime"));
                                AriveDets = fInfo.GetDateTime(fInfo.GetOrdinal("ArrivalDateTime"));
                                flightID = (int)fInfo["FlightID"];
                                price = (decimal)fInfo["Price"];
                                seats = fInfo.IsDBNull(fInfo.GetOrdinal("SeatsAvailable")) ? -1 : (int)fInfo["SeatsAvailable"];
                                firstCon = fInfo.IsDBNull(fInfo.GetOrdinal("FirstConFlight")) ? -1 : (int)fInfo["FirstConFlight"];
                                SecCon = fInfo.IsDBNull(fInfo.GetOrdinal("SecondConFlight")) ? -1 : (int)fInfo["SecondConFlight"];
                                fInfo.Close();
                                if (firstCon != -1 && SecCon != -1) // flight has connections 
                                {
                                    Console.WriteLine($"Selected Flight has a Connection:");
                                    string connectionQuery = $"SELECT * FROM FLights WHERE FlightID = (SELECT FirstConFlight FROM Flights WHERE FlightID = @flightId) " +
                                                                "UNION " +
                                                                "SELECT * FROM Flights WHERE FlightID = (SELECT SecondConFlight FROM Flights WHERE FlightID = @flightId)";
                                    using (SqlCommand connections = new SqlCommand(connectionQuery, sqlConn))
                                    {
                                        connections.Parameters.AddWithValue("@flightId", flightID);
                                        SqlDataReader con = connections.ExecuteReader();
                                        if (con.HasRows)
                                        {
                                            while (con.Read())
                                            {
                                                int conFID = (int)con["FlightID"];
                                                string conOrgCity = (string)con["OriginCity"];
                                                string conDesCity = (string)con["DestinationCity"];
                                                DateTime conDepart = con.GetDateTime(con.GetOrdinal("DepartureDateTime"));
                                                DateTime conArrive = con.GetDateTime(con.GetOrdinal("ArrivalDateTime"));
                                                Console.WriteLine($"Flight ID {conFID} from {conOrgCity} at {conDepart.ToString()} to {conDesCity} at {conArrive.ToString()}");
                                                int conSeats = con.GetInt32(con.GetOrdinal("SeatsAvailable"));
                                                if (conSeats <= 0)
                                                {
                                                    fullConnectedFlight = true;
                                                }
                                            }
                                        }
                                        con.Close();
                                    }

                                }
                            }
                            addPoints = (int)(price * 100);
                            // if there are no seats and its not a connected flight
                            // or if the connected flight is full
                            if ((seats <= 0 & firstCon == -1 & SecCon == -1) || fullConnectedFlight)
                            {
                                Console.WriteLine("Flight is Fully Booked");
                                return;
                            }
                            // begin return flight
                            // Do the same thing for the return flight but reverse the airports
                            checkFlightDets = $"SELECT * FROM Flights WHERE OriginCity = \'{DestinationAirport}\' AND DestinationCity = \'{OriginAirport}\' AND CONVERT(date, DepartureDateTime) = CONVERT(date, \'{rtDepartDate}\')";
                            SqlCommand retFlightCommand = new SqlCommand(checkFlightDets, sqlConn);
                            reader = retFlightCommand.ExecuteReader();

                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    // Retrieve the flight information from the reader
                                    OrgCity1 = (string)reader["OriginCity"];
                                    DesCity1 = (string)reader["DestinationCity"];
                                    DepartDets1 = reader.GetDateTime(reader.GetOrdinal("DepartureDateTime"));
                                    AriveDets1 = reader.GetDateTime(reader.GetOrdinal("ArrivalDateTime"));
                                    flightID1 = (int)reader["FlightID"];
                                    price1 = (decimal)reader["Price"];
                                    firstCon1 = reader.IsDBNull(reader.GetOrdinal("FirstConFlight")) ? -1 : (int)reader["FirstConFlight"];
                                    SecCon1 = reader.IsDBNull(reader.GetOrdinal("SecondConFlight")) ? -1 : (int)reader["SecondConFlight"];
                                    conCity1 = reader.IsDBNull(reader.GetOrdinal("ConCity")) ? null : (string)reader["ConCity"];
                                    int eachFlightSeat = reader.GetInt32(reader.GetOrdinal("SeatsAvailable"));

                                    if (firstCon1 != -1 && SecCon1 != -1 && conCity1 != null) // flight has connections, can not check the seats yet
                                    {
                                        Console.WriteLine($"Flight ID {flightID1} found from {OrgCity1} at {DepartDets1.ToString()} to {DesCity1} at {AriveDets.ToString()} with a connection at {conCity1} for Price: ${price1}");

                                    }
                                    else
                                    {
                                        if (eachFlightSeat > 0) // only show it if there are seats
                                        {
                                            Console.WriteLine($"Flight ID {flightID1} found from {OrgCity1} at {DepartDets1.ToString()} to {DesCity1} at {AriveDets1.ToString()} for Price: ${price1}");
                                        }
                                    }

                                }
                                reader.Close();

                                // add which flight they want to select based on flight ID
                                while (true)
                                {
                                    Console.WriteLine("Please select a return flight by entering its Flight ID");
                                    if (int.TryParse(Console.ReadLine(), out selectFlight1))
                                    {
                                        break;
                                    }
                                }

                                flightInfoQuery = $"SELECT * FROM Flights WHERE FlightID = @flightId";
                                using (SqlCommand flightInfo = new SqlCommand(flightInfoQuery, sqlConn)) // setting all the varibles to the flight user selects
                                {
                                    flightInfo.Parameters.AddWithValue("@flightID", selectFlight1);
                                    SqlDataReader fInfo = flightInfo.ExecuteReader();
                                    fInfo.Read();
                                    if (!fInfo.HasRows)
                                    {
                                        Console.WriteLine("No flights found with this flight ID please try booking again");
                                        BookFlight(username);
                                        return;
                                    }
                                    // Retrieve the flight information from the reader
                                    OrgCity1 = (string)fInfo["OriginCity"];
                                    DesCity1 = (string)fInfo["DestinationCity"];
                                    DepartDets1 = fInfo.GetDateTime(fInfo.GetOrdinal("DepartureDateTime"));
                                    AriveDets1 = fInfo.GetDateTime(fInfo.GetOrdinal("ArrivalDateTime"));
                                    flightID1 = (int)fInfo["FlightID"];
                                    price1 = (decimal)fInfo["Price"];
                                    seats1 = fInfo.IsDBNull(fInfo.GetOrdinal("SeatsAvailable")) ? -1 : (int)fInfo["SeatsAvailable"];
                                    firstCon1 = fInfo.IsDBNull(fInfo.GetOrdinal("FirstConFlight")) ? -1 : (int)fInfo["FirstConFlight"];
                                    SecCon1 = fInfo.IsDBNull(fInfo.GetOrdinal("SecondConFlight")) ? -1 : (int)fInfo["SecondConFlight"];
                                    fInfo.Close();
                                    if (firstCon1 != -1 && SecCon1 != -1) // flight has connections 
                                    {
                                        Console.WriteLine($"Selected Flight has a Connection:");
                                        string connectionQuery = $"SELECT * FROM FLights WHERE FlightID = (SELECT FirstConFlight FROM Flights WHERE FlightID = @flightId) " +
                                                                    "UNION " +
                                                                    "SELECT * FROM Flights WHERE FlightID = (SELECT SecondConFlight FROM Flights WHERE FlightID = @flightId)";
                                        using (SqlCommand connections = new SqlCommand(connectionQuery, sqlConn))
                                        {
                                            connections.Parameters.AddWithValue("@flightId", flightID1);
                                            SqlDataReader con = connections.ExecuteReader();
                                            if (con.HasRows)
                                            {
                                                while (con.Read())
                                                {
                                                    int conFID1 = (int)con["FlightID"];
                                                    string conOrgCity1 = (string)con["OriginCity"];
                                                    string conDesCity1 = (string)con["DestinationCity"];
                                                    DateTime conDepart1 = con.GetDateTime(con.GetOrdinal("DepartureDateTime"));
                                                    DateTime conArrive1 = con.GetDateTime(con.GetOrdinal("ArrivalDateTime"));
                                                    Console.WriteLine($"Flight ID {conFID1} from {conOrgCity1} at {conDepart1.ToString()} to {conDesCity1} at {conArrive1.ToString()}");
                                                    int conSeats1 = con.GetInt32(con.GetOrdinal("SeatsAvailable"));
                                                    if (conSeats1 <= 0)
                                                    {
                                                        fullConnectedFlight1 = true;
                                                    }
                                                }
                                            }
                                            con.Close();
                                        }

                                    }
                                }
                                addPoints = (int)(price1 * 100) + addPoints;
                                // if there are no seats and its not a connected flight
                                // or if the connected flight is full
                                if ((seats1 <= 0 & firstCon1 == -1 & SecCon1 == -1) || fullConnectedFlight1)
                                {
                                    Console.WriteLine("Flight is Fully Booked");
                                    return;
                                }
                                // end of return flight

                                do
                                {
                                    Console.WriteLine("Please select a Payment Method\n1. Credit Card\n2. Points");
                                    paymentMethod = Console.ReadLine();
                                } while (paymentMethod == null | (paymentMethod != "1" & paymentMethod != "2"));


                                checkPaymentDets = $"SELECT * FROM Users WHERE UserID = @UserId"; // Need this to check if the user had a credit card on file or not, and to see points 
                                using (SqlCommand PointsPayment = new SqlCommand(checkPaymentDets, sqlConn))
                                {
                                    PointsPayment.Parameters.AddWithValue("@UserID", username);
                                    using (SqlDataReader paymenreader = PointsPayment.ExecuteReader())
                                    {
                                        paymenreader.Read();
                                        if (paymenreader.HasRows)
                                        {
                                            bool notEnoughPoints = false;
                                            int points = (int)paymenreader["PointsAvailable"];
                                            CCard = paymenreader.IsDBNull(paymenreader.GetOrdinal("CreditCard")) ? null : (string)paymenreader["CreditCard"];
                                            paymenreader.Close();
                                            if (paymentMethod == "2")
                                            {
                                                if (points >= addPoints)
                                                {
                                                    Console.WriteLine("You have Enough points");
                                                    string? usingPoints = $"UPDATE Users SET PointsAvailable = PointsAvailable - @pointsToReduce,PointsUsed = PointsUsed + @pointsToAdd WHERE UserID = @UserId";
                                                    using (SqlCommand updatePoints = new SqlCommand(usingPoints, sqlConn))
                                                    {
                                                        updatePoints.Parameters.AddWithValue("@pointsToReduce", addPoints);
                                                        updatePoints.Parameters.AddWithValue("@pointsToAdd", addPoints);
                                                        updatePoints.Parameters.AddWithValue("@UserId", username);
                                                        rows = updatePoints.ExecuteNonQuery();
                                                        if (rows == 0)
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
                                            if (notEnoughPoints == true || paymentMethod == "1")
                                            {
                                                string? CCsave = null;
                                                if (CCard != null)
                                                {
                                                    Console.WriteLine("Would you like to use the Card you have saved for your account?\n1.Yes\n2.No");

                                                    do
                                                    {
                                                        CCsave = Console.ReadLine();
                                                        if (CCsave == "2")
                                                        {
                                                            //ask user for new card and see if they want to save it to their account
                                                            do
                                                            {
                                                                Console.WriteLine("Enter new Credit Card");
                                                                newCCard = Console.ReadLine();
                                                            } while (newCCard == null || newCCard.Length != 16);
                                                            string? saveNew;
                                                            do
                                                            {
                                                                Console.WriteLine("Would you like to save this card to your account?");
                                                                Console.WriteLine("1. Yes");
                                                                Console.WriteLine("2. No");
                                                                saveNew = Console.ReadLine();
                                                            } while (saveNew == null | (saveNew != "1" && saveNew != "2"));
                                                            if (saveNew == "1")
                                                            {
                                                                CCardUpdate = $"UPDATE Users SET CreditCard = @CreditCard WHERE UserID = @UserId";
                                                                using (SqlCommand queryUpdateCC = new SqlCommand(CCardUpdate, sqlConn))
                                                                {
                                                                    queryUpdateCC.Parameters.AddWithValue("@CreditCard", newCCard);
                                                                    queryUpdateCC.Parameters.AddWithValue("@UserId", username);
                                                                    rows = queryUpdateCC.ExecuteNonQuery();
                                                                    if (rows == 0)
                                                                    {
                                                                        Console.WriteLine("Unsuccessful Credit Card change.");
                                                                    }
                                                                }
                                                            }
                                                        }
                                                        else if (CCsave == "1") break;
                                                    } while (CCsave == null | (CCsave != "1" && CCsave != "2"));
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
                                                        CCardUpdate = $"UPDATE Users SET CreditCard = @CreditCard WHERE UserID = @UserId";
                                                        using (SqlCommand queryUpdateCC = new SqlCommand(CCardUpdate, sqlConn))
                                                        {
                                                            queryUpdateCC.Parameters.AddWithValue("@CreditCard", newCCard);
                                                            queryUpdateCC.Parameters.AddWithValue("@UserId", username);
                                                            rows = queryUpdateCC.ExecuteNonQuery();
                                                            if (rows == 0)
                                                            {
                                                                Console.WriteLine("Unsuccessful Credit Card change.");
                                                            }
                                                        }
                                                    }
                                                }
                                            }

                                            string subSeat = $"UPDATE Flights SET SeatsAvailable = SeatsAvailable - 1 WHERE FlightID = {flightID}";
                                            if (firstCon != -1 && SecCon != -1)
                                            {
                                                subSeat = $"UPDATE Flights SET SeatsAvailable = SeatsAvailable - 1 WHERE FlightID = {firstCon} " +
                                                          $"UPDATE Flights SET SeatsAvailable = SeatsAvailable - 1 WHERE FlightID = {SecCon} ";
                                            }
                                            using (SqlCommand querySubSeat = new SqlCommand(subSeat, sqlConn))
                                            {
                                                rows = querySubSeat.ExecuteNonQuery();
                                                if (rows == 0)
                                                {
                                                    Console.WriteLine("couldn't subtract # of seats");
                                                }
                                            }
                                            string pointUpdate = $"UPDATE Users SET PointsAvailable = PointsAvailable + @Points WHERE UserID = @UserId";
                                            using (SqlCommand queryUpdatePoints = new SqlCommand(pointUpdate, sqlConn))
                                            {
                                                queryUpdatePoints.Parameters.AddWithValue("@Points", addPoints);
                                                queryUpdatePoints.Parameters.AddWithValue("@UserId", username);
                                                rows = queryUpdatePoints.ExecuteNonQuery();
                                                if (rows == 0)
                                                {
                                                    Console.WriteLine("couldn't add points");
                                                }
                                            }
                                            string Transac = $"INSERT INTO Transactions (AmountCharged, IsCard, UserID, FlightID) OUTPUT INSERTED.TransactionID VALUES ( @AmountCharged, @IsCard, @UserId, @FlightID)";
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
                                                int transactionID = (int)addTransaction.ExecuteScalar();
                                                if (isCard == 1)
                                                {
                                                    string cardCharge = $"INSERT INTO CardCharges (FirstName, LastName, UserID, AmountCharged, CardNumber, TransactionID) OUTPUT INSERTED.TransactionID VALUES ( @firstName, @lastName, @UserId, @AmountCharged, @cardNumber, @transactionID)";
                                                    using (SqlCommand addCardCharge = new SqlCommand(cardCharge, sqlConn))
                                                    {
                                                        addCardCharge.Parameters.AddWithValue("@firstName", CurUser.FirstName);
                                                        addCardCharge.Parameters.AddWithValue("@lastName", CurUser.LastName);
                                                        addCardCharge.Parameters.AddWithValue("@UserId", CurUser.UserID);
                                                        addCardCharge.Parameters.AddWithValue("@AmountCharged", price);
                                                        if (newCCard != null)
                                                        {
                                                            addCardCharge.Parameters.AddWithValue("@cardNumber", newCCard);
                                                        }
                                                        else
                                                        {
                                                            addCardCharge.Parameters.AddWithValue("@cardNumber", CCard);
                                                        }
                                                        addCardCharge.Parameters.AddWithValue("@transactionID", transactionID);
                                                        rows = addCardCharge.ExecuteNonQuery();
                                                        if (rows == 0)
                                                        {
                                                            Console.WriteLine("Couldn't create CardCharge");
                                                        }

                                                    }
                                                }
                                            }
                                            string Transac1 = $"INSERT INTO Transactions (AmountCharged, IsCard, UserID, FlightID) OUTPUT INSERTED.TransactionID VALUES ( @AmountCharged, @IsCard, @UserId, @FlightID)";
                                            using (SqlCommand addTransaction = new SqlCommand(Transac1, sqlConn))
                                            {
                                                addTransaction.Parameters.AddWithValue("@AmountCharged", price1);
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
                                                addTransaction.Parameters.AddWithValue("@FlightID", flightID1);
                                                int transactionID = (int)addTransaction.ExecuteScalar();
                                                if (isCard == 1)
                                                {
                                                    string cardCharge = $"INSERT INTO CardCharges (FirstName, LastName, UserID, AmountCharged, CardNumber, TransactionID) OUTPUT INSERTED.TransactionID VALUES ( @firstName, @lastName, @UserId, @AmountCharged, @cardNumber, @transactionID)";
                                                    using (SqlCommand addCardCharge = new SqlCommand(cardCharge, sqlConn))
                                                    {
                                                        addCardCharge.Parameters.AddWithValue("@firstName", CurUser.FirstName);
                                                        addCardCharge.Parameters.AddWithValue("@lastName", CurUser.LastName);
                                                        addCardCharge.Parameters.AddWithValue("@UserId", CurUser.UserID);
                                                        addCardCharge.Parameters.AddWithValue("@AmountCharged", price1);
                                                        if (newCCard != null)
                                                        {
                                                            addCardCharge.Parameters.AddWithValue("@cardNumber", newCCard);
                                                        }
                                                        else
                                                        {
                                                            addCardCharge.Parameters.AddWithValue("@cardNumber", CCard);
                                                        }
                                                        addCardCharge.Parameters.AddWithValue("@transactionID", transactionID);
                                                        rows = addCardCharge.ExecuteNonQuery();
                                                        if (rows == 0)
                                                        {
                                                            Console.WriteLine("Couldn't create CardCharge");
                                                        }

                                                    }
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
                                                    name = name + " " + (string)recieptmaker["LastName"];
                                                    Console.WriteLine($"Flight booked from {OrgCity} at {DepartDets.ToString()} to {DesCity} at {AriveDets.ToString()} at Price: ${price}");
                                                    Console.WriteLine($"Flight booked from {OrgCity1} at {DepartDets1.ToString()} to {DesCity1} at {AriveDets1.ToString()} at Price: ${price1}");
                                                    if (paymentMethod == "1" || notEnoughPoints == true)
                                                    {
                                                        string card = newCCard == null ? CCard : newCCard;
                                                        Console.WriteLine($"The Payment Method Used was Credit Card: {card}");
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
                }
            }
        }
        public void cancel()
        {
            Console.WriteLine("Please enter the flight ID to be cancelled: ");
            string? flightString = Console.ReadLine();
            int flightiD;
            while (!Int32.TryParse(flightString, out flightiD))
            {
                Console.WriteLine("Please input a Flight ID");
                flightString = Console.ReadLine();
            }
            using (SqlConnection sqlConn = new SqlConnection("Server=34.162.94.248; Database=air3550; Uid=sqlserver; Password=123;"))
            {


                sqlConn.Open();
                string queryString = $"SELECT * FROM Flights WHERE FlightID = '{flightiD}'";
                SqlCommand query = new SqlCommand(queryString, sqlConn);
                SqlDataReader reader = query.ExecuteReader();
                if (reader.Read())
                {
                    DateTime departureTime = (DateTime)reader["DepartureDateTime"];
                    DateTime currentTime = DateTime.Now;
                    reader.Close();
                    if (departureTime.Subtract(currentTime).TotalHours >= 1)
                    {
                        RefundCancelledFlight(flightiD);
                        // Mark flight as refunded in Transactions table            
                        string updateString = $"UPDATE Transactions SET IsRefunded = 1 WHERE FlightID = '{flightiD}' AND UserID = '{CurUser.UserID}'";
                        SqlCommand update = new SqlCommand(updateString, sqlConn);
                        update.ExecuteNonQuery();
                        string updateSeatsQueryString = $"UPDATE Flights SET SeatsAvailable = SeatsAvailable + 1 WHERE FlightID = {flightiD}";
                        SqlCommand updateSeatsQuery = new SqlCommand(updateSeatsQueryString, sqlConn);
                        updateSeatsQuery.ExecuteNonQuery();
                        reader.Close();
                    }
                    else
                    {
                        Console.WriteLine("Error: Flight cannot be cancelled as it is less than 1 hour to the departure time.");
                    }
                }
                sqlConn.Close();
            }
        }
        public void RefundCancelledFlight(int flightID)
        {
            using (SqlConnection sqlConn = new SqlConnection("Server=34.162.94.248; Database=air3550; Uid=sqlserver; Password=123;"))
            {
                sqlConn.Open();
                // Get the user ID and payment amount for the transaction associated with this flight
                string transactionQueryString = $"SELECT UserID, AmountCharged, IsRefunded, IsCard, TransactionID FROM Transactions WHERE FlightID = {flightID}";
                SqlCommand transactionQuery = new SqlCommand(transactionQueryString, sqlConn);
                SqlDataReader transactionReader = transactionQuery.ExecuteReader();
                transactionReader.Read();
                if (transactionReader.HasRows)
                {
                    
                    decimal paymentAmount = (decimal)transactionReader["AmountCharged"];
                    int transactionID = (int)transactionReader["TransactionID"];
                    bool isRefunded = transactionReader.GetBoolean(2);
                    bool isCard = transactionReader.GetBoolean(3);
                    transactionReader.Close();
                    if (paymentAmount != -1 && isRefunded != true && transactionID != -1)
                    {
                        // Refund the payment to the user
                        if (isCard)
                        {
                            // deletes the Card Charge from our table, this is how to "refund card"
                            string refundCardQueryString = $"DELETE FROM CardCharges WHERE UserID = {CurUser.UserID} AND TransactionID = {transactionID}";
                            SqlCommand refundCardQuery = new SqlCommand(refundCardQueryString, sqlConn);
                            refundCardQuery.ExecuteNonQuery();
                            Console.WriteLine($"Flight {flightID} has been canceled and a refund of ${paymentAmount} has been issued to user {CurUser.UserID} at original payment");
                        }
                        else
                        {
                            string refundQueryString = $"UPDATE Users SET PointsAvailable = PointsAvailable + {paymentAmount} WHERE UserID = {CurUser.UserID}";
                            SqlCommand refundQuery = new SqlCommand(refundQueryString, sqlConn);
                            refundQuery.ExecuteNonQuery();
                            Console.WriteLine($"Flight {flightID} has been canceled and a refund of ${paymentAmount}, points has been issued to user {CurUser.UserID}'s account\n");
                        }
                        // Mark the transaction as refunded
                        string markAsRefundedQueryString = $"UPDATE Transactions SET IsRefunded = 1 WHERE FlightID = '{flightID}' AND UserID = '{CurUser.UserID}'";
                        SqlCommand markAsRefundedQuery = new SqlCommand(markAsRefundedQueryString, sqlConn);
                        markAsRefundedQuery.ExecuteNonQuery();
                    }
                    else if (isRefunded == true)
                    {
                        Console.WriteLine($"The transaction associated with flight {flightID} has already been canceled and refunded.\n");
                    }
                }
                else
                {
                        Console.WriteLine($"No transaction found for flight {flightID}\n");
                }
                transactionReader.Close();
                sqlConn.Close();
            }
        }
        public void DisplayFlightHistory(string username)
        {
            Console.WriteLine("Flight History: ");
            Console.WriteLine("...............");
            bool hasFlightHistory = false;
            using (SqlConnection sqlConn = new SqlConnection("Server=34.162.94.248; Database=air3550; Uid=sqlserver; Password=123;"))
            {

                sqlConn.Open();
                string queryString = $"SELECT Flights.FlightID, Flights.FlightNumber, Flights.OriginCity, Flights.DestinationCity, Flights.DepartureDateTime, Flights.ArrivalDateTime, Transactions.IsRefunded " +
                                    $"FROM Transactions " +
                                    $"JOIN Flights ON Transactions.FlightID = Flights.FlightID " +
                                    $"WHERE Transactions.UserID = {username} AND Flights.ArrivalDateTime < \'{new SqlDateTime(DateTime.Now)}\'";
                SqlCommand query = new SqlCommand(queryString, sqlConn);
                using (SqlDataReader reader = query.ExecuteReader())
                    while (reader.Read())
                    {
                        hasFlightHistory = true;
                        Console.WriteLine($"Flight ID: {reader["FlightID"]}");
                        Console.WriteLine($"Flight Number: {reader["FlightNumber"]}");
                        Console.WriteLine($"Origin Airport: {reader["OriginCity"]}");
                        Console.WriteLine($"Destination Airport: {reader["DestinationCity"]}");
                        Console.WriteLine($"Departure Date Time: {reader["DepartureDateTime"]}");
                        Console.WriteLine($"Arrival Date Time: {reader["ArrivalDateTime"]}");

                        if (reader["IsRefunded"] != DBNull.Value && (bool)reader["IsRefunded"])
                        {
                            Console.WriteLine("This flight is INACTIVE. A refund has been processed for this flight\n");
                        }
                    }
                sqlConn.Close();
            }
            if (!hasFlightHistory)
            {
                Console.WriteLine("No flight history found for the given user.");
            }
        }
        public void DisplayUpcomingFlights(string username)
        {
            Console.WriteLine("Flight History: ");
            Console.WriteLine("...............");
            bool hasFlightHistory = false;
            using (SqlConnection sqlConn = new SqlConnection("Server=34.162.94.248; Database=air3550; Uid=sqlserver; Password=123;"))
            {

                sqlConn.Open();
                string queryString = $"SELECT Flights.FlightID, Flights.FlightNumber, Flights.OriginCity, Flights.DestinationCity, Flights.DepartureDateTime, Flights.ArrivalDateTime, Transactions.IsRefunded " +
                                    $"FROM Transactions " +
                                    $"JOIN Flights ON Transactions.FlightID = Flights.FlightID " +
                                    $"WHERE Transactions.UserID = {username} AND Flights.DepartureDateTime > \'{new SqlDateTime(DateTime.Now)}\'";
                SqlCommand query = new SqlCommand(queryString, sqlConn);
                using (SqlDataReader reader = query.ExecuteReader())
                    while (reader.Read())
                    {
                        hasFlightHistory = true;
                        Console.WriteLine($"Flight ID: {reader["FlightID"]}");
                        Console.WriteLine($"Flight Number: {reader["FlightNumber"]}");
                        Console.WriteLine($"Origin Airport: {reader["OriginCity"]}");
                        Console.WriteLine($"Destination Airport: {reader["DestinationCity"]}");
                        Console.WriteLine($"Departure Date Time: {reader["DepartureDateTime"]}");
                        Console.WriteLine($"Arrival Date Time: {reader["ArrivalDateTime"]}");

                        if (reader["IsRefunded"] != DBNull.Value && (bool)reader["IsRefunded"])
                        {
                            Console.WriteLine("This flight is INACTIVE. A refund has been processed for this flight\n");
                        }
                    }
                sqlConn.Close();
            }
            if (!hasFlightHistory)
            {
                Console.WriteLine("No flight history found for the given user.");
            }
        }
        public void printBoardingPass(string username)
        {
            using (SqlConnection sqlConn = new SqlConnection("Server=34.162.94.248; Database=air3550; Uid=sqlserver; Password=123;"))
            {

                sqlConn.Open();
                string boardingPassavail = $"SELECT users.FirstName, users.LastName, a.FlightNumber, a.OriginCity, a.DestinationCity, a.DepartureDateTime, a.ArrivalDateTime, a.ConCity, a.FirstConFlight,a.SecondConFlight," +
                                           $"c1.FlightNumber AS FirstConFlightNumber, c1.OriginCity AS FirstConOriginCity, c1.DestinationCity AS FirstConDestCity, c1.DepartureDateTime AS FirstConFlightDepDateTime, c1.ArrivalDateTime AS FirstConFlightArrDateTime," +
                                           $"c2.FlightNumber AS SecondConFlightNumber, c2.OriginCity AS SecondConOriginCity, c2.DestinationCity AS SecondConDestCity, c2.DepartureDateTime AS SecondConFlightDepDateTime, c2.ArrivalDateTime AS SecondConFlightArrDateTime "+
                                           $"FROM Transactions " +
                                           $"INNER JOIN flights AS a ON transactions.FlightID = a.FlightID " +
                                           $"INNER JOIN users ON transactions.UserID = users.UserID " +
                                           $"LEFT JOIN flights AS c1 ON a.FirstConFlight = c1.FlightID " +
                                           $"LEFT JOIN flights AS c2 ON a.SecondConFlight = c2.FlightID " +
                                           $"WHERE a.DepartureDateTime  <= DATEADD(HOUR, 24, GETDATE()) AND users.UserID = {username}";
                SqlCommand BPQuery = new SqlCommand(boardingPassavail, sqlConn);
                using (SqlDataReader reader = BPQuery.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            string Connection = reader.IsDBNull(reader.GetOrdinal("ConCity")) ? null : (string)reader["ConCity"];
                            if (Connection == null)
                            {
                                Console.WriteLine("-------------------------------------------------------------------------------------------------------------------------");
                                Console.WriteLine($"Name: {reader["LastName"]}, {reader["FirstName"]}    Account Number: {username}");
                                Console.WriteLine($"Flight Number: {reader["FlightNumber"]}  {reader["OriginCity"]} to {reader["DestinationCity"]}");
                                Console.WriteLine($"Departure Date & Time from {reader["OriginCity"]} : {reader["DepartureDateTime"]} to Arrival Date & Time at {reader["DestinationCity"]} : {reader["ArrivalDateTime"]}");
                                Console.WriteLine("-------------------------------------------------------------------------------------------------------------------------");
                            }
                            else
                            {
                                Console.WriteLine("-------------------------------------------------------------------------------------------------------------------------");
                                Console.WriteLine($"Name: {reader["LastName"]}, {reader["FirstName"]}    Account Number: {username}");
                                Console.WriteLine($"Flight Number: {reader["FlightNumber"]}  {reader["OriginCity"]} to {reader["DestinationCity"]} with a Connection at Aiport : {reader["ConCity"]}");
                                Console.WriteLine("             -------------------------------------------------------------------");
                                Console.WriteLine($"Flight Number: {reader["FirstConFlightNumber"]}  {reader["FirstConOriginCity"]} to {reader["FirstConDestCity"]}");
                                Console.WriteLine($"Departure Date & Time from {reader["FirstConOriginCity"]} : {reader["FirstConFlightDepDateTime"]} to Arrival Date & Time at {reader["FirstConDestCity"]} : {reader["FirstConFlightArrDateTime"]}");
                                Console.WriteLine("             -------------------------------------------------------------------");
                                Console.WriteLine($"Flight Number: {reader["SecondConFlightNumber"]}  {reader["SecondConOriginCity"]} to {reader["SecondConDestCity"]}");
                                Console.WriteLine($"Departure Date & Time from {reader["SecondConOriginCity"]} : {reader["SecondConFlightDepDateTime"]} to Arrival Date & Time at {reader["SecondConDestCity"]} : {reader["SecondConFlightArrDateTime"]}");
                                Console.WriteLine("-------------------------------------------------------------------------------------------------------------------------");
                            }
                            
                        }
                    }
                    else
                    {
                        Console.WriteLine("No Boarding Pass is available to print at the moment.\n");

                    }

                    sqlConn.Close();
                }
            }
        }
    }
}