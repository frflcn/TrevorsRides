

using Microsoft.Maui.Maps;
using TrevorsRidesHelpers.Ride;

namespace TrevorDrivesMaui
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
        }
        public void AmIDrivingToggled(object sender, EventArgs eventArgs)
        {
            if (AmIDrivingSwitch.IsToggled)
            {
                DrivingStatusLabel.Text = "You're online";
            }
            else
            {
                DrivingStatusLabel.Text = "You're offline";
            }
            App.TrevorStatus.isOnline = AmIDrivingSwitch.IsToggled;
        }
        public void AmIDrivingForUberToggled(object sender, EventArgs eventArgs)
        {
            App.TrevorStatus.isDrivingForUber = AmIDrivingForUberSwitch.IsToggled;
        }
        protected async override void OnAppearing()
        {
           if(await CheckPermissions())
            {
                try
                {
                    Location? location = await Geolocation.Default.GetLastKnownLocationAsync();
                    if (location == null || (DateTimeOffset.Now.UtcTicks - location.Timestamp.UtcTicks > 5000000))
                    {
                        GeolocationRequest request = new GeolocationRequest(GeolocationAccuracy.Best);
                        location = await Geolocation.Default.GetLocationAsync(request);
                    }

                    Map.MoveToRegion(new MapSpan(location, 0.2, 0.2));
                }
                catch (Exception)
                {

                }
                
            }
        }
        public async Task<bool> CheckPermissions()
        {
            PermissionStatus locationStatus = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (locationStatus != PermissionStatus.Granted)
            {
                locationStatus = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                if (locationStatus != PermissionStatus.Granted)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return true;
            }
        }



    }
}