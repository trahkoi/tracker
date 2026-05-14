using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Tracker.Web.Data;

namespace Tracker.Web.Pages;

public class HabitTrackerModel(TrackerDbContext db) : PageModel
{
    private readonly TrackerDbContext _db = db;

    [BindProperty(SupportsGet = true)]
    public int? Year { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? Month { get; set; }

    public string[] Weekdays { get; } = ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"];

    public required string MonthLabel { get; set; }
    public required int PrevYear { get; set; }
    public required int PrevMonth { get; set; }
    public required int NextYear { get; set; }
    public required int NextMonth { get; set; }

    public List<CalendarDayVm> Days { get; } = [];

    public async Task OnGetAsync()
    {
        var now = DateTime.Today;
        var viewYear = Year ?? now.Year;
        var viewMonth = Month ?? now.Month;

        if (viewMonth is < 1 or > 12)
        {
            viewMonth = now.Month;
        }

        var monthStart = new DateTime(viewYear, viewMonth, 1);
        var daysInMonth = DateTime.DaysInMonth(viewYear, viewMonth);
        var offset = ToMondayIndex((int)monthStart.DayOfWeek);
        var totalCells = (int)Math.Ceiling((offset + daysInMonth) / 7.0) * 7;

        var gridStart = monthStart.AddDays(-offset);
        var gridEnd = gridStart.AddDays(totalCells - 1);

        var startKey = gridStart.ToString("yyyy-MM-dd");
        var endKey = gridEnd.ToString("yyyy-MM-dd");

        var logs = (await _db.HabitLogs
            .AsNoTracking()
            .ToListAsync())
            .Where(l => string.Compare(l.Date, startKey, StringComparison.Ordinal) >= 0
                && string.Compare(l.Date, endKey, StringComparison.Ordinal) <= 0)
            .ToList();

        var logsByDateHabit = logs.ToDictionary(l => (l.Date, l.HabitId));

        for (var i = 0; i < totalCells; i++)
        {
            var day = gridStart.AddDays(i);
            var dateKey = day.ToString("yyyy-MM-dd");
            var inMonth = day.Month == viewMonth;

            logsByDateHabit.TryGetValue((dateKey, "pushups"), out var pushupsLog);
            logsByDateHabit.TryGetValue((dateKey, "wcs"), out var wcsLog);
            logsByDateHabit.TryGetValue((dateKey, "veggies"), out var veggiesLog);

            var pushupsCount = pushupsLog?.ValueNumber ?? 0;
            var wcsPractice = wcsLog?.ValueBoolean ?? false;
            var veggiesEnough = veggiesLog?.ValueBoolean ?? false;

            Days.Add(new CalendarDayVm
            {
                Date = day,
                InMonth = inMonth,
                IsToday = day.Date == now,
                PushupsCount = pushupsCount,
                WcsPractice = wcsPractice,
                VeggiesEnough = veggiesEnough,
            });
        }

        var prev = monthStart.AddMonths(-1);
        var next = monthStart.AddMonths(1);

        PrevYear = prev.Year;
        PrevMonth = prev.Month;
        NextYear = next.Year;
        NextMonth = next.Month;
        MonthLabel = monthStart.ToString("MMMM yyyy");
    }

    private static int ToMondayIndex(int dayOfWeek) => (dayOfWeek + 6) % 7;

    public sealed class CalendarDayVm
    {
        public required DateTime Date { get; init; }
        public required bool InMonth { get; init; }
        public required bool IsToday { get; init; }
        public required int PushupsCount { get; init; }
        public required bool WcsPractice { get; init; }
        public required bool VeggiesEnough { get; init; }
        public bool AnyDone => PushupsCount > 0 || WcsPractice || VeggiesEnough;
    }
}
