namespace TravelMap.DTO
{
    public class TravelPostUpdateDto
    {
        public string Content { get; set; }
        public string LocationName { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime BeginTime { get; set; }
        public DateTime EndTime { get; set; }
        public string OldImageUrls { get; set; }  // JSON 字符串形式
        public List<IFormFile> Images { get; set; }  // 新图片文件
    }
}
