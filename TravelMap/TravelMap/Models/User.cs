using System.ComponentModel.DataAnnotations;

namespace TravelMap.Models { 
    public class User
    {
        [Key]  // 表示主键
        public int Id { get; set; }

        [Required]  // 必填
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
    }
}