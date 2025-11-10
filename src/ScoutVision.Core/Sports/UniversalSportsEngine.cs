public class UniversalSportsEngine
{
    public enum SportType
    {
        Football, Basketball, Baseball, Hockey, Tennis, Volleyball, 
        Rugby, Cricket, Golf, Swimming, Track, Gymnastics, Wrestling,
        Boxing, MMA, Esports, AmericanFootball, Soccer
    }
    
    public class SportConfiguration
    {
        public SportType Sport { get; set; }
        public List<string> KeyMetrics { get; set; } = new();
        public List<string> Positions { get; set; } = new();
        public Dictionary<string, object> SportSpecificRules { get; set; } = new();
        public List<string> InjuryRiskFactors { get; set; } = new();
        public string FieldDimensions { get; set; } = "";
        public int TeamSize { get; set; }
        public TimeSpan MatchDuration { get; set; }
    }
    
    public static Dictionary<SportType, SportConfiguration> SportConfigs = new()
    {
        [SportType.Basketball] = new()
        {
            Sport = SportType.Basketball,
            KeyMetrics = new() { "Points", "Rebounds", "Assists", "Steals", "Blocks", "FG%", "3P%", "FT%" },
            Positions = new() { "Point Guard", "Shooting Guard", "Small Forward", "Power Forward", "Center" },
            TeamSize = 5,
            MatchDuration = TimeSpan.FromMinutes(48),
            InjuryRiskFactors = new() { "Ankle", "Knee", "Shoulder", "Back" }
        },
        [SportType.Baseball] = new()
        {
            Sport = SportType.Baseball,
            KeyMetrics = new() { "Batting Average", "Home Runs", "RBIs", "ERA", "Strikeouts", "WHIP", "OPS" },
            Positions = new() { "Pitcher", "Catcher", "First Base", "Second Base", "Third Base", "Shortstop", "Left Field", "Center Field", "Right Field" },
            TeamSize = 9,
            MatchDuration = TimeSpan.FromHours(3),
            InjuryRiskFactors = new() { "Elbow", "Shoulder", "Hamstring", "Back" }
        },
        [SportType.Tennis] = new()
        {
            Sport = SportType.Tennis,
            KeyMetrics = new() { "Aces", "Double Faults", "First Serve %", "Winners", "Unforced Errors", "Break Points Won" },
            Positions = new() { "Singles", "Doubles" },
            TeamSize = 1,
            MatchDuration = TimeSpan.FromHours(2),
            InjuryRiskFactors = new() { "Shoulder", "Elbow", "Knee", "Ankle" }
        }
        // Add all other sports...
    };
}