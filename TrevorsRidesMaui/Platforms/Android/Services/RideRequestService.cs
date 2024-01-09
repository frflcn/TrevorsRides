using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using Microsoft.Maui.Controls.Maps;
using TrevorsRidesHelpers;
using static Microsoft.Maui.ApplicationModel.Platform;

namespace TrevorsRidesMaui.BackgroundTasks
{
    [Service(Name="com.trevorsapps.trevorsrides.RideRequestService")]
    public partial class RideRequestService : Service
    {
        

        public const int RIDE_REQUEST_SERVICE_NOTIFICATION_ID = 10000;
        static Context context = Android.App.Application.Context;
        public static partial void StopService()
        {           
            Instance?.StopSelf();
        }






        public static partial void StartService()
        {
            var intent = new Android.Content.Intent(context, typeof(RideRequestService));
            if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
            {
                NotificationChannel channel = new NotificationChannel("com.trevorsapps.trevorsrides.LocationNotification", "Location", NotificationImportance.Min);
                channel.Description = "Provides a notification when Trevor's Rides is getting your location in the background";
                var notificationManager = context.GetSystemService(Context.NotificationService) as NotificationManager;
                notificationManager.CreateNotificationChannel(channel);
                context.StartForegroundService(intent);
            }
            else
            {
                context.StartService(intent);
            }
            

        }





        public override StartCommandResult OnStartCommand(Android.Content.Intent? intent, StartCommandFlags flags, int startId)
        {
            _ = ConnectWebsocketAsync();
            // Code not directly related to publishing the notification has been omitted for clarity.
            // Normally, this method would hold the code to be run when the service is started.
            timer = new System.Timers.Timer();
            timer.Interval = 1000;
            timer.Elapsed += (s, e) =>
            {
                Log.Debug("Ride Request Service");
            };
            timer.Start();
            var notification = new NotificationCompat.Builder(this, "com.trevorsapps.trevorsrides.LocationNotification")
                .SetContentTitle("Trevor's Rides")
                .SetContentText("Trevor's Rides Is Getting Your Location in the Background")
                .SetOngoing(true)
                .Build();

            // Enlist this instance of the service as a foreground service
            StartForeground(RIDE_REQUEST_SERVICE_NOTIFICATION_ID, notification);

            return Android.App.StartCommandResult.Sticky;
        }






        public override IBinder OnBind(Android.Content.Intent? intent)
        {
            return new Android.OS.Binder();
        }






        public override void OnCreate()
        {
            base.OnCreate();

            IsRunning = true;
            Instance = this;

            Log.Debug("RIDE REQUEST SERVICE ON CREATE", guid.ToString());

            Client = new ClientWebSocket();
        }






        public override void OnDestroy()
        {
            Log.Debug("RIDE REQUEST SERVICE", "OnDestroy");

            Instance = null;
            IsRunning = false;

            if (Client != null)
            {
                Client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Normal closure", CancellationToken.None);
                Client.Dispose();
            }

            if (timer != null)
            {
                timer.Stop();
                timer.Elapsed -= (s, e) =>
                {
                    Log.Debug("Ride Request Service");
                };
                timer.Close();
                timer.Dispose();
            }

            base.OnDestroy();
        }

    }
}
