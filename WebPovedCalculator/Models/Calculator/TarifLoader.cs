using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Office.Interop.Excel;

namespace WebPovedCalculator.Models.Calculator
{
    /// <summary>
    /// Loading tarifs into dictionary from excel
    /// </summary>
    public class TarifLoader
    {

        /// <summary>
        /// Load tarifs for all zones from excel file and save them into dictionary
        /// </summary>
        /// <param name="filename">path with name of excel file</param>
        /// <returns>dictionary filled with excel data</returns>
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

                int columns_count = ws.UsedRange.Columns.Count + 1;
                string[] category = new string[columns_count];
                float[,] days_tarif = new float[columns_count, Kalkulator.NUMBER_OF_DAYS + 1];      // for saving values for 1..123 days

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


        /// <summary>
        /// Iterate over all cells in excel and find all categories. For categories fill array (for 1..123 days) and dictionary obtained from parameters
        /// </summary>
        /// <param name="ws">excel worksheet</param>
        /// <param name="dictionary">Dictionary for each category</param>
        /// <param name="category">Array for categories</param>
        /// <param name="days_tarif">Array for values for 1..123 for each category</param>
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
                                if (first_column > 0 && first_column <= Kalkulator.NUMBER_OF_DAYS)
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


        /// <summary>
        /// Saving tariffs to list - create object tarif for all categories and return arrayList with created objects.
        /// </summary>
        /// <param name="category">all categories</param>
        /// <param name="days_tarif">Array for values for 1..123 for each category</param>
        /// <param name="dictionary">Dictionary for each category</param>
        /// <returns>tariffs in list</returns>
        private static List<Tarif> SaveTarifsToList(string[] category, float[,] days_tarif, Dictionary<String, String>[] dictionary)
        {
            float[] tmp_array = new float[Kalkulator.NUMBER_OF_DAYS + 1];
            List<Tarif> tarifs = new List<Tarif>();
            for (int i = 0; i < category.Length; i++)
            {
                if (!string.IsNullOrEmpty(category[i]))
                {
                    Tarif tarif = new Tarif();
                    tarif.category = category[i];

                    tmp_array = new float[Kalkulator.NUMBER_OF_DAYS + 1];
                    for (int j = 0; j <= Kalkulator.NUMBER_OF_DAYS; j++)
                        tmp_array[j] = days_tarif[i, j];

                    tarif.DayTarif = tmp_array;
                    tarif.Dictionary = dictionary[i];
                    tarifs.Add(tarif);
                }
            }

            return tarifs;
        }



    }
}