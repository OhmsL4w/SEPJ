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

                        addPoints = (int)(price * 100);
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
                                                    if(saveNew == "1")
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
                                        if(isCard == 1)
                                        {
                                            string cardCharge = $"INSERT INTO CardCharges (FirstName, LastName, UserID, AmountCharged, CardNumber, TransactionID) OUTPUT INSERTED.TransactionID VALUES ( @firstName, @lastName, @UserId, @AmountCharged, @cardNumber, @transactionID)";
                                            using (SqlCommand addCardCharge = new SqlCommand(cardCharge, sqlConn))
                                            {
                                                addCardCharge.Parameters.AddWithValue("@firstName", CurUser.FirstName);
                                                addCardCharge.Parameters.AddWithValue("@lastName", CurUser.LastName);
                                                addCardCharge.Parameters.AddWithValue("@UserId", CurUser.UserID);
                                                addCardCharge.Parameters.AddWithValue("@AmountCharged", price);
                                                if(newCCard != null)
                                                {
                                                    addCardCharge.Parameters.AddWithValue("@cardNumber", newCCard);
                                                }else
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
                decimal paymentAmount = (int)transactionReader["AmountCharged"];
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
                else
                {
                    Console.WriteLine($"No transaction found for flight {flightID}\n");
                }
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
                string queryString = $"SELECT Flights.FlightID, Flights.OriginCity, Flights.DestinationCity, Flights.DepartureDateTime, Flights.ArrivalDateTime, Transactions.IsRefunded " +
                                    $"FROM Transactions " +
                                    $"JOIN Flights ON Transactions.FlightID = Flights.FlightID " +
                                    $"WHERE Transactions.UserID = {username} AND Flights.ArrivalDateTime < \'{new SqlDateTime(DateTime.Now)}\'";
                SqlCommand query = new SqlCommand(queryString, sqlConn);
                using (SqlDataReader reader = query.ExecuteReader())
                    while (reader.Read())
                    {
                        hasFlightHistory = true;
                        Console.WriteLine($"Flight Number: {reader["FlightID"]}");
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
                string queryString = $"SELECT Flights.FlightID, Flights.OriginCity, Flights.DestinationCity, Flights.DepartureDateTime, Flights.ArrivalDateTime, Transactions.IsRefunded " +
                                    $"FROM Transactions " +
                                    $"JOIN Flights ON Transactions.FlightID = Flights.FlightID " +
                                    $"WHERE Transactions.UserID = {username} AND Flights.DepartureDateTime > \'{new SqlDateTime(DateTime.Now)}\'";
                SqlCommand query = new SqlCommand(queryString, sqlConn);
                using (SqlDataReader reader = query.ExecuteReader())
                    while (reader.Read())
                    {
                        hasFlightHistory = true;
                        Console.WriteLine($"Flight Number: {reader["FlightID"]}");
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
    }

}