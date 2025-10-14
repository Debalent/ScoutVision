using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ScoutVision.Core.Search;

namespace ScoutVision.Infrastructure.Data.Configurations;

public class FootagePlayerConfiguration : IEntityTypeConfiguration<FootagePlayer>
{
    public void Configure(EntityTypeBuilder<FootagePlayer> builder)
    {
        builder.HasKey(fp => fp.Id);

        builder.Property(fp => fp.ScreenTime)
            .IsRequired();

        builder.Property(fp => fp.PerformanceRating)
            .HasPrecision(5, 2);

        builder.HasOne(fp => fp.GameFootage)
            .WithMany(gf => gf.FootagePlayers)
            .HasForeignKey(fp => fp.GameFootageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(fp => fp.Player)
            .WithMany()
            .HasForeignKey(fp => fp.PlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(fp => new { fp.GameFootageId, fp.PlayerId })
            .IsUnique();
    }
}

public class FootageHighlightConfiguration : IEntityTypeConfiguration<FootageHighlight>
{
    public void Configure(EntityTypeBuilder<FootageHighlight> builder)
    {
        builder.HasKey(fh => fh.Id);

        builder.Property(fh => fh.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(fh => fh.HighlightType)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(fh => fh.ThumbnailUrl)
            .HasMaxLength(500);

        builder.Property(fh => fh.Significance)
            .HasPrecision(5, 2);

        builder.HasOne(fh => fh.GameFootage)
            .WithMany(gf => gf.Highlights)
            .HasForeignKey(fh => fh.GameFootageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(fh => fh.GameFootageId);
        builder.HasIndex(fh => fh.HighlightType);
    }
}

public class StatBookEntryConfiguration : IEntityTypeConfiguration<StatBookEntry>
{
    public void Configure(EntityTypeBuilder<StatBookEntry> builder)
    {
        builder.HasKey(sbe => sbe.Id);

        builder.Property(sbe => sbe.PlayerName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(sbe => sbe.TeamName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(sbe => sbe.Position)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(sbe => sbe.Goals)
            .HasPrecision(8, 2);

        builder.Property(sbe => sbe.Assists)
            .HasPrecision(8, 2);

        builder.Property(sbe => sbe.Rating)
            .HasPrecision(4, 2);

        builder.Property(sbe => sbe.AdditionalStats)
            .HasColumnType("nvarchar(max)");

        builder.HasOne(sbe => sbe.StatBook)
            .WithMany(sb => sb.Entries)
            .HasForeignKey(sbe => sbe.StatBookId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(sbe => sbe.StatBookId);
        builder.HasIndex(sbe => sbe.PlayerName);
        builder.HasIndex(sbe => sbe.TeamName);
    }
}