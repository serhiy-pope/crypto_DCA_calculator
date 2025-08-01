using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TokeroDCACalculator.Models.MultiResultsModels
{
    public class CryptoAllocation
    {
        public string Symbol { get; set; }
        public decimal Percentage { get; set; } // e.g., 25 for 25%
    }
}
