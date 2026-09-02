namespace ServerMonitor.DTOs;

public record CreateMetricRequest(
    string ServerName,
    double CpuUsagePercent,
    double MemoryUsageMb,
    DateTime? Timestamp
);