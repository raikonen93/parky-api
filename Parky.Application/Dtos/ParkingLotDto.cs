using System.ComponentModel.DataAnnotations;

namespace Parky.Application.Dtos
{
    public class ParkingLotDto : IValidatableObject
    {
        public string Name { get; set; } = string.Empty;
        public string? Location { get; set; }
        public int Capacity { get; set; }
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(Name))
                yield return new ValidationResult("Name is required.", new[] { nameof(Name) });

            if (Capacity < 1)
                yield return new ValidationResult("Capacity must be positive.", new[] { nameof(Capacity) });
        }
    }
}
