using Microsoft.Maui.Devices.Sensors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrevorsRidesMaui
{
    internal class PlaceLocation
    {
        public Location Location { get; set; }
        public string PlaceName { get; set; }
        public Address Address { get; set; }
        
    }
    public class Address
    {
        string HouseNumber { get; set; }
        string StreetName { get; set; }
        string City { get; set; }
        string State { get; set; }
        string PostalCode { get; set; }
        string Country { get; set; }
    }
}
