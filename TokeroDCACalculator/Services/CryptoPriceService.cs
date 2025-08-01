using System.Diagnostics;
using System.Text.Json;
using TokeroDCACalculator.Models;

namespace TokeroDCACalculator.Services
{
    public class CryptoPriceService : ICryptoPriceService
    {
        public async Task<List<CryptoPrice>> FetchPricesAsync(string coinId, string symbol, int days)
        {
            try
            {
                using var client = new HttpClient();
                string url = $"https://api.coingecko.com/api/v3/coins/{coinId}/market_chart?vs_currency=usd&days={days}";

                var response = await client.GetStringAsync(url);

                using var jsonDoc = JsonDocument.Parse(response);

                var root = jsonDoc.RootElement;
                if (!root.TryGetProperty("prices", out var pricesElement))
                {
                    return new List<CryptoPrice>();
                }

                var prices = new List<CryptoPrice>();

                foreach (var priceEntry in pricesElement.EnumerateArray())
                {
                    if (priceEntry.GetArrayLength() == 2)
                    {
                        long timestamp = priceEntry[0].GetInt64();
                        decimal price = priceEntry[1].GetDecimal();

                        prices.Add(new CryptoPrice
                        {
                            Symbol = symbol,
                            Date = DateTimeOffset.FromUnixTimeMilliseconds(timestamp).DateTime,
                            PriceUsd = price
                        });
                    }
                }

                return prices;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[FetchPricesAsync] Error fetching prices for {coinId}: {ex.Message}\n{ex.StackTrace}");
                await Shell.Current.DisplayAlert("Error", $"Failed to fetch prices for {symbol}. Please try again later.", "OK");
                return new List<CryptoPrice>();
            }
        }

    }
}
