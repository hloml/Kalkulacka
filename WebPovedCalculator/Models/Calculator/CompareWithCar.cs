using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebPovedCalculator.Models
{
    public class CompareWithCar
    {

        // Car
        public float averageFuelConsumption { get; set; }
        public float literOfFuelPrice { get; set; }
        public float pathDistance { get; set; }
        public int parkingFee { get; set; }

        // Days
        [Display(Name = "Pondělí")]
        public Boolean mon { get; set; }
        [Display(Name = "Úterý")]
        public Boolean tue { get; set; }
        [Display(Name = "Středa")]
        public Boolean wed { get; set; }
        [Display(Name = "Čtvrtek")]
        public Boolean thu { get; set; }
        [Display(Name = "Pátek")]
        public Boolean fri { get; set; }
        [Display(Name = "Sobota")]
        public Boolean sat { get; set; }
        [Display(Name = "Neděle")]
        public Boolean sun { get; set; }




        public CompareWithCar()
        {
            averageFuelConsumption = 8.5f;
            literOfFuelPrice = 35f;
            pathDistance = 10f;
            parkingFee = 0;
            mon = true;
            tue = true;
            wed = true;
            thu = true;
            fri = true;
            sat = true;
            sun = true;
        }

    }
}