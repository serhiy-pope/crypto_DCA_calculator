using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using TokeroDCACalculator.Models;
using TokeroDCACalculator.Models.MultiResultsModels;
using TokeroDCACalculator.Services;
using TokeroDCACalculator.Views;

namespace TokeroDCACalculator.ViewModels
{
    public partial class MultiResultsViewModel : BaseViewModel, IQueryAttributable
    {
        #region - Private fields

        private string _totalInvested;
        private string _totalCrypto;
        private string _portfolioTotalValue;
        private string _profitOrLoss;
        private string _profitOrLossText;
        private string _totalRoi;

        private int _totalMonthsCount;
        private int _selectedCryptosCount;
        private decimal _roiColorForConverter;

        private readonly ICryptoPriceRepository _cryptoPriceRepository;
        private ObservableCollection<CryptoSummaryDetail> _individualcryptoBreakdown;

        #endregion

        public MultiResultsViewModel(ICryptoPriceRepository cryptoPriceRepository)
        {
            _cryptoPriceRepository = cryptoPriceRepository;

            IndividualCryptoBreakdown = [];

            GoBackCommand = new Command(ExecuteGoBackCommand);
        }

        #region - Public Properties

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
        public int SelectedCryptosCount
        {
            get => _selectedCryptosCount;
            set
            {
                _selectedCryptosCount += value;
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

        public ObservableCollection<MultiCryptoDcaResultItem> MultiDcaResultsMonthly { get; } = new();
        public ObservableCollection<CryptoSummaryDetail> IndividualCryptoBreakdown
        { 
            get => _individualcryptoBreakdown;
            set
            {
                _individualcryptoBreakdown = value;
                OnPropertyChanged(); 
            }
        }

        #endregion

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

                if (query.TryGetValue("SelectedCryptos", out var selectedCryptosObj) &&
                    query.TryGetValue("StartDate", out var startDateObj) &&
                    query.TryGetValue("EndDate", out var endDateObj) &&
                    query.TryGetValue("TotalMonthlyInvestmentEUR", out var totalMonthlyInvestmentObj) &&
                    query.TryGetValue("DcaDayOfMonth", out var dcaDayObj) &&
                    query.TryGetValue("CryptoAllocations", out var cryptoAllocationsObj) &&
                    query.TryGetValue("SelectedCryptosCount", out var selectedCryptosCountObj) &&
                    query.TryGetValue("TotalMonths", out var totalMonthsObj))
                {
                    if (selectedCryptosObj is ObservableCollection<CryptoOption> selectedCryptos &&
                        startDateObj is DateTime startDate &&
                        endDateObj is DateTime endDate &&
                        totalMonthlyInvestmentObj is decimal totalMonthlyInvestment &&
                        dcaDayObj is int dcaDayOfMonth &&
                        cryptoAllocationsObj is List<CryptoAllocation> cryptoAllocations &&
                        selectedCryptosCountObj is int selectedCryptosCount &&
                        totalMonthsObj is int totalMonths)
                    {
                        Title = $"Portfolio DCA from {startDate:yyyy.MM.dd}";
                        SelectedCryptosCount = selectedCryptosCount;
                        TotalMonthsCount = totalMonths;
                        await GenerateMultiResults(cryptoAllocations, startDate, endDate, totalMonthlyInvestment, dcaDayOfMonth);
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

        private async Task GenerateMultiResults(List<CryptoAllocation> cryptoAllocations, DateTime startDate, DateTime endDate, 
            decimal totalMonthlyInvestment, int dcaDayOfMonth)
        {
            try
            {
                var detailedResults = await CalculateMultiCryptoDcaAsync(cryptoAllocations, startDate, endDate, totalMonthlyInvestment, dcaDayOfMonth);

                MultiDcaResultsMonthly.Clear();
                foreach (var result in detailedResults)
                    MultiDcaResultsMonthly.Add(result);

                var summary = await CalculateMultiCryptoDcaSummaryAsync(cryptoAllocations, startDate, endDate, totalMonthlyInvestment, dcaDayOfMonth);
                IndividualCryptoBreakdown.Clear();
                foreach (var crypto in summary.CryptoBreakdown)
                    IndividualCryptoBreakdown.Add(crypto);

                FillUpSummaryBlockProperties(summary);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Error] GenerateMultiResults failed: {ex.Message}");

                await Shell.Current.DisplayAlert( "Error", "An error occurred while calculating DCA results. Please try again.", "OK");
            }
        }

        private async Task<List<MultiCryptoDcaResultItem>> CalculateMultiCryptoDcaAsync(List<CryptoAllocation> cryptoAllocations,
            DateTime startDate, DateTime endDate, decimal totalMonthlyInvestmentEUR, int dcaDayOfMonth)
        {
            try
            {
                var results = new List<MultiCryptoDcaResultItem>();
                var today = DateTime.Today;

                // Validate percentages add up to 100%
                var totalPercentage = cryptoAllocations.Sum(c => c.Percentage);
                if (Math.Abs(totalPercentage - 100) > 0.01m)
                    throw new ArgumentException("Crypto allocations must add up to 100%");

                // Track running totals for each cryptocurrency
                var cryptoTotals = cryptoAllocations.ToDictionary(
                    c => c.Symbol,
                    c => new { TotalCrypto = 0m, TotalInvested = 0m }
                );

                var currentMonth = new DateTime(startDate.Year, startDate.Month, 1);

                while (currentMonth <= endDate)
                {
                    int dayOfMonth = Math.Min(dcaDayOfMonth, DateTime.DaysInMonth(currentMonth.Year, currentMonth.Month));
                    var investmentDate = new DateTime(currentMonth.Year, currentMonth.Month, dayOfMonth);

                    if (investmentDate > today || investmentDate < startDate)
                    {
                        if (investmentDate > today) break;
                        currentMonth = currentMonth.AddMonths(1);
                        continue;
                    }

                    var monthResult = new MultiCryptoDcaResultItem
                    {
                        Date = investmentDate,
                        TotalMonthlyInvestment = totalMonthlyInvestmentEUR
                    };

                    decimal totalPortfolioValue = 0m;
                    decimal totalInvestedSoFar = 0m;
                    bool allPricesFound = true;

                    // Process each cryptocurrency
                    foreach (var allocation in cryptoAllocations)
                    {
                        // Get prices for this crypto
                        var prices = await _cryptoPriceRepository.GetPricesAsync(allocation.Symbol, startDate, endDate);
                        if (prices == null || !prices.Any())
                        {
                            allPricesFound = false;
                            continue;
                        }

                        // Find investment price
                        var investmentPrice = FindPriceForInvestmentDate(prices, investmentDate);
                        if (investmentPrice == 0)
                        {
                            allPricesFound = false;
                            continue;
                        }

                        // Calculate investment amount for this crypto
                        var investmentAmountForThisCrypto = totalMonthlyInvestmentEUR * (allocation.Percentage / 100m);
                        var cryptoBought = investmentAmountForThisCrypto / investmentPrice;

                        // Update running totals for this crypto
                        var updatedCryptoTotal = new
                        {
                            TotalCrypto = cryptoTotals[allocation.Symbol].TotalCrypto + cryptoBought,
                            TotalInvested = cryptoTotals[allocation.Symbol].TotalInvested + investmentAmountForThisCrypto
                        };
                        cryptoTotals[allocation.Symbol] = updatedCryptoTotal;

                        // Find current price for portfolio valuation
                        var evaluationDate = GetMinDate(
                            new DateTime(currentMonth.Year, currentMonth.Month, DateTime.DaysInMonth(currentMonth.Year, currentMonth.Month)),
                            today,
                            endDate
                        );
                        var currentPrice = FindPriceForInvestmentDate(prices, evaluationDate);
                        if (currentPrice == 0) currentPrice = investmentPrice;

                        var currentValueForThisCrypto = updatedCryptoTotal.TotalCrypto * currentPrice;
                        totalPortfolioValue += currentValueForThisCrypto;

                        monthResult.CryptoDetails.Add(new CryptoInvestmentDetail
                        {
                            Symbol = allocation.Symbol,
                            InvestedThisMonth = investmentAmountForThisCrypto,
                            CryptoBought = cryptoBought,
                            TotalCryptoOwned = updatedCryptoTotal.TotalCrypto,
                            CurrentValue = currentValueForThisCrypto,
                            TotalInvestedInThisCrypto = updatedCryptoTotal.TotalInvested
                        });
                    }

                    if (!allPricesFound)
                    {
                        currentMonth = currentMonth.AddMonths(1);
                        continue;
                    }

                    // Calculate total invested across all cryptos up to this point
                    totalInvestedSoFar = cryptoTotals.Values.Sum(v => v.TotalInvested);

                    monthResult.TotalPortfolioValue = totalPortfolioValue;
                    monthResult.TotalInvested = totalInvestedSoFar;
                    monthResult.TotalProfitLoss = totalPortfolioValue - totalInvestedSoFar;
                    monthResult.TotalROI = totalInvestedSoFar > 0 ? (monthResult.TotalProfitLoss / totalInvestedSoFar) * 100m : 0m;

                    results.Add(monthResult);
                    currentMonth = currentMonth.AddMonths(1);
                }

                return results;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CalculateMultiCryptoDcaAsync] Error: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        private async Task<MultiCryptoDcaSummary> CalculateMultiCryptoDcaSummaryAsync(List<CryptoAllocation> cryptoAllocations,
            DateTime startDate, DateTime endDate, decimal totalMonthlyInvestmentEUR, int dcaDayOfMonth)
        {
            try
            {
                var detailedResults = await CalculateMultiCryptoDcaAsync(cryptoAllocations, startDate, endDate, totalMonthlyInvestmentEUR, dcaDayOfMonth);

                if (!detailedResults.Any())
                    return null;

                var lastResult = detailedResults.Last();

                var summary = new MultiCryptoDcaSummary
                {
                    TotalInvested = lastResult.TotalInvested,
                    PortfolioTotalValue = lastResult.TotalPortfolioValue,
                    TotalProfitLoss = lastResult.TotalProfitLoss,
                    TotalROI = lastResult.TotalROI
                };

                // Add breakdown by cryptocurrency
                foreach (var cryptoDetail in lastResult.CryptoDetails)
                {
                    var profitLoss = cryptoDetail.CurrentValue - cryptoDetail.TotalInvestedInThisCrypto;
                    var roi = cryptoDetail.TotalInvestedInThisCrypto > 0
                        ? (profitLoss / cryptoDetail.TotalInvestedInThisCrypto) * 100m
                        : 0m;

                    summary.CryptoBreakdown.Add(new CryptoSummaryDetail
                    {
                        Symbol = cryptoDetail.Symbol,
                        TotalInvested = cryptoDetail.TotalInvestedInThisCrypto,
                        TotalCryptoOwned = cryptoDetail.TotalCryptoOwned,
                        CurrentValue = cryptoDetail.CurrentValue,
                        ProfitLoss = profitLoss,
                        ROI = roi
                    });
                }

                return summary;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CalculateMultiCryptoDcaSummaryAsync] Error: {ex.Message}\n{ex.StackTrace}");
                throw;
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

        private void FillUpSummaryBlockProperties(MultiCryptoDcaSummary summary)
        {
            if (summary == null)
                return;

            try
            {
                var euroCulture = CultureInfo.GetCultureInfo("de-DE");
                TotalInvested = summary.TotalInvested.ToString("C", euroCulture);
                PortfolioTotalValue = summary.PortfolioTotalValue.ToString("C", euroCulture);
                ProfitOrLoss = summary.TotalProfitLoss.ToString("C", euroCulture);
                TotalRoi = $"{summary.TotalROI:F2} %";

                ProfitOrLossText = summary.TotalROI > 0 ? "Profit" : "Loss";
                RoiColorForConverter = summary.TotalROI;
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
