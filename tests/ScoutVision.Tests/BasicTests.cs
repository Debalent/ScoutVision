using Xunit;
using ScoutVision.Core.Entities;
using ScoutVision.Core.Enums;

namespace ScoutVision.Tests;

public class BasicTests
{
    [Fact]
    public void Player_Creation_ShouldSetDefaultValues()
    {
        // Arrange & Act
        var player = new Player
        {
            Name = "Test Player",
            Age = 25,
            Position = Position.Forward
        };

        // Assert
        Assert.Equal("Test Player", player.Name);
        Assert.Equal(25, player.Age);
        Assert.Equal(Position.Forward, player.Position);
        Assert.NotEqual(default(DateTime), player.CreatedAt);
    }

    [Fact]
    public void Team_Creation_ShouldInitializePlayersList()
    {
        // Arrange & Act
        var team = new Team
        {
            Name = "Test Team",
            City = "Test City",
            League = "Test League"
        };

        // Assert
        Assert.Equal("Test Team", team.Name);
        Assert.NotNull(team.Players);
        Assert.Empty(team.Players);
    }

    [Fact]
    public void Performance_Creation_ShouldSetCorrectValues()
    {
        // Arrange & Act
        var performance = new Performance
        {
            Goals = 2,
            Assists = 1,
            MinutesPlayed = 90,
            Rating = 8.5
        };

        // Assert
        Assert.Equal(2, performance.Goals);
        Assert.Equal(1, performance.Assists);
        Assert.Equal(90, performance.MinutesPlayed);
        Assert.Equal(8.5, performance.Rating);
    }

    [Theory]
    [InlineData(InjuryType.Muscle, InjurySeverity.Minor)]
    [InlineData(InjuryType.Ligament, InjurySeverity.Moderate)]
    [InlineData(InjuryType.Bone, InjurySeverity.Severe)]
    public void InjuryReport_Creation_ShouldAcceptValidEnums(InjuryType type, InjurySeverity severity)
    {
        // Arrange & Act
        var injury = new InjuryReport
        {
            InjuryType = type,
            Severity = severity,
            Description = "Test injury",
            InjuryDate = DateTime.UtcNow
        };

        // Assert
        Assert.Equal(type, injury.InjuryType);
        Assert.Equal(severity, injury.Severity);
        Assert.Equal("Test injury", injury.Description);
    }

    [Fact]
    public void ApplicationUser_Creation_ShouldSetDefaults()
    {
        // Arrange & Act
        var user = new ApplicationUser
        {
            UserName = "testuser",
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User"
        };

        // Assert
        Assert.Equal("testuser", user.UserName);
        Assert.Equal("test@example.com", user.Email);
        Assert.True(user.IsActive);
        Assert.NotEqual(default(DateTime), user.CreatedAt);
    }
}