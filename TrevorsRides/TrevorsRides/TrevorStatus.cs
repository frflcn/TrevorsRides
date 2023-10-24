namespace TrevorsRides
{
    public class TrevorStatus
    {
        public bool isOnline { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public TrevorStatus(bool isOnline, double latitude, double longitude)
        {
            this.isOnline = isOnline;
            this.latitude = latitude;
            this.longitude = longitude;
        }   
    }
}
