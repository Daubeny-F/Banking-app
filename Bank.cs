using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BankingSystem
{
    public class Bank
    {
        public string BankName { get; set; }
        public string SwiftAccount { get; set; }
        public BankLocation BankLocation { get; set; }
        public BankCountry BankCountry { get; set; }
        public decimal BankBalance { get; set; }
        public List<Account> Accounts { get; set; }

        [JsonIgnore]
        public Dictionary<string, List<Transaction>> TransactionHistory { get; set; }

        // Fee constants
        private const decimal SameBankTransferFee = 0.01m; // 1%
        private const decimal DifferentBankTransferFee = 0.03m; // 3%

        private Dictionary<AccountType, decimal> AccountTypeFees = new Dictionary<AccountType, decimal>
        {
            { AccountType.Person, 0.005m },    // 0.5%
            { AccountType.Company, 0.01m },    // 1%
            { AccountType.Special, 0.0m }      // 0%
        };

        public Bank()
        {
            Accounts = new List<Account>();
            TransactionHistory = new Dictionary<string, List<Transaction>>();
            BankBalance = 0;
        }

        public Bank(string bankName, string swiftAccount, BankLocation location, BankCountry country)
        {
            BankName = bankName;
            SwiftAccount = swiftAccount;
            BankLocation = location;
            BankCountry = country;
            Accounts = new List<Account>();
            TransactionHistory = new Dictionary<string, List<Transaction>>();
            BankBalance = 0;
        }

        private Account GetAccount(string iban)
        {
            return Accounts.FirstOrDefault(a => a.Iban == iban && a.IsActive());
        }

        public bool OpenAccount(string accountHolder, AccountType accountType, AccountCurrency currency, decimal initialDeposit = 0)
        {
            string iban = GenerateIban();
            Account newAccount = new Account(accountHolder, accountType, currency, iban, initialDeposit);
            Accounts.Add(newAccount);
            TransactionHistory[iban] = new List<Transaction>();

            AddTransaction(iban, "ACCOUNT_OPEN", initialDeposit, currency, $"Account opened with initial deposit");

            return true;
        }

        public bool CloseAccount(string iban)
        {
            Account account = GetAccount(iban);
            if (account == null)
            {
                throw new InvalidOperationException("Account not found or already closed.");
            }

            if (account.Amount > 0)
            {
                throw new InvalidOperationException($"Account has a balance of {account.Amount} {account.AccountCurrency}. Please withdraw before closing.");
            }

            account.AccountCloseDate = DateTime.Now;
            AddTransaction(iban, "ACCOUNT_CLOSE", 0, account.AccountCurrency, "Account closed");
            return true;
        }

        public string FindAndDisplayAccount(string iban)
        {
            Account account = Accounts.FirstOrDefault(a => a.Iban == iban);
            if (account == null)
            {
                return "Account not found.";
            }

            return account.ToString();
        }

        public bool DepositMoney(string iban, decimal amount, AccountCurrency currency)
        {
            Account account = GetAccount(iban);
            if (account == null)
            {
                throw new InvalidOperationException("Account not found or not active.");
            }

            decimal convertedAmount = amount;
            if (account.AccountCurrency != currency)
            {
                convertedAmount = ConvertCurrency(amount, currency, account.AccountCurrency);
            }

            account.Deposit(convertedAmount);
            AddTransaction(iban, "DEPOSIT", convertedAmount, account.AccountCurrency, $"Deposited {amount} {currency}");
            return true;
        }

        public bool WithdrawMoney(string iban, decimal amount)
        {
            Account account = GetAccount(iban);
            if (account == null)
            {
                throw new InvalidOperationException("Account not found or not active.");
            }

            decimal fee = amount * AccountTypeFees[account.AccountType];
            decimal totalAmount = amount + fee;

            if (account.Amount < totalAmount)
            {
                throw new InvalidOperationException("Insufficient funds including fee.");
            }

            account.Withdraw(totalAmount);
            BankBalance += fee;
            AddTransaction(iban, "WITHDRAWAL", -amount, account.AccountCurrency, $"Withdrew {amount}, Fee: {fee}");
            return true;
        }

        public async Task<bool> TransferMoney(string fromIban, string toIban, decimal amount, Bank toBank = null)
        {
            Account fromAccount = GetAccount(fromIban);
            if (fromAccount == null)
            {
                throw new InvalidOperationException("Source account not found or not active.");
            }

            Account toAccount;
            bool isSameBank = toBank == null || toBank == this;
            
            if (isSameBank)
            {
                toAccount = GetAccount(toIban);
                if (toAccount == null)
                {
                    throw new InvalidOperationException("Destination account not found or not active.");
                }
            }
            else
            {
                toAccount = toBank.GetAccount(toIban);
                if (toAccount == null)
                {
                    throw new InvalidOperationException("Destination account not found or not active.");
                }
            }

            decimal transferFee = isSameBank ? amount * SameBankTransferFee : amount * DifferentBankTransferFee;
            decimal totalDeduction = amount + transferFee;

            if (fromAccount.Amount < totalDeduction)
            {
                throw new InvalidOperationException("Insufficient funds for transfer including fees.");
            }

            // Simulate transfer delay
            Random random = new Random();
            int delaySeconds = isSameBank ? random.Next(1, 11) : random.Next(11, 21);
            await Task.Delay(delaySeconds * 1000);

            // Deduct from source account
            fromAccount.Amount -= totalDeduction;

            // Convert and deposit to destination account
            decimal convertedAmount = ConvertCurrency(amount, fromAccount.AccountCurrency, toAccount.AccountCurrency);
            toAccount.Amount += convertedAmount;

            // Add fees to bank balance
            BankBalance += transferFee;

            // Record transactions
            AddTransaction(fromIban, "TRANSFER_OUT", -amount, fromAccount.AccountCurrency, 
                          $"Transfer to {toIban}, Fee: {transferFee}");
            
            if (isSameBank)
            {
                AddTransaction(toIban, "TRANSFER_IN", convertedAmount, toAccount.AccountCurrency, 
                              $"Transfer from {fromIban}");
            }
            else
            {
                toBank.AddTransaction(toIban, "TRANSFER_IN", convertedAmount, toAccount.AccountCurrency, 
                                     $"Transfer from {fromIban} ({BankName})");
            }

            return true;
        }

        public List<Transaction> GetTransactionHistory(string iban)
        {
            if (!TransactionHistory.ContainsKey(iban))
            {
                return new List<Transaction>();
            }

            return TransactionHistory[iban];
        }

        private void AddTransaction(string iban, string type, decimal amount, AccountCurrency currency, string description)
        {
            if (!TransactionHistory.ContainsKey(iban))
            {
                TransactionHistory[iban] = new List<Transaction>();
            }

            TransactionHistory[iban].Add(new Transaction
            {
                Date = DateTime.Now,
                Type = type,
                Amount = amount,
                Currency = currency,
                Description = description
            });
        }

        private string GenerateIban()
        {
            string countryCode = BankCountry.ToString();
            string checkDigits = new Random().Next(10, 100).ToString();
            string bankCode = SwiftAccount.Substring(0, Math.Min(4, SwiftAccount.Length)).ToUpper();
            string accountNumber = new Random().Next(100000, 1000000).ToString() + 
                                  new Random().Next(100000, 1000000).ToString();
            return $"{countryCode}{checkDigits}{bankCode}{accountNumber}";
        }

        private decimal ConvertCurrency(decimal amount, AccountCurrency from, AccountCurrency to)
        {
            if (from == to) return amount;

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

        public void ChangeAccountLocation(string iban, BankLocation newLocation)
        {
            Account account = GetAccount(iban);
            if (account == null)
            {
                throw new InvalidOperationException("Account not found or not active.");
            }

            AddTransaction(iban, "LOCATION_CHANGE", 0, account.AccountCurrency, 
                          $"Location changed to {newLocation}");
        }

        public override string ToString()
        {
            return $"\n=== Bank Information ===\n" +
                   $"Name: {BankName}\n" +
                   $"SWIFT: {SwiftAccount}\n" +
                   $"Location: {BankLocation}, {BankCountry}\n" +
                   $"Balance: {BankBalance:F2}\n" +
                   $"Active Accounts: {Accounts.Count(a => a.IsActive())}\n" +
                   $"========================";
        }
    }
}
