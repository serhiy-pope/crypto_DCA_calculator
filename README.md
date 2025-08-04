# Tokero DCA Calculator

A cross-platform .NET MAUI mobile application that simulates **Dollar-Cost Averaging (DCA)** investments into cryptocurrencies. 
The app is designed for Tokero (c) Crypto exchange and helps visualize long-term investment performance using real historical data.

## Features

### DCA Simulation
- Simulates recurring monthly investments into multiple cryptocurrencies.
- Uses real historical prices to calculate investment performance over time.
- Shows total invested amount, current portfolio value, and gain/loss.

### Candlestick Chart
- Displays historical crypto prices in a candlestick format.
- Built using DevExpress Charts with smooth and interactive visuals.

### Investment Summary
- Summarizes portfolio performance with clear and concise statistics.
- Provides breakdowns per coin and overall results.

### Architecture & Persistence
- Clean MVVM structure using CommunityToolkit.Mvvm.
- Data is persisted locally using SQLite for offline support.
- Services and repositories are injected using .NET MAUI's dependency injection.

## Tech Stack

- .NET MAUI
- C#
- MVVM (CommunityToolkit.Mvvm)
- SQLite-net-pcl
- DevExpress MAUI Charts
- CoinGecko API (for historical price data)
- MvvmHelpers (by James Montemagno)

## NuGet Packages Used

These packages will be restored automatically during build:

- `CommunityToolkit.Mvvm`
- `SQLite-net-pcl`
- `DevExpress.Maui.Charts`
- `DevExpress.Maui.Core`
- `Newtonsoft.Json`
- `MvvmHelpers`

To restore manually:
```bash
dotnet restore
```

## Getting Started

1. Clone the repo:
   ```bash
   git clone https://github.com/your-username/TokeroDCACalculator.git
   cd TokeroDCACalculator
   ```

2. Restore dependencies:
   ```bash
   dotnet restore
   ```

3. Build and run the app:
   ```bash
   dotnet build
   ```

> iOS builds require macOS with Xcode installed.

## Project Structure

```
TokeroDCACalculator/
│
├── Models/ // Data models like CryptoPrice, DCAResult
├── Services/ // API services, data fetching, and local persistence
├── ViewModels/ // Page-level logic with data bindings
├── Views/ // XAML pages representing the UI
├── Helpers/ // Constants, enums, FontAwesome icon definitions
├── Converters/ // Value converters for data binding (e.g., BoolToColorConverter)
├── Data/ // Static resources such as local .csv files
├── Resources/ // Fonts, images, and app themes
└── App.xaml / AppShell.xaml // Application entry point and navigation shell
```

## Notes

- All investment logic is testable and easily extendable.
- The app does not use authentication or remote user data.
- Designed to run smoothly on Android and iOS using MAUI’s cross-platform support.

## Contact

**Serhiy Pop**  
.NET MAUI Developer  
[serhiy.pope@gmail.com]
[https://github.com/serhiy-pope]
[https://www.linkedin.com/in/serhiy-pop-ua/]

