
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace TrevorsRidesHelpers.GoogleApiClasses
{
    public class RoutesResponse
    {
        public Route[] routes { get; set; }
        public FallbackInfo fallbackInfo { get; set; }
        public GeocodingResults geocodingResults { get; set; }
    }
    public class Route
    {
        public RouteLabel[]? routeLabels { get; set; }
        public RouteLeg[]? legs { get; set; }
        public int? distanceMeters { get; set; }
        public string? duration { get; set; }
        public string? staticDuration { get; set; }
        public Polyline? polyline { get; set; }
        public string? description { get; set; }
        public string[]? warnings { get; set; }
        public Viewport? viewport { get; set; }
        public RouteTravelAdvisory? travelAdvisory { get; set; }
        public string? routeToken { get; set; }
    }
    public enum RouteLabel
    {
        ROUTE_LABEL_UNSPECIFIED,
        DEFAULT_ROUTE,
        DEFAULT_ROUTE_ALTERNATE,
        FUEL_EFFICIENT
    }
    public class RouteLeg
    {
        public int distanceMeters { get; set; }
        public string duration { get; set; }
        public string staticDuration { get; set; }
        public Polyline polyline { get; set; }
        public Location startLocation { get; set; }
        public RouteLegStep[] steps { get; set; }
        public RouteLegTravelAdvisory travelAdvisory { get; set; }
    }
    public class Polyline
    {
        //Missing geoJsonLinestring Property because @ https://developers.google.com/maps/documentation/routes/reference/rest/v2/TopLevel/computeRoutes#Polyline
        //The description for geoJsonLinestring is confusing, it's not needed anyways as encodedPolyline is all that's needed
        public string? encodedPolyline { get; set; }
    }
    public class RouteLegStep
    {
        public int? distanceMeters { get; set; }
        public string? staticDuration { get; set; }
        public Polyline polyline { get; set; }
        public Location startLocation { get; set; }
        public Location endLocation { get; set; }
        public NavigationInstruction navigationInstruction { get; set; }
        public RouteLegStepTravelAdvisory travelAdvisory { get; set; }
    }
    public class NavigationInstruction
    {
        public string maneuver { get; set; }
        public string instructions { get; set; }
    }
    public enum Maneuver
    {
        MANEUVER_UNSPECIFIED,
        TURN_SLIGHT_LEFT,
        TURN_SHARP_LEFT,
        UTURN_LEFT,
        TURN_LEFT,
        TURN_SLIGHT_RIGHT,
        TURN_SHARP_RIGHT,
        UTURN_RIGHT,
        TURN_RIGHT,
        STRAIGHT,
        RAMP_LEFT,
        RAMP_RIGHT,
        MERGE,
        FORK_LEFT,
        FORK_RIGHT,
        FERRY,
        FERRY_TRAIN,
        ROUNDABOUT_LEFT,
        ROUNDABOUT_RIGHT
    }
    public class RouteLegStepTravelAdvisory
    {
        public SpeedReadingInterval[]? speedReadingInterval { get; set; }
    }
    public class SpeedReadingInterval
    {
        public int startPolylinePointIndex { get; set; }
        public int endPolylinePointIndex { get; set; }
        public Speed speed { get; set; }
    }
    public enum Speed
    {
        SPEED_UNSPECIFIED,
        NORMAL,
        SLOW,
        TRAFFIC_JAM
    }
    public class RouteLegTravelAdvisory
    {
        public TollInfo tollInfo { get; set; }
        public SpeedReadingInterval[] speedReadingIntervals { get; set; }
    }
    public class Viewport
    {
        public LatLng low { get; set; }
        public LatLng high { get; set; }
    }
    public class GeocodingResults
    {
        public GeocodedWaypoint origin { get; set; }
        public GeocodedWaypoint destination { get; set; }
        public GeocodedWaypoint[]? intermediates { get; set; }
    }
    public class GeocodedWaypoint
    {
        public Status gecoderStatus { get; set; }
        public string[] type { get; set; }
        public bool partialMatch { get; set; }
        public string placeId { get; set; }
        public int intermediateWaypointRequestIndex { get; set; }
    }
    public class Status
    {
        public int code { get; set; }
        public string message { get; set; }
        public object[] details { get; set; }
    }
    public class TollInfo
    {
        public Money[] estimatedPrice { get; set; } 
    }
    public class Money
    {
        public string currencyCode { get; set; }
        public string units { get; set; }
        public int nanos { get; set; }
    }



    public class FallbackInfo
    {
        public FallbackRoutingMode routingMode { get; set; }
        public FallbackReason reason { get; set; }
    }
    public enum FallbackRoutingMode
    {
        FALLBACK_ROUTING_MODE_UNSPECIFIED,
        FALLBACK_TRAFFIC_UNAWARE,
        FALLBACK_TRAFFIC_AWARE
    }
    public enum FallbackReason
    {
        FALLBACK_REASON_UNSPECIFIED,
        SERVER_ERROR,
        LATENCY_EXCEEDED
    }

    public class RouteTravelAdvisory
    {
        public TollInfo tollInfo { get; set; }
        public SpeedReadingInterval[] speedReadingIntervals { get; set; }
        public string fuelConsumptionMicroliters { get; set; }
    }



}
