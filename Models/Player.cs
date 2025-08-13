public class Player
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string Position { get; set; }
    public string Team { get; set; }

    public ICollection<PlayerContactInfo> ContactInfos { get; set; }
}
