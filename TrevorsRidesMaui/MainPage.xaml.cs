using System.Collections.ObjectModel;
using TrevorsRidesMaui.Controls;

using TrevorsRidesHelpers.GoogleApiClasses;
using System.Text.Json;
using System.Net.Http.Headers;
using System.Collections;
using MauiLocation = Microsoft.Maui.Devices.Sensors;
//using Maui.GoogleMaps;
using Microsoft.Maui.Controls.Maps;
using System.Diagnostics;
using TrevorsRidesHelpers;
using System.Diagnostics.CodeAnalysis;

namespace TrevorsRidesMaui
{
    
    public partial class MainPage : ContentPage
    {
        Place? ToPlace;
        Place? FromPlace;


        Microsoft.Maui.Controls.Maps.Pin PickUpLocationPin = new Microsoft.Maui.Controls.Maps.Pin()
        {
            Label = "Pick up",
            Type = Microsoft.Maui.Controls.Maps.PinType.Place
        };
        Microsoft.Maui.Controls.Maps.Pin DropOffLocationPin = new Microsoft.Maui.Controls.Maps.Pin()
        {
            Label = "DropOff up",
            Type = Microsoft.Maui.Controls.Maps.PinType.Place
        };

        ObservableCollection<ListViewEntry> ToAddressSuggestions;
        ObservableCollection<ListViewEntry> FromAddressSuggestions;
       
        string GOOGLE_PLACES_API_KEY = APIKeys.GoogleEverythingKey;
                                       
        HttpClient client;
        Microsoft.Maui.Devices.Sensors.Location? location = new Microsoft.Maui.Devices.Sensors.Location(40.79442954080881, -77.86165896747);
        PlacesSessionToken pst;


        [DynamicDependency("OnControlTapped")]
        public MainPage()
        {
            InitializeComponent();
            client = App.HttpClient;
            ToAddressSuggestions = new ObservableCollection<ListViewEntry>(){
                new ListViewEntry(){
                    MainText="Your location" },
                new ListViewEntry(){
                    MainText="Choose on map" } };
            FromAddressSuggestions = new ObservableCollection<ListViewEntry>(){
                new ListViewEntry(){
                    MainText="Your location" },
                new ListViewEntry(){
                    MainText="Choose on map" } };
            this.ToAddress.Suggestions.ItemsSource = ToAddressSuggestions;
            this.FromAddress.Suggestions.ItemsSource = FromAddressSuggestions;


        }
        
        public async void OnControlTapped(object sender, EventArgs e)
        {
            //this.FromAddress.TextEditor.Unfocus();
            //this.ToAddress.TextEditor.Unfocus();
            //Log.Debug("CONTROL: ", "TAPPED");
            
        }

        

        private void Map_MapClicked(object sender, Maui.GoogleMaps.MapClickedEventArgs e)
        {
            this.FromAddress.TextEditor.Unfocus();
            this.ToAddress.TextEditor.Unfocus();
            Log.Debug("Map Tapped", "");
            
        }

