using System.ComponentModel.DataAnnotations;

namespace TravelMap.Models
{
    public class CheckinRequest
    {
        [Required]
        public string Place { get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string Comment { get; set; }
        public List<string> Photos { get; set; } // Base64编码的图片列表
    }
}
