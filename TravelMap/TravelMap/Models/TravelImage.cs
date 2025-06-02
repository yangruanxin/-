namespace TravelMap.Models
{
    public class TravelImage
    {
        public int Id { get; set; }

        // 图片地址，如 /uploads/xxx.jpg
        public string Url { get; set; }

        // 可选字段，用于排序展示
        public int Order { get; set; }

        // 外键字段：指向所属的 TravelPost
        public int TravelPostId { get; set; }

        // 导航属性
        public TravelPost TravelPost { get; set; }
    }
}
