namespace TravelMap.Models
{
    public class CheckinResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public CheckinData Data { get; set; }
    }

    public class CheckinData
    {
        public string CheckinId { get; set; }
        public string Place { get; set; }
        public DateTime CheckinTime { get; set; }
        public int PointsEarned { get; set; }
        public List<string> PhotoUrls { get; set; }
    }
}
