using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using TokeroDCACalculator.Models;
using TokeroDCACalculator.Services;
using TokeroDCACalculator.Views;

namespace TokeroDCACalculator.ViewModels
{
    public partial class ResultsViewModel : BaseViewModel, IQueryAttributable
    {
        #region - Private fields

        private string _totalInvested;
        private string _totalCrypto;
        private string _portfolioTotalValue;
        private string _profitOrLoss;
        private string _profitOrLossText;
        private string _totalRoi;

        private int _totalMonthsCount;
        private decimal _roiColorForConverter;

        private readonly ICryptoPriceRepository _cryptoPriceRepository;

        #endregion

        #region - Public Properties

        [ObservableProperty]
        ObservableCollection<DcaResultItem> dcaResults = [];

        public string TotalInvested
        {
            get => _totalInvested;
            set
            {
                _totalInvested = value;
                OnPropertyChanged();
            }
        }
        public string TotalCrypto
        {
            get => _totalCrypto;
            set
            {
                _totalCrypto = value;
                OnPropertyChanged();
            }
        }
        public string PortfolioTotalValue
        {
            get => _portfolioTotalValue;
            set
            {
                _portfolioTotalValue = value;
                OnPropertyChanged();
            }
        }
        public string ProfitOrLoss
        {
            get => _profitOrLoss;
            set
            {
                _profitOrLoss = value;
                OnPropertyChanged();
            }
        }
        public string ProfitOrLossText
        {
            get => _profitOrLossText;
            set
            {
                _profitOrLossText = value;
                OnPropertyChanged();
            }
        }
        public string TotalRoi
        {
            get => _totalRoi;
            set
            {
                _totalRoi = value;
                OnPropertyChanged();
            }
        }

        public int TotalMonthsCount
        {
            get => _totalMonthsCount;
            set
            {
                _totalMonthsCount += value;
                OnPropertyChanged();
            }
        }
        public decimal RoiColorForConverter
        {
            get => _roiColorForConverter;
            set
            {
                _roiColorForConverter = value;
                OnPropertyChanged();
            }
        }

        #endregion

        public ResultsViewModel(ICryptoPriceRepository cryptoPriceRepository)
        {
            _cryptoPriceRepository = cryptoPriceRepository;

            GoBackCommand = new Command(ExecuteGoBackCommand);
        }

        #region - Commands

        public Command GoBackCommand { get; }

        #endregion

        #region - Public methods

        public async void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;

