using ScoutVision.Core.Enums;

namespace ScoutVision.Core.Entities;

public class InjuryReport
{
    public int Id { get; set; }
    public int PlayerId { get; set; }
    public InjuryType InjuryType { get; set; }
    public InjurySeverity Severity { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime InjuryDate { get; set; }
    public DateTime? RecoveryDate { get; set; }
    public bool IsRecovered { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual Player? Player { get; set; }
}