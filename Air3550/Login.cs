using Air3550;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;
using System.Text;
using System.Transactions;


public class Login
{
    public static void LoginMethod()
    {
        // get the username and password from the inputs
        Console.WriteLine("Input Username");
        string? username = Console.ReadLine();
        Console.WriteLine("Input Password");
        string? password = Console.ReadLine();
        if (username == null || password == null)
        {
            Console.WriteLine("Please enter in a correct Username and Password");
            // retry it
            LoginMethod();
            return;
        }
        // hash the password using SHA-512
        byte[] hashByte;
        var data = Encoding.UTF8.GetBytes(password);
        using (SHA512 shaM = new SHA512Managed())
        {
            hashByte = shaM.ComputeHash(data);
        }
        // Have the byte array of the hash, convert it to a string, replace the - with a blank after
        string hash = BitConverter.ToString(hashByte);
        hash = hash.Replace("-", "");
        //Console.WriteLine(hash);

        // check against the database for the user with that username
        bool loginSuccess = TryLogin(username, hash);
        if (!loginSuccess)
        {
            Console.WriteLine("Invalid Username or Password!");
            Console.WriteLine("1. Try again");
            Console.WriteLine("2. Back");
            string? input = Console.ReadLine();
            switch (input)
            {
                case "1":
                    LoginMethod();
                    return;
                case "2":
                    return;
                default:
                    return;
            }
        }
        LoggedIn logIn = new LoggedIn(username); // give the username to the function to find the user again
        logIn.LoggedInLoop();
        return;
    }

