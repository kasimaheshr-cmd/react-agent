using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol.Transport;
using ReactAgent.Workflow.Models;

namespace ReactAgent.Workflow.Nodes;

public class FetchAdvisorNode
{
    private readonly string _serverPath;

    public FetchAdvisorNode()
    {
        _serverPath = @"C:\Projects\react-agent\ReactAgent.McpServer\bin\Debug\net8.0\ReactAgent.McpServer.exe";
    }

    public async Task<WorkflowState> ExecuteAsync(WorkflowState state)
    {
        Console.WriteLine("[NODE] FetchAdvisorNode executing...");

        var advisorId = ExtractAdvisorId(state.Query);
        var period = ExtractPeriod(state.Query);
        Console.WriteLine($"[NODE] FetchAdvisorNode → advisor: {advisorId}, period: {period}");

        var transport = new StdioClientTransport(new StdioClientTransportOptions
        {
            Command = _serverPath,
            Name = "FinancialComplianceServer"
        });

        await using var client = await McpClientFactory.CreateAsync(transport);

        var result = await client.CallToolAsync(
            "get_advisor_activity",
            new Dictionary<string, object?>
            {
                { "advisor_id", advisorId },
                { "period",     period    }
            }
        );

        state.AdvisorData = result.Content.FirstOrDefault()?.Text ?? """{"error":"No advisor data"}""";
        state.ExecutedNodes.Add("FetchAdvisorNode");

        Console.WriteLine($"[NODE] FetchAdvisorNode → got: {state.AdvisorData}");
        return state;
    }

    private string ExtractAdvisorId(string query)
    {
        var match = System.Text.RegularExpressions.Regex.Match(query, @"\b(A\d+)\b");
        return match.Success ? match.Value : "A12";
    }

    private string ExtractPeriod(string query)
    {
        if (query.Contains("this quarter")) return "this quarter";
        if (query.Contains("last month")) return "last month";
        if (query.Contains("this year")) return "this year";
        return "this quarter";
    }
}