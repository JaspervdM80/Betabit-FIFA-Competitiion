using FC26Competition.Models;
using Microsoft.EntityFrameworkCore;

namespace FC26Competition.Data;

public class FC26CompetitionContext : DbContext
{
    public FC26CompetitionContext(DbContextOptions<FC26CompetitionContext> options)
        : base(options)
    {
    }

    public DbSet<Team> Teams => Set<Team>();
    public DbSet<Match> Matches => Set<Match>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Team entity
        modelBuilder.Entity<Team>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TeamName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Player1Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Player2Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ClubCountry).IsRequired().HasMaxLength(100);
            
            // Computed properties - not stored in database
            entity.Ignore(e => e.GoalDifference);
            entity.Ignore(e => e.Points);
        });

        // Configure Match entity
        modelBuilder.Entity<Match>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.HomeTeamId).IsRequired();
            entity.Property(e => e.AwayTeamId).IsRequired();
            entity.Property(e => e.Stage).IsRequired();
            
            // Computed property - not stored in database
            entity.Ignore(e => e.IsPlayed);
            
            // Create indexes for better query performance
            entity.HasIndex(e => e.HomeTeamId);
            entity.HasIndex(e => e.AwayTeamId);
            entity.HasIndex(e => e.Stage);
            entity.HasIndex(e => e.ScheduledTime);
        });
    }
}
