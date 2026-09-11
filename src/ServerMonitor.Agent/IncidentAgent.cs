namespace ServerMonitor.Agent;

using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;

public class IncidentAgent
{
    private readonly IChatClient _chatClient;

    public IncidentAgent(string ollamaEndpoint = "http://localhost:11434", string modelId = "llama3.2")
    {
        // 1. Create the base Ollama client
        IChatClient baseClient = new OllamaChatClient(new Uri(ollamaEndpoint), modelId);

        // 2. Directly wrap it with function-invocation capabilities
        _chatClient = new FunctionInvokingChatClient(baseClient);
    }

    [Description("Fetches top CPU and memory-consuming processes for a specified server.")]
    public static string GetServerProcesses([Description("The hostname or ID of the server, e.g., 'groot'")] string serverName)
    {
        Console.WriteLine($"\n[TOOL EXECUTED] Inspected running processes on server '{serverName}'...");

        return $"""
        Server ID: {serverName}
        Active Top Processes:
          - PID 1024 | dotnet (ServerMonitor.API) | CPU: 92.4% | RAM: 1840 MB
          - PID 2048 | podman-machine            | CPU: 4.1%  | RAM: 512 MB
          - PID 3096 | sqlite3                   | CPU: 0.8%  | RAM: 48 MB
        Diagnostic Context: Process PID 1024 is consuming excessive CPU cycles.
        """;
    }

    public async Task<string> RunDiagnosticsAsync(string serverName, string anomalyMessage)
    {
        var options = new ChatOptions
        {
            Tools = [AIFunctionFactory.Create(GetServerProcesses)]
        };

        var userPrompt = $"Incident Report: {anomalyMessage} on server '{serverName}'. " +
                         $"Investigate the cause using available tools and summarize your findings.";

        var response = await _chatClient.CompleteAsync(userPrompt, options);
        return response.Message.Text ?? "No diagnosis generated.";
    }
}