        private async void AddressTextChanged(object sender, TextChangedEventArgs e )
        {
            ComboBox comboBox = sender as ComboBox;
            ObservableCollection<ListViewEntry> suggestions = comboBox.Suggestions.ItemsSource as ObservableCollection<ListViewEntry>;
            if (e.NewTextValue != "")
            {
                
                if (await App.CheckPermissions())
                {
                    location = await Geolocation.GetLastKnownLocationAsync();
                    if (location == null || DateTime.UtcNow.Ticks - location.Timestamp.UtcTicks > (10000000L * 60 * 5))
                    {
                        location = await Geolocation.GetLocationAsync();
                        location = location ?? new Microsoft.Maui.Devices.Sensors.Location(40.79442954080881, -77.86165896747);
                    }
                }
                if (pst == null || pst.IsDead())
                {
                    pst = new PlacesSessionToken();
                }
                Uri uri = new Uri($"https://maps.googleapis.com/maps/api/place/autocomplete/json?" +
                    $"input={comboBox.TextEditor.Text}&" +
                    $"location={location.Latitude}%2C{location.Longitude}&" +
                    $"radius=20000&" +
                    $"sessiontoken={pst.GUID}&" +
                    $"key={GOOGLE_PLACES_API_KEY}");
                HttpResponseMessage message = await client.GetAsync(uri);
                Log.Debug("REQUEST: ", uri.OriginalString);
                Log.Debug($"{DateTime.Now}", "New PlacesAutoCompleteRequest");
                
                var options = new JsonSerializerOptions()
                {
                    PropertyNameCaseInsensitive = true
                };
                string jsonMessage = await message.Content.ReadAsStringAsync();
                Log.Debug("DA RESPONSE", jsonMessage);
                PlacesAutocompleteResponse response;
                try
                {
                    response = JsonSerializer.Deserialize<PlacesAutocompleteResponse>(jsonMessage, options);

                    //If text is empty, the response is late and we don't want it to overwrite the options "Your location" and "Choose on map" 
                    if (comboBox.TextEditor.Text == "") return;

                    suggestions.Clear();
                    foreach (var prediction in response.predictions)
                    {

                        suggestions.Add(new ListViewEntry() { 
                            MainText = prediction.structured_formatting.main_text, 
                            SubText = prediction.structured_formatting.secondary_text, 
                            Prediction = prediction});
                    }
                    
                }
                catch (NotSupportedException ex)
                {
                    Log.Debug("NOT SUPPORTED EXCEPTION: ", ex.Message);
                }
                catch (JsonException ex)
                {
                    Log.Debug("JSON EXCEPTION: ", ex.Message);
                    Log.Debug("JSON EXCEPTION: ", ex.Path);
                    Log.Debug("JSON EXCEPTION: ", ex.Data.Keys.ToString());
                    Log.Debug("JSON EXCEPTION: ", ex.Data.Values.ToString());
                    foreach (KeyValuePair keyvalue in ex.Data)
                    {
                        Log.Debug("KEYVALUE: ", $"{keyvalue}");
                    }
                }
                catch (ArgumentNullException ex)
                {
                    Log.Debug("ARGUEMENT NULL EXCEPTION: ", ex.Message);
                }
                catch (Exception ex)
                {
                    Log.Debug("General Error: ", ex.Message);
                    Log.Debug("STack TRACE: ", ex.StackTrace);

                }
            }
            else
            {
                suggestions.Clear();
                suggestions.Add(new ListViewEntry() { MainText = "Your location" });
                suggestions.Add(new ListViewEntry() { MainText = "Choose on map" });
            }

        }
        public class PlacesSessionToken
        {
            public Guid GUID { get; set; }
            public DateTime Birth { get; set; }
            public PlacesSessionToken() 
            {
                
                this.GUID = Guid.NewGuid();
                this.Birth = DateTime.Now;
            }
            public bool IsDead()
            {
                if (this.Birth.Ticks < DateTime.Now.Ticks - (10000000L * 60 * 2))
                {
                    return true;
                }
                else
                    return false;
            }
            public bool IsAlive()
            {
                if (this.Birth.Ticks < DateTime.Now.Ticks - (10000000L * 60 * 2))
                {
                    return false;
                }
                else
                    return true;
            }
        }


        private async void ToAddress_OnSuggestionsTapped(object sender, ItemTappedEventArgs e)
        {
            ToAddress.Suggestions.IsVisible = false;
            ToPlace = await GetPlaceLocationOnSuggestionTapped(e);
            ToAddress.TextEditor.Text = ToPlace?.name;
            ToAddress.TextEditor.Unfocus();
            ToAddress.TextEditor.IsEnabled = false;
            ToAddress.TextEditor.IsEnabled = true;
            


           
            DropOffLocationPin.Location = new MauiLocation.Location(ToPlace.geometry.location.lat, ToPlace.geometry.location.lng);
            if (Map.Pins.Contains(DropOffLocationPin))
            {
                Map.Pins.Remove(DropOffLocationPin);
            }
            Map.Pins.Add(DropOffLocationPin);
            if (FromPlace != null && ToPlace != null)
            {
                GetRoute();
            }

        }

