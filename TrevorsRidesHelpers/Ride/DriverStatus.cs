using System.Text.Json;
using System.Text.Json.Serialization;
using TrevorsRidesHelpers.GoogleApiClasses;



namespace TrevorsRidesHelpers.Ride
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
                if (trips == null)
                    endPoint = value;
                _lastKnownLocation = value;
            }
        }
        public SpaceTime? endPoint { get; set; }
        public DriverTrip[]? trips { get; set; }

        public DriverStatus(bool isOnline, SpaceTime? lastKnownLocation)
        {
            this.isOnline = isOnline;
            isDrivingForUber = false;
            this.lastKnownLocation = lastKnownLocation;
        }
        public DriverStatus()
        {
            isOnline = false;
            isDrivingForUber = false;
        }
        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
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
        public Position(double lat, double lng)
        {
            this.lat = lat;
            this.lng = lng;
        }

        public Waypoint ToWaypoint()
        {
            return new Waypoint()
            {
                location = new GoogleApiClasses.Location()
                {
                    latLng = new LatLng()
                    {
                        latitude = lat,
                        longitude = lng
                    }
                }
            };
        }
        public LatLng ToLatLng()
        {
            return new LatLng()
            {
                latitude = lat,
                longitude = lng
            };
        }
        public LatLngLiteral ToLatLngLiteral()
        {
            return new LatLngLiteral()
            {
                lat = lat,
                lng = lng
            };
        }



    }
    public class SpaceTime
    {
        public Position position { get; set; }
        public DateTimeOffset time { get; set; }
        public SpaceTime(double latitude, double longitude, DateTimeOffset time)
        {
            position = new Position(latitude, longitude);
            this.time = time;
        }
        [JsonConstructor]
        public SpaceTime(Position position, DateTimeOffset time)
        {
            this.position = position;
            this.time = time;
        }
        protected SpaceTime()
        {

        }
    }
}

