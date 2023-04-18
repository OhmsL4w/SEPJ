using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;


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
        //LoggedIn(username); // give the username to the function to find the user again
        return;
    }

    public static void CreateAccount()
    {
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
