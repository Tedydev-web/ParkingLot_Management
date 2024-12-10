namespace ParkingLotManagement.Models.Goong
{
    public class DirectionsResult
    {
        public string Status { get; set; } = string.Empty;
        public List<Route> Routes { get; set; } = new();
    }

    public class Route
    {
        public List<Leg> Legs { get; set; } = new();
        public OverviewPolyline OverviewPolyline { get; set; } = new();
    }

    public class Leg
    {
        public Distance Distance { get; set; } = new();
        public Duration Duration { get; set; } = new();
        public List<Step> Steps { get; set; } = new();
    }

    public class Step
    {
        public string HtmlInstructions { get; set; } = string.Empty;
        public Distance Distance { get; set; } = new();
        public Duration Duration { get; set; } = new();
    }

    public class OverviewPolyline
    {
        public string Points { get; set; } = string.Empty;
    }
}
