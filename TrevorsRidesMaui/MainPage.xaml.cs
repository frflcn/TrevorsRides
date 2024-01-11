using System.Collections.ObjectModel;
using TrevorsRidesMaui.Controls;

using TrevorsRidesHelpers.GoogleApiClasses;
using System.Text.Json;
using System.Net.Http.Headers;
using System.Collections;
using Sensors = Microsoft.Maui.Devices.Sensors;
//using Maui.GoogleMaps;
using Microsoft.Maui.Controls.Maps;
using System.Diagnostics;
using TrevorsRidesHelpers;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Json;
using TrevorsRidesHelpers.Ride;

namespace TrevorsRidesMaui
{
    
    public partial class MainPage : ContentPage
    {
        PlaceCore? Pickup;
        PlaceCore[] Stops = new PlaceCore[0];
        PlaceCore? Dropoff;


        Pin PickUpLocationPin = new Pin()
        {
            Label = "Pick up",
            Type = PinType.Place
        };
        Pin DropOffLocationPin = new Pin()
        {
            Label = "DropOff up",
            Type = PinType.Place
        };

        ObservableCollection<ListViewEntry> ToAddressSuggestions;
        ObservableCollection<ListViewEntry> FromAddressSuggestions;
       
        string GOOGLE_PLACES_API_KEY = APIKeys.GoogleEverythingKey;
                                       
        HttpClient client;
        Sensors.Location? location = new Sensors.Location(40.79442954080881, -77.86165896747);
        PlacesSessionToken pst;


        //[DynamicDependency("OnControlTapped")]
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
            Log.Debug("OBSERVABLE COLLECTIONS CREATED");
            this.ToAddress.Suggestions.ItemsSource = ToAddressSuggestions;
            this.FromAddress.Suggestions.ItemsSource = FromAddressSuggestions;


        }
        
        public async void OnControlTapped(object sender, EventArgs e)
        {
            this.FromAddress.TextEditor.Unfocus();
            this.ToAddress.TextEditor.Unfocus();
            //Log.Debug("CONTROL: ", "TAPPED");
            
        }

        

