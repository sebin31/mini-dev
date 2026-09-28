using Microsoft.EntityFrameworkCore;
using MiniTwise.Api.Data;
using MiniTwise.Api.Models;

var builder = WebApplication.CreateBuilder(args);

// Connection string comes from appsettings.json or env var ConnectionStrings__Default
var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("Connection string 'Default' not found.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Mini DEV API",
        Version = "v1"
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

// Swagger available in all environments for this learning project
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAll");

// Retry DB connection + apply migrations on startup (SQL Server container may still be starting)
await WaitForDatabaseAndMigrateAsync(app);

app.MapGet("/health", () => Results.Ok(new { status = "healthy", time = DateTime.UtcNow }))
   .WithName("HealthCheck");

app.MapGet("/api/items", async (AppDbContext db) =>
    await db.Items.OrderByDescending(i => i.CreatedAt).ToListAsync())
   .WithName("GetItems");

app.MapGet("/api/items/{id:int}", async (int id, AppDbContext db) =>
    await db.Items.FindAsync(id) is Item item ? Results.Ok(item) : Results.NotFound())
   .WithName("GetItemById");

app.MapPost("/api/items", async (Item item, AppDbContext db) =>
{
    item.Id = 0;
    item.CreatedAt = DateTime.UtcNow;
    db.Items.Add(item);
    await db.SaveChangesAsync();
    return Results.Created($"/api/items/{item.Id}", item);
}).WithName("CreateItem");

app.MapDelete("/api/items/{id:int}", async (int id, AppDbContext db) =>
{
    var item = await db.Items.FindAsync(id);
    if (item is null) return Results.NotFound();
    db.Items.Remove(item);
    await db.SaveChangesAsync();
    return Results.NoContent();
}).WithName("DeleteItem");

app.Run();

static async Task WaitForDatabaseAndMigrateAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");

    const int maxRetries = 20;
    for (var attempt = 1; attempt <= maxRetries; attempt++)
    {
        try
        {
            logger.LogInformation("Ensuring database exists (attempt {Attempt}/{Max})...", attempt, maxRetries);
            await db.Database.EnsureCreatedAsync();
            logger.LogInformation("Database ready.");
            return;
        }
        catch (Exception ex)
        {
            logger.LogWarning("DB not ready yet: {Message}", ex.Message);
            if (attempt == maxRetries) throw;
            await Task.Delay(TimeSpan.FromSeconds(10));
        }
    }
}
