using Microsoft.Maui.Controls.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TrevorsRidesHelpers.GoogleApiClasses;
using TrevorsRidesHelpers.Ride;
using Sensors = Microsoft.Maui.Devices.Sensors;
using Maps = Microsoft.Maui.Controls.Maps;
using Onion = Maui.GoogleMaps;

namespace TrevorsRidesHelpers
{
    public class Map
    {
        public static async Task<RoutesResponse> GetRouteAsync(TripRequest tripRequest)
        {
            if (tripRequest.Stops.Length == 0)
            {
                return await GetRouteAsync(tripRequest.Pickup, tripRequest.Dropoff);
            }
            return await GetRouteAsync(tripRequest.Pickup, tripRequest.Dropoff, tripRequest.Stops);
        }
        public static async Task<RoutesResponse> GetRouteAsync(PlaceCore pickup, PlaceCore dropoff, PlaceCore[]? stops = null)
        {
            Uri uri = new Uri("https://routes.googleapis.com/directions/v2:computeRoutes");
            RoutesRequest request;
            Waypoint[]? intermediates = null;
            if (stops != null)
            {
                intermediates = new Waypoint[stops.Length];
                for (int i = 0; i < stops.Length; i++)
                {
                    intermediates[i] = stops[i].ToWaypoint();
                }
            }
            
            request = new RoutesRequest()
            {
                origin = dropoff.ToWaypoint(),
                intermediates = intermediates,
                destination = pickup.ToWaypoint(),
                polylineQuality = PolylineQuality.OVERVIEW
            };
            

            string message = JsonSerializer.Serialize(request);
            StringContent content = new StringContent(message);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            content.Headers.Add("X-Goog-Api-Key", APIKeys.GoogleEverythingKey);
            if (request.intermediates == null)
                content.Headers.Add("X-Goog-FieldMask", "routes.duration,routes.distanceMeters,routes.polyline.encodedPolyline,routes.viewport");
            else
                content.Headers.Add("X-Goog-FieldMask", "routes.duration,routes.legs.duration,routes.legs.polyline.encodedPolyline,routes.viewport");
            HttpResponseMessage response = await Web.HttpClient.PostAsync(uri, content);
            string jsonResponse = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<RoutesResponse>(jsonResponse);
            
        }
        public static List<Sensors.Location> DecodeWithZoom(string encodedPoints, Viewport viewport)
        {
            double latitudeRange = viewport.high.latitude - viewport.low.latitude;
            double longitudeRange = viewport.high.longitude - viewport.low.longitude;
            Sensors.Location? lastPosition = null;
            Sensors.Location thisPosition;

            List<Sensors.Location> decodedPoints = new List<Sensors.Location>();
            if (string.IsNullOrEmpty(encodedPoints))
                throw new ArgumentNullException("encodedPoints");

            char[] polylineChars = encodedPoints.ToCharArray();
            int index = 0;

            int currentLat = 0;
            int currentLng = 0;
            int next5bits;
            int sum;
            int shifter;

            while (index < polylineChars.Length)
            {
                // calculate next latitude
                sum = 0;
                shifter = 0;
                do
                {
                    next5bits = (int)polylineChars[index++] - 63;
                    sum |= (next5bits & 31) << shifter;
                    shifter += 5;
                } while (next5bits >= 32 && index < polylineChars.Length);

                if (index >= polylineChars.Length)
                    return decodedPoints;

                currentLat += (sum & 1) == 1 ? ~(sum >> 1) : (sum >> 1);

                //calculate next longitude
                sum = 0;
                shifter = 0;
                do
                {
                    next5bits = (int)polylineChars[index++] - 63;
                    sum |= (next5bits & 31) << shifter;
                    shifter += 5;
                } while (next5bits >= 32 && index < polylineChars.Length);

                if (index >= polylineChars.Length && next5bits >= 32)
                    return decodedPoints;

                currentLng += (sum & 1) == 1 ? ~(sum >> 1) : (sum >> 1);

                thisPosition = new Sensors.Location(Convert.ToDouble(currentLat) / 1E5, Convert.ToDouble(currentLng) / 1E5);
                if (lastPosition == null || Math.Abs(thisPosition.Latitude - lastPosition.Latitude) > latitudeRange / 100 || Math.Abs(thisPosition.Longitude - lastPosition.Longitude) > longitudeRange / 100)
                {
                    decodedPoints.Add(thisPosition);
                    lastPosition = thisPosition;
                }


            }
            return decodedPoints;
        }
        public static Maps.Polyline DecodeWithZoomReturnPolyline(string encodedPoints, Viewport viewport)
        {
            double latitudeRange = viewport.high.latitude - viewport.low.latitude;
            double longitudeRange = viewport.high.longitude - viewport.low.longitude;
            Sensors.Location? lastPosition = null;
            Sensors.Location thisPosition;
            Maps.Polyline polyline = new Maps.Polyline();

            
            if (string.IsNullOrEmpty(encodedPoints))
                throw new ArgumentNullException("encodedPoints");

            char[] polylineChars = encodedPoints.ToCharArray();
            int index = 0;

            int currentLat = 0;
            int currentLng = 0;
            int next5bits;
            int sum;
            int shifter;

            while (index < polylineChars.Length)
            {
                // calculate next latitude
                sum = 0;
                shifter = 0;
                do
                {
                    next5bits = (int)polylineChars[index++] - 63;
                    sum |= (next5bits & 31) << shifter;
                    shifter += 5;
                } while (next5bits >= 32 && index < polylineChars.Length);

                if (index >= polylineChars.Length)
                    return polyline;

                currentLat += (sum & 1) == 1 ? ~(sum >> 1) : (sum >> 1);

                //calculate next longitude
                sum = 0;
                shifter = 0;
                do
                {
                    next5bits = (int)polylineChars[index++] - 63;
                    sum |= (next5bits & 31) << shifter;
                    shifter += 5;
                } while (next5bits >= 32 && index < polylineChars.Length);

                if (index >= polylineChars.Length && next5bits >= 32)
                    return polyline;

                currentLng += (sum & 1) == 1 ? ~(sum >> 1) : (sum >> 1);

                thisPosition = new Sensors.Location(Convert.ToDouble(currentLat) / 1E5, Convert.ToDouble(currentLng) / 1E5);
                if (lastPosition == null || Math.Abs(thisPosition.Latitude - lastPosition.Latitude) > latitudeRange / 100 || Math.Abs(thisPosition.Longitude - lastPosition.Longitude) > longitudeRange / 100)
                {
                    polyline.Geopath.Add(thisPosition);
                    lastPosition = thisPosition;
                }


            }
            return polyline;
        }
        public static Onion.Polyline DecodeWithZoomReturnOnionPolyline(string encodedPoints, Viewport viewport)
        {
            double latitudeRange = viewport.high.latitude - viewport.low.latitude;
            double longitudeRange = viewport.high.longitude - viewport.low.longitude;
            Onion.Position? lastPosition = null;
            Onion.Position thisPosition;
            Onion.Polyline polyline = new Onion.Polyline();


            if (string.IsNullOrEmpty(encodedPoints))
                throw new ArgumentNullException("encodedPoints");

            char[] polylineChars = encodedPoints.ToCharArray();
            int index = 0;

            int currentLat = 0;
            int currentLng = 0;
            int next5bits;
            int sum;
            int shifter;

            while (index < polylineChars.Length)
            {
                // calculate next latitude
                sum = 0;
                shifter = 0;
                do
                {
                    next5bits = (int)polylineChars[index++] - 63;
                    sum |= (next5bits & 31) << shifter;
                    shifter += 5;
                } while (next5bits >= 32 && index < polylineChars.Length);

                if (index >= polylineChars.Length)
                    return polyline;

                currentLat += (sum & 1) == 1 ? ~(sum >> 1) : (sum >> 1);

                //calculate next longitude
                sum = 0;
                shifter = 0;
                do
                {
                    next5bits = (int)polylineChars[index++] - 63;
                    sum |= (next5bits & 31) << shifter;
                    shifter += 5;
                } while (next5bits >= 32 && index < polylineChars.Length);

                if (index >= polylineChars.Length && next5bits >= 32)
                    return polyline;

                currentLng += (sum & 1) == 1 ? ~(sum >> 1) : (sum >> 1);

                thisPosition = new Onion.Position(Convert.ToDouble(currentLat) / 1E5, Convert.ToDouble(currentLng) / 1E5);
                if (lastPosition == null || Math.Abs(thisPosition.Latitude - lastPosition.Value.Latitude) > latitudeRange / 100 || Math.Abs(thisPosition.Longitude - lastPosition.Value.Longitude) > longitudeRange / 100)
                {
                    polyline.Positions.Add(thisPosition);
                    lastPosition = thisPosition;
                }


            }
            return polyline;
        }
        //Probably cuts off the last point in the polyline but also probably isn't noticable with lots of points
        public static Maps.Polyline DecodeReturnPolyline(string encodedPoints)
        {
            Maps.Polyline decodedPoints = new Maps.Polyline();
            if (string.IsNullOrEmpty(encodedPoints))
                throw new ArgumentNullException("encodedPoints");

            char[] polylineChars = encodedPoints.ToCharArray();
            int index = 0;

            int currentLat = 0;
            int currentLng = 0;
            int next5bits;
            int sum;
            int shifter;

            while (index < polylineChars.Length)
            {
                // calculate next latitude
                sum = 0;
                shifter = 0;
                do
                {
                    next5bits = (int)polylineChars[index++] - 63;
                    sum |= (next5bits & 31) << shifter;
                    shifter += 5;
                } while (next5bits >= 32 && index < polylineChars.Length);

                if (index >= polylineChars.Length)
                    return decodedPoints;

                currentLat += (sum & 1) == 1 ? ~(sum >> 1) : (sum >> 1);

                //calculate next longitude
                sum = 0;
                shifter = 0;
                do
                {
                    next5bits = (int)polylineChars[index++] - 63;
                    sum |= (next5bits & 31) << shifter;
                    shifter += 5;
                } while (next5bits >= 32 && index < polylineChars.Length);

                if (index >= polylineChars.Length && next5bits >= 32)
                    return decodedPoints;

                currentLng += (sum & 1) == 1 ? ~(sum >> 1) : (sum >> 1);

                decodedPoints.Add(new Sensors.Location(Convert.ToDouble(currentLat) / 1E5, Convert.ToDouble(currentLng) / 1E5));

            }
            return decodedPoints;
        }
        //Probably cuts off the last point in the polyline but also probably isn't noticable with lots of points
        public static Onion.Polyline DecodeReturnOnionPolyline(string encodedPoints)
        {
            Onion.Polyline decodedPoints = new Onion.Polyline();
            if (string.IsNullOrEmpty(encodedPoints))
                throw new ArgumentNullException("encodedPoints");

            char[] polylineChars = encodedPoints.ToCharArray();
            int index = 0;

            int currentLat = 0;
            int currentLng = 0;
            int next5bits;
            int sum;
            int shifter;

            while (index < polylineChars.Length)
            {
                // calculate next latitude
                sum = 0;
                shifter = 0;
                do
                {
                    next5bits = (int)polylineChars[index++] - 63;
                    sum |= (next5bits & 31) << shifter;
                    shifter += 5;
                } while (next5bits >= 32 && index < polylineChars.Length);

                if (index >= polylineChars.Length)
                    return decodedPoints;

                currentLat += (sum & 1) == 1 ? ~(sum >> 1) : (sum >> 1);

                //calculate next longitude
                sum = 0;
                shifter = 0;
                do
                {
                    next5bits = (int)polylineChars[index++] - 63;
                    sum |= (next5bits & 31) << shifter;
                    shifter += 5;
                } while (next5bits >= 32 && index < polylineChars.Length);

                if (index >= polylineChars.Length && next5bits >= 32)
                    return decodedPoints;

                currentLng += (sum & 1) == 1 ? ~(sum >> 1) : (sum >> 1);

                
                decodedPoints.Positions.Add(new Onion.Position(Convert.ToDouble(currentLat) / 1E5, Convert.ToDouble(currentLng) / 1E5));

            }
            return decodedPoints;
        }
        //Probably cuts off the last point in the polyline but also probably isn't noticable with lots of points
        public static void DecodeOutPolyline(string encodedPoints, ref Maps.Polyline decodedPoints)
        {
            decodedPoints.Clear();
            if (string.IsNullOrEmpty(encodedPoints))
                throw new ArgumentNullException("encodedPoints");

            char[] polylineChars = encodedPoints.ToCharArray();
            int index = 0;

            int currentLat = 0;
            int currentLng = 0;
            int next5bits;
            int sum;
            int shifter;

            while (index < polylineChars.Length)
            {
                // calculate next latitude
                sum = 0;
                shifter = 0;
                do
                {
                    next5bits = (int)polylineChars[index++] - 63;
                    sum |= (next5bits & 31) << shifter;
                    shifter += 5;
                } while (next5bits >= 32 && index < polylineChars.Length);

                if (index >= polylineChars.Length)
                    return;

                currentLat += (sum & 1) == 1 ? ~(sum >> 1) : (sum >> 1);

                //calculate next longitude
                sum = 0;
                shifter = 0;
                do
                {
                    next5bits = (int)polylineChars[index++] - 63;
                    sum |= (next5bits & 31) << shifter;
                    shifter += 5;
                } while (next5bits >= 32 && index < polylineChars.Length);

                if (index >= polylineChars.Length && next5bits >= 32)
                    return;

                currentLng += (sum & 1) == 1 ? ~(sum >> 1) : (sum >> 1);

                decodedPoints.Add(new Sensors.Location(Convert.ToDouble(currentLat) / 1E5, Convert.ToDouble(currentLng) / 1E5));

            }
            return;
        }
    }
}
