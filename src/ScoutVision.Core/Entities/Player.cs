using ScoutVision.Core.Enums;

namespace ScoutVision.Core.Entities;

public class Player
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public Position Position { get; set; }
    public string? Team { get; set; }
    public string? Nationality { get; set; }
    public double Height { get; set; }
    public double Weight { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public virtual ICollection<Performance> Performances { get; set; } = new List<Performance>();
    public virtual ICollection<InjuryReport> InjuryReports { get; set; } = new List<InjuryReport>();
}
