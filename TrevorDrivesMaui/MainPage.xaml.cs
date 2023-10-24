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
            App.trevorStatus.isOnline = AmIDrivingSwitch.IsToggled;
        }
        public void AmIDrivingForUberToggled(object sender, EventArgs eventArgs)
        {

        }
        protected async override void OnAppearing()
        {
           if(await CheckPermissions())
            {
                //Map.MyLocationEnabled = true;
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