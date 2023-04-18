using System.Security.Cryptography;
using System;
using System.Text;

namespace Air3550;
class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        while (true)
        {
            Console.WriteLine("Input a number to continue");
            Console.WriteLine("1. Login");
            Console.WriteLine("2. Quit");
            string? input = Console.ReadLine();
            if(input == null |( input != "1" & input != "2"))
            {
                Console.WriteLine("Please input a correct input");
                continue;
            }
            switch (input)
            {
                case "1":
                    Login();
                    break;
                case "2":
                    return;
            }
        }
    }
    static void Login()
    {
        // get the username and password from the inputs
        Console.WriteLine("Input Username");
        string? username = Console.ReadLine();
        Console.WriteLine("Input Password");
        string? password = Console.ReadLine();
        // hash the password using SHA-512
        byte[] hashByte;
        var data = Encoding.UTF8.GetBytes(password ?? "");
        using (SHA512 shaM = new SHA512Managed())
        {
            hashByte = shaM.ComputeHash(data);
        }
        // Have the byte array of the hash, convert it to a string, replace the - with a blank after
        string hash = BitConverter.ToString(hashByte);
        hash = hash.Replace("-", "");
        //Console.WriteLine(hash);

        // check against the database for the user with that username
        tryLogin(username, hash);
        return;
    }
}
