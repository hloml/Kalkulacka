using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using WebPovedCalculator.Models.Calculator;


namespace WebPovedCalculator.Models
{
    /// <summary>
    /// Cares of counting tariffs
    /// </summary>
    public class Kalkulator
    {
        // values of tariff types
        public const int NUMBER_OF_DAYS = 123;
        private const int HALF_YEAR = 190;
        private const int ONE_YEAR = 380;
        private const int ONE_MONTH = 30;
        private const int TEN_MONTHS = 300;


        // names of tariff types
        private const String DAYS_TARIF = "denní";
        private const String HALF_YEAR_TARIF = "půlroční";
        private const String ONE_YEAR_TARIF = "roční";
        private const String ONE_MONTH_TARIF = "měsíční";
        private const String TEN_MONTHS_TARIF = "10 měsíční";


        // zones names
        public const String INNER_ZONE_NAME = "předplatné zóna 001 Plzeň";
        public const String OUTER_ZONE_NAME = "předplatné - vnější zóny";
        public const String NETWORK_ZONE_NAME = "síťové jízdné";


        // list of discounts names
        public static String[] DISCOUNTS_LIST = { "ISIC" };

        // categories for counting on holidays
        public const String ISIC = "ISIC";
        public const String fullFare = "plné jízdné";

        private static Dictionary<String, List<Tarif>> tarifDictionary;
        private const String EXCEL_NAME = "operational_tariff_v2.xls";
        private static String EXCEL_PATH;


        /// <summary>
        /// Get dictionary with informations about tariffs
        /// if dictionary is not initialized, than load it from excel
        /// </summary>
        /// <returns>dictionary filled with excel data</returns>
        public static Dictionary<String, List<Tarif>> GetExcel()
        {
            if (tarifDictionary == null)
            {
                EXCEL_PATH = HttpContext.Current.Server.MapPath("~/");
                tarifDictionary = TarifLoader.LoadExcel(EXCEL_PATH + EXCEL_NAME);
            }
            return tarifDictionary;
        }


        /// <summary>
        /// Method combine available tarifs for best posible price
        /// Possible tariffs are 380days(must start at 1.1), 190days(must start at 1 day of month), and 1..123 days 
        /// </summary>
        /// <param name="startDate">Date when customer start using transport</param>
        /// <param name="endDate">Date when customer end using transport</param>
        /// <param name="category">Customer category</param>
        /// <param name="zone">Zone name</param>
        /// <returns>TarifItemsContainer which contain list of recomended tarifs</returns>
        public static TarifItemsContainer CountTariff(DateTime startDate, DateTime endDate, String category, String zone)
        {
            float totalPrice=0, bestPrice = float.MaxValue;
            float price;

            List<TarifItem> tarifItems = new List<TarifItem>();

            int daysDifference = DaysDifference(startDate, endDate);
            int yearsDifference = endDate.Year - startDate.Year ;
            DateTime tmpDate;
            TarifItemsContainer tarifItemsContainer;
            List<TarifItem> bestTarifItems = new List<TarifItem>();

            if (yearsDifference > 1)    // year difference higher than one year
            {

                tarifItemsContainer = bestPriceForYear(startDate, new DateTime(startDate.Year, 12, 31), category, true, zone);  // count best price for start year
                totalPrice += tarifItemsContainer.price;
                tarifItems.AddRange(tarifItemsContainer.tarifsItems);

                price = Count380Price(category, zone);
                totalPrice = price * (yearsDifference -1);
                tmpDate = new DateTime(startDate.Year + 1, 1, 1);
               
                for (int i = 1; i < yearsDifference - 1; i++)                   // for years beetwen is best 380 tariff
                {
                    tarifItems.Add(CreateTarifItem(ONE_YEAR, tmpDate, tmpDate.AddDays(ONE_YEAR), price, ONE_YEAR_TARIF, category));
                    tmpDate = tmpDate.AddYears(1);
                }

                tmpDate = tmpDate.AddDays(-1).AddDays(ONE_YEAR);

                tarifItemsContainer = bestPriceForYear(tmpDate, endDate, category, true, zone);         // count best price for end year
                totalPrice += tarifItemsContainer.price;
                tarifItems.AddRange(tarifItemsContainer.tarifsItems);

                if (bestPrice > totalPrice)
                {
                    bestTarifItems = tarifItems.GetRange(0, tarifItems.Count);
                    
                    bestPrice = totalPrice;
                }
                tarifItems.Clear();

            }
            else if (yearsDifference == 1)  // year difference one year, try combine for each year
            {
                // count for each year separately
                tarifItemsContainer = bestPriceForYear(startDate, new DateTime(startDate.Year, 12, 31), category, true, zone);
                totalPrice += tarifItemsContainer.price;
                tarifItems.AddRange(tarifItemsContainer.tarifsItems);

                tarifItemsContainer = bestPriceForYear(new DateTime(endDate.Year, 1, 1), endDate, category, false, zone);
                totalPrice += tarifItemsContainer.price;
                tarifItems.AddRange(tarifItemsContainer.tarifsItems);

                if (bestPrice > totalPrice)
                {
                    bestTarifItems = tarifItems.ToList();
                    bestPrice = totalPrice;
                }
                tarifItems.Clear(); 

                // try to combine for both years 
                tarifItemsContainer = bestPriceForYear(startDate, endDate, category, true, zone);
                totalPrice = tarifItemsContainer.price;
                tarifItems.AddRange(tarifItemsContainer.tarifsItems);

                
                if (bestPrice > totalPrice)
                {
                    bestTarifItems = tarifItems.ToList();
                    bestPrice = totalPrice;
                }
                tarifItems.Clear();

            }
            else      // find best tariff for one year
            {
                tarifItemsContainer = bestPriceForYear(startDate, endDate, category, false, zone);
                totalPrice = tarifItemsContainer.price;
                tarifItems.AddRange(tarifItemsContainer.tarifsItems);

                if (bestPrice > totalPrice)
                {
                    bestTarifItems = tarifItems.ToList();
                    bestPrice = totalPrice;
                }
                tarifItems.Clear();
            }


            tarifItemsContainer.price = bestPrice;
            tarifItemsContainer.tarifsItems = bestTarifItems.ToList();
      
            return tarifItemsContainer;
        }



