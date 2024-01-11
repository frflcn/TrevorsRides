using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;


using TrevorsRidesHelpers.GoogleApiClasses;
using Goog = TrevorsRidesHelpers.GoogleApiClasses;


namespace TrevorsRidesHelpers.Ride
{
    public class Ride
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

        public Ride()
        {

        }
        public Ride(Guid riderID, PlaceCore pickup, PlaceCore dropoff, PlaceCore[]? stops = null)
        {
            RideId = Guid.NewGuid();
            RiderID = riderID;
            Pickup = new Pickup(pickup);
            DropOff = new DropOff(dropoff);
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
    }

    public class RidePlanUpdate : RidePlan
    {
        [JsonConstructor]
        public RidePlanUpdate(PlaceCore pickup, PlaceCore dropoff, PlaceCore[] stops, DateTimeOffset timeReceived) : base(pickup, dropoff, stops)
        {
            TimeReceived = timeReceived;
        }
        public DateTimeOffset TimeReceived { get; set; }
        public RidePlanUpdate(RidePlan ridePlan) : base(ridePlan)
        {
            TimeReceived = DateTimeOffset.Now;
        }
    }
    public class RidePlan
    {
        public PlaceCore Pickup { get; set; }
        public PlaceCore[] Stops { get; set; }
        public PlaceCore DropOff { get; set; }
        public RidePlan(PlaceCore pickup, PlaceCore dropOff)
        {
            Pickup = pickup;
            DropOff = dropOff;
            Stops = new PlaceCore[0];
        }
        public RidePlan(PlaceCore pickup, PlaceCore dropOff, PlaceCore[] stops)
        {
            Pickup = pickup;
            DropOff = dropOff;
            Stops = stops;
        }
        public RidePlan(RidePlan ridePlan)
        {
            Pickup = ridePlan.Pickup;
            DropOff = ridePlan.DropOff;
            Stops = ridePlan.Stops;
        }
    }

    /// <summary>
    /// A collection of RideEventUpdate objects designed to be stored as a varbinary or blob
    /// </summary>
    public class RideEventUpdateContinuum : IList<RideEventUpdate>
    {
        private RideEventUpdate[] collection;
        public bool IsReadOnly { get; private set; } = true;
        public bool IsFixedSize { get; private set; } = true;

        public int Count { get { return collection.Length; } }

