using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebPovedCalculator.Models.Calculator
{
    public class TariffParameters
    {
        /// <summary>
        /// Category
        /// </summary>
        public String category { get; set; }

        public String zone { get; set; }

        public Boolean isISIC { get; set; }

        public Boolean discountsSchool { get; set; }

    }
}