using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TokeroDCACalculator.Models.MultiResultsModels
{
    public class MultiCryptoDcaResultItem
    {
        public DateTime Date { get; set; }
        public decimal TotalMonthlyInvestment { get; set; }
        public List<CryptoInvestmentDetail> CryptoDetails { get; set; } = new List<CryptoInvestmentDetail>();
        public decimal TotalPortfolioValue { get; set; }
        public decimal TotalInvested { get; set; }
        public decimal TotalProfitLoss { get; set; }
        public decimal TotalROI { get; set; }
    }
}
