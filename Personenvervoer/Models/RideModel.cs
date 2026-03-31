namespace Personenvervoer.Models;

public class RideModel
{
    public Guid Id { get; set; }
    public Guid? RidepatternId { get; set; }
    public Guid? VehicleId { get; set; }
    public Guid? EndLocationId { get; set; }
    public string RideType { get; set; } = "a_to_b";
    public DateTime? MaxBoardingTime { get; set; }
    public DateTime? LocationTime { get; set; }
    public DateTime? RideTime { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation (joined)
    public RidepatternModel? Ridepattern { get; set; }
    public VehicleModel? Vehicle { get; set; }
    public LocationModel? EndLocation { get; set; }
}