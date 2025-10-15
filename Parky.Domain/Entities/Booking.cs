using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Parky.Domain.Entities
{
    public class Booking
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int LotId { get; set; }
        [Required]
        public int UserId { get; set; }
        [Required]
        public DateTime From { get; set; }
        [Required]
        public DateTime To { get; set; }
        [Required]
        [MaxLength(20)]
        public string Status { get; set; }
        [ForeignKey(nameof(LotId))]
        public ParkingLot? Lot { get; set; }

        [Timestamp]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public uint xmin { get; set; }
    }
}
