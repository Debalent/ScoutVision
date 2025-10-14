using Microsoft.EntityFrameworkCore;
using ScoutVision.Core.Entities;
using ScoutVision.Core.Search;

namespace ScoutVision.Infrastructure.Data;

public class ScoutVisionDbContext : DbContext
{
    public ScoutVisionDbContext(DbContextOptions<ScoutVisionDbContext> options) : base(options)
    {
    }

    public DbSet<Player> Players { get; set; }
    public DbSet<PlayerContactInfo> PlayerContactInfos { get; set; }
    public DbSet<PerformanceMetric> PerformanceMetrics { get; set; }
    public DbSet<VideoAnalysis> VideoAnalyses { get; set; }
    public DbSet<VideoTimestamp> VideoTimestamps { get; set; }
    public DbSet<TalentPrediction> TalentPredictions { get; set; }
    public DbSet<MindsetProfile> MindsetProfiles { get; set; }
    public DbSet<ScoutingReport> ScoutingReports { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<PlayerTag> PlayerTags { get; set; }
    
    // Analytics entities
    public DbSet<PlayerAnalytics> PlayerAnalytics { get; set; }
    public DbSet<PerformanceTrend> PerformanceTrends { get; set; }
    public DbSet<HeatMap> HeatMaps { get; set; }
    public DbSet<TeamAnalytics> TeamAnalytics { get; set; }
    
    // Search entities
    public DbSet<GameFootage> GameFootage { get; set; }
    public DbSet<FootagePlayer> FootagePlayers { get; set; }
    public DbSet<FootageHighlight> FootageHighlights { get; set; }
    public DbSet<StatBook> StatBooks { get; set; }
    public DbSet<StatBookEntry> StatBookEntries { get; set; }
    public DbSet<SearchQuery> SearchQueries { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Player Configuration
        modelBuilder.Entity<Player>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Position).HasMaxLength(50);
            entity.Property(e => e.CurrentTeam).HasMaxLength(100);
            entity.Property(e => e.League).HasMaxLength(100);
            entity.Property(e => e.Nationality).HasMaxLength(100);
            entity.Property(e => e.Height).HasPrecision(5, 2);
            entity.Property(e => e.Weight).HasPrecision(5, 2);
            entity.HasIndex(e => new { e.FirstName, e.LastName });
        });

        // Player Contact Info Configuration
        modelBuilder.Entity<PlayerContactInfo>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Player)
                  .WithOne(p => p.ContactInfo)
                  .HasForeignKey<PlayerContactInfo>(e => e.PlayerId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            entity.Property(e => e.EmailAddress).HasMaxLength(255);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
        });

        // Performance Metric Configuration
        modelBuilder.Entity<PerformanceMetric>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Player)
                  .WithMany(p => p.PerformanceMetrics)
                  .HasForeignKey(e => e.PlayerId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            entity.Property(e => e.Value).HasPrecision(18, 4);
            entity.Property(e => e.LeagueAverage).HasPrecision(18, 4);
            entity.Property(e => e.PositionAverage).HasPrecision(18, 4);
        });

        // Video Analysis Configuration
        modelBuilder.Entity<VideoAnalysis>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Player)
                  .WithMany(p => p.VideoAnalyses)
                  .HasForeignKey(e => e.PlayerId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            entity.Property(e => e.OverallScore).HasPrecision(5, 2);
            entity.Property(e => e.AverageSpeed).HasPrecision(5, 2);
            entity.Property(e => e.MaxSpeed).HasPrecision(5, 2);
            entity.Property(e => e.DistanceCovered).HasPrecision(8, 2);
        });

        // Video Timestamp Configuration
        modelBuilder.Entity<VideoTimestamp>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.VideoAnalysis)
                  .WithMany(v => v.Timestamps)
                  .HasForeignKey(e => e.VideoAnalysisId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            entity.Property(e => e.Score).HasPrecision(5, 2);
        });

        // Talent Prediction Configuration
        modelBuilder.Entity<TalentPrediction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Player)
                  .WithMany(p => p.TalentPredictions)
                  .HasForeignKey(e => e.PlayerId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            entity.Property(e => e.OverallPotentialScore).HasPrecision(5, 2);
            entity.Property(e => e.CurrentMarketValue).HasPrecision(15, 2);
            entity.Property(e => e.ProjectedMarketValue1Year).HasPrecision(15, 2);
            entity.Property(e => e.ProjectedMarketValue3Years).HasPrecision(15, 2);
            entity.Property(e => e.ProjectedMarketValue5Years).HasPrecision(15, 2);
        });

        // Mindset Profile Configuration
        modelBuilder.Entity<MindsetProfile>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Player)
                  .WithMany(p => p.MindsetProfiles)
                  .HasForeignKey(e => e.PlayerId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            entity.Property(e => e.LeadershipScore).HasPrecision(3, 1);
            entity.Property(e => e.ResilienceScore).HasPrecision(3, 1);
            entity.Property(e => e.TeamWorkScore).HasPrecision(3, 1);
            entity.Property(e => e.DisciplineScore).HasPrecision(3, 1);
            entity.Property(e => e.MotivationScore).HasPrecision(3, 1);
            entity.Property(e => e.AdaptabilityScore).HasPrecision(3, 1);
            entity.Property(e => e.PressureHandlingScore).HasPrecision(3, 1);
            entity.Property(e => e.OverallMindsetScore).HasPrecision(3, 1);
        });

        // Scouting Report Configuration
        modelBuilder.Entity<ScoutingReport>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Player)
                  .WithMany(p => p.ScoutingReports)
                  .HasForeignKey(e => e.PlayerId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            entity.Property(e => e.OverallRating).HasPrecision(3, 1);
            entity.Property(e => e.TechnicalSkills).HasPrecision(3, 1);
            entity.Property(e => e.PhysicalAttributes).HasPrecision(3, 1);
            entity.Property(e => e.TacticalAwareness).HasPrecision(3, 1);
            entity.Property(e => e.MentalStrength).HasPrecision(3, 1);
            entity.Property(e => e.Potential).HasPrecision(3, 1);
        });

        // Tag Configuration
        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Color).HasMaxLength(7); // Hex color
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // Player Tag Configuration (Many-to-Many)
        modelBuilder.Entity<PlayerTag>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Player)
                  .WithMany(p => p.Tags)
                  .HasForeignKey(e => e.PlayerId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.Tag)
                  .WithMany(t => t.PlayerTags)
                  .HasForeignKey(e => e.TagId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasIndex(e => new { e.PlayerId, e.TagId }).IsUnique();
        });

        // Apply configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ScoutVisionDbContext).Assembly);

        // Global query filters for soft delete
        modelBuilder.Entity<Player>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<PlayerContactInfo>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<PerformanceMetric>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<VideoAnalysis>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<VideoTimestamp>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<TalentPrediction>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<MindsetProfile>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ScoutingReport>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Tag>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<PlayerTag>().HasQueryFilter(e => !e.IsDeleted);
        
        // Analytics entities (no soft delete for now)
        // Search entities (no soft delete for now)
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries<BaseEntity>();

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
            }
        }
    }
}