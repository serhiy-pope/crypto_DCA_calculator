using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TokeroDCACalculator.Models.MultiResultsModels
{
    public class MultiCryptoDcaSummary
    {
        public decimal TotalInvested { get; set; }
        public decimal PortfolioTotalValue { get; set; }
        public decimal TotalProfitLoss { get; set; }
        public decimal TotalROI { get; set; }
        public List<CryptoSummaryDetail> CryptoBreakdown { get; set; } = new List<CryptoSummaryDetail>();
    }
}
