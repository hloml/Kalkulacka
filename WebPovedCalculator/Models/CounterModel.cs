using Kalkulacka;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebPovedCalculator.Models
{
    public class CounterModel
    {
        public CounterModel()
        {
            Kalkulator.LoadExcelOnce();
        }

        public String GetPrice()
        {
         DateTime startDateG;
         DateTime endDateG;
         String discount;
         startDateG = new DateTime(2015, 9, 1);
         endDateG = new DateTime(2016, 4, 1);
         discount = "plne";
            return Kalkulator.CountTariff(startDateG, endDateG, discount);
        }
    }
}