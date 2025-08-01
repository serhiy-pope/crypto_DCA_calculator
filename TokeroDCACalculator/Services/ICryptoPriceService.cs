using TokeroDCACalculator.Models;

namespace TokeroDCACalculator.Services
{
    public interface ICryptoPriceService
    {
        Task<List<CryptoPrice>> FetchPricesAsync(string coinId, string symbol, int days);

    }
}
