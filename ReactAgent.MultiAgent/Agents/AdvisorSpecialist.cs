using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol.Transport;
using OllamaSharp;
using OllamaSharp.Models.Chat;

namespace ReactAgent.MultiAgent.Agents;

public class AdvisorSpecialist
{
    private readonly OllamaApiClient _ollama;
    private readonly string _serverPath;

    public AdvisorSpecialist()
    {
        _ollama = new OllamaApiClient(new Uri("http://localhost:11434"));
        _ollama.SelectedModel = "llama3.2";
        _serverPath = @"C:\Projects\react-agent\ReactAgent.McpServer\bin\Debug\net8.0\ReactAgent.McpServer.exe";
    }

    public async Task<string> ExecuteAsync(string subQuery)
    {
        Console.WriteLine($"\n[ADVISOR SPECIALIST] Query: {subQuery}");

        // extract identifiers
        var advisorId = ExtractAdvisorId(subQuery);
        var clientId = ExtractClientId(subQuery);

        // fetch both data sources in parallel — specialist owns its own tools
        Console.WriteLine($"[ADVISOR SPECIALIST] Fetching advisor: {advisorId}, client: {clientId}");
        var (advisorData, portfolioData) = await FetchDataAsync(advisorId, clientId);

        // synthesize answer scoped to advisor domain only
        var messages = new List<Message>
        {
            new()
            {
                Role    = ChatRole.System,
                Content = "You are an advisor activity specialist. " +
                          "You only answer questions about advisor trades and client portfolios. " +
                          "Use only the provided data. Be concise and specific."
            },
            new()
            {
                Role    = ChatRole.User,
                Content = $"Query: {subQuery}\n\n" +
                          $"Advisor Activity: {advisorData}\n\n" +
                          $"Client Portfolio: {portfolioData}\n\n" +
                          $"Answer:"
            }
        };

        var result = "";
        await foreach (var chunk in _ollama.ChatAsync(new ChatRequest { Messages = messages }))
            result += chunk?.Message?.Content ?? "";

        Console.WriteLine($"[ADVISOR SPECIALIST] Done.");
        return result.Trim();
    }

    private async Task<(string advisorData, string portfolioData)> FetchDataAsync(
        string advisorId, string clientId)
    {
        // fetch both in parallel — two MCP connections simultaneously
        var advisorTask = FetchAdvisorActivityAsync(advisorId);
        var portfolioTask = FetchPortfolioAsync(clientId);
        await Task.WhenAll(advisorTask, portfolioTask);
        return (advisorTask.Result, portfolioTask.Result);
    }

    private async Task<string> FetchAdvisorActivityAsync(string advisorId)
    {
        var transport = new StdioClientTransport(new StdioClientTransportOptions
        {
            Command = _serverPath,
            Name = "AdvisorServer"
        });

        await using var client = await McpClientFactory.CreateAsync(transport);
        var result = await client.CallToolAsync(
            "get_advisor_activity",
            new Dictionary<string, object?>
            {
                { "advisor_id", advisorId },
                { "period",     "this quarter" }
            }
        );

        return result.Content.FirstOrDefault()?.Text ?? """{"error":"No advisor data"}""";
    }

    private async Task<string> FetchPortfolioAsync(string clientId)
    {
        var transport = new StdioClientTransport(new StdioClientTransportOptions
        {
            Command = _serverPath,
            Name = "PortfolioServer"
        });

        await using var client = await McpClientFactory.CreateAsync(transport);
        var result = await client.CallToolAsync(
            "get_client_portfolio",
            new Dictionary<string, object?> { { "client_id", clientId } }
        );

        return result.Content.FirstOrDefault()?.Text ?? """{"error":"No portfolio data"}""";
    }

    private string ExtractAdvisorId(string query)
    {
        var match = System.Text.RegularExpressions.Regex.Match(query, @"\b(A\d+)\b");
        return match.Success ? match.Value : "A12";
    }

    private string ExtractClientId(string query)
    {
        var match = System.Text.RegularExpressions.Regex.Match(query, @"\b(C\d+)\b");
        return match.Success ? match.Value : "C99";
    }
}