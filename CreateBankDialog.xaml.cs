using System;
using System.Windows;

namespace BankingSystem
{
    public partial class CreateBankDialog : Window
    {
        public string BankName { get; private set; }
        public string SwiftCode { get; private set; }
        public BankLocation SelectedLocation { get; private set; }
        public BankCountry SelectedCountry { get; private set; }

        public CreateBankDialog()
        {
            InitializeComponent();
            
            LocationComboBox.ItemsSource = Enum.GetValues(typeof(BankLocation));
            LocationComboBox.SelectedIndex = 0;
            
            CountryComboBox.ItemsSource = Enum.GetValues(typeof(BankCountry));
            CountryComboBox.SelectedIndex = 0;
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(BankNameTextBox.Text))
            {
                MessageBox.Show("Please enter a bank name.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(SwiftCodeTextBox.Text))
            {
                MessageBox.Show("Please enter a SWIFT code.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            BankName = BankNameTextBox.Text.Trim();
            SwiftCode = SwiftCodeTextBox.Text.Trim();
            SelectedLocation = (BankLocation)LocationComboBox.SelectedItem;
            SelectedCountry = (BankCountry)CountryComboBox.SelectedItem;

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
