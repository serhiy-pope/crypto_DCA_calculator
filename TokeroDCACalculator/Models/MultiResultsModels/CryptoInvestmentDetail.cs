using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TokeroDCACalculator.Models.MultiResultsModels
{
    public class CryptoInvestmentDetail
    {
        public string Symbol { get; set; }
        public decimal InvestedThisMonth { get; set; }
        public decimal CryptoBought { get; set; }
        public decimal TotalCryptoOwned { get; set; }
        public decimal CurrentValue { get; set; }
        public decimal TotalInvestedInThisCrypto { get; set; }
    }
}
