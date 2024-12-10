namespace ParkingLotManagement.Models
{
    public class GeocodeResponse
    {
        public string Status { get; set; } = string.Empty;
        public List<GeocodeResult> Results { get; set; } = new();
    }

    public class GeocodeResult
    {
        public string PlaceId { get; set; } = string.Empty;
        public string FormattedAddress { get; set; } = string.Empty;
        public Geometry Geometry { get; set; } = new();
    }

    public class Geometry
    {
        public Location Location { get; set; } = new();
    }

    public class Location
    {
        public double Lat { get; set; }
        public double Lng { get; set; }
    }
} 