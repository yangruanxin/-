using System.ComponentModel.DataAnnotations;

namespace TravelMap.Models
{
    public class Place
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        public double Latitude { get; set; }    // 纬度

        public double Longitude { get; set; }   // 经度

        public ICollection<UserPlace> UserPlaces { get; set; }
    }
}
