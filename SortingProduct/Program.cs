using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using SortingProduct.Api.BackgroundJobs;
using SortingProduct.Application.Abstractions;
using SortingProduct.Application.Services;
using SortingProduct.Infrastructure.Excel;
using SortingProduct.Infrastructure.Persistence;
using SortingProduct.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "SortingProduct API", Version = "v1" });
});

// Railway: set env var ConnectionStrings__Db.
// Locally: use appsettings.Development.json.
var connectionString = builder.Configuration.GetConnectionString("Db");
if (string.IsNullOrWhiteSpace(connectionString))
{
    Console.WriteLine("[Startup] Connection string 'Db' is empty. Set environment variable ConnectionStrings__Db.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        // still register; will fail only when used
        return;
    }

    options.UseNpgsql(connectionString);
});

// Health checks
builder.Services.AddHealthChecks();

// Application services
builder.Services.AddScoped<ProductImportService>();
builder.Services.AddScoped<ProductGroupingService>();

// Infrastructure
builder.Services.AddScoped<IExcelProductImporter, ClosedXmlExcelProductImporter>();
builder.Services.AddScoped<IProductBatchRepository, ProductBatchRepository>();
builder.Services.AddScoped<IProductGroupRepository, ProductGroupRepository>();

// Background job: run grouping every 5 minutes
builder.Services.AddHostedService<ProductGroupingHostedService>();

var app = builder.Build();

// Auto-apply migrations on startup (for easy hosting like Railway).
// We do small retry because managed DB can be not ready for a few seconds.
if (!string.IsNullOrWhiteSpace(connectionString))
{
    using var scope = app.Services.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Migrations");
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    logger.LogInformation("Applying EF Core migrations...");

    var attempt = 0;
    while (true)
    {
        try
        {
            dbContext.Database.Migrate();
            logger.LogInformation("Migrations applied.");
            break;
        }
        catch (Exception ex) when (attempt < 10)
        {
            attempt++;
            var delaySeconds = Math.Min(30, attempt * 2);
            logger.LogWarning(ex, "Migration attempt {Attempt} failed. Waiting {DelaySeconds}s...", attempt, delaySeconds);
            Thread.Sleep(TimeSpan.FromSeconds(delaySeconds));
        }
    }
}

// Simple request logging
app.Use(async (httpContext, next) =>
{
    var logger = httpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("Request");
    logger.LogInformation("{Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);
    await next();
});

var enableSwagger = app.Environment.IsDevelopment() || string.Equals(builder.Configuration["ENABLE_SWAGGER"], "true", StringComparison.OrdinalIgnoreCase);
if (enableSwagger)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapControllers();

app.Run();
