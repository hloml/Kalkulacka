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
        public List<TarifItem> tarifsItems {get; set;}

        public float price { get; set; }

    }
}
