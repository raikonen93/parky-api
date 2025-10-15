using Parky.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Parky.Domain.Entities
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Username { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
        [Required]
        public UserRole Role { get; set; }
    }
}