        /// <summary>
        /// Method combine best posible tariffs for year or less (for two years if parameter allowed is true)
        /// </summary>
        /// <param name="startDate">Date when tariff starts</param>
        /// <param name="endDate">Date when tariff ends</param>
        /// <param name="category">Customer category</param>
        /// <param name="allowed">Method can combine for next year</param>
        /// <param name="zone">Zone name</param>
        /// <returns>TarifItemsContainer which contain list of recomended tarifs</returns>
        public static TarifItemsContainer bestPriceForYear(DateTime startDate, DateTime endDate, String category, Boolean allowed, String zone)
        {
            int daysDifference = DaysDifference(startDate, endDate);
            float totalPrice = 0;        
            float price;
            DateTime tmpDate;

            List<TarifItem> tarifItems = new List<TarifItem>();
            List<TarifItem> bestTarifItems = new List<TarifItem>();
            TarifItemsContainer tarifItemsContainer;

            float bestPrice = Count380Price(category, zone);    // try 380day tariff first
            bestTarifItems.Add(CreateTarifItem(ONE_YEAR, new DateTime(startDate.Year, 1, 1), new DateTime(startDate.Year, 12, 31), bestPrice, ONE_YEAR_TARIF, category));

            if (allowed)    // try combine with next year, if its allowed
            {
                tarifItemsContainer = bestPriceForYear(new DateTime(startDate.Year + 1, 1, 1), endDate, category, false, zone);
                bestPrice += tarifItemsContainer.price;
                bestTarifItems.AddRange(tarifItemsContainer.tarifsItems);
            }


            if (daysDifference > HALF_YEAR)                                   // days difference higher than half year (try more combinations)
            {
                // combine 190 tariff from start of month and count remaining days with 1..123 tariffs
                tmpDate = startDate.AddDays(-startDate.Day + 1);
                price = Count190Price(category, zone);      
                totalPrice = price;
                tarifItems.Add(CreateTarifItem(HALF_YEAR, tmpDate, tmpDate.AddDays(HALF_YEAR), price, HALF_YEAR_TARIF, category));
                tmpDate = tmpDate.AddDays(HALF_YEAR);

                tarifItemsContainer = CountForRemainingDays(DaysDifference(tmpDate, endDate), tmpDate, category, zone);  
                totalPrice += tarifItemsContainer.price;      
                tarifItems.AddRange(tarifItemsContainer.tarifsItems);

                if (bestPrice > totalPrice)
                {
                    bestTarifItems = tarifItems.ToList();
                    bestPrice = totalPrice;
                }
                tarifItems.Clear();

                // count remaining days with 1..123 tariffs to start of next month then 
                //combine 190 tariff from start of month and count remaining days with 1..123 tariffs
                int daysInMonth = DateTime.DaysInMonth(startDate.Year, startDate.Month);
                tarifItemsContainer = CountForRemainingDays(daysInMonth - startDate.Day + 1, startDate, category, zone);     
                totalPrice = tarifItemsContainer.price;                                                                      
                tarifItems.AddRange(tarifItemsContainer.tarifsItems);

                tmpDate = startDate.AddDays(daysInMonth - startDate.Day + 1);

                price = Count190Price(category, zone);
                totalPrice += price;
                tarifItems.Add(CreateTarifItem(HALF_YEAR, tmpDate, tmpDate.AddDays(HALF_YEAR), price, HALF_YEAR_TARIF, category));
                
                tmpDate = tmpDate.AddDays(HALF_YEAR);
                tarifItemsContainer = CountForRemainingDays(DaysDifference(tmpDate, endDate), tmpDate, category, zone);      
                totalPrice += tarifItemsContainer.price;
                tarifItems.AddRange(tarifItemsContainer.tarifsItems);

                if (bestPrice > totalPrice)
                {
                    bestTarifItems = tarifItems.ToList();
                    bestPrice = totalPrice;
                }
                tarifItems.Clear();

                // try combinations with more than one 190 tarifs
                tarifItemsContainer = Combine190tarifs(startDate, endDate, category, zone);         
                totalPrice = tarifItemsContainer.price; 
                tarifItems.AddRange(tarifItemsContainer.tarifsItems);

                if (bestPrice > totalPrice)
                {
                    bestTarifItems = tarifItems.ToList();
                    bestPrice = totalPrice;
                }
                tarifItems.Clear();
            }
            else       // days difference smaller than half year (try another combinations)
            {
                int daysInMonth = DateTime.DaysInMonth(startDate.Year, startDate.Month);

                // count remaining days with 1..123 tariffs to start of next month then combine with 190 tariff
                tmpDate = startDate.AddDays(-(daysInMonth - startDate.Day + 1));
                tarifItemsContainer = CountForRemainingDays(daysInMonth - startDate.Day, tmpDate, category, zone);    
                totalPrice = tarifItemsContainer.price;
                tarifItems.AddRange(tarifItemsContainer.tarifsItems);

                price = Count190Price(category, zone);
                totalPrice += price;
                tarifItems.Add(CreateTarifItem(HALF_YEAR, tmpDate, tmpDate.AddDays(HALF_YEAR), price, HALF_YEAR_TARIF, category));
                
                if (bestPrice > totalPrice)
                {
                    bestTarifItems = tarifItems.ToList();
                    bestPrice = totalPrice;
                }
                tarifItems.Clear();

                // combine 190 tariff from start of month and count remaining days with 1..123 tariffs
                tmpDate = startDate.AddDays(-startDate.Day + 1);
                price = Count190Price(category, zone);              
                totalPrice = price;
                tarifItems.Add(CreateTarifItem(HALF_YEAR, tmpDate, tmpDate.AddDays(HALF_YEAR), price, HALF_YEAR_TARIF, category));
                tmpDate = startDate.AddDays(HALF_YEAR);

                tarifItemsContainer = CountForRemainingDays(DaysDifference(tmpDate, endDate), tmpDate, category, zone);    
                totalPrice += tarifItemsContainer.price;
                tarifItems.AddRange(tarifItemsContainer.tarifsItems);

                if (bestPrice > totalPrice)
                {
                    bestTarifItems = tarifItems.ToList();
                    bestPrice = totalPrice;
                }
                tarifItems.Clear();

                // combine only with 1..123 tariffs
                tarifItemsContainer = CountForRemainingDays(daysDifference, startDate, category, zone);     
                totalPrice = tarifItemsContainer.price;
                tarifItems.AddRange(tarifItemsContainer.tarifsItems);

                if (bestPrice > totalPrice)
                {
                    bestTarifItems = tarifItems.ToList();
                    bestPrice = totalPrice;
                }
                tarifItems.Clear();

            }

            TarifItemsContainer container = new TarifItemsContainer();
            container.tarifsItems = bestTarifItems.ToList();
            container.price = bestPrice;

            return container;
        }


