using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol.Transport;
using OllamaSharp;
using OllamaSharp.Models.Chat;

namespace ReactAgent.MultiAgent.Agents;

public class ComplianceSpecialist
{
    private readonly OllamaApiClient _ollama;
    private readonly string _serverPath;

    public ComplianceSpecialist()
    {
        _ollama = new OllamaApiClient(new Uri("http://localhost:11434"));
        _ollama.SelectedModel = "llama3.2";
        _serverPath = @"C:\Projects\react-agent\ReactAgent.McpServer\bin\Debug\net8.0\ReactAgent.McpServer.exe";
    }

    public async Task<string> ExecuteAsync(string subQuery)
    {
        Console.WriteLine($"\n[COMPLIANCE SPECIALIST] Query: {subQuery}");

        // extract rule id from sub query
        var ruleId = ExtractRuleId(subQuery);

        // fetch rule data via MCP
        var ruleData = await FetchRuleAsync(ruleId);
        Console.WriteLine($"[COMPLIANCE SPECIALIST] Fetched rule: {ruleId}");

        // synthesize answer scoped to compliance domain only
        var messages = new List<Message>
        {
            new()
            {
                Role    = ChatRole.System,
                Content = "You are a FINRA compliance specialist. " +
                          "You only answer questions about compliance rules. " +
                          "Use only the provided rule data. Be concise and specific."
            },
            new()
            {
                Role    = ChatRole.User,
                Content = $"Query: {subQuery}\n\nRule Data: {ruleData}\n\nAnswer:"
            }
        };

        var result = "";
        await foreach (var chunk in _ollama.ChatAsync(new ChatRequest { Messages = messages }))
            result += chunk?.Message?.Content ?? "";

        Console.WriteLine($"[COMPLIANCE SPECIALIST] Done.");
        return result.Trim();
    }

    private async Task<string> FetchRuleAsync(string ruleId)
    {
        var transport = new StdioClientTransport(new StdioClientTransportOptions
        {
            Command = _serverPath,
            Name = "ComplianceServer"
        });

        await using var client = await McpClientFactory.CreateAsync(transport);
        var result = await client.CallToolAsync(
            "check_compliance_rule",
            new Dictionary<string, object?> { { "rule_id", ruleId } }
        );

        return result.Content.FirstOrDefault()?.Text ?? """{"error":"No rule data"}""";
    }

    private string ExtractRuleId(string query)
    {
        var match = System.Text.RegularExpressions.Regex.Match(query, @"\b(\d{4})\b");
        return match.Success ? match.Value : "2111";
    }
}