        public void Add(RideEventUpdate obj)
        {
            throw new NotImplementedException("RideEventUpdateContinuum is fixed size");
        }
        public RideEventUpdate this[int index]
        {
            get
            {
                return collection[index];
            }
            set
            {
                throw new NotImplementedException("RideEventUpdateContinuum is read only");
            }
        }
        public bool Contains(RideEventUpdate rideEventServer)
        {
            return collection.Contains(rideEventServer);
        }
        public int IndexOf(RideEventUpdate rideEventServer)
        {
            return Array.IndexOf(collection, rideEventServer);
        }
        public void Insert(int index, RideEventUpdate rideEventServer)
        {
            throw new NotImplementedException();
        }
        public void RemoveAt(int index)
        {
            throw new NotImplementedException("RideEventUpdateContinuum is fixed size");
        }
        public bool Remove(RideEventUpdate rideEventServer)
        {
            throw new NotImplementedException("RideEventUpdateContinuum is fixed size");
        }
        public void Clear()
        {
            throw new NotImplementedException("RideEventUpdateContinuum is read only");
        }
        public void CopyTo(RideEventUpdate[] array, int Index)
        {
            for (int i = 0; i < Count; i++)
            {
                array[i] = this[i];
            }
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return new RideEventUpdateContinuumEnum(collection);
        }
        public IEnumerator<RideEventUpdate> GetEnumerator()
        {
            return new RideEventUpdateContinuumEnum(collection);
        }
        private RideEventUpdateContinuum(byte[] bytes)
        {
            if (bytes.Length < 4)
            {
                collection = new RideEventUpdate[0];
                return;
            }
            int length = BitConverter.ToInt32(bytes, 0);
            collection = new RideEventUpdate[length];
            for (int i = 0; i < length; i++)
            {
                collection[i] = new RideEventUpdate(
                    new RideEvent(
                        (RideEventType)bytes[i * 49 + 4],
                        new SpaceTime(
                            new Position(
                                BitConverter.ToDouble(bytes, i * 49 + 5),
                                BitConverter.ToDouble(bytes, i * 49 + 13)),
                            new DateTimeOffset(
                                BitConverter.ToInt64(bytes, i * 49 + 21),
                                new TimeSpan(
                                    BitConverter.ToInt64(bytes, i * 49 + 29))))),
                    new DateTimeOffset(
                        BitConverter.ToInt64(bytes, i * 49 + 37),
                        new TimeSpan(
                            BitConverter.ToInt64(bytes, i * 49 + 45))));
            }
        }
        public static RideEventUpdateContinuum FromBlob(byte[] blob)
        {
            return new RideEventUpdateContinuum(blob);
        }
        public byte[] ToBlob()
        {
            byte[] bytes = new byte[Count * 49 + 4];
            Array.Copy(BitConverter.GetBytes(Count), bytes, 4);
            for (int i = 0; i < Count; i++)
            {
                bytes[i * 49 + 4] = (byte)collection[i].EventType;
                Array.Copy(BitConverter.GetBytes(collection[i].SpaceTime.position.lat), 0, bytes, i * 49 + 5, 8);
                Array.Copy(BitConverter.GetBytes(collection[i].SpaceTime.position.lng), 0, bytes, i * 49 + 13, 8);

                Array.Copy(BitConverter.GetBytes(collection[i].SpaceTime.time.Ticks), 0, bytes, i * 49 + 21, 8);
                Array.Copy(BitConverter.GetBytes(collection[i].SpaceTime.time.Offset.Ticks), 0, bytes, i * 49 + 29, 8);

                Array.Copy(BitConverter.GetBytes(collection[i].TimeReceived.Ticks), 0, bytes, i * 49 + 37, 8);
                Array.Copy(BitConverter.GetBytes(collection[i].TimeReceived.Offset.Ticks), 0, bytes, i * 49 + 45, 8);
            }
            return bytes;
        }
        public byte[] ToBlobStreamer()
        {
            byte[] bytes = new byte[Count * 49 + 4];

            using (MemoryStream stream = new MemoryStream(bytes, true))
            {

/* Unmerged change from project 'TrevorsRidesHelpers (net8.0)'
Before:
                stream.Write(BitConverter.GetBytes(Count), 0, 4);

                
                for (int i = 0; i < Count; i++)
After:
                stream.Write(BitConverter.GetBytes(Count), 0, 4);


                for (int i = 0; i < Count; i++)
*/
                stream.Write(BitConverter.GetBytes(Count), 0, 4);


                for (int i = 0; i < Count; i++)
                {
                    stream.Write(new byte[] { (byte)collection[i].EventType }, 0, 1);

                    stream.Write(BitConverter.GetBytes(collection[i].SpaceTime.position.lat), 0, 8);
                    stream.Write(BitConverter.GetBytes(collection[i].SpaceTime.position.lng), 0, 8);

                    stream.Write(BitConverter.GetBytes(collection[i].SpaceTime.time.Ticks), 0, 8);
                    stream.Write(BitConverter.GetBytes(collection[i].SpaceTime.time.Offset.Ticks), 0, 8);

                    stream.Write(BitConverter.GetBytes(collection[i].TimeReceived.Ticks), 0, 8);
                    stream.Write(BitConverter.GetBytes(collection[i].TimeReceived.Offset.Ticks), 0, 8);
                }
            }
            return bytes;
        }
    }






    public class RideEventUpdateContinuumEnum : IEnumerator<RideEventUpdate>
    {
        public RideEventUpdate[] _rideEventUpdateContinuum;

        // Enumerators are positioned before the first element
        // until the first MoveNext() call.
        int position = -1;

        public RideEventUpdateContinuumEnum(RideEventUpdate[] list)
        {
            _rideEventUpdateContinuum = list;
        }

        public bool MoveNext()
        {
            position++;
            return position < _rideEventUpdateContinuum.Length;
        }

        public void Reset()
        {
            position = -1;
        }

        object IEnumerator.Current
        {
            get
            {
                return Current;
            }
        }

