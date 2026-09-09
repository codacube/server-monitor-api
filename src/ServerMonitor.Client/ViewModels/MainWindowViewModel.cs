namespace ServerMonitor.Client.ViewModels;

using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using ServerMonitor.Client.DTOs;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly HttpClient _http = new()
    {
        DefaultRequestHeaders = { { "X-API-KEY", "Telemetry-Secret-Key-98765" } }
    };

    private readonly ObservableCollection<DateTimePoint> _cpuValues = new();
    private readonly DispatcherTimer _pollTimer;

    [ObservableProperty] private string _targetServer = Environment.MachineName;
    [ObservableProperty] private int _serverPort = 5000;
    [ObservableProperty] private double _inputCpu = 85.5;
    [ObservableProperty] private double _inputMemory = 8192.0;
    [ObservableProperty] private string _statusMessage = "Ready";

    public ISeries[] CpuSeries { get; }
    public Axis[] XAxes { get; } = new Axis[]
    {
        new DateTimeAxis(TimeSpan.FromSeconds(30), date => date.ToString("HH:mm:ss"))
    };

    public MainWindowViewModel()
    {
        CpuSeries = new ISeries[]
        {
            new LineSeries<DateTimePoint>
            {
                Values = _cpuValues,
                Name = "CPU Usage (%)",
                Fill = null,
                GeometrySize = 8
            }
        };

        // Initial fetch
        _ = FetchMetricsAsync();

        // Update graph every 3 seconds
        _pollTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(20)
        };
        _pollTimer.Tick += async (sender, e) => await FetchMetricsAsync();
        _pollTimer.Start();
    }

    [RelayCommand]
    public async Task FetchMetricsAsync()
    {
        try
        {
            StatusMessage = $"Fetching telemetry for '{TargetServer}'...";
            var response = await _http.GetFromJsonAsync<MetricDto[]>(BuildApiUri($"api/metrics/{TargetServer}"));

            _cpuValues.Clear();
            if (response is not null)
            {
                foreach (var m in response)
                {
                    _cpuValues.Add(new DateTimePoint(m.Timestamp.ToLocalTime(), m.CpuUsagePercent));
                }
                StatusMessage = $"Loaded {response.Length} metric data points.";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Fetch error: {ex.Message}";
        }
    }

    [RelayCommand]
    public async Task SendMetricAsync()
    {
        try
        {
            StatusMessage = "Posting telemetry...";
            var payload = new CreateMetricDto(TargetServer, InputCpu, InputMemory, DateTime.UtcNow);
            var response = await _http.PostAsJsonAsync(BuildApiUri("api/metrics"), payload);

            if (response.IsSuccessStatusCode)
            {
                StatusMessage = "Metric posted successfully!";
                await FetchMetricsAsync();
            }
            else
            {
                StatusMessage = $"HTTP Error: {response.StatusCode}";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Send error: {ex.Message}";
        }
    }

    private Uri BuildApiUri(string path)
    {
        return new Uri($"http://localhost:{ServerPort}/{path}");
    }
}