using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebPovedCalculator.Models
{
    /// <summary>
    /// For storing informations about tariff for category
    /// Tariff always have values for 1..123 days
    /// and some specific like 380, 190 which are stored in dictionary
    /// </summary>
    public class Tarif
    {
        // Default max number of days for day tariffs
        private const int NUMBER_OF_DAYS = 123;
        // Field for day tariffs prices
        private float[] days_tarif = new float[NUMBER_OF_DAYS + 1];
        // Dictionary with tariffs
        private Dictionary<String, String> dictionary = new Dictionary<string, string>();

        /// <summary>
        /// Category
        /// </summary>
        public String category {get; set;}

        /// <summary>
        /// Day tariff's prices
        /// </summary>
        public float[] DayTarif
        {
            get { return days_tarif; }
            set { days_tarif = value; }
        }

        /// <summary>
        /// Dictionary with tariffs
        /// </summary>
        public Dictionary<String, String> Dictionary
        {
            get { return dictionary; }
            set { dictionary = value; }
        }


    }



    }

