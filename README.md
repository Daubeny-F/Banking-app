<<<<<<< HEAD
# Banking System

A comprehensive C# WPF banking application with a graphical user interface.

## Features

- **Bank Management**: Create and manage multiple banks with unique names per country
- **Account Operations**: Open, close, and manage different types of accounts (Person, Company, Special)
- **Transactions**: Deposit, withdraw, and transfer money between accounts
- **Multi-Currency Support**: RON, EUR, USD, GBP with automatic conversion
- **Async Transfers**: Realistic transfer delays (1-10s same bank, 11-20s different banks)
- **Fee System**: Different fees for account types and transfer types
- **Transaction History**: Complete audit trail for all account operations
- **JSON Persistence**: Automatic save/load of all data

## Requirements

- .NET 10.0 or higher
- Windows OS (WPF application)

## How to Run

```powershell
cd FCPL
dotnet build
dotnet run
```

## Project Structure

- `Account.cs` - Account class with deposit, withdraw, and currency change operations
- `Bank.cs` - Bank class with all banking operations
- `BankingSystemManager.cs` - Manager for multiple banks and JSON persistence
- `Enums.cs` - All enum definitions
- `Transaction.cs` - Transaction record class
- `MainWindow.xaml/.cs` - Main WPF UI
- `CreateBankDialog.xaml/.cs` - Dialog for creating new banks
- `App.xaml/.cs` - WPF application entry point

## License

Educational project for FCPL course.
=======
# Banking-app
FCPL project
>>>>>>> b9f0313b09668c0bc802d35321e2bbf847ecd3ae
