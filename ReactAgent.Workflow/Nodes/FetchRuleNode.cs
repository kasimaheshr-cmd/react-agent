using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol.Transport;
using ReactAgent.Workflow.Models;
using System.Text.Json;

namespace ReactAgent.Workflow.Nodes;

public class FetchRuleNode
{
    private readonly string _serverPath;

    public FetchRuleNode()
    {
        _serverPath = @"C:\Projects\react-agent\ReactAgent.McpServer\bin\Debug\net8.0\ReactAgent.McpServer.exe";
    }

    public async Task<WorkflowState> ExecuteAsync(WorkflowState state)
    {
        Console.WriteLine("[NODE] FetchRuleNode executing...");

        // extract rule number from query
        var ruleId = ExtractRuleId(state.Query);
        Console.WriteLine($"[NODE] FetchRuleNode → looking up rule: {ruleId}");

        var transport = new StdioClientTransport(new StdioClientTransportOptions
        {
            Command = _serverPath,
            Name = "FinancialComplianceServer"
        });

        await using var client = await McpClientFactory.CreateAsync(transport);

        var result = await client.CallToolAsync(
            "check_compliance_rule",
            new Dictionary<string, object?> { { "rule_id", ruleId } }
        );

        state.RuleData = result.Content.FirstOrDefault()?.Text ?? """{"error":"No rule data"}""";
        state.ExecutedNodes.Add("FetchRuleNode");

        Console.WriteLine($"[NODE] FetchRuleNode → got: {state.RuleData}");
        return state;
    }

    private string ExtractRuleId(string query)
    {
        // extract 4-digit rule numbers e.g. 4511, 3110, 2111
        var match = System.Text.RegularExpressions.Regex.Match(query, @"\b(\d{4})\b");
        return match.Success ? match.Value : "4511";
    }
}