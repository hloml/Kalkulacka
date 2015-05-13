﻿using Microsoft.Office.Interop.Excel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;


namespace WebPovedCalculator.Models
{
    public class Kalkulator
    {
        private const int NUMBER_OF_DAYS = 123;
        private const int HALF_YEAR = 190;
        private const int ONE_YEAR = 380;
        private const int ONE_MONTH = 30;
        private const int TEN_MONTHS = 300;



        private const String DAYS_TARIF = "denní";
        private const String HALF_YEAR_TARIF = "půlroční";
        private const String ONE_YEAR_TARIF = "roční";
        private const String ONE_MONTH_TARIF = "měsíční";
        private const String TEN_MONTHS_TARIF = "10 měsíční";


        // zones names
        public const String INNER_ZONE_NAME = "předplatné zóna 001 Plzeň";
        public const String OUTER_ZONE_NAME = "předplatné - vnější zóny";
        public const String NETWORK_ZONE_NAME = "síťové jízdné";


        public const String ISIC = "ISIC";
        public const String fullFare = "plné jízdné";

        private static Dictionary<String, List<Tarif>> tarifDictionary;
        private const String EXCEL_NAME = "operational_tariff_v2.xls";
        private static String EXCEL_PATH;

        //static String defaultZone = "vnejsi"; //TODO



