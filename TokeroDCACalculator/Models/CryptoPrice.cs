using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace TokeroDCACalculator.Models
{
    public class CryptoPrice
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Symbol { get; set; }

        public DateTime Date { get; set; }

        public decimal PriceUsd { get; set; }
    }
}
