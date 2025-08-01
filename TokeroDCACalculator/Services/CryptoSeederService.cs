using System.Diagnostics;

namespace TokeroDCACalculator.Services
{
    public class CryptoSeederService : ICryptoSeederService
    {
        #region - Private fields

        private readonly ICryptoPriceService _priceService;
        private readonly ICryptoPriceRepository _repository;

        #endregion

        public CryptoSeederService(ICryptoPriceService priceService, ICryptoPriceRepository repository)
        {
            _priceService = priceService;
            _repository = repository;
        }

        public async Task SeedAsync()
        {
            var coins = new List<(string Id, string Symbol)>
            {
                ("bitcoin", "BTC"),
                ("ethereum", "ETH"),
                ("solana", "SOL"),
                ("ripple", "XRP")
            };

            foreach (var (id, symbol) in coins)
            {
                try
                {
                    var latestDate = await _repository.GetLatestPriceDateAsync(symbol);
                    var today = DateTime.UtcNow.Date;

                    // If latestDate is null, default to 1 year ago
                    var startDate = latestDate?.AddDays(1) ?? today.AddYears(-1);

                    if (startDate > today)
                        continue;

                    var daysToFetch = (today - startDate).Days + 1;
                    daysToFetch = Math.Min(daysToFetch, 365); // Cap at 365 days as per API limits

                    var prices = await _priceService.FetchPricesAsync(id, symbol, daysToFetch);
                    await _repository.SavePricesAsync(prices);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[SeedAsync] Error updating prices for {symbol}: {ex.Message}\n{ex.StackTrace}");
                    await Shell.Current.DisplayAlert("Error", $"Failed to update prices for {symbol}. Please try again later.", "OK");
                }
            }
        }


    }
}
