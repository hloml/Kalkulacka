using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebPovedCalculator.Models
{

    /// <summary>
    /// Store informations about chossen tariff
    /// when tariff starts and end, his price and number of days for its
    /// tariffName for his identification and category of customer
    /// </summary>
    public class TarifItem
    {
        public int days { get; set; }

        public DateTime dateStart { get; set; }

        public DateTime dateEnd { get; set; }

        public float price { get; set; }

        public String TariffName { get; set; }

        public String category { get; set; }
    }
}
