using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebPovedCalculator.Models
{
    public class CompareWithCarModel
    {

        public String fuelConsumption;

        // Public Transport
        private int TariffLenght { get; set; }
        //private float TariffPrice { get; set; }
        // Car
        //private float AverageFuelConsumption { get; set; }
        //private float LiterOfFuelPrice { get; set; }
        private float PathDistance { get; set; }
        private float ParkingFee { get; set; }
        // help vars
        float CarPriceOnKM;
        float TariffPriceOnKM;


        // PathDistance, TariffLength cannot be ZERO !!!
        public CompareWithCarModel(int TariffLenght, float TariffPrice, float AverageFuelConsumption,
                                float LiterOfFuelPrice, float PathDistance, float ParkingFee)
        {
            this.TariffLenght = TariffLenght;
            //this.TariffPrice = TariffPrice;
            //this.AverageFuelConsumption = AverageFuelConsumption;
            //this.LiterOfFuelPrice = LiterOfFuelPrice;
            this.PathDistance = PathDistance;
            this.ParkingFee = ParkingFee;
            //
            CarPriceOnKM = AverageFuelConsumption * LiterOfFuelPrice / 100;
            TariffPriceOnKM = TariffPrice / TariffLenght / PathDistance;
        }

        public float PriceOfPath()
        {
            float fuelPriceOnPath = CarPriceOnKM * PathDistance;
            return fuelPriceOnPath + ParkingFee;
        }

        public float KilometerPriceDiff()
        {
            return CarPriceOnKM - TariffPriceOnKM;
        }

        public float OnDistancePriceDiff()
        {
            return KilometerPriceDiff() * PathDistance;
        }

        public float OnPrepaidTimePriceDiff()
        {
            return OnDistancePriceDiff() * TariffLenght;
        }

        public float PercentPriceDiff()
        {
            if (KilometerPriceDiff() == 0)
            {
                return 0;
            }
            return 1 / (TariffPriceOnKM / KilometerPriceDiff());
        }
    }
}