#if (ANDROID || IOS)
using TrevorsRidesHelpers;
using Microsoft.Extensions.Logging;

namespace TrevorDrivesMaui
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

            #if DEBUG
		        builder.Logging.AddDebug();
            #endif

            return builder.Build();
        }
    }
}
#endif