        public RideEventUpdate Current
        {
            get
            {
                try
                {
                    return _rideEventUpdateContinuum[position];
                }
                catch (IndexOutOfRangeException)
                {
                    throw new InvalidOperationException();
                }
            }
        }
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        ~RideEventUpdateContinuumEnum()
        {
            Dispose(disposing: false);
        }
    }




    public class RideEventUpdate : RideEvent
    {
        public DateTimeOffset TimeReceived { get; set; }

        public RideEventUpdate(RideEvent rideEvent) : base(rideEvent)
        {
            TimeReceived = DateTimeOffset.Now;
        }
        public RideEventUpdate(RideEvent rideEvent, DateTimeOffset timeReceived) : base(rideEvent)
        {
            TimeReceived = timeReceived;
        }
    }

    public class RideEvent
    {
        public RideEventType EventType { get; set; }
        public SpaceTime SpaceTime { get; set; }

        public RideEvent(RideEventType eventType, SpaceTime spaceTime)
        {
            EventType = eventType;
            SpaceTime = spaceTime;
        }
        public RideEvent(RideEvent rideEvent)
        {
            EventType = rideEvent.EventType;
            SpaceTime = rideEvent.SpaceTime;
        }
    }






    public enum RideEventType
    {
        Requested,
        Paid,
        Accepted,
        Denied,
        EnRouteToPickup,
        ArrivedAtPickup,
        EnRouteToStop,
        ArrivedAtStop,
        EnRouteToDestination,
        ArrivedAtDestination,
        Completed,
        RiderCanceled,
        DriverCanceled
    }






    /// <summary>
    /// A collection of SpaceTime objects designed to be stored as a varbinary or blob
    /// </summary>
    public class SpaceTimeContinuum : IList<SpaceTime>
    {
        private SpaceTime[] collection;
        public bool IsReadOnly { get; private set; } = true;
        public bool IsFixedSize { get; private set; } = true;

        public int Count { get { return collection.Length; } }

        public void Add(SpaceTime obj)
        {
            throw new NotImplementedException("SpaceTimeContinuum is fixed size");
        }
        public SpaceTime this[int index]
        {
            get
            {
                return collection[index];
            }
            set
            {
                throw new NotImplementedException("SpaceTimeContinuum is read only");
            }
        }
        public bool Contains(SpaceTime spaceTime)
        {
            return collection.Contains(spaceTime);
        }
        public int IndexOf(SpaceTime spaceTime)
        {
            return Array.IndexOf(collection, spaceTime);
        }
        public void Insert(int index, SpaceTime spaceTime)
        {
            throw new NotImplementedException();
        }
        public void RemoveAt(int index)
        {
            throw new NotImplementedException("SpaceTimeContinuum is fixed size");
        }
        public bool Remove(SpaceTime spaceTime)
        {
            throw new NotImplementedException("SpaceTimeContinuum is fixed size");
        }
        public void Clear()
        {
            throw new NotImplementedException("SpaceTimeContinuum is read only");
        }
        public void CopyTo(SpaceTime[] array, int Index)
        {
            for (int i = 0; i < Count; i++)
            {
                array[i] = this[i];
            }
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return new SpaceTimeContinuumEnum(collection);
        }
        public IEnumerator<SpaceTime> GetEnumerator()
        {
            return new SpaceTimeContinuumEnum(collection);
        }
        private SpaceTimeContinuum(byte[] bytes)
        {
            if (bytes.Length < 4)
            {
                collection = new SpaceTime[0];
                return;
            }
            int length = BitConverter.ToInt32(bytes, 0);
            collection = new SpaceTime[length];
            for (int i = 0; i < length; i++)
            {
                collection[i] = new SpaceTime(
                    new Position(
                        BitConverter.ToDouble(bytes, i * 32 + 4),
                        BitConverter.ToDouble(bytes, i * 32 + 12)),
                    new DateTimeOffset(
                        BitConverter.ToInt64(bytes, i * 32 + 20),
                        new TimeSpan(BitConverter.ToInt64(bytes, i * 32 + 28))));


            }
        }
        public static SpaceTimeContinuum FromBlob(byte[] blob)
        {
            return new SpaceTimeContinuum(blob);
        }
        public byte[] ToBlob()
        {
            byte[] bytes = new byte[Count * 32 + 4];
            Array.Copy(BitConverter.GetBytes(Count), bytes, 4);
            for (int i = 0; i < Count; i++)
            {
                Array.Copy(BitConverter.GetBytes(collection[i].position.lat), 0, bytes, i * 32 + 4, 8);
                Array.Copy(BitConverter.GetBytes(collection[i].position.lng), 0, bytes, i * 32 + 12, 8);
                Array.Copy(BitConverter.GetBytes(collection[i].time.Ticks), 0, bytes, i * 32 + 20, 8);
                Array.Copy(BitConverter.GetBytes(collection[i].time.Offset.Ticks), 0, bytes, i * 32 + 28, 8);
            }
            return bytes;
        }
    }






