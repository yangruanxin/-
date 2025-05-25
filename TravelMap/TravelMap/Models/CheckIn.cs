namespace TravelMap.Models
{
    public class CheckIn
    {
        public int Id { get; set; }
        public string UserId { get; set; }  // 关联用户
        public int LocationId { get; set; } // 关联地点
        public DateTime CheckInTime { get; set; }
        public string Note { get; set; }    // 打卡备注
        public string PhotoUrl { get; set; } // 打卡照片
    }
}
