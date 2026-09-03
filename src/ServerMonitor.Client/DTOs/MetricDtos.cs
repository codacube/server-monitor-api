namespace ServerMonitor.Client.DTOs;

public record MetricDto(int Id, string ServerName, double CpuUsagePercent, double MemoryUsageMb, DateTime Timestamp);
public record CreateMetricDto(string ServerName, double CpuUsagePercent, double MemoryUsageMb, DateTime? Timestamp = null);