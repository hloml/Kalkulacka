using Microsoft.Office.Interop.Excel;
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

        public static void Main(string[] args)
        {
            //tarifDictionary = LoadExcel("C:\\Users\\user\\Documents\\tarif2.xls");
            tarifDictionary = LoadExcel("C:\\Users\\Jára\\workspace\\git\\tabulky\\tarif2.xls");
            

            DateTime startDate = new DateTime(2013, 7, 15);
            DateTime endDate = new DateTime(2013, 8, 10);

            String countedTariff = CountTariff(startDate, endDate, "ZTP");
            

            Console.ReadLine();
        }

        public static Dictionary<String, List<Tarif>> LoadExcel(string filename)
        {
            Application xlsApp = new Application();

            if (!System.IO.File.Exists(filename))
            {
                Console.WriteLine("File doesnt exist");
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

                    tmp_array = new float[NUMBER_OF_DAYS + 1];
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
            Console.WriteLine("Pocitani tarifu:");

            CountDaysPrice(startDate, endDate, discount);

            //Count380Price();

            //Count190Price();


            return null;
        }


        public static float CountDaysPrice(DateTime startDate, DateTime endDate, String discount) {
            int daysDifference = DaysDifference(startDate, endDate);
            List<Tarif> listTarif;
            if (!tarifDictionary.TryGetValue("předplatné - vnější zóny", out listTarif)) return -1; // nenalezeny zony
            Tarif choosenTariff = null;
            Console.WriteLine("předplatné - vnější zóny - ok");
            foreach (Tarif tariff in listTarif)
            {
                if (tariff.category.Equals(discount))
                {
                    Console.WriteLine("sleva {0} - ok", discount);
                    choosenTariff = tariff;
                    break;
                }
            }
            if (choosenTariff == null) return -1; // nenalezena pozadovana sleva
            if (choosenTariff.DayTarif.Length < daysDifference && daysDifference < 1) return -1; // rozpeti dnu je zaporne nebo prilis vysoke

            float daysPrice = choosenTariff.DayTarif[daysDifference];
            Console.WriteLine("Cena denního tarifu pro {0} dnů je {1} kč", daysDifference, daysPrice);
            return daysPrice;
        }


        public static float Count380Price(DateTime startDate, DateTime endDate, String discount)
        {
            
            return 0;
        }


        public static float Count190Price(DateTime startDate, DateTime endDate, String discount)
        {
            
            return 0;
        }

        public static int DaysDifference(DateTime startDate, DateTime endDate)
        {
            int daysDifference = (int)(endDate.Date - startDate.Date).TotalDays + 1;
            Console.WriteLine("od {0} do {1} ; {2} dnů", startDate, endDate, daysDifference);
            return daysDifference;
        }
    }
}
