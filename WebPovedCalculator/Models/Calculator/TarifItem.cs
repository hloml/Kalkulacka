using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebPovedCalculator.Models
{
    public class TarifItem
    {
        public int days { get; set; }
        public DateTime dateStart { get; set; }

        public DateTime dateEnd { get; set; }

        public float price { get; set; }
    }
}
