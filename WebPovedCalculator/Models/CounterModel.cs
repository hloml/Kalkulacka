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

        [Display(Name = "Zóna 001 Plzeň")]
        public Boolean innerZone { get; set; }


        [Display(Name = "ISIC")]
        public Boolean discountISIC { get; set; }

        public List<TarifItem> tarifs { get; set; }

        public List<TarifItem> tarifsInner { get; set; }

        public Boolean isNetwork { get; set; }

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
            TarifItemsContainer containerInnerZone;
            TarifItemsContainer containerOuterZone;
            TarifItemsContainer containerNetworkZone;
            isNetwork = false;

            switch (Kalkulator.getCountingMethod(category, Kalkulator.OUTER_ZONE_NAME))
            {
                case 1:
                    containerOuterZone = Kalkulator.CountTariff(startDate, endDate, category, Kalkulator.OUTER_ZONE_NAME);
                    break;
                case 2:
                    containerOuterZone = Kalkulator.CountTariffForStudents(startDate, endDate, category, Kalkulator.OUTER_ZONE_NAME, discountISIC);
                    break;
                default:
                    return;
            }

            containerInnerZone = Kalkulator.CountTariff(startDate, endDate, category, Kalkulator.INNER_ZONE_NAME);
            containerNetworkZone = Kalkulator.CountTariff(startDate, endDate, category, Kalkulator.NETWORK_ZONE_NAME);

            if ((((innerZone ? 1 : 0) * containerInnerZone.price) + (zone * containerOuterZone.price)) < containerNetworkZone.price
                || (containerNetworkZone.price < 0)) //network zone doesnt contain category
            {
                price = (innerZone ? 1 : 0) * containerInnerZone.price + (zone * containerOuterZone.price);
                tarifs = containerOuterZone.tarifsItems.ToList();
                //TODO ?
            }
            else
            {
                price = containerNetworkZone.price;
                tarifs = containerNetworkZone.tarifsItems.ToList();
                isNetwork = true;
                //TODO ?
            }
            tarifsInner = containerInnerZone.tarifsItems.ToList();


            daysDifference = Kalkulator.DaysDifference(startDate, endDate);

        }

        private void MakeCategories(){
            List<Tarif> listTariff = Kalkulator.ListTariff(Kalkulator.OUTER_ZONE_NAME);
            string[] dontShowCategories = { "ISIC"};
            categories = new List<SelectListItem>();
            if (listTariff == null)
            {
                Console.WriteLine("ERR: List tarifu je null");
            }
            else
            {
                foreach (Tarif tariff in listTariff)
                {
                    if (!dontShowCategories.Contains(tariff.category))
                    {
                        categories.Add(new SelectListItem { Text = tariff.category, Value = tariff.category });
                    }
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