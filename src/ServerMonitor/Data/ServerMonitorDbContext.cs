using Microsoft.EntityFrameworkCore;
using ServerMonitor.Data.Entities;

namespace ServerMonitor.Data;

public class ServerMonitorDbContext(DbContextOptions<ServerMonitorDbContext> options) : DbContext(options)
{
    public DbSet<ServerMetric> ServerMetrics => Set<ServerMetric>();

    public DbSet<LogEntry> LogEntries => Set<LogEntry>();
}
