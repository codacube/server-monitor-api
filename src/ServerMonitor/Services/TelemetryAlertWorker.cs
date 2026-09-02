using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using ServerMonitor.Data;
using ServerMonitor.Data.Entities;

namespace ServerMonitor.Services;

public sealed class TelemetryAlertWorker(
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    ILogger<TelemetryAlertWorker> logger) : BackgroundService
{
    private static readonly TimeSpan CycleInterval = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan CpuSampleInterval = TimeSpan.FromSeconds(1);
    private const double DefaultCpuThresholdPercent = 80;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CollectTelemetryAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Telemetry collection failed.");
            }

            try
            {
                await Task.Delay(CycleInterval, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }

    private async Task CollectTelemetryAsync(CancellationToken stoppingToken)
    {
        using var process = Process.GetCurrentProcess();
        var initialCpuTime = process.TotalProcessorTime;
        var initialTimestamp = Stopwatch.GetTimestamp();

        await Task.Delay(CpuSampleInterval, stoppingToken);

        process.Refresh();
        var elapsedSeconds = Stopwatch.GetElapsedTime(initialTimestamp).TotalSeconds;
        var cpuSeconds = (process.TotalProcessorTime - initialCpuTime).TotalSeconds;
        var cpuUsagePercent = cpuSeconds / (elapsedSeconds * Environment.ProcessorCount) * 100;
        var timestamp = DateTime.UtcNow;
        var serverName = Environment.MachineName;
        var threshold = configuration.GetValue<double?>("Telemetry:CpuThresholdPercent")
            ?? DefaultCpuThresholdPercent;

        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ServerMonitorDbContext>();

        dbContext.ServerMetrics.Add(new ServerMetric
        {
            ServerName = serverName,
            CpuUsagePercent = cpuUsagePercent,
            Timestamp = timestamp
        });

        if (cpuUsagePercent >= threshold)
        {
            logger.LogWarning(
                "CPU usage for {ServerName} is {CpuUsagePercent:F1}%, above the {Threshold:F1}% threshold.",
                serverName,
                cpuUsagePercent,
                threshold);

            dbContext.LogEntries.Add(new LogEntry
            {
                ApplicationName = "ServerMonitor",
                LogLevel = ServerMonitor.Data.Entities.LogLevel.Warning,
                Message = $"CPU usage reached {cpuUsagePercent:F1}%, above the {threshold:F1}% threshold.",
                Timestamp = timestamp
            });
        }

        await dbContext.SaveChangesAsync(stoppingToken);
    }
}
