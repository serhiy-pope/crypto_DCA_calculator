using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using TokeroDCACalculator.Models;
using TokeroDCACalculator.Models.MultiResultsModels;
using TokeroDCACalculator.Views;

namespace TokeroDCACalculator.ViewModels
{
    public partial class HomePageViewModel : BaseViewModel
    {
        #region - Private fields

        private ObservableCollection<CryptoOption> _selectedCryptos;
        private bool _noCryptoSelected = true;

        private bool _isAndroid = true;
        private bool _isIos = true;

        #endregion

        public HomePageViewModel()
        {
            Title = "Tokero DCA Calculator";

            CryptoOptions = new ObservableCollection<CryptoOption>
            {
                new CryptoOption { Name = "Bitcoin", Symbol = "BTC" },
                new CryptoOption { Name = "Ethereum", Symbol = "ETH" },
                new CryptoOption { Name = "Solana", Symbol = "SOL" },
                new CryptoOption { Name = "Ripple", Symbol = "XRP" }
            };
            SelectedCryptos = [];

            CalculateDcaAsyncCommand = new AsyncRelayCommand(CalculateDcaAsync, ValidateAction);
            PropertyChanged += (_, __) => CalculateDcaAsyncCommand.NotifyCanExecuteChanged();

            CryptoSelectedCommand = new Command(CryptoSelected);

#if ANDROID
            IsAndroid = true;
            IsIos = false;
#elif IOS
            IsAndroid = false;
            IsIos = true;
#else
            IsAndroid = true;
            IsIos = false;
#endif
        }

        #region - Public Properties

        [ObservableProperty]
        ObservableCollection<CryptoOption> cryptoOptions;

        public ObservableCollection<CryptoOption> SelectedCryptos
        {
            get => _selectedCryptos;
            set
            {
                _selectedCryptos = value;
                if (value.Count != 0)
                    NoCryptoSelected = false;
                else
                    NoCryptoSelected = true;
                OnPropertyChanged();
            }
        }

        [ObservableProperty]
        DateTime startDate = DateTime.Today.AddMonths(-6);

        [ObservableProperty]
        DateTime endDate = DateTime.Today;

        [ObservableProperty]
        decimal monthlyInvestedAmountEUR = 200;

        [ObservableProperty]
        int dcaDayOfMonth = 15;

        public bool NoCryptoSelected
        {
            get => _noCryptoSelected;
            set
            {
                _noCryptoSelected = value;
                OnPropertyChanged();
            }
        }

        public bool IsAndroid
        {
            get => _isAndroid;
            set
            {
                _isAndroid = value;
                OnPropertyChanged();
            }
        }

        public bool IsIos
        {
            get => _isIos;
            set
            {
                _isIos = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region - Commands

        public IAsyncRelayCommand CalculateDcaAsyncCommand { get; }
        public Command CryptoSelectedCommand { get; }

        #endregion

        #region - Private methods

        private async Task CalculateDcaAsync()
        {
            try
            {
                if (SelectedCryptos == null)
                {
                    await Shell.Current.DisplayAlert("Error", "Please select a cryptocurrency.", "OK");
                    return;
                }
                if (MonthlyInvestedAmountEUR <= 0)
                {
                    await Shell.Current.DisplayAlert("Error", "Please enter a valid investment amount.", "OK");
                    return;
                }
                if (DcaDayOfMonth <= 0 || DcaDayOfMonth > 31)
                {
                    await Shell.Current.DisplayAlert("Error", "Please enter a valid Dca day of month between 1st and 31st.", "OK");
                    return;
                }
                if (SelectedCryptos.Count == 0)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Please select at least one cryptocurrency", "OK");
                    return;
                }

                int totalMonths = CalculateMonthsBetween(StartDate, EndDate);
                if (SelectedCryptos.Count == 1)
                {
                    // Single crypto - navigate to ResultsPage
                    await Shell.Current.GoToAsync($"{nameof(ResultsPage)}", new Dictionary<string, object>
                    {
                        ["SelectedCrypto"] = SelectedCryptos.First(),
                        ["StartDate"] = StartDate,
                        ["EndDate"] = EndDate,
                        ["MonthlyInvestedAmountEUR"] = MonthlyInvestedAmountEUR,
                        ["DcaDayOfMonth"] = DcaDayOfMonth,
                        ["TotalMonths"] = totalMonths
                    });
                }
                else
                {
                    // Multi crypto - create equal allocations and navigate to MultiResultsPage
                    var cryptoAllocations = HomePageViewModel.CreateEqualAllocations(SelectedCryptos);

                    await Shell.Current.GoToAsync($"{nameof(MultiResultsPage)}", new Dictionary<string, object>
                    {
                        ["SelectedCryptos"] = SelectedCryptos,
                        ["StartDate"] = StartDate,
                        ["EndDate"] = EndDate,
                        ["TotalMonthlyInvestmentEUR"] = MonthlyInvestedAmountEUR,
                        ["DcaDayOfMonth"] = DcaDayOfMonth,
                        ["CryptoAllocations"] = cryptoAllocations,
                        ["SelectedCryptosCount"] = SelectedCryptos.Count,
                        ["TotalMonths"] = totalMonths
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An error occurred while calculating DCA: {ex.Message}\n{ex.StackTrace}");
                await Shell.Current.DisplayAlert("Unexpected Error", $"An error occurred while calculating DCA: {ex.Message}", "OK");
            }
        }

        private static List<CryptoAllocation> CreateEqualAllocations(ObservableCollection<CryptoOption> selectedCryptos)
        {
            try
            {
                if (selectedCryptos == null || selectedCryptos.Count == 0)
                    throw new ArgumentException("The selectedCryptos collection is null or empty.");

                decimal percentagePerCrypto = Math.Round(100m / selectedCryptos.Count, 2);

                return selectedCryptos.Select(crypto => new CryptoAllocation
                {
                    Symbol = crypto.Symbol,
                    Percentage = percentagePerCrypto
                }).ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in CreateEqualAllocations: {ex.Message}");
                return new List<CryptoAllocation>();
            }
        }

        private static int CalculateMonthsBetween(DateTime startDate, DateTime endDate)
        {
            try
            {
                if (endDate < startDate)
                    throw new ArgumentException("End date must be after start date.");

                int months = ((endDate.Year - startDate.Year) * 12) + endDate.Month - startDate.Month;

                if (endDate.Day >= startDate.Day)
                    months++;

                return Math.Max(0, months);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in CalculateMonthsBetween: {ex.Message}");
                return 0;
            }
        }

        private void CryptoSelected()
        {
            if (SelectedCryptos.Count == 0)
                NoCryptoSelected = true;
            else
                NoCryptoSelected = false;
        }

        private bool ValidateAction()
        {
            return !NoCryptoSelected;
        }

        #endregion

    }
}