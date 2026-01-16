using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace BankingSystem
{
    public class BankingSystemManager
    {
        private List<Bank> banks;
        private const string DataFile = "banks.json";

        public BankingSystemManager()
        {
            banks = new List<Bank>();
        }

        public void LoadData()
        {
            try
            {
                if (File.Exists(DataFile))
                {
                    string json = File.ReadAllText(DataFile);
                    banks = JsonSerializer.Deserialize<List<Bank>>(json) ?? new List<Bank>();
                    
                    // Initialize transaction history for loaded banks
                    foreach (var bank in banks)
                    {
                        if (bank.TransactionHistory == null)
                        {
                            bank.TransactionHistory = new Dictionary<string, List<Transaction>>();
                        }
                        foreach (var account in bank.Accounts)
                        {
                            if (!bank.TransactionHistory.ContainsKey(account.Iban))
                            {
                                bank.TransactionHistory[account.Iban] = new List<Transaction>();
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                banks = new List<Bank>();
            }
        }

        public void SaveData()
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                string json = JsonSerializer.Serialize(banks, options);
                File.WriteAllText(DataFile, json);
            }
            catch (Exception)
            {
            }
        }

        public void AddBank(Bank bank)
        {
            // Check for unique bank name per country
            if (banks.Any(b => b.BankName == bank.BankName && b.BankCountry == bank.BankCountry))
            {
                throw new InvalidOperationException("A bank with this name already exists in this country.");
            }

            banks.Add(bank);
        }

        public Bank GetBank(int index)
        {
            if (index >= 0 && index < banks.Count)
            {
                return banks[index];
            }
            return null;
        }

        public void ListBanks()
        {
            if (banks.Count == 0)
            {
                Console.WriteLine("No banks available.");
                return;
            }

            Console.WriteLine("\n=== Available Banks ===");
            for (int i = 0; i < banks.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {banks[i].BankName} ({banks[i].BankCountry}) - SWIFT: {banks[i].SwiftAccount}");
            }
            Console.WriteLine("=======================");
        }

        public int BankCount => banks.Count;
    }
}
