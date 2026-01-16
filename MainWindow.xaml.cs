using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace BankingSystem
{
    public partial class MainWindow : Window
    {
        private BankingSystemManager manager;
        private Bank selectedBank;

        public MainWindow()
        {
            InitializeComponent();
            manager = new BankingSystemManager();
            manager.LoadData();
            
            InitializeComboBoxes();
            RefreshBanksList();
            
            StatusText.Text = $"Loaded {manager.BankCount} bank(s)";
        }

        private void InitializeComboBoxes()
        {
            // Account Type
            AccountTypeComboBox.ItemsSource = Enum.GetValues(typeof(AccountType));
            AccountTypeComboBox.SelectedIndex = 0;

            // Currency combo boxes
            var currencies = Enum.GetValues(typeof(AccountCurrency));
            CurrencyComboBox.ItemsSource = currencies;
            CurrencyComboBox.SelectedIndex = 0;
            
            DepositCurrencyComboBox.ItemsSource = currencies;
            DepositCurrencyComboBox.SelectedIndex = 0;
            
            NewCurrencyComboBox.ItemsSource = currencies;
            NewCurrencyComboBox.SelectedIndex = 0;
        }

        private void RefreshBanksList()
        {
            var banks = new List<Bank>();
            for (int i = 0; i < manager.BankCount; i++)
            {
                banks.Add(manager.GetBank(i));
            }
            
            BanksList.ItemsSource = null;
            BanksList.ItemsSource = banks;
            
            BankComboBox.ItemsSource = null;
            BankComboBox.ItemsSource = banks;
            
            DestBankCombo.ItemsSource = null;
            DestBankCombo.ItemsSource = banks;
        }

        private void BanksList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedBank = BanksList.SelectedItem as Bank;
            
            if (selectedBank != null)
            {
                NoBankSelectedText.Visibility = Visibility.Collapsed;
                BankInfoPanel.Visibility = Visibility.Visible;
                
                BankNameText.Text = selectedBank.BankName;
                BankSwiftText.Text = selectedBank.SwiftAccount;
                BankLocationText.Text = selectedBank.BankLocation.ToString();
                BankCountryText.Text = selectedBank.BankCountry.ToString();
                BankBalanceText.Text = $"{selectedBank.BankBalance:F2}";
                
                AccountsGrid.ItemsSource = null;
                AccountsGrid.ItemsSource = selectedBank.Accounts;
            }
            else
            {
                NoBankSelectedText.Visibility = Visibility.Visible;
                BankInfoPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void CreateBank_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new CreateBankDialog();
                if (dialog.ShowDialog() == true)
                {
                    Bank newBank = new Bank(
                        dialog.BankName,
                        dialog.SwiftCode,
                        dialog.SelectedLocation,
                        dialog.SelectedCountry
                    );
                    
                    manager.AddBank(newBank);
                    RefreshBanksList();
                    StatusText.Text = $"Bank '{newBank.BankName}' created successfully!";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating bank: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenAccount_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var bank = BankComboBox.SelectedItem as Bank;
                if (bank == null)
                {
                    MessageBox.Show("Please select a bank.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(AccountHolderText.Text))
                {
                    MessageBox.Show("Please enter account holder name.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var accountType = (AccountType)AccountTypeComboBox.SelectedItem;
                var currency = (AccountCurrency)CurrencyComboBox.SelectedItem;
                decimal initialDeposit = decimal.Parse(InitialDepositText.Text);

                bank.OpenAccount(AccountHolderText.Text, accountType, currency, initialDeposit);
                
                RefreshBanksList();
                if (selectedBank == bank)
                {
                    AccountsGrid.ItemsSource = null;
                    AccountsGrid.ItemsSource = selectedBank.Accounts;
                }
                
                StatusText.Text = "Account opened successfully!";
                AccountHolderText.Clear();
                InitialDepositText.Text = "0";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Deposit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string iban = DepositIbanText.Text.Trim();
                decimal amount = decimal.Parse(DepositAmountText.Text);
                var currency = (AccountCurrency)DepositCurrencyComboBox.SelectedItem;

                Bank bank = FindBankByIban(iban);
                if (bank == null)
                {
                    MessageBox.Show("Account not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                bank.DepositMoney(iban, amount, currency);
                RefreshBanksList();
                StatusText.Text = "Deposit completed successfully!";
                DepositIbanText.Clear();
                DepositAmountText.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Withdraw_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string iban = WithdrawIbanText.Text.Trim();
                decimal amount = decimal.Parse(WithdrawAmountText.Text);

                Bank bank = FindBankByIban(iban);
                if (bank == null)
                {
                    MessageBox.Show("Account not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                bank.WithdrawMoney(iban, amount);
                RefreshBanksList();
                StatusText.Text = "Withdrawal completed successfully!";
                WithdrawIbanText.Clear();
                WithdrawAmountText.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void Transfer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string fromIban = TransferFromIbanText.Text.Trim();
                string toIban = TransferToIbanText.Text.Trim();
                decimal amount = decimal.Parse(TransferAmountText.Text);

                Bank fromBank = FindBankByIban(fromIban);
                if (fromBank == null)
                {
                    MessageBox.Show("Source account not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                Bank toBank = null;
                if (DifferentBankCheckbox.IsChecked == true)
                {
                    toBank = DestBankCombo.SelectedItem as Bank;
                    if (toBank == null)
                    {
                        MessageBox.Show("Please select destination bank.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }
                else
                {
                    toBank = fromBank;
                }

                StatusText.Text = "Processing transfer...";
                var button = (Button)sender;
                button.IsEnabled = false;

                await fromBank.TransferMoney(fromIban, toIban, amount, toBank == fromBank ? null : toBank);
                
                RefreshBanksList();
                StatusText.Text = "Transfer completed successfully!";
                
                TransferFromIbanText.Clear();
                TransferToIbanText.Clear();
                TransferAmountText.Clear();
                button.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Transfer failed.";
            }
        }

        private void ViewHistory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string iban = HistoryIbanText.Text.Trim();
                Bank bank = FindBankByIban(iban);
                
                if (bank == null)
                {
                    MessageBox.Show("Account not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var history = bank.GetTransactionHistory(iban);
                TransactionHistoryGrid.ItemsSource = null;
                TransactionHistoryGrid.ItemsSource = history;
                StatusText.Text = $"Showing {history.Count} transaction(s)";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ChangeCurrency_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string iban = ChangeCurrencyIbanText.Text.Trim();
                var newCurrency = (AccountCurrency)NewCurrencyComboBox.SelectedItem;

                Bank bank = FindBankByIban(iban);
                if (bank == null)
                {
                    MessageBox.Show("Account not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var account = bank.Accounts.FirstOrDefault(a => a.Iban == iban && a.IsActive());
                if (account != null)
                {
                    account.ChangeAccountCurrency(newCurrency);
                    RefreshBanksList();
                    StatusText.Text = "Currency changed successfully!";
                    ChangeCurrencyIbanText.Clear();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseAccount_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string iban = CloseAccountIbanText.Text.Trim();
                Bank bank = FindBankByIban(iban);
                
                if (bank == null)
                {
                    MessageBox.Show("Account not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show(
                    "Are you sure you want to close this account?",
                    "Confirm",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                );

                if (result == MessageBoxResult.Yes)
                {
                    bank.CloseAccount(iban);
                    RefreshBanksList();
                    StatusText.Text = "Account closed successfully!";
                    CloseAccountIbanText.Clear();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FindAccount_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string iban = FindAccountIbanText.Text.Trim();
                Bank bank = FindBankByIban(iban);
                
                if (bank == null)
                {
                    MessageBox.Show("Account not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string accountDetails = bank.FindAndDisplayAccount(iban);
                AccountDetailsText.Text = accountDetails;
                StatusText.Text = "Account found!";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private Bank FindBankByIban(string iban)
        {
            for (int i = 0; i < manager.BankCount; i++)
            {
                var bank = manager.GetBank(i);
                if (bank.Accounts.Any(a => a.Iban == iban))
                {
                    return bank;
                }
            }
            return null;
        }

        private void AccountsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Optional: Could add account detail view here
        }

        protected override void OnClosed(EventArgs e)
        {
            manager.SaveData();
            base.OnClosed(e);
        }
    }

    // Converter for Date to Status
    public class DateToStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime date)
            {
                return date.Year == 2999 ? "Active" : "Closed";
            }
            return "Unknown";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
