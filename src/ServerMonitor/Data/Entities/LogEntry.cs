using System.Text.Json.Serialization;

namespace ServerMonitor.Data.Entities;

public class LogEntry
{
    public int Id { get; set; }

    public string ApplicationName { get; set; } = string.Empty;

    public LogLevel LogLevel { get; set; }

    public string Message { get; set; } = string.Empty;

    public DateTime Timestamp { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum LogLevel
{
    Info,
    Warning,
    Error
}
