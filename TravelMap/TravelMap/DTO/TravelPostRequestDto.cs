using System.ComponentModel.DataAnnotations;

namespace TravelMap.DTO
{
    public class TravelPostRequest
    {
        
        //public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        public string LocationName { get; set; }

        [Required]
        public double Latitude { get; set; }

        [Required]
        public double Longitude { get; set; }

        [Required]
        public string BeginTime { get; set; }

        [Required]
        public string EndTime { get; set; }

        [Required]
        public List<IFormFile> Images { get; set; }

        [Required]
        public List<string> Orders { get; set; }
    }
}
