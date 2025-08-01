using TokeroDCACalculator.Models;

namespace TokeroDCACalculator.Services
{
    public interface ICryptoPriceRepository
    {
        Task<bool> HasDataAsync();
        Task SavePricesAsync(List<CryptoPrice> prices);
        Task<DateTime?> GetLatestPriceDateAsync(string symbol);
        Task<List<CryptoPrice>> GetPricesAsync(string symbol, DateTime startDate, DateTime endDate);
    }
}
