using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebPovedCalculator.Models.Calculator
{
    /// <summary>
    /// Storing informations about customer (category, zone and his discounts)
    /// </summary>
    public class TariffParameters
    {

        public String category { get; set; }

        public String zone { get; set; }

        public Boolean isISIC { get; set; }

        public Boolean discountsSchool { get; set; }

    }
}