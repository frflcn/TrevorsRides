using Android.App;
using Android.Runtime;
using TrevorsRidesHelpers;


namespace TrevorsRidesMaui
{
    #if DEBUG                                   
        [Application(UsesCleartextTraffic = true)]  
    #else                                      
        [Application]                               
    #endif
    [MetaData("com.google.android.maps.v2.API_KEY",
            Value = APIKeys.GoogleMapsAPIKey)]
    public class MainApplication : MauiApplication
    {
        public MainApplication(IntPtr handle, JniHandleOwnership ownership)
            : base(handle, ownership)
        {
        }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    }
}