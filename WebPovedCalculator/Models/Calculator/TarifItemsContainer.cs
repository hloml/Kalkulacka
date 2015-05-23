using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebPovedCalculator.Models
{
    /// <summary>
    /// Container for list of TariffItems
    /// </summary>
    public class TarifItemsContainer
    {
        /// <summary>
        /// List of tariffs
        /// </summary>
        public List<TarifItem> tarifsItems { get; set;}

        /// <summary>
        /// Price
        /// </summary>
        public float price { get; set; }

    }
}
