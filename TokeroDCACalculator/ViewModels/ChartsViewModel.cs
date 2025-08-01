using MvvmHelpers;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using TokeroDCACalculator.Models;
using TokeroDCACalculator.Views;

namespace TokeroDCACalculator.ViewModels
{
    public class ChartsViewModel : BaseViewModel
    {
        #region - Private fields

        private string _selectedCoin;
        private string _selectedCoinTicker;
        private ObservableRangeCollection<CryptoCandle> candles;

        private Color _candleStickSeriesBackgroundColor;
        private Color _candleStickSeriesLabelTextColor;

        #endregion

        public ChartsViewModel()
        {
            Title = "Charts";

            Candles = [];
            SetSelectedCoinAsyncCommand = new MvvmHelpers.Commands.AsyncCommand<string>(SetSelectedCoinAsync);
            GoBackCommand = new Command(ExecuteGoBackCommand);

            UpdateTextColor(Application.Current.RequestedTheme);
            Application.Current.RequestedThemeChanged += (s, e) => UpdateTextColor(e.RequestedTheme);

            UpdateBackgroundColor(Application.Current.RequestedTheme);
            Application.Current.RequestedThemeChanged += (s, e) => UpdateBackgroundColor(e.RequestedTheme);
        }

        public void OnAppearing()
        {
            SelectedCoin = AvailableCoins.First();
        }

        #region - Public Properties

        public ObservableRangeCollection<CryptoCandle> Candles 
        {
            get => candles;
            set
            {
                candles = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<string> AvailableCoins { get; } = new()
        {
            "Bitcoin",
            "Ethereum",
            "Solana",
            "Ripple"
        }; 

        public string SelectedCoin
        {
            get => _selectedCoin;
            set
            {
                _selectedCoin = value; 
                SetSelectedCoinTicker(_selectedCoin);
                OnPropertyChanged();
            }
        }
        public string SelectedCoinTicker
        {
            get => _selectedCoinTicker;
            set
            {
                _selectedCoinTicker = value;
                OnPropertyChanged();
            }
        }

        public Color CandleStickSeriesBackgroundColor
        {
            get => _candleStickSeriesBackgroundColor; set
            {
                _candleStickSeriesBackgroundColor = value;
                OnPropertyChanged();
            }
        }
        public Color CandleStickSeriesLabelTextColor
        {
            get => _candleStickSeriesLabelTextColor;
            set 
            {
                _candleStickSeriesLabelTextColor = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region - Commands

        public MvvmHelpers.Commands.AsyncCommand<string> SetSelectedCoinAsyncCommand { get; }
        public Command GoBackCommand { get; }

        #endregion

        #region - Private methods

        private async Task LoadCandlesFromCsv(string coinName)
        {
            try
            {
                string fileName = coinName.ToLower() switch
                {
                    "bitcoin" => "btc_daily.csv",
                    "ethereum" => "eth_daily.csv",
                    "solana" => "sol_daily.csv",
                    "ripple" => "xrp_daily.csv",
                    _ => null
                };

                if (fileName == null)
                    return;

                var assembly = typeof(ChartsViewModel).Assembly;
                var resourceName = $"TokeroDCACalculator.Data.{fileName}";

                using Stream stream = assembly.GetManifestResourceStream(resourceName);
                if (stream == null)
                    return;

                using var reader = new StreamReader(stream);
                var content = await reader.ReadToEndAsync();

                var parsed = ParseCandleCsv(content);
                if (parsed != null && parsed.Count != 0)
                    Candles.ReplaceRange(parsed);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LoadCandlesFromCsv] Error loading CSV for {coinName}: {ex.Message}\n{ex.StackTrace}");
                await Shell.Current.DisplayAlert("Error", $"Failed to load candle data for {coinName}. Please try again.", "OK");
            }
        }

        private static ObservableRangeCollection<CryptoCandle> ParseCandleCsv(string csvContent)
        {
            var candles = new ObservableRangeCollection<CryptoCandle>();

            try
            {
                var lines = csvContent.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);

                // int i = 1 to skip header
                for (int i = 1; i < lines.Length; i++)
                {
                    var line = lines[i];
                    var parts = line.Split(';');

                    if (parts.Length < 9)
                        continue;

                    var dateString = parts[0].Trim('"');
                    var openStr = parts[5];
                    var highStr = parts[6];
                    var lowStr = parts[7];
                    var closeStr = parts[8];

                    if (DateTime.TryParse(dateString, null, DateTimeStyles.AdjustToUniversal, out var date) &&
                        decimal.TryParse(openStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var open) &&
                        decimal.TryParse(highStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var high) &&
                        decimal.TryParse(lowStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var low) &&
                        decimal.TryParse(closeStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var close))
                    {
                        candles.Add(new CryptoCandle
                        {
                            Date = date,
                            Open = open,
                            High = high,
                            Low = low,
                            Close = close
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ParseCandleCsv] Error parsing CSV: {ex.Message}");
            }
            return candles;
        }

        private void UpdateBackgroundColor(AppTheme theme)
        {
            CandleStickSeriesBackgroundColor = theme switch
            {
                AppTheme.Dark => (Color)Application.Current.Resources["Gray800"],
                AppTheme.Light => (Color)Application.Current.Resources["SystemGray6"],
                _ => Colors.Gray
            };
        }

        private void UpdateTextColor(AppTheme theme)
        {
            CandleStickSeriesLabelTextColor = theme switch
            {
                AppTheme.Dark => Colors.White,
                AppTheme.Light => Colors.Black,
                _ => Colors.Gray
            };
        }

        public async Task SetSelectedCoinAsync(string coin)
        {
            SelectedCoin = coin;
            await LoadCandlesFromCsv(coin);
        }

        private void SetSelectedCoinTicker(string selectedCoin)
        {
            string ticker = selectedCoin.ToLower() switch
            {
                "bitcoin" => "BTC",
                "ethereum" => "ETH",
                "solana" => "SOL",
                "ripple" => "XRP",
                _ => null
            };
            SelectedCoinTicker = ticker;
        }

        private async void ExecuteGoBackCommand()
        {
            await Shell.Current.GoToAsync($"//{nameof(HomePage)}", true);
        }

        #endregion

    }
}