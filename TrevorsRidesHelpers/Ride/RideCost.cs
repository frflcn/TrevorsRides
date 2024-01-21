using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrevorsRidesHelpers.Ride
{
    public class RideCost
    {
        public static int BaseCents = 150;
        public static int CentsPerMile = 50;
        public static int CentsPerMinute = 50;

        public static int CostInCents(int meters, string secondsFromGoogle)
        {
            return CostInCents(meters, int.Parse(secondsFromGoogle.Substring(0, secondsFromGoogle.Length - 1)));
        }
        public static int CostInCents(int meters, int seconds)
        {
            decimal costForLength = CentsPerMile * (meters / (decimal)1609);
            decimal costForTime = (CentsPerMinute * (seconds / (decimal)60));
            return (int)(costForLength + costForTime + BaseCents);
        }
        public static decimal CostInDollars(int meters, int seconds)
        {
            return decimal.Round(CostInCents(meters, seconds), 2) / 100;
        }
        public static decimal CostInDollars(int meters, string secondsFromGoogle)
        {
            return decimal.Round(CostInCents(meters, secondsFromGoogle), 2) / 100;
        }
    }
}
