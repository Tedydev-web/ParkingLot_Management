namespace ParkingLotManagement.Models.Goong
{
    public class GeocodingResult
    {
        public string? Status { get; set; }
        public List<GeocodingLocation> Results { get; set; } = new();
    }

    public class GeocodingLocation
    {
        public string? FormattedAddress { get; set; }
        public Geometry? Geometry { get; set; }
        public string? PlaceId { get; set; }
    }

    public class Geometry
    {
        public Location? Location { get; set; }
    }

    public class Location
    {
        public double Lat { get; set; }
        public double Lng { get; set; }
    }
}
