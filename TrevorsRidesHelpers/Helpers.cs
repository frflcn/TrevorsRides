using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrevorsRidesHelpers.GoogleApiClasses;

namespace TrevorsRidesHelpers
{
    public class Helpers
    {
        public static bool IsTest {  get; set; }
        public static string Domain { get; set; }
        public static int Port { get; set; }
        public static bool IsBeta { get; set; }

        static Helpers() 
        { 
            IsTest = false;
            IsBeta = true;
            Port = 7061;
            if (IsTest)
            {
                Domain = $"http://10.0.2.2:{Port}";
            }
            else
            {
                Domain = "https://www.trevorsrides.com";
            }

        }
        /// <summary>
        /// Calculates distance in meters from 2 LatLngLiterals.
        /// </summary>
        /// <param name="position1">The first LatLng</param>
        /// <param name="position2">The second LatLng</param>
        /// <returns>Distance in meters</returns>
        public static double CalculateDistance(Position position1, Position position2)
        {
            const double R = 6371000; // metres
            double φ1 = position1.lat * Math.PI / 180; // φ, λ in radians
            double φ2 = position2.lat * Math.PI / 180;
            double Δφ = (position2.lat - position1.lat) * Math.PI / 180;
            double Δλ = (position2.lng - position1.lng) * Math.PI / 180;

            double a = Math.Sin(Δφ / 2) * Math.Sin(Δφ / 2) +
                      Math.Cos(φ1) * Math.Cos(φ2) *
                      Math.Sin(Δλ / 2) * Math.Sin(Δλ / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            double d = R * c; // in metres

            return d;
        }
    }
}
