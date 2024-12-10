namespace ParkingLotManagement.Models.Goong
{
    public class DistanceMatrix
    {
        public string? Status { get; set; }
        public List<Row> Rows { get; set; } = new();
    }

    public class Row
    {
        public List<Element> Elements { get; set; } = new();
    }

    public class Element
    {
        public Distance? Distance { get; set; }
        public Duration? Duration { get; set; }
        public string? Status { get; set; }
    }

    public class Distance
    {
        public string? Text { get; set; }
        public int Value { get; set; }
    }

    public class Duration
    {
        public string? Text { get; set; }
        public int Value { get; set; }
    }
}
