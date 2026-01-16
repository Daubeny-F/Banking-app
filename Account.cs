using System;
using System.Collections.Generic;

namespace BankingSystem
{
    public class Account
    {
        public string AccountHolder { get; set; }
        public AccountType AccountType { get; set; }
        public AccountCurrency AccountCurrency { get; set; }
        public string Iban { get; set; }
        public DateTime AccountOpenDate { get; set; }
        public DateTime AccountCloseDate { get; set; }
        public decimal Amount { get; set; }

        // Fee constants
        private const decimal AccountFeeCurrencyChange = 0.02m; // 2% fee

        public Account()
        {
            AccountOpenDate = DateTime.Now;
            AccountCloseDate = new DateTime(2999, 1, 1);
        }

        public Account(string accountHolder, AccountType accountType, AccountCurrency accountCurrency, string iban, decimal initialAmount = 0)
        {
            AccountHolder = accountHolder;
            AccountType = accountType;
            AccountCurrency = accountCurrency;
            Iban = iban;
            Amount = initialAmount;
            AccountOpenDate = DateTime.Now;
            AccountCloseDate = new DateTime(2999, 1, 1);
        }

        public bool IsActive()
        {
            return AccountCloseDate.Year == 2999;
        }

        public bool Deposit(decimal amount)
        {
            if (amount <= 0)
            {
                throw new InvalidOperationException("Deposit amount must be positive.");
            }

            Amount += amount;
            return true;
        }

        public bool Withdraw(decimal amount)
        {
            if (amount <= 0)
            {
                throw new InvalidOperationException("Withdrawal amount must be positive.");
            }

            if (Amount < amount)
            {
                throw new InvalidOperationException("Insufficient funds.");
            }

            Amount -= amount;
            return true;
        }

        public bool ChangeAccountCurrency(AccountCurrency newCurrency)
        {
            if (AccountCurrency == newCurrency)
            {
                throw new InvalidOperationException("Account is already in this currency.");
            }

            decimal fee = Amount * AccountFeeCurrencyChange;
            if (Amount < fee)
            {
                throw new InvalidOperationException("Insufficient funds to cover currency change fee.");
            }

            decimal amountAfterFee = Amount - fee;
            decimal convertedAmount = ConvertCurrency(amountAfterFee, AccountCurrency, newCurrency);

            Amount = convertedAmount;
            AccountCurrency = newCurrency;
            
            return true;
        }

        private decimal ConvertCurrency(decimal amount, AccountCurrency from, AccountCurrency to)
        {
            // Simple conversion rates (using RON as base)
            Dictionary<AccountCurrency, decimal> toRON = new Dictionary<AccountCurrency, decimal>
            {
                { AccountCurrency.RON, 1.0m },
                { AccountCurrency.EUR, 4.95m },
                { AccountCurrency.USD, 4.50m },
                { AccountCurrency.GBP, 5.80m }
            };

            decimal inRON = amount * toRON[from];
            return inRON / toRON[to];
        }

        public override string ToString()
        {
            return $"\n--- Account Details ---\n" +
                   $"IBAN: {Iban}\n" +
                   $"Holder: {AccountHolder}\n" +
                   $"Type: {AccountType}\n" +
                   $"Currency: {AccountCurrency}\n" +
                   $"Balance: {Amount:F2} {AccountCurrency}\n" +
                   $"Open Date: {AccountOpenDate:yyyy-MM-dd}\n" +
                   $"Status: {(IsActive() ? "Active" : "Closed")}\n" +
                   $"----------------------";
        }
    }
}
