using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebPovedCalculator.Models
{
    /// <summary>
    /// Model of MVC for counting tariffs
    /// </summary>
    public class CounterModel
    {

        /// <summary>
        /// Client's demand of start date of tariff
        /// </summary>
        [Required(ErrorMessage = "Zadejte prosím datum")]
        [Display(Name = "Od")]
        [DataType(DataType.Date)]
        public DateTime startDate { get; set; }

        /// <summary>
        /// Client's demand of end date of tariff
        /// </summary>
        [Required(ErrorMessage = "Zadejte prosím datum")]
        [Display(Name = "Do")]
        [DataType(DataType.Date)]
        public DateTime endDate { get; set; }

        /// <summary>
        /// Final price of counted tariff
        /// </summary>
        public float price { get; set; }
        
        /// <summary>
        /// Client's demand of category
        /// </summary>
        [Display(Name = "Kategorie")]
        public String category { get; set; }

        /// <summary>
        /// Available list of categories
        /// </summary>
        public List<SelectListItem> categories { get; set; }

        /// <summary>
        /// Client's demand of number of outer zones
        /// </summary>
        [Display(Name = "Vnějších zón")]
        public int zone { get; set; }

        /// <summary>
        /// Available list of number of outer zones
        /// </summary>
        public List<SelectListItem> zones { get; set; }

        /// <summary>
        /// Client's demand of inner zone
        /// </summary>
        [Display(Name = "Zóna 001 Plzeň")]
        public Boolean innerZone { get; set; }

        /// <summary>
        /// Client's demand of ISIC discount
        /// </summary>
        [Display(Name = "ISIC")]
        public Boolean discountISIC { get; set; }

        /// <summary>
        /// List of counted tariffs for outer zone  OR  all (network) zones
        /// </summary>
        public List<TarifItem> tarifs { get; set; }

        /// <summary>
        /// List of counted tariffs for inner zone
        /// </summary>
        public List<TarifItem> tarifsInner { get; set; }

        /// <summary>
        /// True if counted tariffs are all (network) zones
        /// </summary>
        public Boolean isNetwork { get; set; }

        /// <summary>
        /// Days difference for start and end date of tariffs
        /// </summary>
        public int daysDifference { get; set; }

        /// <summary>
        /// Sets default values for client's demands
        /// </summary>
        public CounterModel()
        {
            Dictionary<String, List<Tarif>> excel = Kalkulator.GetExcel();

            MakeCategories();
            MakeZones();
            // Set time for calendars
            startDate = DateTime.Today;
            endDate = DateTime.Today.AddYears(1);


        }

        /// <summary>
        /// Counts prices for client's demands
        /// </summary>
        public void GetPrice()
        {
            TarifItemsContainer containerInnerZone;
            TarifItemsContainer containerOuterZone;
            TarifItemsContainer containerNetworkZone;
            isNetwork = false;
            int countingMethod = Kalkulator.getCountingMethod(category, Kalkulator.OUTER_ZONE_NAME);    

            // selects how will be tariff for outer zones counted and count it
            switch (countingMethod)
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

            // count tariff for inner zone and all (network) zones
            containerInnerZone = Kalkulator.CountTariff(startDate, endDate, category, Kalkulator.INNER_ZONE_NAME);
            containerNetworkZone = Kalkulator.CountTariff(startDate, endDate, category, Kalkulator.NETWORK_ZONE_NAME);

            // selects if tariffs are for all (network) zones
            if (countingMethod == 2  ||        // students dont have network zone
                (((innerZone ? 1 : 0) * containerInnerZone.price) + (zone * containerOuterZone.price)) < containerNetworkZone.price
                || (containerNetworkZone.price < 0)) //network zone doesnt contain category
            {
                price = (innerZone ? 1 : 0) * containerInnerZone.price + (zone * containerOuterZone.price);
                tarifs = containerOuterZone.tarifsItems.ToList();
            }
            else
            {
                price = containerNetworkZone.price;
                tarifs = containerNetworkZone.tarifsItems.ToList();
                isNetwork = true;
            }
            tarifsInner = containerInnerZone.tarifsItems.ToList();
            // counts days difference
            daysDifference = Kalkulator.DaysDifference(startDate, endDate);

        }

        /// <summary>
        /// Creates list of available categories
        /// </summary>
        private void MakeCategories(){
            List<Tarif> listTariff = Kalkulator.ListTariff(Kalkulator.OUTER_ZONE_NAME);
           
            categories = new List<SelectListItem>();
            if (listTariff == null)
            {
                Console.WriteLine("ERR: List tarifu je null");
            }
            else
            {
                foreach (Tarif tariff in listTariff)
                {
                    if (!Kalkulator.DISCOUNTS_LIST.Contains(tariff.category))
                    {
                        categories.Add(new SelectListItem { Text = tariff.category, Value = tariff.category });
                    }
                }
            }
        }

        /// <summary>
        /// Creates list of available number of outer zones
        /// </summary>
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