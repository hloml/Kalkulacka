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
        /// Client's demand of school discount
        /// </summary>
        [Display(Name = "žákovské jízdné")]
        public Boolean discountsSchool { get; set; }

        /// <summary>
        /// Client's demand of ISIC discount
        /// </summary>
        [Display(Name = "Zlatá Jánského Plaketa")]
        public Boolean discountsJanskeho { get; set; }


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
        /// Note for category
        /// </summary>
        public String note { get; set; }

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


        public void CountPrice()
        {
            switch (category)
            {
                case Kalkulator.ztpFare:          //ZTP
                    GetPrice("free", Kalkulator.ztpFare, Kalkulator.ztpFare);
                    note = "Cestující se ve vozidlech PMDP prokazuje občanským průkazem, ve vozidlech ostatních dopravců se musí prokázat Plzeňskou kartou s nahraným bezplatným tarifem";
                    break;
                case Kalkulator.adultFare:           //Dospely
                    if (discountsJanskeho)
                    {
                        GetPrice(Kalkulator.halfFare, Kalkulator.halfFare, Kalkulator.halfFare);
                    }
                    else
                    {
                        GetPrice(Kalkulator.fullFare, Kalkulator.fullFare, Kalkulator.fullFare);
                    }         
                    break;
                case Kalkulator.studentFare:              //student (15 - 26let)
                    if (discountsSchool)
                    {
                        GetPrice(Kalkulator.studentFare, Kalkulator.studentFare, Kalkulator.studentFare);
                    }
                    else
                    {
                        GetPrice(Kalkulator.fullFare, Kalkulator.fullFare, Kalkulator.fullFare);
                    }
                    break;
                case Kalkulator.businessFare:            //firemní
                    GetPrice(Kalkulator.businessFare, Kalkulator.businessFare, Kalkulator.businessFare);
                    break;
                case Kalkulator.adolescentFare:            //dítě (6 - 15 let)
                    if (discountsSchool)
                    {
                        GetPrice(Kalkulator.schoolFare, Kalkulator.schoolFare, Kalkulator.schoolFare);
                    }
                    else
                    {
                        GetPrice(Kalkulator.halfFare, Kalkulator.halfFare, Kalkulator.halfFare);
                    }
                    break;
                case Kalkulator.pensionerTo65Fare:         //důchodce (do 65 let)
                    if (discountsJanskeho)
                    {
                        GetPriceForJanskehoDiscount();
                        return;
                    }
                    GetPrice(Kalkulator.halfFare, Kalkulator.fullFare, Kalkulator.pensionerFare);
                    break;
                case Kalkulator.pensionerTo70Fare:    //důchodce (65 - 70 let)
                    if (discountsJanskeho)
                    {
                        GetPriceForJanskehoDiscount();
                        return;
                    }
                    GetPrice(Kalkulator.halfFare, Kalkulator.pensionerFare, Kalkulator.pensionerFare);
                    break;
                case Kalkulator.pensioner70AndMoreFare:            //důchodce (70 a více let)
                    if (discountsJanskeho)
                    {
                        GetPriceForJanskehoDiscount();
                        return;
                    }
                    GetPrice("free", Kalkulator.pensionerFare, Kalkulator.pensionerFare);
                    note = "Cestující se ve vozidlech PMDP prokazuje občanským průkazem, ve vozidlech ostatních dopravců se musí prokázat Plzeňskou kartou s nahraným bezplatným tarifem";
                    break;
                case Kalkulator.childFare:                //dítě (do 6 let)
                    price = 0;
                    note = "cestující s platným jízdním dokladem IDP má nárok na bezplatnou přepravu dvou dětí do 6 let";
                    break;
                default:
                    return;
            }
        }


        public void GetPriceForJanskehoDiscount()
        {
            TarifItemsContainer containerInnerZone = Kalkulator.CountPriceForJanskehoPensioner("Zlatá Jánského Plaketa", Kalkulator.ZONES, startDate, endDate, Kalkulator.DISCOUNT_ZONE_NAME);
            TarifItemsContainer containerOuterZone = Kalkulator.CountPriceForJanskehoPensioner("Zlatá Jánského Plaketa", Kalkulator.ZONES, startDate, endDate, Kalkulator.DISCOUNT_ZONE_NAME);
            TarifItemsContainer containerNetworkZone = Kalkulator.CountPriceForJanskehoPensioner("Zlatá Jánského Plaketa", Kalkulator.NETWORK_ZONE, startDate, endDate, Kalkulator.DISCOUNT_ZONE_NAME);

            if ((((innerZone ? 1 : 0) * containerInnerZone.price) + (zone * containerOuterZone.price)) < containerNetworkZone.price
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
        /// Counts prices for client's demands
        /// </summary>
        public void GetPrice(String innerCategory, String outerCategory, String networkCategory)
        {
            TarifItemsContainer containerInnerZone;
            TarifItemsContainer containerOuterZone;
            TarifItemsContainer containerNetworkZone;
            isNetwork = false;
            int countingMethod = 1;
            if (outerCategory.Equals(Kalkulator.studentFare) || outerCategory.Equals(Kalkulator.schoolFare))
            {
                countingMethod = 2;
            }
            

            // selects how will be tariff for outer zones counted and count it
            switch (countingMethod)
            {
                case 1:
                    containerOuterZone = Kalkulator.CountTariff(startDate, endDate, outerCategory, Kalkulator.OUTER_ZONE_NAME);
                    break;
                case 2:
                    containerOuterZone = Kalkulator.CountTariffForStudents(startDate, endDate, outerCategory, Kalkulator.OUTER_ZONE_NAME, discountISIC);
                    break;
                default:
                    return;
            }

            // count tariff for inner zone and all (network) zones
            if (innerCategory.Equals("free"))
            {
                containerInnerZone = new TarifItemsContainer();
                containerInnerZone.price = 0;
                containerInnerZone.tarifsItems = new List<TarifItem>();
            }
            else
            {
                containerInnerZone = Kalkulator.CountTariff(startDate, endDate, innerCategory, Kalkulator.INNER_ZONE_NAME);
            }

            containerNetworkZone = Kalkulator.CountTariff(startDate, endDate, networkCategory, Kalkulator.NETWORK_ZONE_NAME);

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

                foreach (String tariff in Kalkulator.categoriesList)
                {
                    categories.Add(new SelectListItem { Text = tariff, Value = tariff });
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