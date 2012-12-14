using System;

namespace SampleLibrary
{
    public class Employee
    {
        private readonly BankAccount _checkingAccount = new BankAccount(42);
        public void Pay(int wage)
        {
            var currentAmount = wage;
            _checkingAccount.Deposit(currentAmount);

            Console.WriteLine("Paid {0} units", wage);
        }

        public static void SayName(string s)
        {
            Console.WriteLine("My name is: " + s);
        }

        public static void Wave(string s)
        {
            Console.WriteLine("Waving: " + s);
        }
    }
}