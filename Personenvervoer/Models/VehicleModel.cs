namespace Personenvervoer.Models;

public class VehicleModel
{
    public Guid Id { get; set; }
    public string VehicleType { get; set; } = string.Empty;
    public string LicensePlate { get; set; } = string.Empty;
    public int Seats { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
