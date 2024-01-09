using System.Text.Json.Serialization;
using TrevorsRidesHelpers.GoogleApiClasses;

namespace TrevorsRidesHelpers
{
    public class DriverStatus
    {
        public bool isOnline { get; set; }
        public bool isDrivingForUber { get; set; }
        public double? latitude 
        {
            get => lastKnownLocation?.position.lat;
        }
        public double? longitude 
        { 
            get => lastKnownLocation?.position.lng; 
        }
        private SpaceTime? _lastKnownLocation;
        public SpaceTime? lastKnownLocation 
        { 
            get => _lastKnownLocation;
            set
            {
                if (this.trips == null)
                    endPoint = value;
                _lastKnownLocation = value;
            } 
        }
        public SpaceTime? endPoint { get; set; }
        public DriverTrip[]? trips { get; set; }

        public DriverStatus(bool isOnline, SpaceTime? lastKnownLocation)
        {
            this.isOnline = isOnline;
            this.isDrivingForUber = false;
            this.lastKnownLocation = lastKnownLocation;
        }
        public DriverStatus()
        {
            this.isOnline = false;
            this.isDrivingForUber = false;
        }
        
    }
    public class DriverTrip
    {
        public string type { get; set; }
        public string polyline { get; set; }
    }
    public class Position
    {
        public double lat { get; set; }
        public double lng { get; set; }
        public Position(double latitude, double longitude)
        {
            this.lat = latitude;
            this.lng = longitude;
        }

        public Waypoint ToWaypoint()
        {
            return new Waypoint()
            {
                location = new GoogleApiClasses.Location()
                {
                    latLng = new GoogleApiClasses.LatLng()
                    {
                        latitude = this.lat,
                        longitude = this.lng
                    }
                }
            };
        }



    }
    public class SpaceTime
    {
        public Position position { get; set; }
        public DateTime time { get; set; }
        public SpaceTime(double latitude, double longitude, DateTime time)
        {
            this.position = new Position(latitude, longitude);
            this.time = time;
        }
        [JsonConstructorAttribute]
        public SpaceTime(Position position, DateTime time)
        {
            this.position = position;
            this.time = time;
        }
    }
}

