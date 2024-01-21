using Maui.GoogleMaps;
using System.Diagnostics;
using TrevorsRidesHelpers;
using TrevorsRidesHelpers.GoogleApiClasses;
using Help = TrevorsRidesHelpers;
using Onion = Maui.GoogleMaps;

namespace TrevorsRidesMaui;

public partial class MapTestPage : ContentPage
{
	string encodedPolyline = "cecxFfukzMt@Vr@b@DD|CsHXuAT_BP_BAg@r@wGVy@PWh@_@^ER@ZFj@XnHzEz@r@|@~@`AnAxClEt@vAlC`GZz@RbArArNPdBNfANlBN|@HTv@vA\\b@^Zt@^lATf@F`@cHVeD^sCNcAn@wCl@uB`AsCfFwLzM{ZvC{G|@wBdEoJDINKbG_Nf@aApDgFdDkEp@oAjCwGnGsMnAiBpAuA|D_DpA{Az@yA~AgDvA_DF_@^_Ad@iBXg@|BcDuJsNsAuB{@kAeDtE";
	Viewport viewport = new Viewport()
	{
		low = new LatLng()
		{
			latitude = 40.7908356,
			longitude = -77.895273

		},
		high = new LatLng()
		{
			latitude = 40.817619,
			longitude = -77.860316

		}
	};
	public MapTestPage()
	{
		InitializeComponent();
		Pin pin = new Pin()
		{
			Label= "Trevor",
			Icon = BitmapDescriptorFactory.FromBundle("car_50p"),
			Position = new Position(40.79808, -77.85997)
		};
		Map.Pins.Add(pin);
		Stopwatch stopwatch = Stopwatch.StartNew();
		Onion.Polyline polyline = Help.Map.DecodeWithZoomReturnOnionPolyline(encodedPolyline, viewport);
		stopwatch.Stop();
		Log.Debug("DECODING Polyline Time: ", $"{stopwatch.Elapsed}");
		stopwatch.Restart();
		Map.Polylines.Add(polyline);
		stopwatch.Stop();
        Log.Debug("DRAWING Polyline Time: ", $"{stopwatch.Elapsed}");
    }
}