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
        /// <summary>
        /// Days
        /// </summary>
        public int days { get; set; }

        /// <summary>
        /// Start date
        /// </summary>
        public DateTime dateStart { get; set; }

        /// <summary>
        /// End date
        /// </summary>
        public DateTime dateEnd { get; set; }

        /// <summary>
        /// Price
        /// </summary>
        public float price { get; set; }

        /// <summary>
        /// Tariff's name
        /// </summary>
        public String TariffName { get; set; }

        /// <summary>
        /// Category
        /// </summary>
        public String category { get; set; }
    }
}