                if (query.TryGetValue("SelectedCrypto", out var selectedCryptoObj) &&
                    query.TryGetValue("StartDate", out var startDateObj) &&
                    query.TryGetValue("EndDate", out var endDateObj) &&
                    query.TryGetValue("MonthlyInvestedAmountEUR", out var monthlyInvestedAmountEURObj) &&
                    query.TryGetValue("DcaDayOfMonth", out var dcaDayObj) &&
                    query.TryGetValue("TotalMonths", out var totalMonthsObj))
                {
                    if (selectedCryptoObj is CryptoOption selectedCrypto &&
                        startDateObj is DateTime startDate &&
                        endDateObj is DateTime endDate &&
                        monthlyInvestedAmountEURObj is decimal monthlyAmount &&
                        dcaDayObj is int dcaDay &&
                        totalMonthsObj is int totalMonths)
                    {
                        Title = $"DCA for {selectedCrypto.Symbol} from {startDate:yyyy.MM.dd}";
                        TotalMonthsCount = totalMonths;
                        await GenerateResults(selectedCrypto.Symbol, startDate, endDate, monthlyAmount, dcaDay);
                    }
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        #endregion

        #region - Private methods

        private async Task GenerateResults(string symbol, DateTime startDate, DateTime endDate, decimal monthlyInvestedAmountEUR, int dcaDayOfMonth)
        {
            try
            {
                var detailedResults = await CalculateDcaAsync(symbol, startDate, endDate, monthlyInvestedAmountEUR, dcaDayOfMonth);
                DcaResults.Clear();

                foreach (var result in detailedResults)
                    DcaResults.Add(result);

                var summary = await CalculateSummaryDcaAsync(symbol, startDate, endDate, monthlyInvestedAmountEUR, dcaDayOfMonth);
                FillUpSummaryBlockProperties(summary, symbol);
            }
            catch (Exception ex)
            {
                // Log the error (replace with your actual logger if needed)
                System.Diagnostics.Debug.WriteLine($"[GenerateResults] Error: {ex.Message}");

                // Optionally notify the user
                await Shell.Current.DisplayAlert("Error", "An error occurred while generating DCA results. Please try again.", "OK");
            }
        }

        private async Task<List<DcaResultItem>> CalculateDcaAsync(string symbol, DateTime startDate, DateTime endDate,
                                                                    decimal monthlyInvestedAmountEUR, int dcaDayOfMonth)
        {
            try
            {
                var prices = await _cryptoPriceRepository.GetPricesAsync(symbol, startDate, endDate);
                if (prices == null || prices.Count == 0)
                    return new List<DcaResultItem>();

                var results = new List<DcaResultItem>();
                var today = DateTime.Today;

                // Running totals across all months
                decimal totalInvested = 0m;
                decimal totalCrypto = 0m;

                // Start from the first month
                var currentMonth = new DateTime(startDate.Year, startDate.Month, 1);

                while (currentMonth <= endDate)
                {
                    // Calculate the DCA investment date for current month
                    int dayOfMonth = Math.Min(dcaDayOfMonth, DateTime.DaysInMonth(currentMonth.Year, currentMonth.Month));
                    var investmentDate = new DateTime(currentMonth.Year, currentMonth.Month, dayOfMonth);

                    // Don't process future investments
                    if (investmentDate > today)
                        break;

                    // Don't process if investment date is before our data starts
                    if (investmentDate < startDate)
                    {
                        currentMonth = currentMonth.AddMonths(1);
                        continue;
                    }

                    // Find the price on the investment date (or closest available)
                    var investmentPrice = FindPriceForInvestmentDate(prices, investmentDate);
                    if (investmentPrice == 0)
                    {
                        currentMonth = currentMonth.AddMonths(1);
                        continue;
                    }

                    // Make the investment - buy crypto with EUR
                    var cryptoBoughtThisMonth = monthlyInvestedAmountEUR / investmentPrice;
                    totalInvested += monthlyInvestedAmountEUR;
                    totalCrypto += cryptoBoughtThisMonth;

                    // Find the price at the end of this month to calculate current portfolio value
                    var endOfMonth = new DateTime(currentMonth.Year, currentMonth.Month, DateTime.DaysInMonth(currentMonth.Year, currentMonth.Month));
                    // Don't go beyond today or endDate
                    var evaluationDate = GetMinDate(endOfMonth, today, endDate);

                    var currentPrice = FindPriceForInvestmentDate(prices, evaluationDate);
                    if (currentPrice == 0)
                        currentPrice = investmentPrice; // fallback to investment price

                    // Calculate current value of entire portfolio at end of this month
                    var portfolioValue = totalCrypto * currentPrice;

                    // Calculate profit/loss and ROI for the entire portfolio up to this point
                    var profitLoss = portfolioValue - totalInvested;
                    var roi = totalInvested > 0 ? (profitLoss / totalInvested) * 100m : 0m;

                    results.Add(new DcaResultItem
                    {
                        Date = investmentDate,
                        MonthlyInvestedAmountEUR = monthlyInvestedAmountEUR,
                        CryptoAmount = cryptoBoughtThisMonth,
                        TotalCryptoAmount = totalCrypto,
                        TotalInvested = totalInvested,
                        TotalPortfolioValue = portfolioValue,
                        ProfitLoss = profitLoss,
                        ROI = roi
                    });

                    currentMonth = currentMonth.AddMonths(1);
                }

                return results;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CalculateDcaAsync] Error: {ex.Message}\n{ex.StackTrace}");
                await Shell.Current.DisplayAlert("Error", "An error occurred while calculating DCA results. Please try again.", "OK");
                return new List<DcaResultItem>();
            }
        }

