using System;
using System.Threading.Tasks;
using ServerMonitor.Agent;

Console.WriteLine("Initializing AI Agent connection to Ollama...");

try
{
    var agent = new IncidentAgent();

    string result = await agent.RunDiagnosticsAsync(
        serverName: "prod-web-01",
        anomalyMessage: "CPU usage exceeded 90% threshold for 3 consecutive ticks"
    );

    Console.WriteLine("\n=== AGENT DIAGNOSTIC SUMMARY ===");
    Console.WriteLine(result);
}
catch (Exception ex)
{
    Console.WriteLine($"\nAgent Failure: {ex.Message}");
}