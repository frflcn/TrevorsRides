using System.Text.Json;
using TrevorDrivesMaui.BackgroundTasks;
using TrevorsRidesHelpers;
using Help = TrevorsRidesHelpers;
using TrevorsRidesHelpers.GoogleApiClasses;
using TrevorsRidesHelpers.Ride;
using Microsoft.Maui.Controls.Maps;
using Sensors = Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Maps;
using Maps = Microsoft.Maui.Controls.Maps;

namespace TrevorDrivesMaui;

public partial class RideRequestPage : ContentPage
{
    TripRequest TripRequest { get; set; }
    Guid MessageId { get; set; }
	public RideRequestPage(WebsocketMessage websocketMessage)
	{
        TripRequest tripRequest = JsonSerializer.Deserialize<TripRequest>((JsonElement)websocketMessage.Message);
        TripRequest = tripRequest;
        MessageId = websocketMessage.MessageID;
		InitializeComponent();

        ShowRouteAsync(TripRequest);
	}

    private async void Decline_Clicked(object sender, EventArgs e)
    {
        WebsocketMessage websocketMessage = new WebsocketMessage(MessageType.RideRequestDeclined, TripRequest.TripId, MessageId);
        string message = JsonSerializer.Serialize(websocketMessage);
        await RideRequestService.Instance!.Send(message);
        App.Current.MainPage = new MyFlyoutPage();
    }

    private async void Accept_Clicked(object sender, EventArgs e)
    {
        WebsocketMessage websocketMessage = new WebsocketMessage(MessageType.RideRequestAccepted, TripRequest, MessageId);
        string message = JsonSerializer.Serialize(websocketMessage);
        await RideRequestService.Instance!.Send(message);
        LatLngLiteral latLng = TripRequest.Pickup.LatLng;
        var location = new Sensors.Location(latLng.lat, latLng.lng);
        var options = new MapLaunchOptions
        {
            Name = "Rider Pickup",
            NavigationMode = NavigationMode.Driving
        };

        try
        {
            await Microsoft.Maui.ApplicationModel.Map.Default.OpenAsync(location, options);
        }
        catch (Exception ex)
        {
            _ = DisplayAlert("Unable to open Maps App", ex.Message, "Cancel");
        }
        App.Current.MainPage = new MyFlyoutPage();
    }

    private async Task ShowRouteAsync(TripRequest trip)
    {
        //GetRoute
        RoutesResponse route = await Help.Map.GetRouteAsync(trip);

        //Add Pins
        Map.Pins.Add(new Pin()
        {
            Label = "Pickup",
            Type = PinType.Place,
            Location = new Sensors.Location(trip.Pickup.LatLng.lat, trip.Pickup.LatLng.lng)
        });
        Map.Pins.Add(new Pin()
        {
            Label = "Dropoff",
            Type = PinType.Place,
            Location = new Sensors.Location(trip.Dropoff.LatLng.lat, trip.Dropoff.LatLng.lng)
        });
        for (int i = 0; i < trip.Stops.Length; i++)
        {
            Map.Pins.Add(new Pin()
            {
                Label = $"Stop {i + 1}",
                Type = PinType.Place,
                Location = new Sensors.Location(trip.Stops[i].LatLng.lat, trip.Stops[i].LatLng.lng)
            });
        }

        //Calculate polyline
        Maps.Polyline polyline = Help.Map.DecodeWithZoomReturnPolyline(route.routes[0].polyline!.encodedPolyline!, route.routes[0].viewport!);
        Map.MapElements.Add(polyline);

        //Move the map
        Viewport viewport = route.routes[0].viewport!;
        double latRadius = (viewport.high.latitude - viewport.low.latitude);
        double lngRadius = (viewport.high.longitude - viewport.low.longitude);
        Sensors.Location center = new Sensors.Location(viewport.low.latitude + latRadius / 2, viewport.low.longitude + lngRadius / 2);
        Map.MoveToRegion(new MapSpan(center, latRadius * 1.3, lngRadius * 1.3));
    }
}