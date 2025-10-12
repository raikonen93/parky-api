using System.ComponentModel.DataAnnotations;

namespace Parky.Domain.Entities
{
    public class ParkingLot
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Location { get; set; }

        [Range(1, int.MaxValue)]
        public int Capacity { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
