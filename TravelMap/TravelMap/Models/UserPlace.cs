using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TravelMap.Models
{
    public class UserPlace
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }

        [Required]
        public int PlaceId { get; set; }

        [ForeignKey("PlaceId")]
        public Place Place { get; set; }

        public DateTime CheckInTime { get; set; } = DateTime.UtcNow;
    }
}

