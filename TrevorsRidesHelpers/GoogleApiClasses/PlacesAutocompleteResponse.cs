using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TrevorsRidesHelpers.GoogleApiClasses
{
    public class PlacesAutocompleteResponse
    {
        public PlaceAutocompletePrediction[] predictions { get; set; }
        public string status { get; set; }
        public string? error_message { get; set; }
        public string[]? info_messages { get; set; }
        [JsonConstructor]

        public PlacesAutocompleteResponse(PlaceAutocompletePrediction[] predictions, string status, string? error_message = null, string[]? info_messages = null)
        {
            this.predictions = predictions;
            this.status = status;
            this.error_message = error_message;
            this.info_messages = info_messages;
            
        }
    }
    public class PlaceAutocompletePrediction
    {
        public string description { get; set; }
        public PlaceAutocompleteMatchedSubstring[] matched_substrings { get; set; }
        public PlaceAutocompleteStructuredFormat structured_formatting { get; set; }
        public PlaceAutocompleteTerm[] terms { get; set; }
        public int? distance_meters { get; set; }
        public string? place_id { get; set; }
        public string? reference { get; set; }
        public string[]? types { get; set; }
        [JsonConstructor]
        public PlaceAutocompletePrediction(string description, PlaceAutocompleteMatchedSubstring[] matched_substrings, PlaceAutocompleteStructuredFormat structured_formatting, PlaceAutocompleteTerm[] terms, int? distance_meters = null, string? place_id = null, string? reference = null, string[]? types = null)
        {
            this.description  = description;
            this.matched_substrings = matched_substrings;
            this.structured_formatting = structured_formatting;
            this.terms = terms;
            this.distance_meters = distance_meters;
            this.place_id = place_id;
            this.reference = reference;
            this.types = types;

        }
    }
    public class PlaceAutocompleteStructuredFormat
    {
        public string main_text { get; set; }
        public PlaceAutocompleteMatchedSubstring[] main_text_matched_substrings { get; set; }
        public string? secondary_text { get; set; }
        public PlaceAutocompleteMatchedSubstring[]? secondary_text_matched_substrings { get; set; }
        [JsonConstructor]
        public PlaceAutocompleteStructuredFormat(string main_text, PlaceAutocompleteMatchedSubstring[] main_text_matched_substrings, string? secondary_text = null, PlaceAutocompleteMatchedSubstring[]? secondary_text_matched_substrings = null)
        {
            this.main_text = main_text;
            this.main_text_matched_substrings = main_text_matched_substrings;
            this.secondary_text = secondary_text;
            this.secondary_text_matched_substrings = secondary_text_matched_substrings;
        }
    }
    public class PlaceAutocompleteTerm
    {
        public int offset { get; set; }
        public string value { get; set; }
        [JsonConstructor]
        public PlaceAutocompleteTerm(int offset, string value)
        {
            this.offset = offset;
            this.value = value;
        }
    }
    public class PlaceAutocompleteMatchedSubstring
    {
        public int length { get; set; }
        public int offset { get; set; }
        [JsonConstructor]
        public PlaceAutocompleteMatchedSubstring(int length, int offset)
        {
            this.length = length;
            this.offset = offset;
        }
    }
    public class PlacesAutocompleteStatus
    {
        public string status { get; set; }
        [JsonConstructor]
        public PlacesAutocompleteStatus(string status)
        {
            this.status = status;
        }
    }



}
