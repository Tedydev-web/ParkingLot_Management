namespace ParkingLotManagement.Models.Goong
{
    public class PlacesResult
    {
        public string Status { get; set; } = string.Empty;
        public List<Place> Results { get; set; } = new();
    }

    public class Place
    {
        public string PlaceId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string FormattedAddress { get; set; } = string.Empty;
        public PlaceGeometry Geometry { get; set; } = new();
        public List<string> Types { get; set; } = new();
    }

    public class PlaceGeometry 
    {
        public Location Location { get; set; } = new();
        public string LocationType { get; set; } = string.Empty;
        public Viewport Viewport { get; set; } = new();
    }

    public class Viewport
    {
        public Location Northeast { get; set; } = new();
        public Location Southwest { get; set; } = new();
    }
}

public class PlacesAutocompleteResult
{
    public string Status { get; set; } = string.Empty;
    public List<PredictionItem> Predictions { get; set; } = new();
}

public class PredictionItem
{
    public string PlaceId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
