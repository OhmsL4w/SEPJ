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
            string? OriginAirport, DestinationAirport, DDate, ADate, checkFlightDets, paymentMethod, checkPaymentDets, CCardUpdate, CCard = null, newCCard;

            int rows = 0, isCard = 0, addPoints = 0;
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
                string OrgCity = null, DesCity = null,conCity =null;
                int flightID = 0,firstCon = -1, SecCon = -1,selectFlight = -1, seats = -1;
                DateTime DepartDets = default(DateTime);
                DateTime AriveDets = default(DateTime);
                decimal price = 0;

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

                        addPoints = (int)price * 100;
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
                                        }
                                    }
                                    con.Close();
                                }

                            }
                        }
                        if (seats <= 0)
                        {
                            Console.WriteLine("Flight is Fully Booked");
                            return;
                        }
                        

                        do{
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
                                        CCard = paymenreader.IsDBNull(paymenreader.GetOrdinal("CreditCard")) ? null : (string)paymenreader["CreditCard"];
                                        string CCsave = null;
                                        if (CCard != null)
                                        {
                                            Console.WriteLine("Would you like to use the Card you have saved for your account?\n1.Y\n2.N");
                                            
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
                                                }
                                                else if (CCsave == "1")break;                                                
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
                                                paymenreader.Close();
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
                                    paymenreader.Close();

                                    string subSeat;
                                    if (firstCon != -1 && SecCon != -1)
                                    {
                                        subSeat = $"UPDATE Flights SET SeatsAvailable = SeatsAvailable - 1 WHERE FlightID = {flightID}";
                                    }
                                    else
                                    {
                                        subSeat = $"UPDATE Flights SET SeatsAvailable = SeatsAvailable -1 where FlightID = {firstCon} " +
                                                  $"UPDATE Flights SET SeatsAvailable = SeatsAvailable -1 where FlightID = {SecCon} ";
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
                                    string Transac = $"INSERT INTO Transactions (AmountCharged, IsCard, UserID, FlightID) VALUES ( @AmountCharged, @IsCard, @UserId, @FlightID)";
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
                                        if (rows == 0)
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
                                            name = name + " " + (string)recieptmaker["LastName"];                                           
                                            Console.WriteLine($"Flight booked from {OrgCity} at {DepartDets.ToString()} to {DesCity} at {AriveDets.ToString()} at Price: ${price}");
                                            if (paymentMethod == "1" || notEnoughPoints == true)
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