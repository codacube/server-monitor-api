using System;
using Avalonia;

namespace ServerMonitor.Client;

internal class Program
{
    // Initialization code. Do not use any Avalonia, third-party APIs, or 
    // SynchronizationContext-reliant code before BuildAvaloniaApp is called.
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    // Avalonia configuration, also used by the visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}