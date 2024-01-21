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
using Help = TrevorsRidesHelpers;
using Maps = Microsoft.Maui.Controls.Maps;
using Onion = Maui.GoogleMaps;
using Maui.GoogleMaps;

namespace TrevorsRidesMaui
{
    
    public class MainPageViewModel
    {
        public ObservableCollection<ListViewEntry> ToAddressSuggestions { get; set; }
        public ObservableCollection<ListViewEntry> FromAddressSuggestions { get; set; }
    }
    public partial class MainPage : ContentPage
    {
        PlaceCore? Pickup;
        PlaceCore[] Stops = new PlaceCore[0];
        PlaceCore? Dropoff;

        public Onion.Pin TrevorPIN = new Onion.Pin()
        {
            Label = "Trevor",
            Type = Onion.PinType.Place,
            Icon = BitmapDescriptorFactory.FromBundle("car_50p")
        };
        

        Onion.Pin PickUpLocationPin = new Onion.Pin()
        {
            Label = "Pick Up",
            Type = Onion.PinType.Place
        };
        Onion.Pin DropOffLocationPin = new Onion.Pin()
        {
            Label = "Drop Off",
            Type = Onion.PinType.Place
        };
        MainPageViewModel viewModel = new MainPageViewModel();
        public ObservableCollection<ListViewEntry> ToAddressSuggestions { get; set; }
        public ObservableCollection<ListViewEntry> FromAddressSuggestions { get; set; }
       
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


            this.ToAddress.Suggestions.ItemsSource = ToAddressSuggestions;
            this.FromAddress.Suggestions.ItemsSource = FromAddressSuggestions;




        }
        
        public async void OnControlTapped(object sender, EventArgs e)
        {
            this.FromAddress.TextEditor.Unfocus();
            this.ToAddress.TextEditor.Unfocus();
            Log.Debug("CONTROL: ", "TAPPED");
            
        }

        

        private void Map_MapClicked(object sender, Onion.MapClickedEventArgs e)
        {
            this.FromAddress.TextEditor.Unfocus();
            this.ToAddress.TextEditor.Unfocus();
            Log.Debug("Maps Tapped", "");
            
        }

        private async void AddressTextChanged(object sender, TextChangedEventArgs e )
        {

            ComboBox comboBox = sender as ComboBox;

            
            ObservableCollection<ListViewEntry> suggestions = comboBox.Suggestions.ItemsSource as ObservableCollection<ListViewEntry>;
            
            //ObservableCollection<ListViewEntry> suggestions = viewModel.
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

                
                var options = new JsonSerializerOptions()
                {
                    PropertyNameCaseInsensitive = true
                };
                string jsonMessage = await message.Content.ReadAsStringAsync();

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
            Dropoff = await GetPlaceLocationOnSuggestionTapped(e);
            ToAddress.TextEditor.Text = Dropoff?.Name;
            ToAddress.TextEditor.Unfocus();
            ToAddress.TextEditor.IsEnabled = false;
            ToAddress.TextEditor.IsEnabled = true;
            


           
            
            
            if (Dropoff != null && Pickup != null)
            {
                await GetRoute();
            }
            DropOffLocationPin.Position = new Onion.Position(Dropoff.LatLng.lat, Dropoff.LatLng.lng);
            if (Map.Pins.Contains(DropOffLocationPin))
            {
                Map.Pins.Remove(DropOffLocationPin);
            }
            Map.Pins.Add(DropOffLocationPin);


        }