    public class SpaceTimeContinuumEnum : IEnumerator<SpaceTime>
    {
        public SpaceTime[] _spaceTimeContinuum;

        // Enumerators are positioned before the first element
        // until the first MoveNext() call.
        int position = -1;

        public SpaceTimeContinuumEnum(SpaceTime[] list)
        {
            _spaceTimeContinuum = list;
        }

        public bool MoveNext()
        {
            position++;
            return position < _spaceTimeContinuum.Length;
        }

        public void Reset()
        {
            position = -1;
        }

        object IEnumerator.Current
        {
            get
            {
                return Current;
            }
        }

        public SpaceTime Current
        {
            get
            {
                try
                {
                    return _spaceTimeContinuum[position];
                }
                catch (IndexOutOfRangeException)
                {
                    throw new InvalidOperationException();
                }
            }
        }
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        ~SpaceTimeContinuumEnum()
        {
            Dispose(disposing: false);
        }
    }



    /// <summary>
    /// A collection of SpaceTimeUpdate objects designed to be stored as a varbinary or blob
    /// </summary>
    public class SpaceTimeUpdateContinuum : IList<SpaceTimeUpdate>
    {
        private SpaceTimeUpdate[] collection;
        public bool IsReadOnly { get; private set; } = true;
        public bool IsFixedSize { get; private set; } = true;

        public int Count { get { return collection.Length; } }

        public void Add(SpaceTimeUpdate obj)
        {
            throw new NotImplementedException("SpaceTimeServerContinuum is fixed size");
        }
        public SpaceTimeUpdate this[int index]
        {
            get
            {
                return collection[index];
            }
            set
            {
                throw new NotImplementedException("SpaceTimeServerContinuum is read only");
            }
        }
        public bool Contains(SpaceTimeUpdate spaceTimeServer)
        {
            return collection.Contains(spaceTimeServer);
        }
        public int IndexOf(SpaceTimeUpdate spaceTimeServer)
        {
            return Array.IndexOf(collection, spaceTimeServer);
        }
        public void Insert(int index, SpaceTimeUpdate spaceTimeServer)
        {
            throw new NotImplementedException();
        }
        public void RemoveAt(int index)
        {
            throw new NotImplementedException("SpaceTimeServerContinuum is fixed size");
        }
        public bool Remove(SpaceTimeUpdate spaceTimeServer)
        {
            throw new NotImplementedException("SpaceTimeServerContinuum is fixed size");
        }
        public void Clear()
        {
            throw new NotImplementedException("SpaceTimeServerContinuum is read only");
        }
        public void CopyTo(SpaceTimeUpdate[] array, int Index)
        {
            for (int i = 0; i < Count; i++)
            {
                array[i] = this[i];
            }
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return new SpaceTimeUpdateContinuumEnum(collection);
        }
        public IEnumerator<SpaceTimeUpdate> GetEnumerator()
        {
            return new SpaceTimeUpdateContinuumEnum(collection);
        }
        private SpaceTimeUpdateContinuum(byte[] bytes)
        {
            if (bytes.Length < 4)
            {
                collection = new SpaceTimeUpdate[0];
                return;
            }
            int length = BitConverter.ToInt32(bytes, 0);
            collection = new SpaceTimeUpdate[length];
            for (int i = 0; i < length; i++)
            {
                collection[i] = new SpaceTimeUpdate(
                    new SpaceTime(
                        new Position(
                            BitConverter.ToDouble(bytes, i * 48 + 4),
                            BitConverter.ToDouble(bytes, i * 48 + 12)),
                        new DateTimeOffset(
                            BitConverter.ToInt64(bytes, i * 48 + 20),
                            new TimeSpan(
                                BitConverter.ToInt64(bytes, i * 48 + 28)))),
                    new DateTimeOffset(
                        BitConverter.ToInt64(bytes, i * 48 + 36),
                        new TimeSpan(
                            BitConverter.ToInt64(bytes, i * 48 + 44))));
            }
        }
        public static SpaceTimeUpdateContinuum FromBlob(byte[] blob)
        {
            return new SpaceTimeUpdateContinuum(blob);
        }
        public byte[] ToBlob()
        {
            byte[] bytes = new byte[Count * 48 + 4];
            Array.Copy(BitConverter.GetBytes(Count), bytes, 4);
            for (int i = 0; i < Count; i++)
            {
                Array.Copy(BitConverter.GetBytes(collection[i].position.lat), 0, bytes, i * 48 + 4, 8);
                Array.Copy(BitConverter.GetBytes(collection[i].position.lng), 0, bytes, i * 48 + 12, 8);
                Array.Copy(BitConverter.GetBytes(collection[i].time.Ticks), 0, bytes, i * 48 + 20, 8);
                Array.Copy(BitConverter.GetBytes(collection[i].time.Offset.Ticks), 0, bytes, i * 48 + 28, 8);
                Array.Copy(BitConverter.GetBytes(collection[i].TimeReceived.Ticks), 0, bytes, i * 48 + 36, 8);
                Array.Copy(BitConverter.GetBytes(collection[i].TimeReceived.Offset.Ticks), 0, bytes, i * 48 + 44, 8);
            }
            return bytes;
        }
    }






