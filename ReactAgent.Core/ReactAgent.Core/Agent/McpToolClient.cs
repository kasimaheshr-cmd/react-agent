using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol.Transport;
using System.Text.Json;

namespace ReactAgent.Core.Agent;

public class McpToolClient : IAsyncDisposable
{
    private IMcpClient? _client;

    public async Task ConnectAsync()
    {
        var serverPath = @"C:\Projects\react-agent\ReactAgent.McpServer\bin\Debug\net8.0\ReactAgent.McpServer.exe";

        Console.WriteLine($"[MCP] Connecting to server at: {serverPath}");

        if (!File.Exists(serverPath))
        {
            Console.WriteLine("[MCP] ERROR: Server executable not found. Build ReactAgent.McpServer first.");
            throw new FileNotFoundException("MCP Server not found.", serverPath);
        }

        var transport = new StdioClientTransport(new StdioClientTransportOptions
        {
            Command = serverPath,
            Name = "FinancialComplianceServer"
        });

        _client = await McpClientFactory.CreateAsync(transport);
        Console.WriteLine("[MCP] Connected.");
    }

    public async Task<List<string>> ListToolsAsync()
    {
        if (_client == null) throw new InvalidOperationException("Not connected.");
        var tools = await _client.ListToolsAsync();
        return tools.Select(t => $"Name: {t.Name} — {t.Description}").ToList();
    }

    public async Task<string> CallToolAsync(string toolName, Dictionary<string, string> parameters)
    {
        if (_client == null) throw new InvalidOperationException("Not connected.");

        // convert to IReadOnlyDictionary<string, object?>
        var jsonParams = parameters.ToDictionary(
            k => k.Key,
            v => (object?)v.Value
        );

        var result = await _client.CallToolAsync(toolName, jsonParams);
        return result.Content.FirstOrDefault()?.Text ?? """{"error":"No result"}""";
    }

    public async ValueTask DisposeAsync()
    {
        if (_client != null)
            await _client.DisposeAsync();
    }
}