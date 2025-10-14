namespace ScoutVision.Core.Entities;

public class PlayerContactInfo : BaseEntity
{
    public int PlayerId { get; set; }
    
    // Personal Contact
    public string PhoneNumber { get; set; } = string.Empty;
    public string EmailAddress { get; set; } = string.Empty;
    public string EmergencyContact { get; set; } = string.Empty;
    public string EmergencyPhone { get; set; } = string.Empty;
    
    // Address
    public string StreetAddress { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    
    // Social Media & Professional
    public string InstagramHandle { get; set; } = string.Empty;
    public string TwitterHandle { get; set; } = string.Empty;
    public string TikTokHandle { get; set; } = string.Empty;
    public string LinkedInProfile { get; set; } = string.Empty;
    public string PersonalWebsite { get; set; } = string.Empty;
    
    // Agent/Representative
    public string AgentName { get; set; } = string.Empty;
    public string AgentPhone { get; set; } = string.Empty;
    public string AgentEmail { get; set; } = string.Empty;
    
    // Navigation properties
    public Player Player { get; set; } = null!;
}