        /// <summary>
        /// Combine best posible tariffs which contains combinations of 190days tariff for specific date
        /// </summary>
        /// <param name="startDate">Date when tariff starts</param>
        /// <param name="endDate">Date when tariff ends</param>
        /// <param name="category">Customer category</param>
        /// <param name="zone">Zone name</param>
        /// <returns>TarifItemsContainer which contain list of recomended tarifs</returns>
        public static TarifItemsContainer Combine190tarifs(DateTime startDate, DateTime endDate, String category, String zone)
        {
            float totalPrice;
            DateTime tmpDate;
            TarifItemsContainer container = new TarifItemsContainer();
            float price;
            List<TarifItem> tarifItems = new List<TarifItem>();
            TarifItemsContainer tarifItemsContainer;
            int daysDifference = DaysDifference(startDate, endDate);
            float bestPrice = float.MaxValue;
            List<TarifItem> bestTarifItems = new List<TarifItem>();

                //combine 190 tariff from start of month and count remaining days to next month with 1..123 tariffs
                // then 190 tariff and count remaining days to the end with 1..123 tariffs
                tmpDate = startDate.AddDays(-startDate.Day + 1);
                price = Count190Price(category, zone);                  
                totalPrice = price * 2;                                 
   
                tmpDate = tmpDate.AddDays(HALF_YEAR);

                tarifItems.Add(CreateTarifItem(HALF_YEAR, startDate.AddDays(-startDate.Day + 1), tmpDate, price, HALF_YEAR_TARIF, category));
               
                int daysInMonth = DateTime.DaysInMonth(tmpDate.Year, tmpDate.Month);
              
                tarifItemsContainer = CountForRemainingDays(daysInMonth - tmpDate.Day , tmpDate, category, zone);     
                totalPrice += tarifItemsContainer.price;
                tarifItems.AddRange(tarifItemsContainer.tarifsItems);

                tmpDate = tmpDate.AddDays(daysInMonth - tmpDate.Day + 1);
                tarifItems.Add(CreateTarifItem(HALF_YEAR, tmpDate, tmpDate.AddDays(HALF_YEAR), price, HALF_YEAR_TARIF, category));
                
                tarifItemsContainer = CountForRemainingDays(DaysDifference(tmpDate.AddDays(HALF_YEAR), endDate), tmpDate.AddDays(HALF_YEAR), category, zone);    
                totalPrice += tarifItemsContainer.price;
                tarifItems.AddRange(tarifItemsContainer.tarifsItems);

                if (bestPrice > totalPrice)
                {
                    bestTarifItems = tarifItems.ToList();
                    bestPrice = totalPrice;
                }
                tarifItems.Clear();

                // count remaining days to next month with 1..123 tariffs, add 190 tariff and count remaining days to next month with 1..123 tariffs
                // then 190 tariff and count remaining days to the end with 1..123 tariffs
                daysInMonth = DateTime.DaysInMonth(startDate.Year, startDate.Month);
                tarifItemsContainer = CountForRemainingDays(daysInMonth - startDate.Day, startDate, category, zone);     
                totalPrice = tarifItemsContainer.price;                                                                  
                tarifItems.AddRange(tarifItemsContainer.tarifsItems);

                tmpDate = startDate.AddDays(daysInMonth - startDate.Day + 1);
                price = Count190Price(category, zone);
                totalPrice += price * 2;
                tarifItems.Add(CreateTarifItem(HALF_YEAR, tmpDate, tmpDate.AddDays(HALF_YEAR), price, HALF_YEAR_TARIF, category));
                tmpDate = tmpDate.AddDays(HALF_YEAR);
                
                daysInMonth = DateTime.DaysInMonth(tmpDate.Year, tmpDate.Month);
                tarifItemsContainer = CountForRemainingDays(daysInMonth - tmpDate.Day + 1, tmpDate, category, zone);     
                totalPrice += tarifItemsContainer.price;
                tarifItems.AddRange(tarifItemsContainer.tarifsItems);

                tmpDate = tmpDate.AddDays(daysInMonth - tmpDate.Day + 1);
                tarifItems.Add(CreateTarifItem(HALF_YEAR, tmpDate, tmpDate.AddDays(HALF_YEAR), price, HALF_YEAR_TARIF, category));
                tarifItemsContainer = CountForRemainingDays(DaysDifference(tmpDate.AddDays(HALF_YEAR), endDate), tmpDate.AddDays(HALF_YEAR), category, zone);     
                totalPrice += tarifItemsContainer.price;
                tarifItems.AddRange(tarifItemsContainer.tarifsItems);

                if (bestPrice > totalPrice)
                {
                    bestTarifItems = tarifItems.ToList();
                    bestPrice = totalPrice;
                }
                tarifItems.Clear();


                // count remaining days to next month with 1..123 tariffs, add 190 tariff and another 190 tariff from start of month
                // then count remaining days to the end with 1..123 tariffs
                daysInMonth = DateTime.DaysInMonth(startDate.Year, startDate.Month);
                tarifItemsContainer = CountForRemainingDays(daysInMonth - startDate.Day + 1, startDate, category, zone);    
                totalPrice = tarifItemsContainer.price;
                tarifItems.AddRange(tarifItemsContainer.tarifsItems);

                tmpDate = startDate.AddDays(daysInMonth - startDate.Day + 1);
                price = Count190Price(category, zone);
                totalPrice += price * 2;
                tmpDate = tmpDate.AddDays(HALF_YEAR);

                tarifItems.Add(CreateTarifItem(HALF_YEAR, startDate.AddDays(daysInMonth - startDate.Day + 1), tmpDate, price, HALF_YEAR_TARIF, category));

                tmpDate = new DateTime(tmpDate.Year, tmpDate.Month, 1);
                tarifItems.Add(CreateTarifItem(HALF_YEAR, tmpDate, tmpDate.AddDays(HALF_YEAR), price, HALF_YEAR_TARIF, category));
                tarifItemsContainer = CountForRemainingDays(DaysDifference(tmpDate.AddDays(HALF_YEAR), endDate), tmpDate.AddDays(HALF_YEAR), category, zone);     
                totalPrice += tarifItemsContainer.price;
                tarifItems.AddRange(tarifItemsContainer.tarifsItems);
            
                if (bestPrice > totalPrice)
                {
                    bestTarifItems = tarifItems.ToList();
                    bestPrice = totalPrice;
                }
                tarifItems.Clear();

                // 190 tariff from start of month and another 190 tariff from start of month
                // then count remaining days to the end with 1..123 tariffs
                tmpDate = startDate.AddDays(-startDate.Day + 1);
                price = Count190Price(category, zone);
                totalPrice = price * 2;

                tmpDate = tmpDate.AddDays(HALF_YEAR);

                tarifItems.Add(CreateTarifItem(HALF_YEAR, startDate.AddDays(-startDate.Day + 1), tmpDate, price, HALF_YEAR_TARIF, category));

                daysInMonth = DateTime.DaysInMonth(tmpDate.Year, tmpDate.Month);
                tmpDate = tmpDate.AddDays(-tmpDate.Day + 1);
                tarifItems.Add(CreateTarifItem(HALF_YEAR, tmpDate, tmpDate.AddDays(HALF_YEAR), price, HALF_YEAR_TARIF, category));
                tarifItemsContainer = CountForRemainingDays(DaysDifference(tmpDate.AddDays(HALF_YEAR), endDate), tmpDate.AddDays(HALF_YEAR), category, zone);     //pricteme pocet dnu od zacatku mesice, protoze tarif na 190 dnu musi zacinat od 1 dne mesice
                totalPrice += tarifItemsContainer.price;
                tarifItems.AddRange(tarifItemsContainer.tarifsItems);

                if (bestPrice > totalPrice)
                {
                    bestTarifItems = tarifItems.ToList();
                    bestPrice = totalPrice;
                }
                tarifItems.Clear();

                container.price = bestPrice;
                container.tarifsItems = bestTarifItems.ToList();
                return container;
        }


