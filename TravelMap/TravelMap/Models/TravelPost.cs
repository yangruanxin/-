namespace TravelMap.Models
{
    public class TravelPost
    {
        public int Id { get; set; }
        public int UserId { get; set; }  // 关联用户ID
        public string Title { get; set; }
        public string Content { get; set; }
        public string LocationName { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime BeginTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime UpdatedTime { get; set; }

        public List<TravelPostImage> Images { get; set; }  // 相关图片
    }

    public class TravelPostImage
    {
        public int Id { get; set; }
        public int TravelPostId { get; set; }
        public string ImageUrl { get; set; }
        public int SortOrder { get; set; }
    }

}