        private async void FromAddress_OnSuggestionsTapped(object sender, ItemTappedEventArgs e)
        {
            FromAddress.Suggestions.IsVisible = false;
            FromPlace = await GetPlaceLocationOnSuggestionTapped(e);
            FromAddress.TextEditor.Text = FromPlace?.name;
            FromAddress.TextEditor.Unfocus();
            FromAddress.TextEditor.IsEnabled = false;
            FromAddress.TextEditor.IsEnabled = true;
            


            PickUpLocationPin.Location = new MauiLocation.Location(FromPlace.geometry.location.lat, FromPlace.geometry.location.lng);
            if (Map.Pins.Contains(PickUpLocationPin))
            {
                Map.Pins.Remove(PickUpLocationPin);
            }
            Map.Pins.Add(PickUpLocationPin);
            if (FromPlace != null && ToPlace != null)
            {
                GetRoute();
            }
        }
        private async Task<Place?> GetPlaceLocationOnSuggestionTapped(ItemTappedEventArgs e)
        {
            ListViewEntry listViewEntry = e.Item as ListViewEntry;
            if (listViewEntry.MainText == "Choose on map")
            {
                //Have user choose on map
            }
            else if (listViewEntry.MainText == "Your location")
            {
                //Select users current location
            }
            else
            {
                if (pst == null || pst.IsDead())
                {
                    pst = new PlacesSessionToken();
                }
                Uri uri = new Uri($"https://maps.googleapis.com/maps/api/place/details/json?" +
                    $"fields=address_components%2Cadr_address%2Cformatted_address%2Cgeometry%2Cname%2Cplus_code&" +
                    $"place_id={listViewEntry.Prediction.place_id}&" +
                    $"sessiontoken={pst.GUID}&" +
                    $"key={GOOGLE_PLACES_API_KEY}");
                pst = new PlacesSessionToken();
                HttpResponseMessage message = await client.GetAsync(uri);
                string json = await message.Content.ReadAsStringAsync();
                Log.Debug("Place Details: ", json);
                PlacesDetailsResponse response;
                try
                {
                    response  = JsonSerializer.Deserialize<PlacesDetailsResponse>(json);
                    if (response.status == "OK")
                    {
                        return response.result;
                    }
                    else
                    {
                        return null;
                    }
                    
                }
                catch (NotSupportedException ex)
                {
                    Log.Debug("NOT SUPPORTED EXCEPTION: ", ex.Message);
                }
                catch (JsonException ex)
                {
                    Log.Debug("JSON EXCEPTION: ", ex.Message);
                    Log.Debug("JSON EXCEPTION: ", ex.Path);
                    Log.Debug("JSON EXCEPTION: ", ex.Data.Keys.ToString());
                    Log.Debug("JSON EXCEPTION: ", ex.Data.Values.ToString());
                    foreach (KeyValuePair keyvalue in ex.Data)
                    {
                        Log.Debug("KEYVALUE: ", $"{keyvalue}");
                    }
                }
                catch (ArgumentNullException ex)
                {
                    Log.Debug("ARGUEMENT NULL EXCEPTION: ", ex.Message);
                }
                catch (Exception ex)
                {
                    Log.Debug("General Error: ", ex.Message);
                    Log.Debug("Stack TRACE: ", ex.StackTrace);
                }


            }
            return null;
        } 
        private async void GetRoute()
        {
            if (FromPlace == null || ToPlace == null)
            {
                throw new NullReferenceException("ToPlace or FromPlace null");
            }
            Uri uri = new Uri("https://routes.googleapis.com/directions/v2:computeRoutes");
            RoutesRequest request;
            if ((Application.Current as App) == null)
                Log.Debug("APPLICATION MAUI", "NULLLLL");
            if ((Application.Current as App)!.trevor.isOnline)
            {
                request = new RoutesRequest()
                {
                    origin = (Application.Current as App)!.trevor.endPoint!.position.ToWaypoint(),
                    intermediates = new Waypoint[] { FromPlace.ToWaypoint() },
                    destination = ToPlace.ToWaypoint(),
                    polylineQuality = PolylineQuality.OVERVIEW
                };
            }
            else
            {
                request = new RoutesRequest()
                {
                    origin = FromPlace.ToWaypoint(),
                    destination = ToPlace.ToWaypoint(),
                    polylineQuality = PolylineQuality.OVERVIEW
                };
            }
            
            string message = JsonSerializer.Serialize(request);
            Log.Debug("Request", message);
            StringContent content = new StringContent(message);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            content.Headers.Add("X-Goog-Api-Key", GOOGLE_PLACES_API_KEY);
            if (request.intermediates == null)
                content.Headers.Add("X-Goog-FieldMask", "routes.duration,routes.distanceMeters,routes.polyline.encodedPolyline,routes.viewport");
            else
                content.Headers.Add("X-Goog-FieldMask", "routes.duration,routes.legs.duration,routes.legs.polyline.encodedPolyline");
            HttpResponseMessage response = await client.PostAsync(uri, content);
            string jsonResponse = await response.Content.ReadAsStringAsync();
            Log.Debug("Response", jsonResponse);
            RoutesResponse route = JsonSerializer.Deserialize<RoutesResponse>(jsonResponse);

            Stopwatch stopwatch = Stopwatch.StartNew();
            List<Maui.GoogleMaps.Position> polyline;
            Viewport viewPort;
            if (request.intermediates == null)
            {
                viewPort = route.routes[0].viewport;
                polyline = DecodeWithZoom(route.routes[0].polyline.encodedPolyline, route.routes[0].viewport);
            }
                
            else
            {
                polyline = Decode(route.routes[0].legs[1].polyline.encodedPolyline);
                viewPort = GetViewPort(polyline);
                polyline = DecodeWithZoom(route.routes[0].legs[1].polyline.encodedPolyline, viewPort);
            }
            Log.Debug("Stopwatch AfterDecode", stopwatch.Elapsed.ToString());
            RoutePolyline.Geopath.Clear();
            //RoutePolyline.Positions.Clear();
            foreach (var point in polyline)
            {
                //RoutePolyline.Positions.Add(point);
                RoutePolyline.Geopath.Add(new MauiLocation.Location(point.Latitude, point.Longitude));
            }
            Log.Debug("Stopwatch AfterGraph", stopwatch.Elapsed.ToString());
            
            double radiusLng = (viewPort.high.longitude - viewPort.low.longitude) / 2;
            double radiusLat = (viewPort.high.latitude - viewPort.low.latitude) / 2;
            double centerLng = radiusLng + viewPort.low.longitude;
            double centerLat = radiusLat + viewPort.low. latitude;
            double radius;
            
            if (radiusLng > radiusLat)
            {
                radius = radiusLng;
            }
            else
            {
                radius = radiusLat;
            }
            Map.MoveToRegion(new Microsoft.Maui.Maps.MapSpan(new MauiLocation.Location(centerLat, centerLng), radiusLat * 2.6, radiusLng * 2.6));
            if (request.intermediates == null)
            {
                RideDetailsControl.PickupLabel.Text = "Trevor is currently offline";
                RideDetailsControl.DropOffLabel.Text = $"Ride will take approximately {route.routes[0].duration}";
                RideDetailsControl.Cost = (decimal.Parse(route.routes[0].duration.Substring(0, route.routes[0].duration.Length - 1))) / 60;
            }
            else
            {
                RideDetailsControl.WaitTime = RideDetails.ToTimeSpan(route.routes[0].legs[0].duration);
                RideDetailsControl.RideDuration = RideDetails.ToTimeSpan(route.routes[0].legs[1].duration);
                RideDetailsControl.Cost = (decimal.Parse(route.routes[0].legs[1].duration.Substring(0, route.routes[0].legs[1].duration.Length - 1))) / 60;
            }
            
            

        }