       /// <summary>
       /// Count tariff for added number of days (even if its higher than 123)
       /// </summary>
       /// <param name="days">Number of day</param>
        /// <param name="startDate">Date when tariff starts</param>
       /// <param name="category">Customer category</param>
       /// <param name="zone">Zone name</param>
        /// <returns>TarifItemsContainer which contain list of recomended tarifs</returns>
        public static TarifItemsContainer CountForRemainingDays(int days, DateTime startDate, String category, String zone)
        {
            float totalPrice = 0;
            float price;
            DateTime tmpDate = startDate;
            TarifItemsContainer container = new TarifItemsContainer();
            
            List<TarifItem> tarifItems = new List<TarifItem>();
                    
            while (days > 0) {
                if (days > NUMBER_OF_DAYS)
                {
                    price = CountDaysPrice(NUMBER_OF_DAYS, category, zone);
                    totalPrice += price;
                    days -= NUMBER_OF_DAYS;
                    tarifItems.Add(CreateTarifItem(NUMBER_OF_DAYS, tmpDate, tmpDate.AddDays(NUMBER_OF_DAYS), price, DAYS_TARIF, category));
                    tmpDate = tmpDate.AddDays(NUMBER_OF_DAYS);
                }
                else
                {
                    price = CountDaysPrice(days, category, zone);
                    totalPrice += price;

                    tarifItems.Add(CreateTarifItem(days, tmpDate, tmpDate.AddDays(days), price, DAYS_TARIF, category));
                    days = 0;
                }          
            }

            container.tarifsItems = tarifItems.ToList();
            container.price = totalPrice;
            return container;
        }


