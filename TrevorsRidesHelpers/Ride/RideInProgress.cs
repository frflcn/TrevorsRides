using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrevorsRidesHelpers.Ride
{
    public class RideInProgress
    {
        [Key]
        public Guid RideId { get; set; }
        public RideEventType Status { get; set; }
        
        public Guid RiderID { get; set; }
        public Guid? DriverID { get; set; }
        public Pickup Pickup { get; set; }
        public Stop[] Stops { get; set; }
        public DropOff DropOff { get; set; }
        public SpaceTimeContinuum? DriversHistoryFinalized { get; set; }
        public SpaceTimeUpdateContinuum DriversHistory { get; set; }
        public SpaceTimeUpdateContinuum RidersHistory { get; set; }
        public SpaceTimeContinuum? RidersHistoryFinalized { get; set; }
        public RideEventUpdateContinuum RideEvents { get; set; }
        public List<RidePlanUpdate> RidePlanUpdates { get; set; }
        public string? CancellationReason { get; set; }
        public string CheckoutSessionId { get; set; }

        public RideInProgress()
        {

        }
        public RideInProgress(Guid rideId, Guid riderID, string checkoutSessionId, PlaceCore pickup, PlaceCore dropoff, PlaceCore[]? stops = null)
        {
            RideId = rideId;
            RiderID = riderID;
            Pickup = new Pickup(pickup);
            DropOff = new DropOff(dropoff);
            CheckoutSessionId = checkoutSessionId;
            if (stops == null)
            {
                Stops = [];
                RidePlanUpdates = new List<RidePlanUpdate>() { new RidePlanUpdate(new RidePlan(pickup, dropoff)) };
            }
            else
            {
                Stops = new Stop[stops.Length];
                for (int i = 0; i < stops.Length; i++)
                {
                    Stops[i] = new Stop(stops[i]);
                }
                RidePlanUpdates = new List<RidePlanUpdate>() { new RidePlanUpdate(new RidePlan(pickup, dropoff, stops)) };
            }
            Status = RideEventType.Requested;
            RideEvents = RideEventUpdateContinuum.FromBlob(new byte[] { 0 });
            RidersHistory = SpaceTimeUpdateContinuum.FromBlob(new byte[] { 0 });
            DriversHistory = SpaceTimeUpdateContinuum.FromBlob(new byte[] { 0 });

        }
        public RideInProgress(Guid riderID, string checkoutSessionId, PlaceCore pickup, PlaceCore dropoff, PlaceCore[]? stops = null) : this (Guid.NewGuid(), riderID, checkoutSessionId, pickup, dropoff, stops) { }
                
    }
}
