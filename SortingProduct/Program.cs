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

builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("Db")
        ?? throw new InvalidOperationException("Connection string 'Db' was not found.");

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
// If DB is not reachable, app will fail fast with a clear error.
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Migrations");
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    logger.LogInformation("Applying EF Core migrations...");
    dbContext.Database.Migrate();
    logger.LogInformation("Migrations applied.");
}

// Simple request logging
app.Use(async (httpContext, next) =>
{
    var logger = httpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("Request");
    logger.LogInformation("{Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);
    await next();
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapControllers();

app.Run();
