using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrevorsRidesHelpers.GoogleApiClasses
{
    public class PlacesDetailsResponse
    {
        public string[] html_attributions { get; set; }
        public Place result { get; set; }
        public string status { get; set; }
        public string[] info_messages { get; set; }
    }
    public class Place
    {
        public AddressComponent[]? address_components { get; set; }
        public string? adr_address { get; set; }
        public string? business_status { get; set; }
        public bool? curbside_pickup { get; set; }
        public PlaceOpeningHours? current_opening_hours { get; set; }
        public bool? delivery { get; set; }
        public bool? dine_in { get; set; }
        public PlaceEditorialSummary? editorial_summary { get; set; }
        public string? formatted_address { get; set; }
        public string? formatted_phone_number { get; set; }
        public Geometry? geometry { get; set; }
        public string? icon { get; set; }
        public string? icon_background_color { get; set; }
        public string? icon_mask_base_uri { get; set; }
        public string? international_phone_number { get; set; }
        public string? name { get; set; }
        public PlaceOpeningHours? opening_hours { get; set; }
        public bool? permanently_closed { get; set; }
        public PlacePhoto[]? photos { get; set; }
        public string? place_id { get; set; }
        public PlusCode? plus_code { get; set; }
        public int? price_level { get; set; }
        public double? rating { get; set; }
        public string? reference { get; set; }
        public bool? reservable { get; set; }
        public PlaceReview[]? reviews { get; set; }
        public string? scope { get; set; }
        public PlaceOpeningHours[]? secondary_opening_hours { get; set; }
        public bool? serves_beer { get; set; }
        public bool? serves_breakfast { get; set; }
        public bool? serves_brunch { get; set; }
        public bool? serves_dinner { get; set; }
        public bool? serves_lunch { get; set; }
        public bool? serves_vegetarian_food { get; set; }
        public bool? serves_wine { get; set; }
        public bool? takeout { get; set; }
        public string[]? types { get; set; }
        public string? url { get; set; }
        public int? user_ratings_total { get; set; }
        public int? utc_offset { get; set; }
        public string? vicinity { get; set; }
        public string? website { get; set; }
        public bool? wheelchair_accessible_entrance { get; set; }

        public Waypoint ToWaypoint()
        {
            if (this.geometry == null)
            {
                throw new NullReferenceException(nameof(this.geometry));
            }
            Waypoint waypoint = new Waypoint();
            waypoint.location = new Location()
            {
                latLng = new LatLng()
                {
                    latitude = this.geometry.location.lat,
                    longitude = this.geometry.location.lng
                }
            };
            return waypoint;
        }
    }
    public class AddressComponent
    {
        public string long_name { get; set; }
        public string short_name { get; set; }
        public string[] types { get; set; }
    }
    public class PlaceEditorialSummary
    {
        public string? language { get; set; }
        public string? overview { get; set; }
    }
    public class Geometry
    {
        public LatLngLiteral location { get; set; }
        public Bounds viewport { get; set; }
    }
    public class LatLngLiteral
    {
        public double lat { get; set; }
        public double lng { get; set; }
    }
    public class Bounds
    {
        public LatLngLiteral northeast { get; set; }
        public LatLngLiteral southwest { get; set; }
    }
    public class PlaceOpeningHours
    {
        public bool? open_now { get; set; }
        public PlaceOpeningHoursPeriod[]? periods { get; set; }
        public PlaceSpecialDay[]? special_days { get; set; }
        public string? type { get; set; }
        public string[]? weekday_text { get; set; }
    }
    public class PlaceOpeningHoursPeriod
    {
        public PlaceOpeningHoursPeriodDetail open { get; set; }
        public PlaceOpeningHoursPeriodDetail? close { get; set; }
    }
    public class PlaceSpecialDay
    {
        public string? date { get; set; }
        public bool? exceptional_hours { get; set; }
    }
    public class PlaceOpeningHoursPeriodDetail
    {
        public int day { get; set; }
        public string time { get; set; }
        public string? date { get; set; }
        public bool? truncated { get; set; }
    }
    public class PlacePhoto
    {
        public double height { get; set; }
        public string[] html_attributions { get; set; }
        public string photo_reference { get; set; }
        public double width { get; set; }
    }
    public class PlusCode
    {
        public string global_code { get; set; }
        public string? compound_code { get; set; }
    }
    public class PlaceReview
    {
        public string author_name { get; set; }
        public int rating { get; set; }
        public string relative_time_description { get; set; }
        public long time { get; set; }
        public string? author_url { get; set; }
        public string? language { get; set; }
        public string? original_language { get; set; }
        public string? profile_photo_url { get; set; }
        public string? text { get; set; }
        public bool? translated { get; set; }
    }


}
