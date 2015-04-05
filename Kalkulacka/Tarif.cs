using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kalkulacka
{
    class Tarif
    {
        
        private const int NUMBER_OF_DAYS = 123;
        private float[] days_tarif = new float[NUMBER_OF_DAYS + 1];
        
        private Dictionary<String, String> dictionary = new Dictionary<string, string>();

        public String category {get; set;}

        public float[] DayTarif
        {
            get { return days_tarif; }
            set { days_tarif = value; }
        }

        public Dictionary<String, String> Dictionary
        {
            get { return dictionary; }
            set { dictionary = value; }
        }


    }



    }

