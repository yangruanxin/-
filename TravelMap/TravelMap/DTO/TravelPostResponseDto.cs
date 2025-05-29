namespace TravelMap.DTO
{
    public class TravelPostResponseDto
    {
        public int Code { get; set; }
        public string Message { get; set; }
        public TravelPostData Data { get; set; }
    }

    public class TravelPostData
    {
        public int PostId { get; set; }
        public int UserId { get; set; }
        public string LocationName { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public List<string> ImageUrls { get; set; } = new List<string>();
        public string BeginTime { get; set; }
        public string EndTime { get; set; }
    }
}