        /// <summary>
        /// Get price for days difference from loaded tariffs 
        /// </summary>
        /// <param name="daysDifference">number of days (should be beetwen 1 and 123)</param>
        /// <param name="category">Customer category</param>
        /// <param name="zone">Zone name</param>
        /// <returns>Price</returns>
        public static float CountDaysPrice(int daysDifference, String category, String zone)
        {
            Tarif choosenTariff = TariffChooser(category, zone);
            if (choosenTariff == null) return float.MaxValue; // discount not found
            if (choosenTariff.DayTarif.Length < daysDifference || daysDifference < 1) // time between days is too long or less than 0
            {
                return float.MaxValue;
            }

            float daysPrice = choosenTariff.DayTarif[daysDifference];
            return daysPrice;
        }


        /// <summary>
        /// Method combine available tarifs for best posible price, counting for student is different
        /// Student have tarifs for 10 months, 1 month and for 1..123 days, can have discount ISIC for holidays
        /// </summary>
        /// <param name="startDate">Date when customer start using transport</param>
        /// <param name="endDate">Date when customer end using transport</param>
        /// <param name="category">Customer category</param>
        /// <param name="zone">Zone</param>
        /// <param name="isISIC">students ISIC discount</param>
        /// <returns>TarifItemsContainer which contain list of recomended tarifs</returns>
        public static TarifItemsContainer CountTariffForStudents(DateTime startDate, DateTime endDate, String category, String zone, Boolean isISIC)
        {
            float totalPrice = 0;
            List<TarifItem> tarifItems = new List<TarifItem>();
            DateTime tmpDate = startDate;
            TarifItemsContainer tarifItemsContainer = new TarifItemsContainer();
            DateTime tmp;

            do
            {
                if ((DateTime.Compare(startDate, new DateTime(startDate.Year, 8, 31)) < 0) && (DateTime.Compare(startDate, new DateTime(startDate.Year, 6, 1)) >= 0))    // tafiff starts on holidays
                {
                    tmpDate = MakeDatetimeForHolidays(tmpDate, endDate).AddDays(1);
                    tarifItemsContainer = CountForHoliday(startDate, tmpDate, category, zone, isISIC);
                    totalPrice += tarifItemsContainer.price;
                    tarifItems.AddRange(tarifItemsContainer.tarifsItems);
                }


                tmp = MakeDatetimeToHolidays(tmpDate, endDate);
                tarifItemsContainer = CountForSchoolYear(tmpDate, tmp, category, zone);     // count for school year except holidays
                totalPrice += tarifItemsContainer.price;
                tarifItems.AddRange(tarifItemsContainer.tarifsItems);
                tmpDate = tmp;

                if (DateTime.Compare(tmpDate, endDate) < 0)      // count for holidays
                {
                    tmp = MakeDatetimeForHolidays(tmpDate, endDate);
                    tarifItemsContainer = CountForHoliday(tmpDate.AddDays(1), tmp, category, zone, isISIC);
                    totalPrice += tarifItemsContainer.price;
                    tarifItems.AddRange(tarifItemsContainer.tarifsItems);
                    tmpDate = tmp.AddDays(1);
                }
            } while (DateTime.Compare(tmpDate, endDate) < 0);
           
            tarifItemsContainer.price = totalPrice;
            tarifItemsContainer.tarifsItems = tarifItems.ToList();

            return tarifItemsContainer;
        }

