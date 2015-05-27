using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Office.Interop.Excel;
using System.IO;

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
        /// <param name="filepath">path with name of excel file</param>
        /// <returns></returns>
        public static Dictionary<String, List<Tarif>> LoadExcel(string filePath)
        {
            String[] files = { Kalkulator.INNER_ZONE_NAME, Kalkulator.NETWORK_ZONE_NAME, Kalkulator.OUTER_ZONE_NAME };

            Dictionary<String, List<Tarif>> tarifs_dictionary = new Dictionary<string, List<Tarif>>();

            for (int list_index = 0; list_index < files.Length; list_index++)
            {    // iterate over all files
                List<Tarif> tarifs = IterateOverAllCells(filePath + files[list_index] + ".csv");       // iterate all cells in actual list
                tarifs_dictionary.Add(files[list_index], tarifs);      // save tarifs to dictionary with zone name as a key          
            }
            return tarifs_dictionary;
        }



        private static List<Tarif> IterateOverAllCells(String file)
        {
            bool isNumeric, isCategory;
            float column_value;
            int columns_count, first_column;

            if (!System.IO.File.Exists(file))
            {
                Console.WriteLine("File " + file + " doesnt exist");
                return null;
            }

            var reader = new StreamReader(File.OpenRead(@file));
            string line = reader.ReadLine().Replace(@"""", "");
            string[] values = line.Split(';');
            columns_count = values.Length - 1;

            Dictionary<String, String>[] dictionary = new Dictionary<string, string>[columns_count];
            string[] category = new String[columns_count];
            float[,] days_tarif = new float[columns_count, Kalkulator.NUMBER_OF_DAYS + 1];
            for (int j = 0; j < columns_count; j++)      // initiate array of dictionaries
            {
                dictionary[j] = new Dictionary<string, string>();
            }

            for (int i = 1; i < values.Length; i++)
            {
                category[i - 1] = values[i];
            }

            while (!reader.EndOfStream)
            {
                line = reader.ReadLine().Replace(@"""", "");
                values = line.Split(';');



                if (!String.IsNullOrEmpty(values[0]))
                {

                    isCategory = int.TryParse(values[0], out first_column);


                    for (int i = 1; i < values.Length; i++)
                    {
                        if (isCategory && (first_column > 0 && first_column <= Kalkulator.NUMBER_OF_DAYS))
                        {
                            isNumeric = float.TryParse(values[i], out column_value);
                            days_tarif[i - 1, first_column] = column_value;

                        }
                        else if (!String.IsNullOrEmpty(values[i]))
                        {
                            dictionary[i - 1].Add(values[0], values[i]);
                        }
                    }
                }
            }


            List<Tarif> tarifs = SaveTarifsToList(category, days_tarif, dictionary);    // create object tarif for each category and save objects to arraylist
            return tarifs;
        }




        /// <summary>
        /// Saving tarifs to list - create object tarif for all categories and return arrayList with created objects.
        /// </summary>
        /// <param name="category">all categories</param>
        /// <param name="days_tarif">Array for values for 1..123 for each category</param>
        /// <param name="dictionary">Dictionary for each category</param>
        /// <returns></returns>
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