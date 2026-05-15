using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Tracker.Web.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

if (builder.Environment.IsDevelopment())
{
    // In-memory SQLite for development (no DB files)
    var keepAliveConnection = new SqliteConnection("Data Source=:memory:");
    keepAliveConnection.Open();

    builder.Services.AddSingleton(keepAliveConnection);
    builder.Services.AddDbContext<TrackerDbContext>((sp, options) =>
    {
        var connection = sp.GetRequiredService<SqliteConnection>();
        options.UseSqlite(connection);
    });
}
else
{
    var databasePath = builder.Configuration["Database:Path"] ?? "/storage/tracker.db";
    var directory = Path.GetDirectoryName(databasePath);

    if (!string.IsNullOrWhiteSpace(directory))
    {
        Directory.CreateDirectory(directory);
    }

    builder.Services.AddDbContext<TrackerDbContext>(options =>
        options.UseSqlite($"Data Source={databasePath}"));
}

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TrackerDbContext>();
    await db.Database.EnsureCreatedAsync();

    if (app.Environment.IsDevelopment())
    {
        await TrackerDbSeeder.SeedAsync(db);
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.MapGet("/", () => Results.Redirect("/habit-tracker"));
app.MapGet("/up", () => Results.Ok());

app.Run();