        /// <summary>
        /// Check date and modify if necessary to end before holidays, because tariff student can start at 1.9 and must end at 30.6 of next year
        /// if endDate is smaller than 30.6 of next year it return endDate, else it return 30.6 off next year
        /// </summary>
        /// <param name="startDate">Date when tariff starts</param>
        /// <param name="endDate">Date when tariff ends</param>
        /// <returns>Corrected date</returns>
        static DateTime MakeDatetimeToHolidays(DateTime startDate, DateTime endDate)
        {
            DateTime tmpDate = new DateTime(startDate.Year, 6, 30);

            if (DateTime.Compare(tmpDate, startDate) <= 0 )
            {
                tmpDate = tmpDate.AddYears(1);
            }

            if (DateTime.Compare(tmpDate, endDate) > 0)
            {
                tmpDate = endDate;
            }

            return tmpDate;
        }

        /// <summary>
        /// Check date and modify if necessary to end after holidays, because counting tariffs for students is different on holidays
        /// if endDate is smaller than end of holidays it return endDate, else it returns end of holidays
        /// </summary>
        /// <param name="startDate">Date when tariff starts</param>
        /// <param name="endDate">Date when tariff ends</param>
        /// <returns>Corrected date</returns>
        static DateTime MakeDatetimeForHolidays(DateTime startDate, DateTime endDate)
        {
            DateTime tmpDate = new DateTime(startDate.Year, 8, 31);

            if (DateTime.Compare(tmpDate, endDate) > 0)
            {
                tmpDate = endDate;
            }
            return tmpDate;
        }


