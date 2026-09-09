using ServerMonitor.Data.Entities;
using ServerMonitor.Data;
using ServerMonitor.Services;
using ServerMonitor.DTOs;
using ServerMonitor.Filters;
using ServerMonitor.Validators;
using ApplicationLogLevel = ServerMonitor.Data.Entities.LogLevel;
using Microsoft.EntityFrameworkCore;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddDbContext<ServerMonitorDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("ServerMonitor")));
builder.Services.AddHostedService<TelemetryAlertWorker>();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ServerMonitorDbContext>();
    dbContext.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapPost("/api/metrics", async (CreateMetricRequest request, ServerMonitorDbContext dbContext) =>
{
    var metric = new ServerMetric
    {
        ServerName = request.ServerName,
        CpuUsagePercent = request.CpuUsagePercent,
        MemoryUsageMb = request.MemoryUsageMb,
        Timestamp = request.Timestamp ?? DateTime.UtcNow
    };
    dbContext.ServerMetrics.Add(metric);
    await dbContext.SaveChangesAsync();

    return Results.Created($"/api/metrics/{metric.ServerName}", metric);
})
.AddEndpointFilter<ValidationFilter<CreateMetricRequest>>()
.AddEndpointFilter<ApiKeyFilter>();

app.MapGet("/api/metrics/{serverName}", async (string serverName, ServerMonitorDbContext dbContext) =>
{
    var metrics = await dbContext.ServerMetrics
        .Where(metric => metric.ServerName == serverName)
        .OrderByDescending(metric => metric.Timestamp)
        .Take(50)
        .ToListAsync();

    return Results.Ok(metrics);
});

app.MapPost("/api/logs", async (LogEntry logEntry, ServerMonitorDbContext dbContext) =>
{
    dbContext.LogEntries.Add(logEntry);
    await dbContext.SaveChangesAsync();
    return Results.Created($"/api/logs/{logEntry.Id}", logEntry);
});

app.MapGet("/api/logs", async (ApplicationLogLevel? level, ServerMonitorDbContext dbContext) =>
{
    var query = dbContext.LogEntries.AsQueryable();

    if (level.HasValue)
    {
        query = query.Where(logEntry => logEntry.LogLevel == level.Value);
    }

    var logs = await query
        .OrderByDescending(logEntry => logEntry.Timestamp)
        .ToListAsync();

    return Results.Ok(logs);
});

app.Run();
