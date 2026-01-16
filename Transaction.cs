using System;

namespace BankingSystem
{
    public class Transaction
    {
        public DateTime Date { get; set; }
        public string Type { get; set; }
        public decimal Amount { get; set; }
        public AccountCurrency Currency { get; set; }
        public string Description { get; set; }

        public override string ToString()
        {
            return $"[{Date:yyyy-MM-dd HH:mm:ss}] {Type}: {Amount:F2} {Currency} - {Description}";
        }
    }
}
