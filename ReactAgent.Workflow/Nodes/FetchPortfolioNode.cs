using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol.Transport;
using ReactAgent.Workflow.Models;

namespace ReactAgent.Workflow.Nodes;

public class FetchPortfolioNode
{
    private readonly string _serverPath;

    public FetchPortfolioNode()
    {
        _serverPath = @"C:\Projects\react-agent\ReactAgent.McpServer\bin\Debug\net8.0\ReactAgent.McpServer.exe";
    }

    public async Task<WorkflowState> ExecuteAsync(WorkflowState state)
    {
        Console.WriteLine("[NODE] FetchPortfolioNode executing...");

        var clientId = ExtractClientId(state.Query);
        Console.WriteLine($"[NODE] FetchPortfolioNode → client: {clientId}");

        var transport = new StdioClientTransport(new StdioClientTransportOptions
        {
            Command = _serverPath,
            Name = "FinancialComplianceServer"
        });

        await using var client = await McpClientFactory.CreateAsync(transport);

        var result = await client.CallToolAsync(
            "get_client_portfolio",
            new Dictionary<string, object?> { { "client_id", clientId } }
        );

        state.PortfolioData = result.Content.FirstOrDefault()?.Text ?? """{"error":"No portfolio data"}""";
        state.ExecutedNodes.Add("FetchPortfolioNode");

        Console.WriteLine($"[NODE] FetchPortfolioNode → got: {state.PortfolioData}");
        return state;
    }

    private string ExtractClientId(string query)
    {
        var match = System.Text.RegularExpressions.Regex.Match(query, @"\b(C\d+)\b");
        return match.Success ? match.Value : "C99";
    }
}