        private async Task<DcaSummaryResult> CalculateSummaryDcaAsync(string symbol, DateTime startDate, DateTime endDate,
                                                                        decimal monthlyInvestedAmountEUR, int dcaDayOfMonth)
        {
            try
            {
                var detailedResults = await CalculateDcaAsync(symbol, startDate, endDate, monthlyInvestedAmountEUR, dcaDayOfMonth);

                if (!detailedResults.Any())
                {
                    // Return empty result to prevent UI crashes
                    return new DcaSummaryResult
                    {
                        TotalInvested = 0m,
                        TotalCrypto = 0m,
                        PortfolioTotalValue = 0m,
                        ProfitOrLoss = 0m,
                        ROI = 0m
                    };
                }

                // Get the final results from the last month
                var finalResult = detailedResults.Last();

                return new DcaSummaryResult
                {
                    TotalInvested = finalResult.TotalInvested,
                    TotalCrypto = finalResult.TotalCryptoAmount,
                    PortfolioTotalValue = finalResult.TotalPortfolioValue,
                    ProfitOrLoss = finalResult.ProfitLoss,
                    ROI = finalResult.ROI // Overall ROI for the entire investment period
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in CalculateSummaryDcaAsync: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "An error occurred while calculating DCA summary results. Please try again.", "OK");

                // Return empty result to prevent UI crashes
                return new DcaSummaryResult
                {
                    TotalInvested = 0m,
                    TotalCrypto = 0m,
                    PortfolioTotalValue = 0m,
                    ProfitOrLoss = 0m,
                    ROI = 0m
                };
            }
        }

        private static decimal FindPriceForInvestmentDate(List<CryptoPrice> prices, DateTime targetDate)
        {
            try
            {
                // Find exact date match
                var exactMatch = prices.FirstOrDefault(p => p.Date.Date == targetDate.Date);
                if (exactMatch != null)
                    return exactMatch.PriceUsd;

                // If no exact match, find the closest date within a reasonable range (±3 days)
                var closestPrice = prices
                    .Where(p => Math.Abs((p.Date.Date - targetDate.Date).TotalDays) <= 3)
                    .OrderBy(p => Math.Abs((p.Date.Date - targetDate.Date).TotalDays))
                    .FirstOrDefault();

                if (closestPrice != null)
                    return closestPrice.PriceUsd;

                // If no closest Price, find the latest available price before the target date
                var priceBeforeDate = prices
                    .Where(p => p.Date.Date <= targetDate.Date)
                    .OrderByDescending(p => p.Date)
                    .FirstOrDefault();

                if (priceBeforeDate != null)
                    return priceBeforeDate.PriceUsd;

                throw new InvalidOperationException($"No price data found for or near the target date: {targetDate:yyyy-MM-dd}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] Failed to find price for {targetDate:yyyy-MM-dd}: {ex.Message}");
                throw;
            }
        }

        private static DateTime GetMinDate(DateTime date1, DateTime date2, DateTime date3)
        {
            var min = date1;
            if (date2 < min) min = date2;
            if (date3 < min) min = date3;
            return min;
        }

        private void FillUpSummaryBlockProperties(DcaSummaryResult summary, string symbol)
        {
            if (summary == null)
                return;

            try
            {
                var euroCulture = CultureInfo.GetCultureInfo("de-DE");

                TotalInvested = summary.TotalInvested.ToString("C", euroCulture);
                TotalCrypto = $"{summary.TotalCrypto:F6} {symbol}";
                PortfolioTotalValue = summary.PortfolioTotalValue.ToString("C", euroCulture);
                ProfitOrLoss = summary.ProfitOrLoss.ToString("C", euroCulture);
                TotalRoi = $"{summary.ROI:F2} %";

                ProfitOrLossText = summary.ROI > 0 ? "Profit" : "Loss";
                RoiColorForConverter = summary.ROI;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Error] Failed to fill summary block: {ex.Message}");
            }
        }

        private async void ExecuteGoBackCommand()
        {
            await Shell.Current.GoToAsync($"//{nameof(HomePage)}", true);
        }

        #endregion

    }
}