       /// <summary>
       /// Count tariff for students on holidays, if he doesnt have isic then he pay full price
       /// with isic he has special tariff only for holidays
       /// </summary>
        /// <param name="startDate">Date when tariff starts</param>
        /// <param name="endDate">Date when tariff ends</param>
       /// <param name="category">Customer category</param>
       /// <param name="zone">Zone name</param>
       /// <param name="isISIC">has isic discount</param>
       /// <returns>Tariff</returns>
        static TarifItemsContainer CountForHoliday(DateTime startDate, DateTime endDate, String category, String zone, Boolean isISIC)
        {
            TarifItemsContainer tarifItemsContainer;
            if (isISIC == true)     
            {
                category = ISIC;
            }
            else 
            {
                category = fullFare;
            }
            tarifItemsContainer = CountForRemainingDays(DaysDifference(startDate, endDate) - 1, startDate, category, zone);
            return tarifItemsContainer;
        }


       /// <summary>
       /// Count best posible tariffs for students
       /// </summary>
       /// <param name="startDate">Date when tariff starts</param>
       /// <param name="endDate">Date when tariff ends</param>
       /// <param name="category">Customer category</param>
       /// <param name="zone">Zone name</param>
        /// <returns>TarifItemsContainer which contain list of recomended tarifs</returns>
        static TarifItemsContainer CountForSchoolYear(DateTime startDate, DateTime endDate, String category, String zone)
        {
            DateTime tmpDate = startDate;
            List<TarifItem> bestTarifItems = new List<TarifItem>();
            List<TarifItem> tarifItems = new List<TarifItem>();
            TarifItemsContainer tarifItemsContainer;
            float price=0;
            float totalPrice;
            float bestPrice = float.MaxValue;

            // get price for 10 months tariff
            price += Get10MonthsPrice(category, zone);          
            tarifItems.Add(CreateTarifItem(TEN_MONTHS, new DateTime(startDate.Year, 9, 1), new DateTime(startDate.Year + 1, 6, 30), price, TEN_MONTHS_TARIF, category));
            totalPrice = price;
           
            if (bestPrice > totalPrice)
            {
                bestTarifItems = tarifItems.ToList();
                bestPrice = totalPrice;
            }
            tarifItems.Clear();
            totalPrice = 0;

            // combine month tariff and 1..123 tariff
            if (!(tmpDate.Year == endDate.Year && tmpDate.Month == endDate.Month))  // Algorithm counts for tariff longer than one month
            {
                price = GetMonthPrice(category, zone);
                int daysInMonth = DateTime.DaysInMonth(startDate.Year, startDate.Month);

                tarifItemsContainer = CountForRemainingDays(daysInMonth - startDate.Day + 1, startDate, category, zone);     // count days to next month with 1..123 tariff
                totalPrice = tarifItemsContainer.price;
                tarifItems.AddRange(tarifItemsContainer.tarifsItems);
                tmpDate = startDate.AddDays(daysInMonth - startDate.Day + 1);

                while (tmpDate.Year != endDate.Year || tmpDate.Month != endDate.Month)
                {    // for each month beetwen start and end
                    tarifItems.Add(CreateTarifItem(ONE_MONTH, tmpDate, tmpDate.AddMonths(1), price, ONE_MONTH_TARIF, category));
                    totalPrice += price;
                    tmpDate = tmpDate.AddMonths(1);
                }

            }

            tarifItemsContainer = CountForRemainingDays(DaysDifference(tmpDate, endDate), tmpDate, category, zone);     // count days to end with 1..123 tariff
            totalPrice += tarifItemsContainer.price;
            tarifItems.AddRange(tarifItemsContainer.tarifsItems);
            
            if (bestPrice > totalPrice)
            {
                bestTarifItems = tarifItems.ToList();
                bestPrice = totalPrice;
            }
            tarifItems.Clear();

            tarifItemsContainer.price = bestPrice;
            tarifItemsContainer.tarifsItems = bestTarifItems.ToList();

            return tarifItemsContainer;
        }


        /// <summary>
        /// Get price for year tariff
        /// </summary>
        /// <param name="category">Customer category</param>
        /// <param name="zone">Zone name</param>
        /// <returns>price or -2 if price wasnt found</returns>
        public static float Count380Price(String category, String zone)
        {
            Tarif choosenTariff = TariffChooser(category, zone);
            if (choosenTariff == null) return -1; 

            int oneYearPrice;
            String yearsPriceString;
            if (!choosenTariff.Dictionary.TryGetValue("" + ONE_YEAR, out yearsPriceString)) return -2; // year prepay not found

            if (!Int32.TryParse(yearsPriceString, out oneYearPrice)) return float.MaxValue; // error in string to int
      
            return oneYearPrice;
        }

