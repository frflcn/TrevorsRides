#if (ANDROID || IOS || NET)
using System.Text.Json.Serialization;
using TrevorsRidesHelpers.GoogleApiClasses;

namespace TrevorsRidesHelpers
{
    public class TrevorStatus
    {
        public bool isOnline { get; set; }
        public bool isDrivingForUber { get; set; }
        public double? latitude 
        {
            get => lastKnownLocation?.position.latitude;
        }
        public double? longitude 
        { 
            get => lastKnownLocation?.position.longitude; 
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
        public TrevorTrip[]? trips { get; set; }

        public TrevorStatus(bool isOnline, SpaceTime? lastKnownLocation)
        {
            this.isOnline = isOnline;
            this.isDrivingForUber = false;
            this.lastKnownLocation = lastKnownLocation;
        }
        public TrevorStatus()
        {
            this.isOnline = false;
            this.isDrivingForUber = false;
        }
        
    }
    public class TrevorTrip
    {
        public string type { get; set; }
        public string polyline { get; set; }
    }
    public class Position
    {
        public double latitude { get; set; }
        public double longitude { get; set; }
        public Position(double latitude, double longitude)
        {
            this.latitude = latitude;
            this.longitude = longitude;
        }
#endif
#if (ANDROID || IOS)
        public Waypoint ToWaypoint()
        {
            return new Waypoint()
            {
                location = new GoogleApiClasses.Location()
                {
                    latLng = new GoogleApiClasses.LatLng()
                    {
                        latitude = this.latitude,
                        longitude = this.longitude
                    }
                }
            };
        }
#endif
#if (ANDROID || IOS || NET)

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

#endif