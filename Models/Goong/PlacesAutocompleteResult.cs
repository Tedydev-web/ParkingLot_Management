namespace ParkingLotManagement.Models.Goong
{
    public class PlacesAutocompleteResult
    {
        public string Status { get; set; } = string.Empty;
        public List<Prediction> Predictions { get; set; } = new();
    }

    public class Prediction
    {
        public string PlaceId { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string StructuredFormatting { get; set; } = string.Empty;
    }
}
