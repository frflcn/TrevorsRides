using System.Diagnostics;
using System.Text.Json;
using TrevorsRidesHelpers;
using TrevorsRidesHelpers.Ride;
using TrevorsRidesServer.Controllers;
using TrevorsRidesServer.Models;

namespace TrevorsRidesServer
{
    public class RideMatchingService
    {
        private static readonly RideMatchingService instance = new RideMatchingService();
        public static ILogger<RideMatchingService> Logger { get; set; }
        public static Guid TrevorsId { get; set; }
        private int logCounter = 0;
        public static bool IsShuttingDown = false;
        public static string ServiceIsShuttingDownMessage = "Service is shutting down, please try again in a few minutes";
        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static RideMatchingService()
        {
            using(RidesModel context = new RidesModel())
            {
                TrevorsId = context.DriverAccounts.Single(e => e.Email == "tmstauff@gmail.com").Id;
            }
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
        public Dictionary<Guid, Trip> Trips { get; set; } = new Dictionary<Guid, Trip>();
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
        public async Task FindDriver(Guid tripId)
        {
            double[] distances = new double[Drivers.Count];
            Guid[] driverIDs = Drivers.Keys.ToArray();
            DropOff dropoff;
            Position dropoffPosition;
            using (RidesModel model = new RidesModel())
            {
                RideInProgress  ride = model.RidesInProgress.Single(e => e.RideId == tripId);
                dropoff = ride.DropOff;
                dropoffPosition = new Position(ride.DropOff.Location.LatLng.lat, ride.DropOff.Location.LatLng.lng);
                
            }
            for (int i = 0; i < Drivers.Count; i++)
            {
                distances[i] = Helpers.CalculateDistance(dropoffPosition, Drivers[driverIDs[i]].Status.lastKnownLocation!.position);
            }
            Array.Sort(distances, driverIDs);
            foreach (Guid driver in driverIDs)
            {
                await SendRequestToDriver(driver, tripId);
            }
        }
        public async Task<bool> SendRequestToDriver(Guid driverId, Guid tripId)
        {
            Logger.LogInformation($"Send Request to Driver: {tripId}");

            using (RidesModel context = new RidesModel())
            {
                RideInProgress ride = context.RidesInProgress.Single(e => e.RideId == tripId);
                PlaceCore[] stops = new PlaceCore[ride.Stops.Length];
                for (int i = 0; i < stops.Length; i++)
                {
                    stops[i] = ride.Stops[i].ToPlaceCore();
                }

                TripRequest trip = new TripRequest(ride.RiderID, ride.Pickup.ToPlaceCore(), ride.DropOff.ToPlaceCore(), stops, tripId: tripId);
                WebsocketMessage webSocketMessage = new WebsocketMessage(MessageType.RideRequest, trip);
                string json = JsonSerializer.Serialize(webSocketMessage);


                Driver driver;
                if (!Drivers.TryGetValue(driverId, out driver))
                {
                    return false; //Driver logged out
                }
                TaskCompletionSource<WebsocketMessage> receiveReplyTaskSource = new TaskCompletionSource<WebsocketMessage>();
                await driver.AddReplyListener(webSocketMessage.MessageID, receiveReplyTaskSource);
                await driver.Send(json);
                Task<WebsocketMessage> receiveReplyTask = receiveReplyTaskSource.Task;
                Task timeDelay = Task.Delay(15000);
                Task firstCompletedTask = await Task.WhenAny(timeDelay, receiveReplyTask);
                if (firstCompletedTask == timeDelay) 
                {
                    Logger.LogInformation("Task == timeDelay");
                    await driver.RemoveReplyListener(webSocketMessage.MessageID);
                    return false;
                }
                else if (firstCompletedTask == receiveReplyTask) 
                {
                    Logger.LogInformation("Task == receiveReplyTask");
                    if (receiveReplyTask.IsCompletedSuccessfully)
                    {
                        if (receiveReplyTask.Result.MessageType == MessageType.RideRequestAccepted)
                        {
                            return true;
                        }
                        else 
                        { 
                            return false; 
                        }
                    }
                    return false;
                }
                else
                {
                    Logger.LogInformation("Ooopps cant compare tasks like that");
                }
                throw new UnreachableException("You need a new way to compare tasks");
            }
        }
        public void UpdateDriverStatus(Guid driverId, DriverStatus status)
        {
            Drivers[driverId].Status = status;
        }
 
        public DriverStatus GetTrevorsStatus()
        {
            logCounter++;
            Driver? Trevor;
            if (Drivers.TryGetValue(TrevorsId, out Trevor))
            {
                if (logCounter == 7)
                {
                    Logger.LogDebug($"Got Trevor: {TrevorsId}");
                    logCounter = 0;
                }
                
                return Trevor.Status;
                
            }
            else
            {
                if (logCounter == 7)
                {
                    Logger.LogDebug($"Where is Trevor: {TrevorsId}");
                    logCounter = 0;
                }
                return new DriverStatus(false, null);
            }
            
        }
        public async Task<bool> RequestDriver(Guid tripId, Guid driverId)
        {
            Logger.LogInformation($"Requst Driver is trevors id: {driverId == TrevorsId}");
            bool hasAccepted;
            hasAccepted = await SendRequestToDriver(driverId, tripId);
            if (hasAccepted)
            {
                await ChangeRideToAccepted(tripId, driverId);
                return true;
            }
            else
            {
                await RemoveRide(tripId);
                return false;
            }
        }
        public async Task<bool> RequestTrevor(Guid tripId)
        {
            return await RequestDriver(tripId, TrevorsId);
        }
        public async Task TripPaid(Guid rideId)
        {
            using (RidesModel context = new RidesModel())
            {
                Console.WriteLine($"RideMatchingService: {rideId}");
                Logger.LogInformation("TRY PAID Trevor");
                RideInProgress ride = context.RidesInProgress.Single(e => e.RideId == rideId);
                ride.Status = RideEventType.Paid;
                await context.SaveChangesAsync();
                _ = RequestTrevor(rideId);
            }
        }
        public async Task TripPaid(Guid rideId, Guid driverId)
        {
            using (RidesModel context = new RidesModel())
            {
                Console.WriteLine($"RideMatchingService: {rideId}");
                Logger.LogInformation("TRY PAID NOt Trevor");
                RideInProgress ride = context.RidesInProgress.Single(e => e.RideId == rideId);
                ride.Status = RideEventType.Paid;
                await context.SaveChangesAsync();
                _ = RequestDriver(rideId, driverId);
            }
        }
        public async Task RemoveRide(Guid tripId)
        {
            using (RidesModel context = new RidesModel())
            {
                RideInProgress ride = context.RidesInProgress.Single(e => e.RideId == tripId);
                context.RidesInProgress.Remove(ride); //TODO: Refund Rider
                await context.SaveChangesAsync();
            }
        }
        public async Task ChangeRideToAccepted(Guid tripId, Guid driverId)
        {
            using (RidesModel context = new RidesModel())
            {
                RideInProgress ride = context.RidesInProgress.Single(e => e.RideId == tripId);
                ride.Status = RideEventType.Accepted;
                ride.DriverID = TrevorsId;
                context.RidesInProgress.Remove(ride); //TODO: Have the ability to actually complete the ride
                await context.SaveChangesAsync();
            }
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
        public delegate Task AddReplyListenerDelegate(Guid messageId, TaskCompletionSource<WebsocketMessage> replyListener);
        public delegate Task RemoveReplyListenerDelegate(Guid messageId);
        public AddReplyListenerDelegate AddReplyListener { get; set; }
        public RemoveReplyListenerDelegate RemoveReplyListener { get; set; }
        public SendDelegate Send { get; set; }
        public SemaphoreSlim RideRequestSemaphore { get; set; }
        //public SpaceTime SpaceTime { get; set; }
        public DriverStatus Status { get; set; }
        public Rider? MatchedRider { get; set; }
        public Driver(SendDelegate send, AddReplyListenerDelegate addReplyListener, RemoveReplyListenerDelegate removeReplyListener, DriverStatus status)
        {
            RemoveReplyListener = removeReplyListener;
            AddReplyListener = addReplyListener;
            Send = send;
            Status = status;
            RideRequestSemaphore = new SemaphoreSlim(1, 1);
        }

        //public Driver(SendDelegate sendDelegate, SpaceTime spaceTime) 
        //{
        //    SpaceTime = spaceTime;
        //    Send = sendDelegate;
        //}
    }

}
