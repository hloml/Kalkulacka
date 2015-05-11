using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebPovedCalculator.Models
{
    public class CounterModel
    {


        [Required(ErrorMessage = "Zadejte prosím datum")]
        [Display(Name = "Od")]
        [DataType(DataType.Date)]
        public DateTime startDate { get; set; }

        [Required(ErrorMessage = "Zadejte prosím datum")]
        [Display(Name = "Do")]
        [DataType(DataType.Date)]
        public DateTime endDate { get; set; }

        public float price { get; set; }
        
        [Display(Name = "Kategorie")]
        public String category { get; set; }

        public List<SelectListItem> categories { get; set; }

        [Display(Name = "Vnějších zón")]
        public int zone { get; set; }

        public List<SelectListItem> zones { get; set; }

        [Display(Name = "Plzeň město")]
        public String innerZoneName { get; set; }

        public Boolean innerZone { get; set; }

        public List<TarifItem> tarifs { get; set; }

        public int daysDifference { get; set; }

        public CounterModel()
        {
            Dictionary<String, List<Tarif>> excel = Kalkulator.GetExcel();

            MakeCategories();
            MakeZones();
            // Set time for calendars
            startDate = DateTime.Today;
            endDate = DateTime.Today.AddYears(1);


        }

        public void GetPrice()
        {
            TarifItemsContainer container;



            switch (Kalkulator.getCountingMethod(category))
            {
                case 1:
                    container = Kalkulator.CountTariff(startDate, endDate, category);
                    break;
                case 2:
                    container = Kalkulator.CountTariffForStudents(startDate, endDate, category);
                    break;
                default:
                    return;
            }

            price = container.price;
            tarifs = container.tarifsItems.ToList();
            daysDifference = Kalkulator.DaysDifference(startDate, endDate);

            // TODO - number of zones
            int numberOfZones = zone;
            if (innerZone)
            {
                numberOfZones++;
            }
            if ((numberOfZones * price) < Kalkulator.GetNetworkFare(category))
            {
                price = numberOfZones * price;
            }
            else
            {
                price = Kalkulator.GetNetworkFare(category);
            }
        }

        private void MakeCategories(){
            List<Tarif> listTariff = Kalkulator.ListTariff("vnejsi"); // TODO
            categories = new List<SelectListItem>();
            if (listTariff == null)
            {
                Console.WriteLine("ERR: List tarifu je null");
            }
            else
            {
                foreach (Tarif tariff in listTariff)
                {
                    categories.Add(new SelectListItem { Text = tariff.category, Value = tariff.category });
                }
            }
        }

        private void MakeZones()
        {
            zones = new List<SelectListItem>();
            for (int i = 0; i < 8; i++)
            {
                zones.Add(new SelectListItem { Text = "" + i, Value = "" + i });
            }
        }
    }
}