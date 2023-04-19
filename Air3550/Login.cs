using Air3550;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Security.Cryptography;
using System.Text;


public class Login
{
    public static void LoginMethod()
    {
        // get the username and password from the inputs
        Console.WriteLine("Input Username");
        string? username = Console.ReadLine();
        Console.WriteLine("Input Password");
        string? password = Console.ReadLine();
        if(username == null || password == null)
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
        if(!loginSuccess)
        {
            Console.WriteLine("Invalid Username or Password!");
            Console.WriteLine("1. Try again");
            Console.WriteLine("2. Back");
            string? input = Console.ReadLine();
            switch(input)
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
        while(birthDay == null)
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
        using (SqlConnection sqlConn = new SqlConnection("Data Source=(local);Database=Air3550;Integrated Security=true;"))
        {
            sqlConn.Open();
            while(true)
            {
                int i = 0;
                // generate a random userID, then check if there is already one in the database, inefficient but easier
                userID = r.Next(100000, 999999);
                string queryStringChecking = @"SELECT TOP 1 UserID FROM Users WHERE Users.UserID =" + userID;
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
            if(rows > 0)
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
        using (SqlConnection sqlConn = new SqlConnection("Data Source=(local);Database=Air3550;Integrated Security=true;"))
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
                    if(usersHash == hash)
                    {
                        return true;
                    }
                }
            }
            sqlConn.Close();
        }
        return false;
    }
}
