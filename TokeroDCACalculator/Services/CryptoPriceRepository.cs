using SQLite;
using System.Diagnostics;
using TokeroDCACalculator.Models;

namespace TokeroDCACalculator.Services
{
    public class CryptoPriceRepository : ICryptoPriceRepository
    {
        #region - Private fields

        private readonly SQLiteAsyncConnection _db;

        #endregion

        public CryptoPriceRepository()
        {
            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "crypto_prices.db");
            _db = new SQLiteAsyncConnection(dbPath);
            _db.CreateTableAsync<CryptoPrice>().Wait();
        }

        #region - Public methods

        public async Task<bool> HasDataAsync()
        {
            var first = await _db.Table<CryptoPrice>().FirstOrDefaultAsync();
            return first != null;
        }

        public async Task SavePricesAsync(List<CryptoPrice> prices)
        {
            try
            {
                foreach (var price in prices)
                {
                    // Prevent duplicates (symbol + date)
                    var exists = await _db.Table<CryptoPrice>()
                                          .Where(p => p.Symbol == price.Symbol && p.Date == price.Date)
                                          .FirstOrDefaultAsync();

                    if (exists == null)
                        await _db.InsertAsync(price);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SavePricesAsync] Error: {ex.Message}\n{ex.StackTrace}");
                throw; // Let caller handle user notification or retry
            }
        }

        public async Task<DateTime?> GetLatestPriceDateAsync(string symbol)
        {
            try
            {
                var latest = await _db.Table<CryptoPrice>()
                    .Where(p => p.Symbol == symbol)
                    .OrderByDescending(p => p.Date)
                    .FirstOrDefaultAsync();

                return latest?.Date;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[GetLatestPriceDateAsync] Error: {ex.Message}\n{ex.StackTrace}");
                return null;
            }
        }

        public async Task<List<CryptoPrice>> GetPricesAsync(string symbol, DateTime startDate, DateTime endDate)
        {
            try
            {
                return await _db.Table<CryptoPrice>()
                                .Where(p => p.Symbol == symbol && p.Date >= startDate && p.Date <= endDate)
                                .OrderBy(p => p.Date)
                                .ToListAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[GetPricesAsync] Error fetching prices for {symbol}: {ex.Message}\n{ex.StackTrace}");
                await Shell.Current.DisplayAlert("Error", $"Failed to retrieve prices for {symbol}. Please try again later.", "OK");
                return new List<CryptoPrice>();
            }
        }


        #endregion

    }
}
