using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrevorsRidesHelpers.GoogleApiClasses;
using TrevorsRidesHelpers.Ride;

namespace TrevorsRidesHelpers
{
    public class Helpers
    {
        public const bool IsLive = true;
        public const bool IsBeta = true;


        public static string WebsocketDomain { get; set; }
        public static string Domain { get; set; }
        public static string DataFolder { get; set; }
        public static string WWWFolder { get; set; }
        public static string SecretsFolder { get; set; }
        
        static Helpers() 
        {
            SecretsFolder = "/var/secrets/trevorsrides/"; //Same folder for test and live
            #pragma warning disable CS0162 // Unreachable code detected
            if (IsLive)
            {
                Domain = "https://www.trevorsrides.com";
                WebsocketDomain = "wss://www.trevorsrides.com";
                DataFolder = "/var/data/trevorsrides/";
                WWWFolder = "/var/www/trevorsrides/";
            }
            else
            {

                Domain = $"https://www.test.trevorsrides.com";
                WebsocketDomain = "wss://www.test.trevorsrides.com";
                DataFolder = "/var/data/testtrevorsrides/";
                WWWFolder = "/var/www/testtrevorsrides/";
            }
            #pragma warning restore CS0162 // Unreachable code detected
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
        public static double CalculateDistance(LatLngLiteral position1, LatLngLiteral position2)
        {
            Position pos1 = new Position(position1.lat, position1.lng);
            Position pos2 = new Position(position2.lat, position2.lng);

            return CalculateDistance(pos1, pos2);
        }
        public static double CalculateDistance(LatLng position1, LatLng position2)
        {
            Position pos1 = new Position(position1.latitude, position1.longitude);
            Position pos2 = new Position(position2.latitude, position2.longitude);

            return CalculateDistance(pos1, pos2);
        }
    }
}
