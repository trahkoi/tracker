using Microsoft.EntityFrameworkCore;

namespace Tracker.Web.Data;

public static class TrackerDbSeeder
{
    public static async Task SeedAsync(TrackerDbContext db)
    {
        await db.Database.EnsureCreatedAsync();

        if (await db.Habits.AnyAsync())
        {
            return;
        }

        var now = DateTime.UtcNow;

        var habits = new[]
        {
            new Habit { Id = "pushups", Name = "Push-ups", Type = "count", Unit = "reps", Active = true, CreatedAt = now },
            new Habit { Id = "wcs", Name = "West Coast Swing", Type = "boolean", Unit = null, Active = true, CreatedAt = now },
            new Habit { Id = "veggies", Name = "Veggies (enough)", Type = "boolean", Unit = null, Active = true, CreatedAt = now },
        };

        await db.Habits.AddRangeAsync(habits);

        var logs = new List<HabitLog>();
        var baseDate = DateTime.Today;
        for (var i = 0; i < 35; i++)
        {
            var date = baseDate.AddDays(-i).ToString("yyyy-MM-dd");
            var didPushups = i % 6 != 0;
            var didWcs = i % 4 != 1;
            var didVeggies = i % 5 != 2;

            logs.Add(new HabitLog
            {
                Date = date,
                HabitId = "pushups",
                ValueNumber = didPushups ? 20 + ((i * 9) % 36) : 0,
                UpdatedAt = now,
            });

            logs.Add(new HabitLog
            {
                Date = date,
                HabitId = "wcs",
                ValueBoolean = didWcs,
                UpdatedAt = now,
            });

            logs.Add(new HabitLog
            {
                Date = date,
                HabitId = "veggies",
                ValueBoolean = didVeggies,
                UpdatedAt = now,
            });
        }

        await db.HabitLogs.AddRangeAsync(logs);
        await db.SaveChangesAsync();
    }
}
