namespace Personenvervoer.Models;

public class RidepatternModel
{
    public Guid Id { get; set; }
    public Guid? MemberId { get; set; }
    public bool IsWheelchair { get; set; }
    public int Rank { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation (joined)
    public MemberModel? Member { get; set; }
}