        public static Dictionary<String, List<Tarif>> GetExcel()
        {
            if (tarifDictionary == null)
            {
                EXCEL_PATH = HttpContext.Current.Server.MapPath("~/");
                tarifDictionary = LoadExcel(EXCEL_PATH + EXCEL_NAME);
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
        public static TarifItemsContainer CountTariff(DateTime startDate, DateTime endDate, String discount, String zone)
        {
            float totalPrice=0, bestPrice = float.MaxValue;
            float price;

            List<TarifItem> tarifItems = new List<TarifItem>();

            int daysDifference = DaysDifference(startDate, endDate);
            int yearsDifference = endDate.Year - startDate.Year ;
            DateTime tmpDate;
            TarifItemsContainer tarifItemsContainer;
            List<TarifItem> bestTarifItems = new List<TarifItem>();

            if (yearsDifference > 1)    // roky mezi nima maj rocni jizdny a prvni a posledni dopocitat
            {

                tarifItemsContainer = bestPriceForYear(startDate, new DateTime(startDate.Year, 12, 31), discount, true, zone);
                totalPrice += tarifItemsContainer.price;
                tarifItems.AddRange(tarifItemsContainer.tarifsItems);

                price = Count380Price(discount, zone);
                totalPrice = price * (yearsDifference -1);
                tmpDate = new DateTime(startDate.Year + 1, 1, 1);
               
                for (int i = 1; i < yearsDifference - 1; i++)
                {
                    tarifItems.Add(CreateTarifItem(ONE_YEAR, tmpDate, tmpDate.AddDays(ONE_YEAR), price, ONE_YEAR_TARIF, discount));
                    tmpDate = tmpDate.AddYears(1);
                }

                tmpDate = tmpDate.AddDays(-1).AddDays(ONE_YEAR);

                tarifItemsContainer = bestPriceForYear(tmpDate, endDate, discount, true, zone);
                totalPrice += tarifItemsContainer.price;
                tarifItems.AddRange(tarifItemsContainer.tarifsItems);

                if (bestPrice > totalPrice)
                {
                    bestTarifItems = tarifItems.GetRange(0, tarifItems.Count);
                    
                    bestPrice = totalPrice;
                }
                tarifItems.Clear();

            }
            else if (yearsDifference == 1)  // musime zkusit zkombinovat mezi obema rokama
            {
                tarifItemsContainer = bestPriceForYear(startDate, new DateTime(startDate.Year, 12, 31), discount, true, zone);
                totalPrice += tarifItemsContainer.price;
                tarifItems.AddRange(tarifItemsContainer.tarifsItems);

                tarifItemsContainer = bestPriceForYear(new DateTime(endDate.Year, 1, 1), endDate, discount, false, zone);
                totalPrice += tarifItemsContainer.price;
                tarifItems.AddRange(tarifItemsContainer.tarifsItems);

                if (bestPrice > totalPrice)
                {
                    bestTarifItems = tarifItems.ToList();
                    bestPrice = totalPrice;
                }
                tarifItems.Clear(); 

                tarifItemsContainer = bestPriceForYear(startDate, endDate, discount, true, zone);
                totalPrice = tarifItemsContainer.price;
                tarifItems.AddRange(tarifItemsContainer.tarifsItems);

                
                if (bestPrice > totalPrice)
                {
                    bestTarifItems = tarifItems.ToList();
                    bestPrice = totalPrice;
                }
                tarifItems.Clear();

            }
            else                         //pro jeden rok kouknout co se nejvic vyplati
            {
                tarifItemsContainer = bestPriceForYear(startDate, endDate, discount, false, zone);
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



        public static TarifItemsContainer bestPriceForYear(DateTime startDate, DateTime endDate, String discount, Boolean allowed, String zone)
        {
            int daysDifference = DaysDifference(startDate, endDate);
            float totalPrice = 0;
            float bestPrice = Count380Price(discount, zone);    // zkusime rocni
            float price;
            DateTime tmpDate;

            List<TarifItem> tarifItems = new List<TarifItem>();
            List<TarifItem> bestTarifItems = new List<TarifItem>();
            TarifItemsContainer tarifItemsContainer;

            bestTarifItems.Add(CreateTarifItem(ONE_YEAR, new DateTime(startDate.Year, 1, 1), new DateTime(startDate.Year, 12, 31), bestPrice, ONE_YEAR_TARIF, discount));
            if (allowed)    
            {
                tarifItemsContainer = bestPriceForYear(new DateTime(startDate.Year + 1, 1, 1), endDate, discount, false, zone);

                bestPrice += tarifItemsContainer.price;
                bestTarifItems.AddRange(tarifItemsContainer.tarifsItems);
            }


            if (daysDifference > HALF_YEAR)                                   // vetsi nez pulrocni, zkusime nakombinovat
            {
                tmpDate = startDate.AddDays(-startDate.Day + 1);
                price = Count190Price(discount, zone);
                totalPrice = price;
                tarifItems.Add(CreateTarifItem(HALF_YEAR, tmpDate, tmpDate.AddDays(HALF_YEAR), price, HALF_YEAR_TARIF, discount));
                tmpDate = tmpDate.AddDays(HALF_YEAR);

                tarifItemsContainer = CountForRemainingDays(DaysDifference(tmpDate, endDate), tmpDate, discount, zone);  
                totalPrice += tarifItemsContainer.price;      
                tarifItems.AddRange(tarifItemsContainer.tarifsItems);

                if (bestPrice > totalPrice)
                {
                    bestTarifItems = tarifItems.ToList();
                    bestPrice = totalPrice;
                }
                tarifItems.Clear();

                int daysInMonth = DateTime.DaysInMonth(startDate.Year, startDate.Month);
                tarifItemsContainer = CountForRemainingDays(daysInMonth - startDate.Day + 1, startDate, discount, zone);     //pricteme pocet dnu od zacatku mesice, protoze tarif na 190 dnu musi zacinat od 1 dne mesice
                totalPrice = tarifItemsContainer.price;
                tarifItems.AddRange(tarifItemsContainer.tarifsItems);

                tmpDate = startDate.AddDays(daysInMonth - startDate.Day + 1);

                price = Count190Price(discount, zone);
                totalPrice += price;
                tarifItems.Add(CreateTarifItem(HALF_YEAR, tmpDate, tmpDate.AddDays(HALF_YEAR), price, HALF_YEAR_TARIF, discount));
                
                tmpDate = tmpDate.AddDays(HALF_YEAR);
                tarifItemsContainer = CountForRemainingDays(DaysDifference(tmpDate, endDate), tmpDate, discount, zone);      
                totalPrice += tarifItemsContainer.price;
                tarifItems.AddRange(tarifItemsContainer.tarifsItems);

                if (bestPrice > totalPrice)
                {
                    bestTarifItems = tarifItems.ToList();
                    bestPrice = totalPrice;
                }
                tarifItems.Clear();

                tarifItemsContainer = Combine190tarifs(startDate, endDate, discount, zone); 
                totalPrice = tarifItemsContainer.price; 
                tarifItems.AddRange(tarifItemsContainer.tarifsItems);

                if (bestPrice > totalPrice)
                {
                    bestTarifItems = tarifItems.ToList();
                    bestPrice = totalPrice;
                }
                tarifItems.Clear();
            }
            else                                           // porovname pulrocni a denni
            {
                int daysInMonth = DateTime.DaysInMonth(startDate.Year, startDate.Month);
                tmpDate = startDate.AddDays(-(daysInMonth - startDate.Day + 1));

                tarifItemsContainer = CountForRemainingDays(daysInMonth - startDate.Day, tmpDate, discount, zone);    //pricteme pocet dnu od zacatku mesice, protoze tarif na 190 dnu musi zacinat od 1 dne mesice
                totalPrice = tarifItemsContainer.price;
                tarifItems.AddRange(tarifItemsContainer.tarifsItems);

                price = Count190Price(discount, zone);
                totalPrice += price;
                tarifItems.Add(CreateTarifItem(HALF_YEAR, tmpDate, tmpDate.AddDays(HALF_YEAR), price, HALF_YEAR_TARIF, discount));
                
                if (bestPrice > totalPrice)
                {
                    bestTarifItems = tarifItems.ToList();
                    bestPrice = totalPrice;
                }
                tarifItems.Clear();
            
                tmpDate = startDate.AddDays(-startDate.Day + 1);
                price = Count190Price(discount, zone);
                totalPrice = price;
                tarifItems.Add(CreateTarifItem(HALF_YEAR, tmpDate, tmpDate.AddDays(HALF_YEAR), price, HALF_YEAR_TARIF, discount));
                tmpDate = startDate.AddDays(HALF_YEAR);

                tarifItemsContainer = CountForRemainingDays(DaysDifference(tmpDate, endDate), tmpDate, discount, zone);    
                totalPrice += tarifItemsContainer.price;
                tarifItems.AddRange(tarifItemsContainer.tarifsItems);

                if (bestPrice > totalPrice)
                {
                    bestTarifItems = tarifItems.ToList();
                    bestPrice = totalPrice;
                }
                tarifItems.Clear();

                tarifItemsContainer = CountForRemainingDays(daysDifference, startDate, discount, zone);   
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



        public static TarifItemsContainer Combine190tarifs(DateTime startDate, DateTime endDate, String discount, String zone)
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
            
         
                tmpDate = startDate.AddDays(-startDate.Day + 1);
                price = Count190Price(discount, zone);
                totalPrice = price * 2;
   
                tmpDate = tmpDate.AddDays(HALF_YEAR);

                tarifItems.Add(CreateTarifItem(HALF_YEAR, startDate.AddDays(-startDate.Day + 1), tmpDate, price, HALF_YEAR_TARIF, discount));
               
                int daysInMonth = DateTime.DaysInMonth(tmpDate.Year, tmpDate.Month);
              
                tarifItemsContainer = CountForRemainingDays(daysInMonth - tmpDate.Day , tmpDate, discount, zone);     //pricteme pocet dnu od zacatku mesice, protoze tarif na 190 dnu musi zacinat od 1 dne mesice
                totalPrice += tarifItemsContainer.price;
                tarifItems.AddRange(tarifItemsContainer.tarifsItems);

                tmpDate = tmpDate.AddDays(daysInMonth - tmpDate.Day + 1);
                tarifItems.Add(CreateTarifItem(HALF_YEAR, tmpDate, tmpDate.AddDays(HALF_YEAR), price, HALF_YEAR_TARIF, discount));
                
                tarifItemsContainer = CountForRemainingDays(DaysDifference(tmpDate.AddDays(HALF_YEAR), endDate), tmpDate.AddDays(HALF_YEAR), discount, zone);     //pricteme pocet dnu od zacatku mesice, protoze tarif na 190 dnu musi zacinat od 1 dne mesice
                totalPrice += tarifItemsContainer.price;
                tarifItems.AddRange(tarifItemsContainer.tarifsItems);

                if (bestPrice > totalPrice)
                {
                    bestTarifItems = tarifItems.ToList();
                    bestPrice = totalPrice;
                }
                tarifItems.Clear();

                daysInMonth = DateTime.DaysInMonth(startDate.Year, startDate.Month);     
                tarifItemsContainer = CountForRemainingDays(daysInMonth - startDate.Day, startDate, discount, zone);     //pricteme pocet dnu od zacatku mesice, protoze tarif na 190 dnu musi zacinat od 1 dne mesice
                totalPrice = tarifItemsContainer.price;
                tarifItems.AddRange(tarifItemsContainer.tarifsItems);

                tmpDate = startDate.AddDays(daysInMonth - startDate.Day + 1);
                price = Count190Price(discount, zone);
                totalPrice += price * 2;
                tarifItems.Add(CreateTarifItem(HALF_YEAR, tmpDate, tmpDate.AddDays(HALF_YEAR), price, HALF_YEAR_TARIF, discount));
                tmpDate = tmpDate.AddDays(HALF_YEAR);
                
                daysInMonth = DateTime.DaysInMonth(tmpDate.Year, tmpDate.Month);
                tarifItemsContainer = CountForRemainingDays(daysInMonth - tmpDate.Day + 1, tmpDate, discount, zone);     //pricteme pocet dnu od zacatku mesice, protoze tarif na 190 dnu musi zacinat od 1 dne mesice
                totalPrice += tarifItemsContainer.price;
                tarifItems.AddRange(tarifItemsContainer.tarifsItems);

                tmpDate = tmpDate.AddDays(daysInMonth - tmpDate.Day + 1);
                tarifItems.Add(CreateTarifItem(HALF_YEAR, tmpDate, tmpDate.AddDays(HALF_YEAR), price, HALF_YEAR_TARIF, discount));
                tarifItemsContainer = CountForRemainingDays(DaysDifference(tmpDate.AddDays(HALF_YEAR), endDate), tmpDate.AddDays(HALF_YEAR), discount, zone);     //pricteme pocet dnu od zacatku mesice, protoze tarif na 190 dnu musi zacinat od 1 dne mesice
                totalPrice += tarifItemsContainer.price;
                tarifItems.AddRange(tarifItemsContainer.tarifsItems);

                if (bestPrice > totalPrice)
                {
                    bestTarifItems = tarifItems.ToList();
                    bestPrice = totalPrice;
                }
                tarifItems.Clear();

                daysInMonth = DateTime.DaysInMonth(startDate.Year, startDate.Month);
                tarifItemsContainer = CountForRemainingDays(daysInMonth - startDate.Day + 1, startDate, discount, zone);     //pricteme pocet dnu od zacatku mesice, protoze tarif na 190 dnu musi zacinat od 1 dne mesice
                totalPrice = tarifItemsContainer.price;
                tarifItems.AddRange(tarifItemsContainer.tarifsItems);

                tmpDate = startDate.AddDays(daysInMonth - startDate.Day + 1);
                price = Count190Price(discount, zone);
                totalPrice += price * 2;
                tmpDate = tmpDate.AddDays(HALF_YEAR);

                tarifItems.Add(CreateTarifItem(HALF_YEAR, startDate.AddDays(daysInMonth - startDate.Day + 1), tmpDate, price, HALF_YEAR_TARIF, discount));

                tmpDate = new DateTime(tmpDate.Year, tmpDate.Month, 1);
                tarifItems.Add(CreateTarifItem(HALF_YEAR, tmpDate, tmpDate.AddDays(HALF_YEAR), price, HALF_YEAR_TARIF, discount));
                tarifItemsContainer = CountForRemainingDays(DaysDifference(tmpDate.AddDays(HALF_YEAR), endDate), tmpDate.AddDays(HALF_YEAR), discount, zone);     //pricteme pocet dnu od zacatku mesice, protoze tarif na 190 dnu musi zacinat od 1 dne mesice
                totalPrice += tarifItemsContainer.price;
                tarifItems.AddRange(tarifItemsContainer.tarifsItems);
            
                if (bestPrice > totalPrice)
                {
                    bestTarifItems = tarifItems.ToList();
                    bestPrice = totalPrice;
                }
                tarifItems.Clear();

                tmpDate = startDate.AddDays(-startDate.Day + 1);
                price = Count190Price(discount, zone);
                totalPrice = price * 2;

                tmpDate = tmpDate.AddDays(HALF_YEAR);

                tarifItems.Add(CreateTarifItem(HALF_YEAR, startDate.AddDays(-startDate.Day + 1), tmpDate, price, HALF_YEAR_TARIF, discount));

                daysInMonth = DateTime.DaysInMonth(tmpDate.Year, tmpDate.Month);
                tmpDate = tmpDate.AddDays(-tmpDate.Day + 1);
                tarifItems.Add(CreateTarifItem(HALF_YEAR, tmpDate, tmpDate.AddDays(HALF_YEAR), price, HALF_YEAR_TARIF, discount));
                tarifItemsContainer = CountForRemainingDays(DaysDifference(tmpDate.AddDays(HALF_YEAR), endDate), tmpDate.AddDays(HALF_YEAR), discount, zone);     //pricteme pocet dnu od zacatku mesice, protoze tarif na 190 dnu musi zacinat od 1 dne mesice
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


        // metoda dopocita cenu pro zbyvajici pocet dnu (ikdyz je vetsi nez 123)
        public static TarifItemsContainer CountForRemainingDays(int days, DateTime startDate, String discount, String zone)
        {
            float totalPrice = 0;
            float price;
            DateTime tmpDate = startDate;
            TarifItemsContainer container = new TarifItemsContainer();
            
            List<TarifItem> tarifItems = new List<TarifItem>();
                    
            while (days > 0) {
                if (days > NUMBER_OF_DAYS)
                {
                    price = CountDaysPrice(NUMBER_OF_DAYS, discount, zone);
                    totalPrice += price;
                    days -= NUMBER_OF_DAYS;
                    tarifItems.Add(CreateTarifItem(NUMBER_OF_DAYS, tmpDate, tmpDate.AddDays(NUMBER_OF_DAYS), price, DAYS_TARIF, discount));
                    tmpDate = tmpDate.AddDays(NUMBER_OF_DAYS);
                }
                else
                {
                    price = CountDaysPrice(days, discount, zone);
                    totalPrice += price;

                    tarifItems.Add(CreateTarifItem(days, tmpDate, tmpDate.AddDays(days), price, DAYS_TARIF, discount));
                    days = 0;
                }          
            }

            container.tarifsItems = tarifItems.ToList();
            container.price = totalPrice;
            return container;
        }


        public static float CountDaysPrice(int daysDifference, String discount, String zone)
        {
            Tarif choosenTariff = TariffChooser(discount, zone);
            if (choosenTariff == null) return float.MaxValue; // discount not found
            if (choosenTariff.DayTarif.Length < daysDifference || daysDifference < 1) // time between days is too long or less than 0
            {
                return float.MaxValue;
            }

            float daysPrice = choosenTariff.DayTarif[daysDifference];
            return daysPrice;
        }


        public static TarifItemsContainer CountTariffForStudents(DateTime startDate, DateTime endDate, String discount, String zone, Boolean isISIC)
        {
            float totalPrice = 0;
            List<TarifItem> tarifItems = new List<TarifItem>();
            DateTime tmpDate = startDate;
            TarifItemsContainer tarifItemsContainer = new TarifItemsContainer();
            DateTime tmp;

            do
            {
                tmp = MakeDatetimeToHolidays(tmpDate, endDate);
                tarifItemsContainer = CountForSchoolYear(tmpDate, tmp, discount, zone);     //pricteme pocet dnu od zacatku mesice, protoze tarif na 190 dnu musi zacinat od 1 dne mesice
                totalPrice += tarifItemsContainer.price;
                tarifItems.AddRange(tarifItemsContainer.tarifsItems);
                tmpDate = tmp;

                if (DateTime.Compare(tmpDate, endDate) < 0)      // spocteme pro prazdniny
                {
                    tmp = MakeDatetimeForHolidays(tmpDate, endDate);
                    tarifItemsContainer = CountForHoliday(tmpDate.AddDays(1), tmp, discount, zone, isISIC);
                    totalPrice += tarifItemsContainer.price;
                    tarifItems.AddRange(tarifItemsContainer.tarifsItems);
                    tmpDate = tmp.AddDays(1);
                }
            } while (DateTime.Compare(tmpDate, endDate) < 0);
           
            tarifItemsContainer.price = totalPrice;
            tarifItemsContainer.tarifsItems = tarifItems.ToList();

            return tarifItemsContainer;
        }


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


        static DateTime MakeDatetimeForHolidays(DateTime startDate, DateTime endDate)
        {
            DateTime tmpDate = new DateTime(startDate.Year, 8, 31);

            if (DateTime.Compare(tmpDate, endDate) > 0)
            {
                tmpDate = endDate;
            }
            return tmpDate;
        }


        // if student have isic, change category for holidays
        static TarifItemsContainer CountForHoliday(DateTime startDate, DateTime endDate, String discount, String zone, Boolean isISIC)
        {
            TarifItemsContainer tarifItemsContainer;
            if (isISIC == true)     
            {
                discount = ISIC;
            }
            else // student doesnt have isic, he pay full price on holidays
            {
                discount = fullFare;
            }
            tarifItemsContainer = CountForRemainingDays(DaysDifference(startDate, endDate) - 1, startDate, discount, zone);
            return tarifItemsContainer;
        }

        static TarifItemsContainer CountForSchoolYear(DateTime startDate, DateTime endDate, String discount, String zone)
        {
            DateTime tmpDate = startDate;
            List<TarifItem> bestTarifItems = new List<TarifItem>();
            List<TarifItem> tarifItems = new List<TarifItem>();
            TarifItemsContainer tarifItemsContainer;
            float price;
            float totalPrice;
            float bestPrice = float.MaxValue;

            price = Get10MonthsPrice(discount, zone);

            tarifItems.Add(CreateTarifItem(TEN_MONTHS, new DateTime(startDate.Year, 9, 1), new DateTime(startDate.Year + 1, 6, 30), price, TEN_MONTHS_TARIF, discount));
            totalPrice = price;
           
            if (bestPrice > totalPrice)
            {
                bestTarifItems = tarifItems.ToList();
                bestPrice = totalPrice;
            }
            tarifItems.Clear();
            totalPrice = 0;

            if (!(tmpDate.Year == endDate.Year && tmpDate.Month == endDate.Month))  // algoritmus je pro vice nez 1 mesic
            {
                price = GetMonthPrice(discount, zone);
                int daysInMonth = DateTime.DaysInMonth(startDate.Year, startDate.Month);

                tarifItemsContainer = CountForRemainingDays(daysInMonth - startDate.Day + 1, startDate, discount, zone);     //pricteme pocet dnu od zacatku mesice, protoze tarif na 190 dnu musi zacinat od 1 dne mesice
                totalPrice = tarifItemsContainer.price;
                tarifItems.AddRange(tarifItemsContainer.tarifsItems);
                tmpDate = startDate.AddDays(daysInMonth - startDate.Day + 1);

                while (tmpDate.Year != endDate.Year || tmpDate.Month != endDate.Month)
                {    // pro kazdy mesic mezi
                    tarifItems.Add(CreateTarifItem(ONE_MONTH, tmpDate, tmpDate.AddMonths(1), price, ONE_MONTH_TARIF, discount));
                    totalPrice += price;
                    tmpDate = tmpDate.AddMonths(1);
                }

            }

            tarifItemsContainer = CountForRemainingDays(DaysDifference(tmpDate, endDate), tmpDate, discount, zone);     //pricteme pocet dnu od zacatku mesice, protoze tarif na 190 dnu musi zacinat od 1 dne mesice
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



        // count price for year tariff
        public static float Count380Price(String discount, String zone)
        {
            Tarif choosenTariff = TariffChooser(discount, zone);
            if (choosenTariff == null) return -1; 

            int oneYearPrice;
            String yearsPriceString;
            if (!choosenTariff.Dictionary.TryGetValue("" + ONE_YEAR, out yearsPriceString)) return -2; // year prepay not found

            if (!Int32.TryParse(yearsPriceString, out oneYearPrice)) return float.MaxValue; // error in string to int
      
            return oneYearPrice;
        }

        // count price for half-year tariff
        public static float Count190Price(String discount, String zone)
        {
            Tarif choosenTariff = TariffChooser(discount, zone);
            if (choosenTariff == null) return -1; // discount not found

            int oneSixMonthsPrice;
            String sixMonthsPriceString;
            if (!choosenTariff.Dictionary.TryGetValue("" + HALF_YEAR, out sixMonthsPriceString)) return -2; // half-year prepay not found

            if (!Int32.TryParse(sixMonthsPriceString, out oneSixMonthsPrice)) return float.MaxValue; // error in string to int
            return oneSixMonthsPrice;
        }


        public static float Get10MonthsPrice(String discount, String zone)
        {
            Tarif choosenTariff = TariffChooser(discount, zone);
            if (choosenTariff == null) return -1; // discount not found

            int price;
            String priceString;
            if (!choosenTariff.Dictionary.TryGetValue("10 měsíční", out priceString)) return -2; // 10 months prepay not found

            if (!Int32.TryParse(priceString, out price)) return float.MaxValue; // error in string to int
            return price;
        }

        public static float GetMonthPrice(String discount, String zone)
        {
            Tarif choosenTariff = TariffChooser(discount, zone);
            if (choosenTariff == null) return -1; // discount not found

            int price;
            String priceString;
            if (!choosenTariff.Dictionary.TryGetValue("měsíční", out priceString)) return -2; // monts prepay not found

            if (!Int32.TryParse(priceString, out price)) return float.MaxValue; // error in string to int
            return price;
        }




        // counts day difference between start and end date
        public static int DaysDifference(DateTime startDate, DateTime endDate)
        {
            int daysDifference = (int)(endDate.Date - startDate.Date).TotalDays + 1;
            return daysDifference;
        }

        // select right tariff by discount
        public static Tarif TariffChooser(String discount, String zone)
        {
            List<Tarif> listTariff = ListTariff(zone);
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


        public static int getCountingMethod(String discount, String zone)
        {
            if (Count190Price(discount, zone) >= 0 && Count380Price(discount, zone) >= 0) return 1;
            if (Get10MonthsPrice(discount, zone) >= 0 && GetMonthPrice(discount, zone) >= 0) return 2;
            return 0;

        }


        private static TarifItem CreateTarifItem(int days, DateTime dateStart, DateTime dateEnd, float price, String tarifName, String category)
        {
            return new TarifItem { days = days, dateStart = dateStart, dateEnd = dateEnd, price = price, TariffName = tarifName, category = category };
        }


    }
}