        private async void FromAddress_OnSuggestionsTapped(object sender, ItemTappedEventArgs e)
        {
            FromAddress.Suggestions.IsVisible = false;
            Pickup = await GetPlaceLocationOnSuggestionTapped(e);
            FromAddress.TextEditor.Text = Pickup?.Name;
            FromAddress.TextEditor.Unfocus();
            FromAddress.TextEditor.IsEnabled = false;
            FromAddress.TextEditor.IsEnabled = true;
            


            
            
            if (Dropoff != null && Pickup != null)
            {
                await GetRoute();
            }
            PickUpLocationPin.Position = new Onion.Position(Pickup.LatLng.lat, Pickup.LatLng.lng);
            if (Map.Pins.Contains(PickUpLocationPin))
            {
                Map.Pins.Remove(PickUpLocationPin);  
            }
            Map.Pins.Add(PickUpLocationPin);

        }
        private async Task<PlaceCore?> GetPlaceLocationOnSuggestionTapped(ItemTappedEventArgs e)
        {
            ListViewEntry listViewEntry = e.Item as ListViewEntry;
            if (listViewEntry.MainText == "Choose on map")
            {
                //Have user choose on map
                _ = DisplayAlert("Feature Coming soon...", null, "OK");
            }
            else if (listViewEntry.MainText == "Your location")
            {
                //Select users current location
                _ = DisplayAlert("Feature Coming soon...", null, "OK");
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
        private async Task GetRoute()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            Uri uri = new Uri("https://routes.googleapis.com/directions/v2:computeRoutes");
            RoutesRequest request;


            if (App.TrevorsStatus != null && App.TrevorsStatus.isOnline)
            {
                request = new RoutesRequest()
                {
                    origin = App.TrevorsStatus.endPoint!.position.ToWaypoint(),
                    intermediates = new Waypoint[] { Pickup.ToWaypoint() },
                    destination = Dropoff.ToWaypoint(),
                    polylineQuality = PolylineQuality.OVERVIEW
                };
            }
            else
            {
                request = new RoutesRequest()
                {
                    origin = Pickup.ToWaypoint(),
                    destination = Dropoff.ToWaypoint(),
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
                content.Headers.Add("X-Goog-FieldMask", "routes.duration,routes.legs.duration,routes.legs.distanceMeters,routes.legs.polyline.encodedPolyline");
            HttpResponseMessage response = await client.PostAsync(uri, content);
            string jsonResponse = await response.Content.ReadAsStringAsync();
            Log.Debug("Response", jsonResponse);
            RoutesResponse route = JsonSerializer.Deserialize<RoutesResponse>(jsonResponse);

            Viewport viewPort;
            Onion.Polyline polylineAct;
            if (request.intermediates == null)
            {
                viewPort = route.routes[0].viewport;
                polylineAct = Help.Map.DecodeReturnOnionPolyline(route.routes[0].polyline.encodedPolyline);
            }

            else
            {
                polylineAct = Help.Map.DecodeReturnOnionPolyline(route.routes[0].legs[1].polyline.encodedPolyline);
                viewPort = GetViewPort(polylineAct);
            }

           

            Map.Polylines.Clear();
            
            
            Map.Polylines.Add(polylineAct);
            



            
            
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
            Map.MoveToRegion(new Onion.MapSpan(new Onion.Position(centerLat, centerLng), radiusLat * 2.6, radiusLng * 2.6));
            if (request.intermediates == null)
            {
                RideDetailsControl.PickupLabel.Text = "Trevor is currently offline";
                RideDetailsControl.DropOffLabel.Text = $"{int.Parse(route.routes[0].duration.Substring(0, route!.routes[0].duration!.Length - 1)) / 60} minute ride";
                decimal rideCost = decimal.Round(((int)(RideCost.CentsPerMinute * double.Parse(route!.routes[0].duration!.Substring(0, route!.routes[0].duration!.Length - 1)) / 60
                    + RideCost.CentsPerMile * route.routes[0].distanceMeters / 1609)!) / 100.0M, 2);
                RideDetailsControl.Cost = RideCost.CostInDollars(route.routes[0].distanceMeters!.Value, route.routes[0].duration!);
            }
            else
            {
                RideDetailsControl.WaitTime = RideDetails.ToTimeSpan(route.routes[0].legs![0].duration);
                RideDetailsControl.RideDuration = RideDetails.ToTimeSpan(route.routes[0].legs![1].duration);

                
                decimal rideCost = decimal.Round(((int)(RideCost.CentsPerMinute * double.Parse(route!.routes[0].legs![1].duration!.Substring(0, route!.routes[0].legs![1].duration!.Length - 1)) / 60
                    + RideCost.CentsPerMile * route.routes[0].legs![1].distanceMeters / 1609)!) / 100.0M, 2);
                RideDetailsControl.Cost = RideCost.CostInDollars(route.routes[0].legs![1].distanceMeters, route!.routes[0].legs![1].duration);
            }
            
            

        }



        public Viewport GetViewPort(Onion.Polyline polyline)
        {
            Viewport viewport = new Viewport()
            {
                low = new LatLng()
                {
                    latitude = polyline.Positions[0].Latitude,
                    longitude = polyline.Positions[0].Longitude
                },
                high = new LatLng()
                {
                    latitude = polyline.Positions[0].Latitude,
                    longitude = polyline.Positions[0].Longitude
                }
            };
            foreach (var position in polyline.Positions)
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
            if (App.IsTesting)
            {
                content.Headers.Add("EmailOfRequestedDriver", App.AccountSession.Account.Email);
            }
            //else
            //{
            //    content.Headers.Add("EmailOfRequestedDriver", "");
            //}
            Log.Debug("RoutesRequest", JsonSerializer.Serialize<RoutesRequest>(requestedRoute));
            Log.Debug("HEADERS", content.Headers.ToString());
            HttpResponseMessage response = await client.PutAsync($"{Helpers.Domain}/api/CreateCheckoutSession", content);

            Log.Debug("CreateCheckoutSessionURL", await response.Content.ReadAsStringAsync());

            await Navigation.PushAsync(new BookRidePage(await response.Content.ReadAsStringAsync()));
  
            

            
            Log.Debug("AFTER PUSH ASYNC", "YUPPP");
                
        }
    }
}