        //Not currently using
        private void PolylineDecode(string polyline)
        {
            List<short> polylineInt = new List<short>();
            List<List<byte[]>> polylinePointsBytes = new List<List<byte[]>>();
            List<byte[]> polylinePointBytesTemp = new List<byte[]>();
            
            foreach(char c in polyline)
            {
                polylineInt.Add((short)c);
            }
            for(int i = 0; i < polylineInt.Count; i++)
            {
                
                polylineInt[i] -= 63;
             
                polylinePointBytesTemp.Add(new byte[] { (byte)polylineInt[i] });
                if ((polylinePointBytesTemp[polylinePointBytesTemp.Count - 1][0] & 0x20) != 0x20)
                {
                    polylinePointsBytes.Add(new List<byte[]>());
                    for (int j = polylinePointBytesTemp.Count - 1; j >= 0; j--)
                    {
                        polylinePointsBytes[polylinePointsBytes.Count - 1].Add(polylinePointBytesTemp[j]);
                    }
                    
                    polylinePointBytesTemp.Clear();
                }
            }
            BitArray[] polylinePoints5BitArrays = new BitArray[polylinePointsBytes.Count];
            for(int i = 0; i < polylinePointsBytes.Count; i++)
            {

            }
        }

        //Probably cuts off the last point in the polyline but also probably isn't noticable with lots of points
        public static List<Maui.GoogleMaps.Position> Decode(string encodedPoints)
        {
            List<Maui.GoogleMaps.Position> decodedPoints = new List<Maui.GoogleMaps.Position>();
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

                decodedPoints.Add(new Maui.GoogleMaps.Position(Convert.ToDouble(currentLat) / 1E5, Convert.ToDouble(currentLng) / 1E5));

            }
            return decodedPoints;
        }
        public static List<Maui.GoogleMaps.Position> DecodeWithZoom(string encodedPoints, Viewport viewport)
        {
            double latitudeRange = viewport.high.latitude - viewport.low.latitude;
            double longitudeRange = viewport.high.longitude - viewport.low.longitude;
            Maui.GoogleMaps.Position? lastPosition = null;
            Maui.GoogleMaps.Position thisPosition;

            List<Maui.GoogleMaps.Position> decodedPoints = new List<Maui.GoogleMaps.Position>();
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
                
                thisPosition = new Maui.GoogleMaps.Position(Convert.ToDouble(currentLat) / 1E5, Convert.ToDouble(currentLng) / 1E5);
                if (lastPosition == null || Math.Abs(thisPosition.Latitude - lastPosition.Value.Latitude) > latitudeRange / 100 || Math.Abs(thisPosition.Longitude - lastPosition.Value.Longitude) > longitudeRange /100)
                {
                    decodedPoints.Add(thisPosition);
                    lastPosition = thisPosition;
                }
                    

            }
            return decodedPoints;
        }
        public Viewport GetViewPort(List<Maui.GoogleMaps.Position> polyline)
        {
            Viewport viewport = new Viewport()
            {
                low = new LatLng()
                {
                    latitude = polyline[0].Latitude,
                    longitude = polyline[0].Longitude
                },
                high = new LatLng()
                {
                    latitude = polyline[0].Latitude,
                    longitude = polyline[0].Longitude
                }
            };
            foreach (var position in polyline)
            {
                if (viewport.low.latitude > position.Latitude)
                {
                    viewport.low.latitude = position.Latitude;
                }
                if (viewport.low.longitude > position.Longitude)
                {
                    viewport.low.longitude = position.Longitude;
                }
                if (viewport.high.latitude < position.Latitude)
                {
                    viewport.high.latitude = position.Latitude;
                }
                if (viewport.high.longitude < position.Longitude)
                {
                    viewport.high.longitude = position.Longitude;
                }
            }
            return viewport;
        }

        private void RideDetailsControl_BookRidePressed(object sender, EventArgs e)
        {
            Navigation.PushAsync(new BookRidePage());
                
        }
    }
}


