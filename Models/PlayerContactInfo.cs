public class PlayerContactInfo
{
    public int Id { get; set; }
    public int PlayerId { get; set; }

    public string PhoneNumber { get; set; }
    public string EmailAddress { get; set; }
    public string StreetAddress { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string ZipCode { get; set; }

    public string InstagramHandle { get; set; }
    public string TwitterHandle { get; set; }
    public string TikTokHandle { get; set; }
    public string LinkedInProfile { get; set; }

    public Player Player { get; set; }
}
