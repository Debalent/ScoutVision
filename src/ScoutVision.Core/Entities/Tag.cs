namespace ScoutVision.Core.Entities;

public class PlayerTag : BaseEntity
{
    public int PlayerId { get; set; }
    public int TagId { get; set; }
    
    // Navigation properties
    public Player Player { get; set; } = null!;
    public Tag Tag { get; set; } = null!;
}

public class Tag : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#007ACC"; // Hex color
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // Position, Skill, Status, etc.
    
    // Navigation properties
    public ICollection<PlayerTag> PlayerTags { get; set; } = [];
}