    public class SpaceTimeUpdateContinuumEnum : IEnumerator<SpaceTimeUpdate>
    {
        public SpaceTimeUpdate[] _spaceTimeServerContinuum;

        // Enumerators are positioned before the first element
        // until the first MoveNext() call.
        int position = -1;

        public SpaceTimeUpdateContinuumEnum(SpaceTimeUpdate[] list)
        {
            _spaceTimeServerContinuum = list;
        }

        public bool MoveNext()
        {
            position++;
            return position < _spaceTimeServerContinuum.Length;
        }

        public void Reset()
        {
            position = -1;
        }

        object IEnumerator.Current
        {
            get
            {
                return Current;
            }
        }

        public SpaceTimeUpdate Current
        {
            get
            {
                try
                {
                    return _spaceTimeServerContinuum[position];
                }
                catch (IndexOutOfRangeException)
                {
                    throw new InvalidOperationException();
                }
            }
        }
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        ~SpaceTimeUpdateContinuumEnum()
        {
            Dispose(disposing: false);
        }
    }





    /// <summary>
    /// This class is used to store SpaceTime objects on the server with an additional property denoting the time the SpaceTime object was received
    /// </summary>
    public class SpaceTimeUpdate : SpaceTime
    {
        public DateTimeOffset TimeReceived { get; set; }
        public SpaceTimeUpdate(SpaceTime spaceTime) : base(spaceTime.position, spaceTime.time)
        {
            TimeReceived = DateTimeOffset.Now;
        }
        public SpaceTimeUpdate(SpaceTime spaceTime, DateTimeOffset timeReceived) : base(spaceTime.position, spaceTime.time)
        {
            TimeReceived = timeReceived;
        }
        public SpaceTimeUpdate(byte[] bytes)
        {
            byte[] latitude = new byte[8];
            byte[] longitude = new byte[8];
            byte[] clientTime = new byte[8];
            byte[] clientTimeOffset = new byte[8];
            byte[] serverTime = new byte[8];
            byte[] serverTimeOffset = new byte[8];

            Array.Copy(bytes, 0, latitude, 0, 8);
            Array.Copy(bytes, 8, longitude, 0, 8);
            Array.Copy(bytes, 16, clientTime, 0, 8);
            Array.Copy(bytes, 24, clientTimeOffset, 0, 8);
            Array.Copy(bytes, 32, serverTime, 0, 8);
            Array.Copy(bytes, 40, serverTimeOffset, 0, 8);

            position =
                new Position(
                    BitConverter.ToDouble(latitude),
                    BitConverter.ToDouble(longitude));

            time =
                new DateTimeOffset(
                    BitConverter.ToInt64(clientTime),
                    new TimeSpan(
                        BitConverter.ToInt64(clientTimeOffset)));

            TimeReceived =
                new DateTimeOffset(
                    BitConverter.ToInt64(serverTime),
                    new TimeSpan(
                        BitConverter.ToInt64(serverTimeOffset)));
        }
        public byte[] ToBytes()
        {
            byte[] latitude = BitConverter.GetBytes(position.lat);
            byte[] longitude = BitConverter.GetBytes(position.lng);
            byte[] clientTime = BitConverter.GetBytes(time.Ticks);
            byte[] clientTimeOffset = BitConverter.GetBytes(time.Offset.Ticks);
            byte[] serverTime = BitConverter.GetBytes(TimeReceived.Ticks);
            byte[] serverTimeOffset = BitConverter.GetBytes(TimeReceived.Offset.Ticks);
            byte[] bytes = new byte[48];

            for (int i = 0; i < 8; i++)
            {
                bytes[i] = latitude[i];
                bytes[i + 8] = longitude[i];
                bytes[i + 16] = clientTime[i];
                bytes[i + 24] = clientTimeOffset[i];
                bytes[i + 32] = serverTime[i];
                bytes[i + 40] = serverTimeOffset[i];
            }

            return bytes;
        }
    }


