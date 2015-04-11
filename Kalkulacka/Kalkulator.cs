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
    class Kalkulator
    {
        private const int NUMBER_OF_DAYS = 123;
        private static Dictionary<String, List<Tarif>> tarifDictionary;

        // testing parammeters for console
        static DateTime startDateG;
        static DateTime endDateG;
        static String discount;
        static String defaultZone = "vnejsi";


        public static void Main(string[] args)
        {
            String tariffPath = System.IO.Path.GetFullPath("..\\..\\operational_tariff.xls");
            tarifDictionary = LoadExcel(tariffPath);


            
            Console.WriteLine("-Pocitani tarifu-");
            Console.WriteLine();
            while (true)
            {
                InsertValues();
                String countedTariff = CountTariff(startDateG, endDateG, "ZTP");


            }
            Console.ReadLine();
        }

        public static Dictionary<String, List<Tarif>> LoadExcel(string filename)
        {
            Application xlsApp = new Application();

            if (!System.IO.File.Exists(filename))
            {
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

            Dictionary<String, List<Tarif>> tarifs_dictionary = new Dictionary<string,List<Tarif>>();

            for (int list_index = 1; list_index <= sheets.Count; list_index++) { 


            Worksheet ws = (Worksheet)sheets.get_Item(list_index);
            
            
            
            bool isNumeric, isCategory;
            float column_value;
            int column_index, columns_count = ws.UsedRange.Columns.Count, first_column;
            string first_column_string;
            string[] category = new string[columns_count];
            float[,] days_tarif = new float[columns_count, NUMBER_OF_DAYS + 1];

            Dictionary<String, String>[] dictionary = new Dictionary<string, string>[columns_count];

            for (int j = 0; j < columns_count; j++)
            {
                dictionary[j] = new Dictionary<string, string>();
            }



            foreach (Range row in ws.UsedRange.Rows)
            {
                column_index = 0;
                first_column = -1;
                isCategory = false;
                first_column_string = "";

                foreach (Range cell in row.Columns)
                {
                    column_index++;

                    if (cell.Value2 != null)    // bunka neni prazdna
                    {
                        isNumeric = float.TryParse(cell.Value2.ToString(), out column_value);

                        if (isNumeric)  // hodnota bunky je cislo
                        {
                            if (column_index == 1)     // prvni sloupec
                            {
                                first_column = (int)Math.Ceiling(column_value); ;
                            }
                            else if (first_column != -1)
                            {
                                if (first_column > 0 && first_column <= NUMBER_OF_DAYS)
                                {
                                    days_tarif[column_index, first_column] = column_value;
                                }
                                else    //  cislo do slovniku
                                {
                                    dictionary[column_index].Add(first_column.ToString(), cell.Value2.ToString());
                                }
                            }
                            else if (!string.IsNullOrEmpty(first_column_string))
                            {
                                dictionary[column_index].Add(first_column_string, cell.Value2.ToString());
                            }
                        }
                        else     // hodnota bunky je retezec
                        {
                            String s = cell.Value2.ToString();
                            if (column_index == 1)  // prvni sloupec
                            {
                                switch (s)
                                {
                                    case "dny": isCategory = true; break;
                                    default: first_column_string = s; break;
                                }

                            }
                            else if (isCategory == true)    // zjisteni kategorii
                            {
                                category[column_index] = cell.Value2.ToString();
                            }
                            else     // jina hodnota, ulozime ji do slovniku
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

            float[] tmp_array = new float[NUMBER_OF_DAYS + 1];

            List<Tarif> tarifs = new List<Tarif>();
            for (int i = 0; i < category.Length; i++)
            {
                if (!string.IsNullOrEmpty(category[i]))
                {
                    Tarif tarif = new Tarif();
                    tarif.category = category[i];

                    for (int j = 0; j <= NUMBER_OF_DAYS; j++)
                        tmp_array[j] = days_tarif[i, j];

                    tarif.DayTarif = tmp_array;
                    tarif.Dictionary = dictionary[i];
                    tarifs.Add(tarif);
                }
            }
            tarifs_dictionary.Add(ws.Name, tarifs); // ulozeni tarifu do slovniku dle zony(jmeno listu excelu)
            

            }
            return tarifs_dictionary;
        }
        

        public static String CountTariff(DateTime startDate, DateTime endDate, String discount)
        {
            float price;
            price = CountDaysPrice(startDate, endDate, discount);
            ErrorWritter(price);

            price = Count380Price(startDate, endDate, discount);
            ErrorWritter(price);

            price = Count190Price(startDate, endDate, discount);
            ErrorWritter(price);

            Console.WriteLine("Stiskni enter pro nove zadani");
            Console.ReadLine();

            return null;
        }


        public static float CountDaysPrice(DateTime startDate, DateTime endDate, String discount) {
            int daysDifference = DaysDifference(startDate, endDate);

            Tarif choosenTariff = TariffChooser(discount);
            if (choosenTariff == null) return -1; // discount not found
            if (choosenTariff.DayTarif.Length < daysDifference || daysDifference < 1) // time between days is too long or less than 0
            {
                return -4;
            }

            float daysPrice = choosenTariff.DayTarif[daysDifference];
            Console.WriteLine("Cena denniho tarifu pro {0} dnu je {1} kc", daysDifference, daysPrice);
            Console.WriteLine();
            return daysPrice;
        }


        public static float Count380Price(DateTime startDate, DateTime endDate, String discount)
        {
            int daysDifference = DaysDifference(startDate, endDate);
            int yearsDifference = endDate.Year - startDate.Year + 1;

            Tarif choosenTariff = TariffChooser(discount);
            if (choosenTariff == null) return -1; // discount not found

            int oneYearPrice;
            float yearsPrice;
            String yearsPriceString;
            if (!choosenTariff.Dictionary.TryGetValue("380 dni", out yearsPriceString)) return -2; // year prepay not found

            if (!Int32.TryParse(yearsPriceString, out oneYearPrice)) return -3; // error in string to int
            yearsPrice = oneYearPrice * yearsDifference;
            Console.WriteLine("Cena rocniho tarifu pro {0} let je {1} kc", yearsDifference, yearsPrice);
            Console.WriteLine();
            return yearsPrice;
        }


        public static float Count190Price(DateTime startDate, DateTime endDate, String discount)
        {
            int daysDifference = DaysDifference(startDate, endDate);
            int monthsDifference = ((endDate.Year - startDate.Year) * 12) + endDate.Month - startDate.Month + 1;

            Tarif choosenTariff = TariffChooser(discount);
            if (choosenTariff == null) return -1; // discount not found

            int oneSixMonthsPrice;
            float sixMonthsPrice;
            String sixMonthsPriceString;
            if (!choosenTariff.Dictionary.TryGetValue("190 dni", out sixMonthsPriceString)) return -2; // half-year prepay not found

            if (!Int32.TryParse(sixMonthsPriceString, out oneSixMonthsPrice)) return -3; // error in string to int
            sixMonthsPrice = oneSixMonthsPrice * (monthsDifference / 6 + 1);
            Console.WriteLine("Cena pulrocniho tarifu pro {0} mesicu je {1} kc", monthsDifference, sixMonthsPrice);
            Console.WriteLine();
            return sixMonthsPrice;
        }

        public static int DaysDifference(DateTime startDate, DateTime endDate)
        {
            int daysDifference = (int)(endDate.Date - startDate.Date).TotalDays + 1;
            Console.WriteLine("od {0} do {1} ; {2} dnu", startDate, endDate, daysDifference);
            return daysDifference;
        }

        public static Tarif TariffChooser(String discount)
        {
            List<Tarif> listTariff = ListTariff(defaultZone);
            if (listTariff == null) return null; // zones not found
            Tarif choosenTariff = null;
            Console.WriteLine("predplatne - vnejsi zony - ok");
            foreach (Tarif tariff in listTariff)
            {
                if (tariff.category.Equals(discount))
                {
                    Console.WriteLine("sleva {0} - ok", discount);
                    choosenTariff = tariff;
                    break;
                }
            }
            return choosenTariff;
        }

        public static List<Tarif> ListTariff(String zone)
        {
            List<Tarif> listTariff;
            if (!tarifDictionary.TryGetValue(zone, out listTariff)) return null; // zones not found
            return listTariff;
        }



        public static void InsertValues()
        {
            String sYear, sMonth, sDay;
            int y, m, d;
            Console.WriteLine("Zadej datum od:");
            Console.Write("Rok: ");
            sYear = Console.ReadLine();
            Console.Write("Mesic: ");
            sMonth = Console.ReadLine();
            Console.Write("Den: ");
            sDay = Console.ReadLine();
            y = Int32.Parse(sYear);
            m = Int32.Parse(sMonth);
            d = Int32.Parse(sDay);
            startDateG = new DateTime(y, m, d);

            Console.WriteLine();
            Console.WriteLine("Zadej datum do:");
            Console.Write("Rok: ");
            sYear = Console.ReadLine();
            Console.Write("Mesic: ");
            sMonth = Console.ReadLine();
            Console.Write("Den: ");
            sDay = Console.ReadLine();
            y = Int32.Parse(sYear);
            m = Int32.Parse(sMonth);
            d = Int32.Parse(sDay);
            endDateG = new DateTime(y, m, d);

            Console.WriteLine();
            StringBuilder builder = new StringBuilder();
            List<Tarif> listTariff = ListTariff(defaultZone);
            if (listTariff == null)
            {
                Console.WriteLine("ERR: List tarifu je null");
            }
            else
            {
                foreach (Tarif tariff in listTariff)
                {
                    builder.Append(tariff.category).Append(", ");
                }
            }
            string result = builder.ToString();
            Console.WriteLine("Zadej nazev slevy({0}):", result);
            discount = Console.ReadLine();


            Console.WriteLine();
            Console.WriteLine("------------------------------");
            Console.WriteLine();

        }

        public static void ErrorWritter(float errNumberF)
        {
            int errNumber = (int)errNumberF;
            if (errNumber < 0)
            {
                String errMsg;
                switch (errNumber)
                {
                    case -1: errMsg = "nenalezena pozadovana sleva";
                        break;
                    case -2: errMsg = "nenalezeno takove predplatne";
                        break;
                    case -3: errMsg = "nepodarilo se prevest cenu ze string na int";
                        break;
                    case -4: errMsg = "rozpeti dnu je zaporne nebo prilis vysoke pro Denni tarif";
                        break;
                    default: errMsg = "jina chyba";
                        break;
                }
                Console.WriteLine("ERR: {0}", errMsg);
            }
        }
    }
}