        /// <summary>
        /// Get price for half-year tariff
        /// </summary>
        /// <param name="category">Customer category</param>
        /// <param name="zone">Zone name</param>
        /// <returns>price or -2 if price wasnt found</returns>
        public static float Count190Price(String category, String zone)
        {
            Tarif choosenTariff = TariffChooser(category, zone);
            if (choosenTariff == null) return -1; // discount not found

            int oneSixMonthsPrice;
            String sixMonthsPriceString;
            if (!choosenTariff.Dictionary.TryGetValue("" + HALF_YEAR, out sixMonthsPriceString)) return -2; // half-year prepay not found

            if (!Int32.TryParse(sixMonthsPriceString, out oneSixMonthsPrice)) return float.MaxValue; // error in string to int
            return oneSixMonthsPrice;
        }

        /// <summary>
        /// Get price for 10 months tariff
        /// </summary>
        /// <param name="category">Customer category</param>
        /// <param name="zone">Zone name</param>
        /// <returns>price or -2 if price wasnt found</returns>
        public static float Get10MonthsPrice(String category, String zone)
        {
            Tarif choosenTariff = TariffChooser(category, zone);
            if (choosenTariff == null) return -1; // discount not found

            int price;
            String priceString;
            if (!choosenTariff.Dictionary.TryGetValue("10 měsíční", out priceString)) return -2; // 10 months prepay not found

            if (!Int32.TryParse(priceString, out price)) return float.MaxValue; // error in string to int
            return price;
        }

        /// <summary>
        /// Get price for one month tariff
        /// </summary>
        /// <param name="category">Customer category</param>
        /// <param name="zone">Zone name</param>
        /// <returns>price or -2 if price wasnt found</returns>
        public static float GetMonthPrice(String catrgory, String zone)
        {
            Tarif choosenTariff = TariffChooser(catrgory, zone);
            if (choosenTariff == null) return -1; // discount not found

            int price;
            String priceString;
            if (!choosenTariff.Dictionary.TryGetValue("měsíční", out priceString)) return -2; // monts prepay not found

            if (!Int32.TryParse(priceString, out price)) return float.MaxValue; // error in string to int
            return price;
        }




        /// <summary>
        /// Counts day difference between start and end date
        /// </summary>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <returns>Days difference</returns>
        public static int DaysDifference(DateTime startDate, DateTime endDate)
        {
            int daysDifference = (int)(endDate.Date - startDate.Date).TotalDays + 1;
            return daysDifference;
        }

        /// <summary>
        /// Select right tariff by category
        /// </summary>
        /// <param name="category">Customer category</param>
        /// <param name="zone">Zone name</param>
        /// <returns>Tariff</returns>
        public static Tarif TariffChooser(String category, String zone)
        {
            List<Tarif> listTariff = ListTariff(zone);
            if (listTariff == null) return null; // zones not found
            Tarif choosenTariff = null;
            foreach (Tarif tariff in listTariff)
            {
                if (tariff.category.Equals(category))
                {
                    choosenTariff = tariff;
                    break;
                }
            }
            return choosenTariff;
        }

        /// <summary>
        /// Return list of all tariffs (for categories)
        /// </summary>
        /// <param name="zone">Zone name</param>
        /// <returns>List of all tariffs in zone</returns>
        public static List<Tarif> ListTariff(String zone)
        {
            List<Tarif> listTariff;
            if (!tarifDictionary.TryGetValue(zone, out listTariff)) return null; // zones not found
            return listTariff;
        }


        /// <summary>
        /// Get method for counting best tariff
        /// </summary>
        /// <param name="category">Customer category</param>
        /// <param name="zone">Zone name</param>
        /// <returns>1 - normal counting, 2 - students counting, 0 - cant calculate</returns>
        public static int getCountingMethod(String category, String zone)
        {
            if (Count190Price(category, zone) >= 0 && Count380Price(category, zone) >= 0) return 1;
            if (Get10MonthsPrice(category, zone) >= 0 && GetMonthPrice(category, zone) >= 0) return 2;
            return 0;

        }

        /// <summary>
        /// Help method for creating tarifItems
        /// </summary>
        /// <param name="days">Days</param>
        /// <param name="dateStart">Start date</param>
        /// <param name="dateEnd">End date</param>
        /// <param name="price">Price</param>
        /// <param name="tarifName">Name of tariff</param>
        /// <param name="category">Category</param>
        /// <returns>Tariff item</returns>
        private static TarifItem CreateTarifItem(int days, DateTime dateStart, DateTime dateEnd, float price, String tarifName, String category)
        {
            return new TarifItem { days = days, dateStart = dateStart, dateEnd = dateEnd, price = price, TariffName = tarifName, category = category };
        }


    }
}