    public class TripRequest
    {
        public PlaceCore Pickup { get; set; }
        public PlaceCore[] Stops { get; set; }
        public PlaceCore Dropoff { get; set; }
        [JsonConstructor]
        public TripRequest(PlaceCore pickup, PlaceCore dropoff, PlaceCore[] stops)
        {
            Stops = stops;
            Pickup = pickup;
            Dropoff = dropoff;
        }
        public TripRequest(PlaceCore pickup, PlaceCore dropoff) : this(pickup, dropoff, new PlaceCore[0]) { }
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
        [JsonConstructor]
        public PlaceCore(LatLngLiteral latLng, string name, Address address)
        {
            LatLng = latLng;
            Name = name;
            Address = address;
        }
        public Waypoint ToWaypoint()
        {
            Waypoint waypoint = new Waypoint();
            waypoint.vehicleStopover = true;
            waypoint.location = new Goog.Location()
            {

                latLng = new Goog.LatLng()
                {
                    latitude = LatLng.lat,
                    longitude = LatLng.lng
                }
            };
            return waypoint;
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
        [JsonConstructor]
        public Address(string streetAddress, string city, string state, string postalCode, string country) 
        {
            StreetAddress = streetAddress;
            City = city;
            State = state;
            PostalCode = postalCode;
            Country = country;
            ShortName = StreetAddress + ", " + City;
            LongName = ShortName + ", " + State + " " + PostalCode;
            FullName = LongName + ", " + Country;
        }
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
        public Pickup(PlaceCore location, DateTimeOffset? arrivalTime = null, DateTimeOffset? departureTime = null) : base(location, arrivalTime, departureTime)
        {
            Location = location;
            ArrivalTime = arrivalTime;
            DepartureTime = departureTime;
        }
    }





    public class Stop
    {
        private DateTimeOffset? _arrivalTime;
        private DateTimeOffset? _departureTime;

        public PlaceCore Location { get; set; }
        public DateTimeOffset? ArrivalTime
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
        public DateTimeOffset? DepartureTime
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
        public Stop(PlaceCore location, DateTimeOffset? arrivalTime = null, DateTimeOffset? departureTime = null)
        {
            Location = location;
            ArrivalTime = arrivalTime;
            DepartureTime = departureTime;
        }
    }






    public class DropOff
    {
        private DateTimeOffset? _dropOffTime;
        public PlaceCore Location { get; set; }
        public DateTimeOffset? DropOffTime
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

        public DropOff(PlaceCore location, DateTimeOffset? dropOffTime = null)
        {
            Location = location;
            DropOffTime = dropOffTime;
        }
    }
}
