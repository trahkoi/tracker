using Microsoft.EntityFrameworkCore;

namespace Tracker.Web.Data;

public sealed class TrackerDbContext(DbContextOptions<TrackerDbContext> options) : DbContext(options)
{
    public DbSet<Habit> Habits => Set<Habit>();
    public DbSet<HabitLog> HabitLogs => Set<HabitLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Habit>(entity =>
        {
            entity.ToTable("Habits");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasMaxLength(64);
            entity.Property(x => x.Name).IsRequired().HasMaxLength(200);
            entity.Property(x => x.Type).IsRequired().HasMaxLength(32);
            entity.Property(x => x.Unit).HasMaxLength(32);
            entity.Property(x => x.CreatedAt).IsRequired();
        });

        modelBuilder.Entity<HabitLog>(entity =>
        {
            entity.ToTable("HabitLogs");
            entity.HasKey(x => new { x.Date, x.HabitId });
            entity.Property(x => x.Date).HasMaxLength(10);
            entity.Property(x => x.HabitId).HasMaxLength(64);
            entity.Property(x => x.Note).HasMaxLength(500);

            entity
                .HasOne(x => x.Habit)
                .WithMany(x => x.Logs)
                .HasForeignKey(x => x.HabitId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

public sealed class Habit
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string Type { get; set; }
    public string? Unit { get; set; }
    public bool Active { get; set; }
    public DateTime CreatedAt { get; set; }

    public ICollection<HabitLog> Logs { get; set; } = new List<HabitLog>();
}

public sealed class HabitLog
{
    public required string Date { get; set; } // yyyy-MM-dd
    public required string HabitId { get; set; }
    public bool? ValueBoolean { get; set; }
    public int? ValueNumber { get; set; }
    public string? Note { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Habit? Habit { get; set; }
}