        private void Map_MapClicked(object sender, MapClickedEventArgs e)
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
                        location = location ?? new Sensors.Location(40.79442954080881, -77.86165896747);
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
            Pickup = await GetPlaceLocationOnSuggestionTapped(e);
            ToAddress.TextEditor.Text = Pickup?.Name;
            ToAddress.TextEditor.Unfocus();
            ToAddress.TextEditor.IsEnabled = false;
            ToAddress.TextEditor.IsEnabled = true;
            


           
            DropOffLocationPin.Location = new Sensors.Location(Pickup.LatLng.lat, Pickup.LatLng.lng);
            if (Map.Pins.Contains(DropOffLocationPin))
            {
                Map.Pins.Remove(DropOffLocationPin);
            }
            Map.Pins.Add(DropOffLocationPin);
            if (Dropoff != null && Pickup != null)
            {
                GetRoute();
            }

        }

        private async void FromAddress_OnSuggestionsTapped(object sender, ItemTappedEventArgs e)
        {
            FromAddress.Suggestions.IsVisible = false;
            Dropoff = await GetPlaceLocationOnSuggestionTapped(e);
            FromAddress.TextEditor.Text = Dropoff?.Name;
            FromAddress.TextEditor.Unfocus();
            FromAddress.TextEditor.IsEnabled = false;
            FromAddress.TextEditor.IsEnabled = true;
            


            PickUpLocationPin.Location = new Sensors.Location(Dropoff.LatLng.lat, Dropoff.LatLng.lng);
            if (Map.Pins.Contains(PickUpLocationPin))
            {
                Map.Pins.Remove(PickUpLocationPin);
            }
            Map.Pins.Add(PickUpLocationPin);
            if (Dropoff != null && Pickup != null)
            {
                GetRoute();
            }
        }
        private async Task<PlaceCore?> GetPlaceLocationOnSuggestionTapped(ItemTappedEventArgs e)
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
                        return new PlaceCore(response.result);
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
            if (Dropoff == null || Pickup == null)
            {
                throw new NullReferenceException("Pickup or Dropoff null");
            }
            Uri uri = new Uri("https://routes.googleapis.com/directions/v2:computeRoutes");
            RoutesRequest request;

            if (Application.Current == null)
            {
                Log.Debug("Application Null");
            }
            
            if (App.TrevorsStatus == null)
            {
                Log.Debug("Trevor Null");
            }

            if (App.TrevorsStatus != null && App.TrevorsStatus.isOnline)
            {
                request = new RoutesRequest()
                {
                    origin = App.TrevorsStatus.endPoint!.position.ToWaypoint(),
                    intermediates = new Waypoint[] { Dropoff.ToWaypoint() },
                    destination = Pickup.ToWaypoint(),
                    polylineQuality = PolylineQuality.OVERVIEW
                };
            }
            else
            {
                request = new RoutesRequest()
                {
                    origin = Dropoff.ToWaypoint(),
                    destination = Pickup.ToWaypoint(),
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
            //List<Maui.GoogleMaps.Position> polyline;
            List<Sensors.Location> polyline;
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
                RoutePolyline.Geopath.Add(new Sensors.Location(point.Latitude, point.Longitude));
            }
            Log.Debug("Stopwatch AfterGraph", stopwatch.Elapsed.ToString());
            
            double radiusLng = (viewPort!.high.longitude - viewPort.low.longitude) / 2;
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
            Map.MoveToRegion(new Microsoft.Maui.Maps.MapSpan(new Sensors.Location(centerLat, centerLng), radiusLat * 2.6, radiusLng * 2.6));
            if (request.intermediates == null)
            {
                RideDetailsControl.PickupLabel.Text = "Trevor is currently offline";
                RideDetailsControl.DropOffLabel.Text = $"Ride will take approximately {route.routes[0].duration}";
                RideDetailsControl.Cost = (decimal.Parse(route.routes[0].duration!.Substring(0, route.routes[0].duration!.Length - 1))) / 60;
            }
            else
            {
                RideDetailsControl.WaitTime = RideDetails.ToTimeSpan(route.routes[0].legs![0].duration);
                RideDetailsControl.RideDuration = RideDetails.ToTimeSpan(route.routes[0].legs![1].duration);
                RideDetailsControl.Cost = (decimal.Parse(route.routes[0].legs![1].duration.Substring(0, route.routes[0].legs![1].duration.Length - 1))) / 60;
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
        public static List<Sensors.Location> Decode(string encodedPoints)
        {
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

                decodedPoints.Add(new Sensors.Location(Convert.ToDouble(currentLat) / 1E5, Convert.ToDouble(currentLng) / 1E5));

            }
            return decodedPoints;
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
                if (lastPosition == null || Math.Abs(thisPosition.Latitude - lastPosition.Latitude) > latitudeRange / 100 || Math.Abs(thisPosition.Longitude - lastPosition.Longitude) > longitudeRange /100)
                {
                    decodedPoints.Add(thisPosition);
                    lastPosition = thisPosition;
                }
                    

            }
            return decodedPoints;
        }
        public Viewport GetViewPort(List<Sensors.Location> polyline)
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

        private async void RideDetailsControl_BookRidePressed(object sender, EventArgs e)
        {
            if (Dropoff == null || Pickup == null)
            {
                return;
            }

            RoutesRequest requestedRoute = new RoutesRequest()
            {
                origin = Dropoff.ToWaypoint(),
                destination = Pickup.ToWaypoint()
            };
            client = App.HttpClient;
            //JsonContent content = JsonContent.Create<RoutesRequest>(requestedRoute);
            TripRequest tripRequest = new TripRequest(Pickup, Dropoff);
            JsonContent content = JsonContent.Create<TripRequest>(tripRequest);
            content.Headers.Add("User-ID", App.AccountSession!.Account.Id.ToString());
            content.Headers.Add("Session-Token", App.AccountSession.SessionToken.Token);
            Log.Debug("RoutesRequest", JsonSerializer.Serialize<RoutesRequest>(requestedRoute));
            HttpResponseMessage response = await client.PutAsync($"{Helpers.Domain}/api/CreateCheckoutSession", content);

            Log.Debug("CreateCheckoutSessionURL", await response.Content.ReadAsStringAsync());
            await Navigation.PushAsync(new BookRidePage(await response.Content.ReadAsStringAsync()));
                
        }
    }
}


