namespace ServerMonitor.Data.Entities;

public class ServerMetric
{
    public int Id { get; set; }

    public string ServerName { get; set; } = string.Empty;

    public double CpuUsagePercent { get; set; }

    public double MemoryUsageMb { get; set; }

    public DateTime Timestamp { get; set; }
}
