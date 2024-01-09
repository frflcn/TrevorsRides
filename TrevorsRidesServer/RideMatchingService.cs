using TrevorsRidesHelpers;
using TrevorsRidesServer.Controllers;

namespace TrevorsRidesServer
{
    public class RideMatchingService
    {
        private static readonly RideMatchingService instance = new RideMatchingService();

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static RideMatchingService()
        {
        }

        private RideMatchingService()
        {
        }

        public static RideMatchingService Instance
        {
            get
            {
                return instance;
            }
        }

        public Dictionary<Guid, Rider> Riders { get; set; } = new Dictionary<Guid, Rider>();
        public Dictionary<Guid, Driver> Drivers { get; set; } = new Dictionary<Guid, Driver>();
        public bool TryRegisterDriver(Guid key, Driver driver)
        {
            return Drivers.TryAdd(key, driver);
        }
        public void DeRegisterDriver(Guid key)
        {
            Drivers.Remove(key);
        }
        public bool TryRegisterRider(Guid key, Rider rider)
        {
            return Riders.TryAdd(key, rider);
        }
        public void DeRegisterRider(Guid key)
        {
            Riders.Remove(key);
        }
        public async Task FindDriver(Rider rider)
        {
            double[] distances = new double[Drivers.Count];
            Driver[] drivers = Drivers.Values.ToArray();
            for (int i = 0; i < Drivers.Count; i++)
            {
                distances[i] = Helpers.CalculateDistance(rider.SpaceTime.position, drivers[i].SpaceTime.position);
            }
            Array.Sort(distances, drivers);
            foreach (Driver driver in drivers)
            {
                await RequestDriver(driver, "RideRequest");
            }
        }
        public async Task RequestDriver(Driver driver, string rideRequest)
        {
            driver.Send(rideRequest);
        }
    }
    public class Rider
    {
        public delegate Task SendDelegate(string message);
        public SendDelegate Send {  get; set; } 
        public SpaceTime? SpaceTime { get; set; }
        public Driver? MatchedDriver { get; set; }
        public Rider(SendDelegate sendDelegate)
        {
            Send = sendDelegate;
        }
        public Rider(SendDelegate sendDelegate, SpaceTime spaceTime)
        {
            SpaceTime = spaceTime;
            Send = sendDelegate;
        }
    }
    public class Driver
    {
        public delegate Task SendDelegate(string message);
        public SendDelegate Send { get; set; }
        public SpaceTime SpaceTime { get; set; }
        public Rider? MatchedRider { get; set; }
        public Driver(SendDelegate sendDelegate, SpaceTime spaceTime) 
        {
            SpaceTime = spaceTime;
            Send = sendDelegate;
        }
    }

}
