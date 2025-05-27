using System.ComponentModel.DataAnnotations;

namespace TravelMap.DTO
{
    public class TravelPostCreateDto
    {
        [Required]
        public string Title { get; set; }

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

        public List<IFormFile> Images { get; set; }

        public List<string> Orders { get; set; }
    }

}
