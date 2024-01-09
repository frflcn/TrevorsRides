using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrevorsRidesHelpers.GoogleApiClasses;

namespace TrevorsRidesHelpers
{
    public class Ride
    {
        private EventType _status;
        public EventType Status 
        {
            get
            {
                return _status;
            } 
            set 
            {
                if (value == EventType.RidePlanChanged || value == EventType.SpaceTimeUpdate)
                {
                    throw new ArgumentException("Status cannot be RidePlanChanged or SpaceTimeUpdate");
                }
                _status = value;
            } 
        }
        public Guid RiderID { get; set; }
        public Guid? DriverID { get; set; }
        public Pickup Pickup { get; set; }
        public Stop[] Stops { get; set; } 
        public DropOff DropOff { get; set; }
        public byte[]? DriversHistoryFinalized { get; set; }
        public List<byte> DriversHistory { get; set; } = new List<byte>();
        public List<byte> RidersHistory { get; set; } = new List<byte>();
        public byte[]? RidersHistoryFinalized { get; set; }
        public string? CancellationReason { get; set; }

        public Ride(Guid riderID, PlaceCore pickup, PlaceCore dropoff, PlaceCore[]? stops = null)
        {
            RiderID = riderID;
            Pickup = new Pickup(pickup);
            DropOff = new DropOff(dropoff);
            if (stops == null) Stops = [];
            else
            {
                Stops = new Stop[stops.Length];
                for (int i = 0; i < stops.Length; i++)
                {
                    Stops[i] = new Stop(stops[i]);
                }
            }
        }
    }
    public class SpaceTimeContinuum
    {
        public uint Length;
        List<byte> Bytes = new List<byte>();

    }
    public class RideEvent
    {
        public EventType EventType { get; set; }
        public SpaceTime SpaceTime { get; set; }

        public RideEvent(SpaceTime spaceTime)
        {
            EventType = EventType.SpaceTimeUpdate;
            SpaceTime = spaceTime;
        }
        public RideEvent(SpaceTime spaceTime, EventType eventType)
        {
            EventType = eventType;
            SpaceTime = spaceTime;
        }
    }
    public enum EventType
    {
        SpaceTimeUpdate,
        Requested,
        Accepted,
        Denied,
        EnRouteToPickup,
        ArrivedAtPickup,
        EnRouteToStop,
        ArrivedAtStop,
        EnRouteToDestination,
        ArrivedAtDestination,
        Completed,
        RidePlanChanged,
        RiderCanceled,
        DriverCanceled
    }
    public enum RideStatus
    {
        Requested,
        Accepted,
        Denied,
        EnRouteToPickup,
        WaitingAtPickup,
        EnRouteToStop,
        WaitingAtStop,
        EnRouteToDestination,
        Completed,
        RiderCanceled,
        DriverCanceled
    }
    public class PlaceCore
    {
        public LatLngLiteral LatLng { get; set; }
        public string Name { get; set; }
        public Address Address { get; set; }
        public PlaceCore(Place place)
        {
            Name = place.name;
            LatLng = place.geometry.location;
            Address = new Address(place.address_components);
        }
        
    }
    public class Address
    {
        public string StreetAddress { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string ShortName { get; set; } = string.Empty;
        public string LongName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public Address(AddressComponent[] addressComponents)
        {
            string streetNumber = string.Empty;
            string street = string.Empty;
            foreach (AddressComponent component in addressComponents)
            {
                if (component.types.Contains("street_number"))
                {
                    streetNumber = component.short_name; 
                }
                if (component.types.Contains("route"))
                {
                    street = component.short_name;
                }
                if (component.types.Contains("locality"))
                {
                    City = component.short_name;
                }
                if (component.types.Contains("administrative_area_level_1"))
                {
                    State = component.short_name;
                }
                if (component.types.Contains("country"))
                {
                    Country = component.short_name;
                }
                if (component.types.Contains("postal_code"))
                {
                    PostalCode = component.short_name;
                }
            }
            StreetAddress = streetNumber + " " + street;
            ShortName = StreetAddress + ", " + City;
            LongName = ShortName + ", " + State + " " + PostalCode;
            FullName = LongName + ", " + Country;
        }
    }
    public class Pickup : Stop
    {
        public Pickup(PlaceCore location, DateTime? arrivalTime = null, DateTime? departureTime = null) : base(location, arrivalTime, departureTime)
        {
            Location = location;
            ArrivalTime = arrivalTime;
            DepartureTime = departureTime;
        }
    }
    public class Stop
    {
        private DateTime? _arrivalTime;
        private DateTime? _departureTime;

        public PlaceCore Location { get; set; }
        public DateTime? ArrivalTime
        {
            get
            {
                return _arrivalTime;
            }
            set
            {
                if (value != null) HasArrived = true;
                else HasArrived = false;
                _arrivalTime = value;
            }
        }
        public TimeSpan? WaitTime 
        {
            get
            {
                if (_arrivalTime == null || _departureTime == null) return null;
                else return _departureTime - _arrivalTime;
            }
        }
        public DateTime? DepartureTime 
        {
            get
            {
                return _departureTime;
            }
            set
            {
                if (value != null) HasDeparted = true;
                else HasDeparted = false;
                _departureTime = value;
            }
        }
        public bool HasArrived { get; private set; }
        public bool HasDeparted { get; private set; }
        public Stop(PlaceCore location, DateTime? arrivalTime = null, DateTime? departureTime = null)
        {
            Location = location;
            ArrivalTime = arrivalTime;
            DepartureTime = departureTime;
        }
    }
    public class DropOff
    {
        private DateTime? _dropOffTime;
        public PlaceCore Location { get; set; }
        public DateTime? DropOffTime 
        {
            get
            {
                return _dropOffTime;
            }
            set
            {
                if (value != null) HasDroppedOff = true;
                else HasDroppedOff = false;
            }
        }
        public bool HasDroppedOff { get; private set; }

        public DropOff(PlaceCore location, DateTime? dropOffTime = null)
        {
            Location = location;
            DropOffTime = dropOffTime;
        }
    }
}
