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
            Console.WriteLine("2. Create Account");
            Console.WriteLine("Q. Quit");
            string? input = Console.ReadLine();
            if(input == null |( input != "1" & input != "2" & input != "Q"))
            {
                Console.WriteLine("Please input a correct input");
                continue;
            }
            switch (input)
            {
                case "1":
                    Login.LoginMethod();
                    break;
                case "Q":
                    return;
                case "2":
                    Login.CreateAccount();
                    break;
            }
        }
    }
}
