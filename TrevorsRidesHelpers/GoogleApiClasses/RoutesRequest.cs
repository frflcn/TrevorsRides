#if (ANDROID || IOS)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Maui.GoogleMaps;

namespace TrevorsRidesHelpers.GoogleApiClasses
{
    public class RoutesRequest
    {
        public Waypoint origin { get; set; }
        public Waypoint destination { get; set; }
        public Waypoint[]? intermediates { get; set; }
        public RouteTravelMode? travelMode { get; set; }
        public RoutingPreference? routingPreference { get; set; }
        public PolylineQuality? polylineQuality { get; set; }
        public PolylineEncoding? polylineEncoding { get; set; }
        public string? departureTime { get; set; }
        public bool? computeAlternativeRoutes { get; set; }
        public RouteModifiers? routeModifiers { get; set; }
        public string? languageCode { get; set; }
        public string? regionCode { get; set; }
        public string? units { get; set; }
        public ReferenceRoute[]? requestedReferenceRoutes { get; set; }
        public ExtraComputation[]? extraComputations { get; set; }
        
    }
    public class Waypoint
    {
        public Location location { get; set; }
        public bool? via { get; set; }
        public bool? vehicleStopover { get; set; }
        public string? placeId { get; set; }
        public string? address { get; set; }
    }
    public class Location
    {
        public LatLng latLng { get; set; }
        public int? heading { get; set; }
    }
    public class LatLng
    {
        public double longitude { get; set; }
        public double latitude { get; set; }

        public Maui.GoogleMaps.Position ToPosition()
        {
            return new Maui.GoogleMaps.Position(latitude, longitude);
        }
    }
    public class RouteModifiers
    {
        public bool avoidTolls { get; set; }
        public bool avoidHighways { get; set; }
        public bool avoidFerries { get; set; }
    }
    public enum RouteTravelMode
    {
        TRAVEL_MODE_UNSPECIFIED,
        DRIVE,
        BICYCLE,
        WALK,
        TWO_WHEELER
    }
    public enum RoutingPreference
    {
        ROUTING_PREFERENCE_UNSPECIFIED,
        TRAFFIC_UNAWARE,
        TRAFFIC_AWARE,
        TRAFFIC_AWARE_OPTIMAL
    }
    public enum PolylineQuality
    {
        POLYLINE_QUALITY_UNSPECIFIED,
        HIGH_QUALITY,
        OVERVIEW
    }
    public enum PolylineEncoding
    {
        POLYLINE_ENCODING_UNSPECIFIED,
        ENCODED_POLYLINE,
        GEO_JSON_LINESTRING
    }
    public enum Units
    {
        UNITS_UNSPECIFIED,
        METRIC,
        IMPERIAL
    }
    public enum ReferenceRoute
    {
        REFERENCE_ROUTE_UNSPECIFIED,
        FUEL_EFFICIENT
    }
    public enum ExtraComputation
    {
        EXTRA_COMPUTATION_UNSPECIFIED,
        TOLLS,
        FUEL_CONSUMPTION,
        TRAFFIC_ON_POLYLINE
    }
    
}
#endif