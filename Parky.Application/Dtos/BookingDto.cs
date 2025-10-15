using System.ComponentModel.DataAnnotations;

namespace Parky.Application.Dtos
{
    public class BookingDto : IValidatableObject
    {
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
        public string Status { get; set; } = "Active";

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (From >= To)
                yield return new ValidationResult(
                    "The 'From' date must be earlier than 'To' date.",
                    new[] { nameof(From), nameof(To) });
        }
    }
}
