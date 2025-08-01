namespace TokeroDCACalculator.Models
{
    public class DcaResultItem
    {
        public DateTime Date { get; set; } 
        public decimal MonthlyInvestedAmountEUR { get; set; }
        public decimal CryptoAmount { get; set; }
        public decimal TotalCryptoAmount { get; set; }
        public decimal TotalInvested { get; set; }
        public decimal TotalPortfolioValue { get; set; }
        public decimal ProfitLoss { get; set; }
        public decimal ROI { get; set; }
    }
}
