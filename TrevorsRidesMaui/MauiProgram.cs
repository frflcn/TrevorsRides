using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.Extensions.Logging;
using TrevorsRidesHelpers;
[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace TrevorsRidesMaui
{

    public static class MauiProgram
    {
        
        public static MauiApp CreateMauiApp()
        {

            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                })
                .UseMauiMaps();

            AppCenter.Start($"android={APIKeys.AndroidAppCenterString};" +
                  $"ios={APIKeys.IOSAppCenterString};" +
                  typeof(Analytics), typeof(Crashes));

            #if DEBUG
                builder.Logging.AddDebug();
            #endif

            return builder.Build();
        }
    }
}