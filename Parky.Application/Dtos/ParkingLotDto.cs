namespace Parky.Application.Dtos
{
    public class ParkingLotDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Location { get; set; }
        public int Capacity { get; set; }
    }
}
