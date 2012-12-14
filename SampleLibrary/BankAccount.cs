using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SampleLibrary
{
    public class BankAccount
    {
        private double _balance;
        public BankAccount(double balance)
        {
            _balance = balance;
        }
        public void Deposit(double amount)
        {
            Console.WriteLine("{0} Deposited");
            _balance += amount;
        }
    }
}
