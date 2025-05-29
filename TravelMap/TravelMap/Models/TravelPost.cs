namespace TravelMap.Models
{
    public class TravelPost
    {
        public int Id { get; set; }
        //public string Title { get; set; }
        public string Content { get; set; }
        public string LocationName { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime BeginTime { get; set; }
        public DateTime EndTime { get; set; }
        public List<string> ImageUrls { get; set; } = new List<string>();
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }
    }

}
