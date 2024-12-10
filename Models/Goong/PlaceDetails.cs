namespace ParkingLotManagement.Models.Goong
{
    public class PlaceDetails
    {
        public string PlaceId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string FormattedAddress { get; set; } = string.Empty;
        public Geometry Geometry { get; set; } = new();
        public string Icon { get; set; } = string.Empty;
        public List<string> Types { get; set; } = new();
        public double Rating { get; set; }
        public OpeningHours OpeningHours { get; set; } = new();
    }

    public class OpeningHours
    {
        public bool OpenNow { get; set; }
        public List<Period> Periods { get; set; } = new();
        public List<string> WeekdayText { get; set; } = new();
    }

    public class Period
    {
        public DayTime Open { get; set; } = new();
        public DayTime Close { get; set; } = new();
    }

    public class DayTime
    {
        public int Day { get; set; }
        public string Time { get; set; } = string.Empty;
    }
}
