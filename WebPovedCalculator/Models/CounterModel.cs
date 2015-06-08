using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebPovedCalculator.Models.Calculator;

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
        /// if tariff's enddate is after user's enddate
        /// </summary>
        public Boolean tariffIsLonger { get; set; }

        public CompareWithCar compare { get; set; }

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
            endDate = DateTime.Today;

            compare = new CompareWithCar();
        }


        /// <summary>
        /// Counts prices for client's demands
        /// </summary>
        public void CountPrice()
        {
            // counts days difference
            daysDifference = Kalkulator.DaysDifference(startDate, endDate);

            switch (category)
            {
                case Kalkulator.ztpFare:          //ZTP
                    GetPrice("free", Kalkulator.ztpFare, Kalkulator.ztpFare);
                    if (innerZone && !isNetwork)
                    {
                    }
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
                       GetPrice(Kalkulator.halfFare, Kalkulator.studentFare, Kalkulator.studentFare);
                    }
                    else
                    {
                        GetPrice(Kalkulator.halfFare, Kalkulator.fullFare, Kalkulator.fullFare);
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
                        GetPriceForJanskehoDiscount("");
                    }
                    else
                    {
                        GetPrice(Kalkulator.halfFare, Kalkulator.fullFare, Kalkulator.pensionerFare);
                    }
                    break;
                case Kalkulator.pensionerTo70Fare:    //důchodce (65 - 70 let)
                    if (discountsJanskeho)
                    {
                        GetPriceForJanskehoDiscount("");
                    }
                    else
                    {
                        GetPrice(Kalkulator.halfFare, Kalkulator.pensionerFare, Kalkulator.pensionerFare);
                    }
                    break;
                case Kalkulator.pensioner70AndMoreFare:            //důchodce (70 a více let)
                    if (discountsJanskeho)
                    {
                        GetPriceForJanskehoDiscount("free");
                    }
                    else
                    {
                        GetPrice("free", Kalkulator.pensionerFare, Kalkulator.pensionerFare);
                    }
                    break;
                case Kalkulator.childFare:                //dítě (do 6 let)
                    price = 0;
                    break;
                default:
                    return;
            }
            setNotes();  // get notes for category
        }


        /// <summary>
        /// Method set notes for customer category
        /// </summary>
        private void setNotes()
        {
            String value = "";
            // get notes for category
            if (discountsJanskeho)
            {
                value = haveNote(category, "janskeho");

                if (!String.IsNullOrEmpty(value))
                {
                    note = value;
                }
            }
            if (discountsSchool)
            {
                value = haveNote(category, "student");

                if (!String.IsNullOrEmpty(value))
                {
                    note += value;
                }
            }

            if (innerZone && !isNetwork)
            {
                value = haveNote(category, "vnitřní zóna");

                if (!String.IsNullOrEmpty(value))
                {
                    note += value;
                }
            }

            value = haveNote(category, "category");

            if (!String.IsNullOrEmpty(value))
            {
                note += value;
            }
        }


        /// <summary>
        /// Get for category and note type from dictionary with notes
        /// </summary>
        /// <param name="category">Customer category</param>
        /// <param name="noteType">type of note(discount, category, inner zone)</param>
        /// <returns>Note or empty string, if there is not note</returns>
        private String haveNote(String category, String noteType)
        {
            Dictionary<String, String> dictionary;
            String value;

            if (Kalkulator.getNotes().TryGetValue(category, out dictionary))
            {
                if (dictionary.TryGetValue(noteType, out value))
                {
                    return value;
                }
            }
            return "";
        }


        /// <summary>
        /// Count best price for janskeho discount 
        /// </summary>
        /// <param name="categoryInner">pensioner can have free tariff in inner zone</param>
        public void GetPriceForJanskehoDiscount(string categoryInner)
        {
            TarifItemsContainer containerInnerZone;
            TariffParameters parameters;

            if (categoryInner.Equals("free"))
            {
                containerInnerZone = new TarifItemsContainer();
                containerInnerZone.price = 0;
                containerInnerZone.tarifsItems = new List<TarifItem>();
            }
            else
            {
                parameters = new TariffParameters { category = Kalkulator.ZONES, zone = Kalkulator.DISCOUNT_ZONE_NAME, isISIC = discountISIC, discountsSchool = discountsSchool };
                containerInnerZone= Kalkulator.CountPriceForJanskehoPensioner("Zlatá Jánského Plaketa", parameters, startDate, endDate);
            }

            parameters = new TariffParameters { category = Kalkulator.ZONES, zone = Kalkulator.DISCOUNT_ZONE_NAME, isISIC = discountISIC, discountsSchool = discountsSchool };
            TarifItemsContainer containerOuterZone = Kalkulator.CountPriceForJanskehoPensioner("Zlatá Jánského Plaketa", parameters, startDate, endDate);
            parameters.category = Kalkulator.NETWORK_ZONE;
            TarifItemsContainer containerNetworkZone = Kalkulator.CountPriceForJanskehoPensioner("Zlatá Jánského Plaketa", parameters, startDate, endDate);

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

            // 7 and more zones is network
            if ((((innerZone ? 1 : 0)  + zone) >= 7)
                && (containerNetworkZone.price >= 0))
            {
                price = containerNetworkZone.price;
                tarifs = containerNetworkZone.tarifsItems.ToList();
                isNetwork = true;
            }

            // if tariff's enddate is after user's enddate
            tariffIsLonger = false;
            if (zone > 0)
            {
                if (tarifs != null)
                {
                    if (DateTime.Compare(tarifs.Last<TarifItem>().dateEnd, endDate) > 0)
                    {
                        tariffIsLonger = true;
                    }
                }
            }
            if (!isNetwork && innerZone)
            {
                if (tarifsInner != null)
                {
                    if (DateTime.Compare(tarifsInner.Last<TarifItem>().dateEnd, endDate) > 0)
                    {
                        tariffIsLonger = true;
                    }
                }
            }
            //

            tarifsInner = containerInnerZone.tarifsItems.ToList();

        }


        /// <summary>
        /// Counts prices for client's demands
        /// </summary>
        public void GetPrice(String innerCategory, String outerCategory, String networkCategory)
        {
            TarifItemsContainer containerInnerZone;
            TarifItemsContainer containerOuterZone;
            TarifItemsContainer containerNetworkZone;
            TariffParameters parameters;


            isNetwork = false;
            int countingMethod = 1;
            if (outerCategory.Equals(Kalkulator.studentFare) || outerCategory.Equals(Kalkulator.schoolFare))
            {
                countingMethod = 2;
            }

            parameters = new TariffParameters { category = outerCategory, zone = Kalkulator.OUTER_ZONE_NAME, isISIC = discountISIC, discountsSchool = discountsSchool };

            // selects how will be tariff for outer zones counted and count it
            switch (countingMethod)
            {
                case 1:
                    containerOuterZone = Kalkulator.CountTariff(startDate, endDate, parameters);
                    break;
                case 2:
                    containerOuterZone = Kalkulator.CountTariffForStudents(startDate, endDate, parameters);
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
                parameters = new TariffParameters { category = innerCategory, zone = Kalkulator.INNER_ZONE_NAME, isISIC = discountISIC, discountsSchool = discountsSchool };
                containerInnerZone = Kalkulator.CountTariff(startDate, endDate, parameters);
            }


            if (((innerZone ? 1 : 0) + zone) >= 7)      // network zone for students and school is different (students pay full Fare, for school half Fare)
            {
                if (networkCategory.Equals(Kalkulator.studentFare) || discountISIC)
                {
                    networkCategory = Kalkulator.fullFare;
                }
                if (networkCategory.Equals(Kalkulator.schoolFare)) {
                    networkCategory = Kalkulator.halfFare;
                }
            }

            parameters = new TariffParameters { category = networkCategory, zone = Kalkulator.NETWORK_ZONE_NAME, isISIC = discountISIC, discountsSchool = discountsSchool };
            containerNetworkZone = Kalkulator.CountTariff(startDate, endDate, parameters);

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



            // 7 and more zones is network
            if ((((innerZone ? 1 : 0) + zone) >= 7)
                && (containerNetworkZone.price >= 0))
            {
                price = containerNetworkZone.price;
                tarifs = containerNetworkZone.tarifsItems.ToList();
                isNetwork = true;
            }

            // if tariff's enddate is after user's enddate
            tariffIsLonger = false;
            if (zone > 0)
            {
                if (tarifs != null)
                {
                    if (DateTime.Compare(tarifs.Last<TarifItem>().dateEnd, endDate) > 0)
                    {
                        tariffIsLonger = true;
                    }
                }
            }
            if (!isNetwork && innerZone)
            {
                if (tarifsInner != null)
                {
                    if (DateTime.Compare(tarifsInner.Last<TarifItem>().dateEnd, endDate) > 0)
                    {
                        tariffIsLonger = true;
                    }
                }
            }
            //

                tarifsInner = containerInnerZone.tarifsItems.ToList();

            

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