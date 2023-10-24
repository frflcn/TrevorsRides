﻿#if (ANDROID || IOS)
using Maui.GoogleMaps.Hosting;
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
                });

            #if DEBUG
		        builder.Logging.AddDebug();
            #endif
            #if ANDROID
                builder.UseGoogleMaps();
            #elif IOS
                //builder.UseGoogleMaps(Variables.GOOGLE_MAPS_IOS_API_KEY);
            #endif
            return builder.Build();
        }
    }
}
#endif