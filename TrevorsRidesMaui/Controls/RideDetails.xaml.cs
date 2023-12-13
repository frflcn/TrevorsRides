namespace TrevorsRidesMaui.Controls;

public partial class RideDetails : ContentView
{
    decimal? _rideAmount;
    public decimal? RideAmount
    {
        get => _rideAmount;
        set
        {
            _rideAmount = Math.Round(value.Value, 2);
            this.CostLabel.Text = $"Approx Ride Cost: ${_rideAmount.Value}";
        }
    }
    TimeSpan? _waitTime;
    public TimeSpan? WaitTime
    {
        get => _waitTime;
        set
        {
            _waitTime = value.Value;
            this.PickupLabel.Text = $"Approx Pickup Time: {(DateTime.Now + _waitTime.Value).ToShortTimeString()} ({Math.Round(_waitTime.Value.TotalMinutes)} minutes from now)";
            if (_rideDuration != null)
                this.DropOffLabel.Text = $"Approx DropOff Time: {(DateTime.Now + _waitTime.Value + _rideDuration.Value).ToShortTimeString()} ({Math.Round(_waitTime.Value.TotalMinutes + _rideDuration.Value.TotalMinutes)} minutes from now)";
        }
    }
    TimeSpan? _rideDuration;
    public TimeSpan? RideDuration
    {
        get => _rideDuration;
        set
        {
            _rideDuration = value.Value;
            this.DropOffLabel.Text = $"Approx DropOff Time: {(DateTime.Now + _waitTime.Value + _rideDuration.Value).ToShortTimeString()} ({Math.Round(_waitTime.Value.TotalMinutes + _rideDuration.Value.TotalMinutes)} minutes from now)";
        }
    }
    decimal? _cost;
    public decimal? Cost 
    {   get => _cost;
        set
        {
            _cost = value.Value;
            this.CostLabel.Text = $"Approx Ride Cost: ${Math.Round(_cost.Value)}";
        }
    }

    public event EventHandler<EventArgs> BookRidePressed;
    public RideDetails()
	{
		InitializeComponent();
	}
    public static TimeSpan ToTimeSpan(string duration)
    {
        duration = duration.Substring(0, duration.Length - 1);
        return new TimeSpan(0, 0, int.Parse(duration));
    }

    private void Button_Pressed(object sender, EventArgs e)
    {
        BookRidePressed?.Invoke(this, e);
    }
}