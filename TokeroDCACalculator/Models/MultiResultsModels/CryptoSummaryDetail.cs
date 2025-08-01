using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TokeroDCACalculator.Models.MultiResultsModels
{
    public class CryptoSummaryDetail
    {
        public string Symbol { get; set; }
        public decimal TotalInvested { get; set; }
        public decimal TotalCryptoOwned { get; set; }
        public decimal CurrentValue { get; set; }
        public decimal ProfitLoss { get; set; }
        public decimal ROI { get; set; }
    }
}