    public static void CreateAccount()
    {
        Console.WriteLine("Please enter Account information");
        Console.WriteLine("First Name");
        string? firstName = Console.ReadLine();
        while (firstName == null)
        {
            Console.WriteLine("Please enter a name\nFirst Name");
            firstName = Console.ReadLine();
        }
        Console.WriteLine("Last Name");
        string? lastName = Console.ReadLine();
        while (lastName == null)
        {
            Console.WriteLine("Please enter a name\nLast Name");
            lastName = Console.ReadLine();
        }
        Console.WriteLine("Date of Birth (mm/dd/yyyy)");
        string? birthDay = Console.ReadLine();
        while (birthDay == null)
        {
            Console.WriteLine("Please enter a correct date\nDate of Birth (mm/dd/yyyy)");
            birthDay = Console.ReadLine();
        }
        // convert that into a sql Date
        // first got to a date time with the format
        DateTime bdayDateTime = DateTime.Parse(birthDay);
        // using the dateTime, create the SqlDateTime
        SqlDateTime sqlBdayDateTime = new SqlDateTime(bdayDateTime.Year, bdayDateTime.Month, bdayDateTime.Day);
        //Console.WriteLine($"{bdayDateTime.Year}/{bdayDateTime.Month}/{bdayDateTime.Day}");
        Console.WriteLine("Street Address");
        string? address = Console.ReadLine();
        while (address == null)
        {
            Console.WriteLine("Please enter an address\nAddress");
            address = Console.ReadLine();
        }
        Console.WriteLine("Phone Number (xxx-xxx-xxxx)");
        string? phone = Console.ReadLine();
        while (phone == null)
        {
            Console.WriteLine("Please enter a correct phone number\nPhone Number");
            phone = Console.ReadLine();
        }
        Console.WriteLine("Password");
        string? password = Console.ReadLine();
        while (password == null)
        {
            Console.WriteLine("Please enter a password\nPassword");
            password = Console.ReadLine();
        }
        // encrypt immediately
        byte[] hashByte;
        var data = Encoding.UTF8.GetBytes(password);
        using (SHA512 shaM = new SHA512Managed())
        {
            hashByte = shaM.ComputeHash(data);
        }
        // Have the byte array of the hash, convert it to a string, replace the - with a blank after
        string hashedPassword = BitConverter.ToString(hashByte);
        hashedPassword = hashedPassword.Replace("-", "");
        //Console.WriteLine(hash);
        Random r = new Random();
        int userID;
        using (SqlConnection sqlConn = new SqlConnection("Server=34.162.94.248; Database=air3550; Uid=sqlserver; Password=123;"))
        {
            sqlConn.Open();
            while (true)
            {
                int i = 0;
                // generate a random userID, then check if there is already one in the database, inefficient but easier
                userID = r.Next(100000, 999999);
                string queryStringChecking = $"SELECT TOP 1 UserID FROM Users WHERE Users.UserID = {userID}";
                SqlCommand query = new SqlCommand(queryStringChecking, sqlConn);
                using (SqlDataReader reader = query.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        // there is already a user with this ID
                        i++;
                    }
                }

                if (i == 0) // there was no results
                {
                    break;
                }
            }
            // have all the information, now input it into the database
            string queryStringInsert = $"INSERT INTO Users (UserID, IsManager, IsEngineer, Password, FirstName, LastName, Address, PointsAvailable, Phone, Birthday, PointsUsed) " +
                $"VALUES ({userID}, 0, 0, \'{hashedPassword}\', \'{firstName}\', \'{lastName}\', \'{address}\', 0, \'{phone}\', \'{sqlBdayDateTime}\', 0)";
            SqlCommand queryInsert = new SqlCommand(queryStringInsert, sqlConn);
            int rows = queryInsert.ExecuteNonQuery();
            if (rows > 0)
            {
                Console.WriteLine($"Successfully created user! UserID:{userID}");
            }
            sqlConn.Close();
        }
        return;
    }

    public static bool TryLogin(string username, string hash)
    {
        // open the database
        // "jdbc:sqlserver://localhost;integratedSecurity=true;encrypt=false"
        using (SqlConnection sqlConn = new SqlConnection("Server=34.162.94.248; Database=air3550; Uid=sqlserver; Password=123;"))
        {
            sqlConn.Open();
            string queryString = "SELECT TOP 1 Password FROM Users WHERE Users.UserID =" + "\'" + username + "\'";
            SqlCommand query = new SqlCommand(queryString, sqlConn);
            using (SqlDataReader reader = query.ExecuteReader())
            {
                while (reader.Read())
                {
                    // get the hashed password saved in the database and check in against the inputted password
                    string usersHash = reader.GetString(0);
                    if (usersHash == hash)
                    {
                        return true;
                    }
                }
            }
            sqlConn.Close();
        }
        return false;
    }
    public static void ChangeAccountSettings(string username)
    {

        Console.WriteLine("Select an option for your Account to change");
        Console.WriteLine("1. Password");
        Console.WriteLine("2. Phone Number");
        Console.WriteLine("3. Address");
        Console.WriteLine("4. Credit Card");
        Console.WriteLine("Q. Quit");

        string? input = Console.ReadLine();
        if (input == null | (input != "1" & input != "2" & input != "3" & input != "4" & input != "Q"))
        {
            Console.WriteLine("Please input a correct input");
            ChangeAccountSettings(username);
        }
        string? password, PNumber, Address, CCard, queryStringUpdate;
        int rows;

        // open the database
        // "jdbc:sqlserver://localhost;integratedSecurity=true;encrypt=false"
        using (SqlConnection sqlConn = new SqlConnection("Server=34.162.94.248; Database=air3550; Uid=sqlserver; Password=123;"))
        {
            sqlConn.Open();
            switch (input)
            {
                case "1": // Changing Password
                    do
                    {
                        Console.WriteLine("Please enter a password\nPassword");
                        password = Console.ReadLine();
                    } while (password == null);
                    //using Colbys password hashing method to hash and store the new password 
                    byte[] hashByte;
                    var data = Encoding.UTF8.GetBytes(password);
                    using (SHA512 shaM = new SHA512Managed())
                    {
                        hashByte = shaM.ComputeHash(data);
                    }
                    // Have the byte array of the hash, convert it to a string, replace the - with a blank after
                    string hashedPassword = BitConverter.ToString(hashByte);

                    hashedPassword = hashedPassword.Replace("-", "");
                    queryStringUpdate = $"UPDATE Users SET Password = @Password WHERE Users.UserID = @UserId";
                    // updating the password
                    using (SqlCommand queryUpdatePass = new SqlCommand(queryStringUpdate, sqlConn))
                    {
                        queryUpdatePass.Parameters.AddWithValue("@Password", hashedPassword);
                        queryUpdatePass.Parameters.AddWithValue("@UserId", username);
                        rows = queryUpdatePass.ExecuteNonQuery();
                        if (rows > 0)
                        {
                            Console.WriteLine("Successfully changed password ");
                        }
                        else
                        {
                            Console.WriteLine("Unsuccessful password change.");
                        }
                    }
                    break;
                case "2": // Changing Phone Number

                    do
                    {
                        Console.WriteLine("Enter new Phone Number(XXX-XXX-XXXX)");
                        PNumber = Console.ReadLine();
                    } while (PNumber == null);

                    queryStringUpdate = $"UPDATE Users SET Phone = @Phone WHERE Users.UserID = @UserId";
                    using (SqlCommand queryUpdatePhon = new SqlCommand(queryStringUpdate, sqlConn))
                    {
                        queryUpdatePhon.Parameters.AddWithValue("@Phone", PNumber);
                        queryUpdatePhon.Parameters.AddWithValue("@UserId", username);
                        rows = queryUpdatePhon.ExecuteNonQuery();
                        if (rows > 0)
                        {
                            Console.WriteLine("Successfully changed Phone");
                        }
                        else
                        {
                            Console.WriteLine("Unsuccessful Phone change.");
                        }
                    }
                    break;
                case "3": // Changing Address
                    do
                    {
                        Console.WriteLine("Enter new Address");
                        Address = Console.ReadLine();
                    } while (Address == null);
                    queryStringUpdate = $"UPDATE Users SET Address = @Address WHERE Users.UserID = @UserId";

                    using (SqlCommand queryUpdateAddr = new SqlCommand(queryStringUpdate, sqlConn))
                    {
                        queryUpdateAddr.Parameters.AddWithValue("@Address", Address);
                        queryUpdateAddr.Parameters.AddWithValue("@UserId", username);
                        rows = queryUpdateAddr.ExecuteNonQuery();
                        if (rows > 0)
                        {
                            Console.WriteLine("Successfully changed Address ");
                        }
                        else
                        {
                            Console.WriteLine("Unsuccessful Address change.");
                        }
                    }
                    break;
                case "4": // Changing Credit Card
                    do
                    {
                        Console.WriteLine("Enter new Credit Card");
                        CCard = Console.ReadLine();
                    } while (CCard == null || CCard.Length != 16);
                    queryStringUpdate = $"UPDATE Users SET CreditCard = @CreditCard WHERE Users.UserID = @UserId";
                    using (SqlCommand queryUpdateCC = new SqlCommand(queryStringUpdate, sqlConn))
                    {
                        queryUpdateCC.Parameters.AddWithValue("@CreditCard", CCard);
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
                    break;
                case "Q":// Quit
                    return;
            }

            sqlConn.Close();
        }
    }
}
