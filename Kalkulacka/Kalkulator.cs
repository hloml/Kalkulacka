﻿using Microsoft.Office.Interop.Excel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Kalkulacka
{
    public class Kalkulator
    {
        private const int NUMBER_OF_DAYS = 123;
        private const int HALF_YEAR = 190;
        private const int ONE_YEAR = 380;

        private static Dictionary<String, List<Tarif>> tarifDictionary;
        private const String EXCEL_NAME = "operational_tariff.xls";

        // testing parammeters for console
        static DateTime startDateG;
        static DateTime endDateG;
        static String discount;
        static String defaultZone = "vnejsi";


        public static void Main(string[] args)
        {
            String tariffPath = System.IO.Path.GetFullPath("..\\..\\" + EXCEL_NAME);
            tarifDictionary = LoadExcel(tariffPath);

            startDateG = new DateTime(2015, 1, 1);
            endDateG = new DateTime(2015, 6, 1);
            discount = "plne";
            CountTariff(startDateG, endDateG, discount);


            endDateG = endDateG.AddDays(500);
            startDateG = endDateG.AddDays(300);

            startDateG = new DateTime(2015, 10, 25);
            endDateG = new DateTime(2019, 6, 1);
            discount = "plne";
            CountTariff(startDateG, endDateG, discount);



            startDateG = new DateTime(2015, 10, 1);
            endDateG = new DateTime(2016, 4, 1);
            discount = "plne";
            CountTariff(startDateG, endDateG, discount);


            startDateG = new DateTime(2015, 9, 1);
            endDateG = new DateTime(2016, 4, 1);
            discount = "plne";
            CountTariff(startDateG, endDateG, discount);

            Console.ReadLine();
            //Console.WriteLine("-Pocitani tarifu-");
            //Console.WriteLine();
            //while (true)
            //{
            //    InsertValues();
            //    String countedTariff = CountTariff(startDateG, endDateG, discount);

            //}
        }

        public static Dictionary<String, List<Tarif>> GetExcel(String path)
        {
            if (tarifDictionary == null)
            {
                tarifDictionary = LoadExcel(path + EXCEL_NAME);
            }
            return tarifDictionary;
        }


        // Load tarifs for all zones from excel file and save them into dictionary
        public static Dictionary<String, List<Tarif>> LoadExcel(string filename)
        {
            Application xlsApp = new Application();

            if (!System.IO.File.Exists(filename))
            {
                Console.WriteLine("File " + filename + " doesnt exist");
                return null;
            }

            if (xlsApp == null)
            {
                Console.WriteLine("EXCEL could not be started. Check that your office installation and project references are correct.");
                return null;
            }

            Workbook wb = xlsApp.Workbooks.Open(filename,
                0, true, 5, "", "", true, XlPlatform.xlWindows, "\t", false, false, 0, true);
            Sheets sheets = wb.Worksheets;

            Dictionary<String, List<Tarif>> tarifs_dictionary = new Dictionary<string, List<Tarif>>();

            for (int list_index = 1; list_index <= sheets.Count; list_index++)
            {    // iterate over all lists from excel

                Worksheet ws = (Worksheet)sheets.get_Item(list_index);

                int columns_count = ws.UsedRange.Columns.Count;
                string[] category = new string[columns_count];
                float[,] days_tarif = new float[columns_count, NUMBER_OF_DAYS + 1];      // for saving values for 1..123 days

                Dictionary<String, String>[] dictionary = new Dictionary<string, string>[columns_count];     // for other values, storing them with key (380 dni, 180dni, mesicni ..)

                for (int j = 0; j < columns_count; j++)      // initiate array of dictionaries
                {
                    dictionary[j] = new Dictionary<string, string>();
                }

                IterateOverAllCells(ws, dictionary, category, days_tarif);       // iterate all cells in actual list

                List<Tarif> tarifs = SaveTarifsToList(category, days_tarif, dictionary);    // create object tarif for each category and save objects to arraylist
                tarifs_dictionary.Add(ws.Name, tarifs);      // save tarifs to dictionary with zone name as a key

            }
            return tarifs_dictionary;
        }



        // iterate over all cells in excel and find all categories. For categories fill array (for 1..123 days) and dictionary obtained from parameters
        private static void IterateOverAllCells(Worksheet ws, Dictionary<String, String>[] dictionary, string[] category, float[,] days_tarif)
        {
            bool isNumeric, isCategory;
            float column_value;
            int column_index, columns_count = ws.UsedRange.Columns.Count, first_column;
            string first_column_string;


            foreach (Range row in ws.UsedRange.Rows)        // iterate all rows
            {
                column_index = 0;
                first_column = -1;
                isCategory = false;
                first_column_string = "";

                foreach (Range cell in row.Columns)     // iterate all columns
                {
                    column_index++;

                    if (cell.Value2 != null)    // cell is not empty
                    {
                        isNumeric = float.TryParse(cell.Value2.ToString(), out column_value);

                        if (isNumeric)  // cell value is number
                        {
                            if (column_index == 1)     // first column
                            {
                                first_column = (int)Math.Ceiling(column_value); ;
                            }
                            else if (first_column != -1)    // first column is number (1, 123)
                            {
                                if (first_column > 0 && first_column <= NUMBER_OF_DAYS)
                                {
                                    days_tarif[column_index, first_column] = column_value;
                                }
                                else
                                {
                                    dictionary[column_index].Add(first_column.ToString(), cell.Value2.ToString());
                                }
                            }
                            else if (!string.IsNullOrEmpty(first_column_string))    // first column is not number (mesicni, 380 dni)
                            {
                                dictionary[column_index].Add(first_column_string, cell.Value2.ToString());
                            }
                        }
                        else     // cell value is string
                        {
                            String s = cell.Value2.ToString();
                            if (column_index == 1)  //  first column
                            {
                                switch (s)
                                {
                                    case "dny": isCategory = true; break;
                                    default: first_column_string = s; break;
                                }

                            }
                            else if (isCategory == true)    // actual row values are category
                            {
                                category[column_index] = cell.Value2.ToString();
                            }
                            else     // some other value, save to dictionary
                            {
                                if (first_column != -1)
                                {
                                    dictionary[column_index].Add(first_column.ToString(), s);
                                }
                                else if (!string.IsNullOrEmpty(first_column_string))
                                {
                                    dictionary[column_index].Add(first_column_string, s);
                                }
                            }
                        }
                    }
                }
            }

        }



        // create object tarif for all categories and return arrayList with created objects.
        private static List<Tarif> SaveTarifsToList(string[] category, float[,] days_tarif, Dictionary<String, String>[] dictionary)
        {
            float[] tmp_array = new float[NUMBER_OF_DAYS + 1];
            List<Tarif> tarifs = new List<Tarif>();
            for (int i = 0; i < category.Length; i++)
            {
                if (!string.IsNullOrEmpty(category[i]))
                {
                    Tarif tarif = new Tarif();
                    tarif.category = category[i];

                    tmp_array = new float[NUMBER_OF_DAYS + 1];
                    for (int j = 0; j <= NUMBER_OF_DAYS; j++)
                        tmp_array[j] = days_tarif[i, j];

                    tarif.DayTarif = tmp_array;
                    tarif.Dictionary = dictionary[i];
                    tarifs.Add(tarif);
                }
            }

            return tarifs;
        }



        // counts prices for day, year and half-year tariffs
        public static TarifItemsContainer CountTariff(DateTime startDate, DateTime endDate, String discount)
        {
            float price=0, bestPrice = float.MaxValue;

            List<TarifItem> tarifItems = new List<TarifItem>();

            int daysDifference = DaysDifference(startDate, endDate);
            int yearsDifference = endDate.Year - startDate.Year ;
            DateTime tmpDate;
            TarifItemsContainer tarifItemsContainer;
            List<TarifItem> bestTarifItems = new List<TarifItem>();

            if (yearsDifference > 1)    // roky mezi nima maj rocni jizdny a prvni a posledni dopocitat
            {

                tarifItemsContainer = bestPriceForYear(startDate, new DateTime(startDate.Year, 12, 31), discount);
                price += tarifItemsContainer.price;
                tarifItems.AddRange(tarifItemsContainer.tarifsItems);
                
                price = Count380Price(startDate, endDate, discount) * (yearsDifference -1);
                tmpDate = new DateTime(startDate.Year + 1, 1, 1);
                for (int i = 1; i < yearsDifference - 1; i++)
                {
                    tarifItems.Add(new TarifItem { days = ONE_YEAR, dateStart = tmpDate, dateEnd = new DateTime(tmpDate.Year, 12, 31) });
                    tmpDate = tmpDate.AddYears(1);
                }

                tarifItemsContainer = bestPriceForYear(new DateTime(endDate.Year, 1, 1), endDate, discount);
                price += tarifItemsContainer.price;
                tarifItems.AddRange(tarifItemsContainer.tarifsItems);

                if (bestPrice > price)
                {
                    bestTarifItems = tarifItems.GetRange(0, tarifItems.Count);
                    
                    bestPrice = price;
                }
                tarifItems.Clear();

            }
            else if (yearsDifference == 1)  // musime zkusit zkombinovat mezi obema rokama
            {
                tarifItemsContainer = bestPriceForYear(startDate, new DateTime(startDate.Year, 12, 31), discount);
                price += tarifItemsContainer.price;
                tarifItems.AddRange(tarifItemsContainer.tarifsItems);

                tarifItemsContainer = bestPriceForYear(new DateTime(endDate.Year, 1, 1), endDate, discount);
                price += tarifItemsContainer.price;
                tarifItems.AddRange(tarifItemsContainer.tarifsItems);

                if (bestPrice > price)
                {
                    bestTarifItems = tarifItems.ToList();
                    bestPrice = price;
                }
                tarifItems.Clear();

                tarifItemsContainer = bestPriceForYear(startDate, endDate, discount);
                price = tarifItemsContainer.price;
                tarifItems.AddRange(tarifItemsContainer.tarifsItems);


                if (bestPrice > price)
                {
                    bestTarifItems = tarifItems.ToList();
                    bestPrice = price;
                }
                tarifItems.Clear();

            }
            else                        //pro jeden rok kouknout co se nejvic vyplati
            {
                tarifItemsContainer = bestPriceForYear(startDate, endDate, discount);
                price = tarifItemsContainer.price;
                tarifItems.AddRange(tarifItemsContainer.tarifsItems);

                if (bestPrice > price)
                {
                    bestTarifItems = tarifItems.ToList();
                    bestPrice = price;
                }
                tarifItems.Clear();
            }


            tarifItemsContainer.price = bestPrice;
            tarifItemsContainer.tarifsItems = bestTarifItems.ToList();
      
            return tarifItemsContainer;
        }



        public static TarifItemsContainer bestPriceForYear(DateTime startDate, DateTime endDate, String discount)
        {
            int daysDifference = DaysDifference(startDate, endDate);
            float price = 0;
            float bestPrice = Count380Price(startDate, endDate, discount);    // zkusime rocni
            DateTime tmpDate;

            List<TarifItem> tarifItems = new List<TarifItem>();
            List<TarifItem> bestTarifItems = new List<TarifItem>();
            TarifItemsContainer tarifItemsContainer;

            bestTarifItems.Add(new TarifItem { days = HALF_YEAR, dateStart = new DateTime(startDate.Year, 1, 1), dateEnd = new DateTime(startDate.Year + 1, 12, 31) });


            if (daysDifference > HALF_YEAR)                                   // vetsi nez pulrocni, zkusime nakombinovat
            {
                price = Count190Price(startDate, endDate, discount);

                tarifItems.Add(new TarifItem { days = HALF_YEAR, dateStart = startDate, dateEnd = startDate.AddDays(HALF_YEAR)});
                tarifItemsContainer = CountForRemainingDays(daysDifference - HALF_YEAR + startDate.Day, startDate.AddDays(HALF_YEAR), discount);   //pricteme pocet dnu od zacatku mesice, protoze tarif na 190 dnu musi zacinat od 1 dne mesice

                price += tarifItemsContainer.price;      
                tarifItems.AddRange(tarifItemsContainer.tarifsItems);


                if (bestPrice > price)
                {
                    bestTarifItems = tarifItems.ToList();
                    bestPrice = price;
                }
                tarifItems.Clear();


                int daysInMonth = DateTime.DaysInMonth(startDate.Year, startDate.Month);

                tarifItemsContainer = CountForRemainingDays(daysInMonth - startDate.Day, startDate.AddDays(-(daysInMonth - startDate.Day)), discount); // musime spocitat pocet dnu do startu mesice, protoze tarif na 190 dnu musi zacinat od 1 dne mesice      
                price = tarifItemsContainer.price;
                tarifItems.AddRange(tarifItemsContainer.tarifsItems);

                tmpDate = startDate.AddDays(HALF_YEAR);
                tarifItems.Add(new TarifItem { days = HALF_YEAR, dateStart = startDate, dateEnd = tmpDate });
                price += Count190Price(startDate, endDate, discount);

                tarifItemsContainer = CountForRemainingDays(daysDifference - HALF_YEAR, tmpDate, discount);       //odecteme pocet dnu od zacatku mesice, protoze tarif na 190 dnu musi zacinat od 1 dne mesice

                price += tarifItemsContainer.price;
                tarifItems.AddRange(tarifItemsContainer.tarifsItems);



                if (bestPrice > price)
                {
                    bestTarifItems = tarifItems.ToList();
                    bestPrice = price;
                }
                tarifItems.Clear();



                price = Count190Price(startDate, endDate, discount) * 2;
                tmpDate = startDate.AddDays(HALF_YEAR - startDate.Day);
                tarifItems.Add(new TarifItem { days = HALF_YEAR, dateStart = startDate.AddDays(-startDate.Day), dateEnd = tmpDate });
                tarifItems.Add(new TarifItem { days = HALF_YEAR, dateStart = tmpDate, dateEnd = tmpDate.AddDays(HALF_YEAR) });


                tarifItemsContainer = CountForRemainingDays(daysDifference - ONE_YEAR + startDate.Day, startDate.AddDays(ONE_YEAR), discount);     //pricteme pocet dnu od zacatku mesice, protoze tarif na 190 dnu musi zacinat od 1 dne mesice
                price += tarifItemsContainer.price;
                tarifItems.AddRange(tarifItemsContainer.tarifsItems);


                if (bestPrice > price)
                {
                    bestTarifItems = tarifItems.ToList();
                    bestPrice = price;
                }
                tarifItems.Clear();

                tarifItemsContainer = CountForRemainingDays(daysInMonth - startDate.Day, startDate.AddDays(-(daysInMonth - startDate.Day)), discount);    //pricteme pocet dnu od zacatku mesice, protoze tarif na 190 dnu musi zacinat od 1 dne mesice
                price = tarifItemsContainer.price;
                tarifItems.AddRange(tarifItemsContainer.tarifsItems);

                price += Count190Price(startDate, endDate, discount) * 2;

                tmpDate = startDate.AddDays(HALF_YEAR + daysInMonth - startDate.Day);
                tarifItems.Add(new TarifItem { days = HALF_YEAR, dateStart = startDate.AddDays(daysInMonth - startDate.Day), dateEnd = tmpDate });
                tarifItems.Add(new TarifItem { days = HALF_YEAR, dateStart = tmpDate, dateEnd = tmpDate.AddDays(HALF_YEAR) });
  
                tarifItemsContainer = CountForRemainingDays(daysDifference - ONE_YEAR, tmpDate.AddDays(HALF_YEAR), discount);    //pricteme pocet dnu od zacatku mesice, protoze tarif na 190 dnu musi zacinat od 1 dne mesice
                price += tarifItemsContainer.price;
                tarifItems.AddRange(tarifItemsContainer.tarifsItems);



                if (bestPrice > price)
                {
                    bestTarifItems = tarifItems.ToList();
                    bestPrice = price;
                }
                tarifItems.Clear();

            }
            else                                           // porovname pulrocni a denni
            {

                int daysInMonth = DateTime.DaysInMonth(startDate.Year, startDate.Month);

                tarifItemsContainer = CountForRemainingDays(daysInMonth - startDate.Day, startDate.AddDays(-(daysInMonth - startDate.Day)), discount);    //pricteme pocet dnu od zacatku mesice, protoze tarif na 190 dnu musi zacinat od 1 dne mesice
                price = tarifItemsContainer.price;
                tarifItems.AddRange(tarifItemsContainer.tarifsItems);

                price += Count190Price(startDate, endDate, discount);
                tarifItems.Add(new TarifItem { days = HALF_YEAR, dateStart = startDate, dateEnd = startDate.AddDays(HALF_YEAR) });
                
                if (bestPrice > price)
                {
                    bestTarifItems = tarifItems.ToList();
                    bestPrice = price;
                }
                tarifItems.Clear();

                price = Count190Price(startDate, endDate, discount);
                tarifItems.Add(new TarifItem { days = HALF_YEAR, dateStart = startDate, dateEnd = startDate.AddDays(HALF_YEAR) });


                tarifItemsContainer = CountForRemainingDays(daysDifference - HALF_YEAR + startDate.Day, startDate.AddDays(HALF_YEAR), discount);    

                price += tarifItemsContainer.price;
                tarifItems.AddRange(tarifItemsContainer.tarifsItems);


                if (bestPrice > price)
                {
                    bestTarifItems = tarifItems.ToList();
                    bestPrice = price;
                }
                tarifItems.Clear();

                tarifItemsContainer = CountForRemainingDays(daysDifference, startDate, discount);   
                price = tarifItemsContainer.price;
                tarifItems.AddRange(tarifItemsContainer.tarifsItems);


                if (bestPrice > price)
                {
                    bestTarifItems = tarifItems.ToList();
                    bestPrice = price;
                }
                tarifItems.Clear();

            }

            TarifItemsContainer container = new TarifItemsContainer();
            container.tarifsItems = bestTarifItems.ToList();
            container.price = bestPrice;

            return container;
        }




        // metoda dopocita cenu pro zbyvajici pocet dnu (ikdyz je vetsi nez 123)
        public static TarifItemsContainer CountForRemainingDays(int days, DateTime startDate, String discount)
        {
            float price = 0;
            DateTime tmpDate = startDate;
            TarifItemsContainer container = new TarifItemsContainer();
            
            List<TarifItem> tarifItems = new List<TarifItem>();
                    
            while (days > 0) {
                if (days > NUMBER_OF_DAYS)
                {
                    price += CountDaysPrice2(NUMBER_OF_DAYS, discount);
                    days -= NUMBER_OF_DAYS;
                    tarifItems.Add(new TarifItem { days = NUMBER_OF_DAYS, dateStart = tmpDate, dateEnd = tmpDate.AddDays(NUMBER_OF_DAYS) });
                    tmpDate = tmpDate.AddDays(NUMBER_OF_DAYS);
                }
                else
                {
                    price += CountDaysPrice2(days, discount);                  
                    tarifItems.Add(new TarifItem { days = days, dateStart = tmpDate, dateEnd = tmpDate.AddDays(days) });
                    days = 0;
                }          
            }

            container.tarifsItems = tarifItems.ToList();
            container.price = price;

            return container;
        }


        public static float CountDaysPrice2(int daysDifference, String discount)
        {
            Tarif choosenTariff = TariffChooser(discount);
            if (choosenTariff == null) return -1; // discount not found
            if (choosenTariff.DayTarif.Length < daysDifference || daysDifference < 1) // time between days is too long or less than 0
            {
                return -4;
            }

            float daysPrice = choosenTariff.DayTarif[daysDifference];

            return daysPrice;
        }


        // count price for day tariff
        public static float CountDaysPrice(DateTime startDate, DateTime endDate, String discount) {
            int daysDifference = DaysDifference(startDate, endDate);

            Tarif choosenTariff = TariffChooser(discount);
            if (choosenTariff == null) return -1; // discount not found
            if (choosenTariff.DayTarif.Length < daysDifference || daysDifference < 1) // time between days is too long or less than 0
            {
                return -4;
            }

            float daysPrice = choosenTariff.DayTarif[daysDifference];

            return daysPrice;
        }

        // count price for year tariff
        public static float Count380Price(DateTime startDate, DateTime endDate, String discount)
        {
            int daysDifference = DaysDifference(startDate, endDate);
            int yearsDifference = endDate.Year - startDate.Year + 1;

            Tarif choosenTariff = TariffChooser(discount);
            if (choosenTariff == null) return -1; // discount not found

            int oneYearPrice;
        //    float yearsPrice;
            String yearsPriceString;
            if (!choosenTariff.Dictionary.TryGetValue("380 dni", out yearsPriceString)) return -2; // year prepay not found

            if (!Int32.TryParse(yearsPriceString, out oneYearPrice)) return -3; // error in string to int
        //    yearsPrice = oneYearPrice * yearsDifference;

            return oneYearPrice;
        }

        // count price for half-year tariff
        public static float Count190Price(DateTime startDate, DateTime endDate, String discount)
        {
            int daysDifference = DaysDifference(startDate, endDate);
            int monthsDifference = ((endDate.Year - startDate.Year) * 12) + endDate.Month - startDate.Month + 1;

            Tarif choosenTariff = TariffChooser(discount);
            if (choosenTariff == null) return -1; // discount not found

            int oneSixMonthsPrice;
      //      float sixMonthsPrice;
            String sixMonthsPriceString;
            if (!choosenTariff.Dictionary.TryGetValue("190 dni", out sixMonthsPriceString)) return -2; // half-year prepay not found

            if (!Int32.TryParse(sixMonthsPriceString, out oneSixMonthsPrice)) return -3; // error in string to int
        //    sixMonthsPrice = oneSixMonthsPrice * (monthsDifference / 6 + 1);
       //     Console.WriteLine("Cena pulrocniho tarifu pro {0} mesicu je {1} kc", monthsDifference, sixMonthsPrice);
            return oneSixMonthsPrice;
        }

        // counts day difference between start and end date
        public static int DaysDifference(DateTime startDate, DateTime endDate)
        {
            int daysDifference = (int)(endDate.Date - startDate.Date).TotalDays + 1;
            return daysDifference;
        }

        // select right tariff by discount
        public static Tarif TariffChooser(String discount)
        {
            List<Tarif> listTariff = ListTariff(defaultZone);
            if (listTariff == null) return null; // zones not found
            Tarif choosenTariff = null;
            foreach (Tarif tariff in listTariff)
            {
                if (tariff.category.Equals(discount))
                {
                    choosenTariff = tariff;
                    break;
                }
            }
            return choosenTariff;
        }

        // return list of all tariffs (for discounts)
        public static List<Tarif> ListTariff(String zone)
        {
            List<Tarif> listTariff;
            if (!tarifDictionary.TryGetValue(zone, out listTariff)) return null; // zones not found
            return listTariff;
        }



    }
}
