using System.Text.Json;
using OllamaSharp;
using OllamaSharp.Models.Chat;
using ReactAgent.Core.Models;

namespace ReactAgent.Core.Agent;

public class ReActAgent : IAsyncDisposable
{
    private readonly OllamaApiClient _ollama;
    private readonly McpToolClient _mcpClient;
    private readonly TrajectoryLogger _logger;
    private readonly List<AgentMessage> _history;
    private const int MaxIterations = 6;

    public ReActAgent()
    {
        _ollama = new OllamaApiClient(new Uri("http://localhost:11434"));
        _ollama.SelectedModel = "llama3.2";
        _mcpClient = new McpToolClient();
        _logger = new TrajectoryLogger();
        _history = new List<AgentMessage>();
    }

    public async Task InitAsync()
    {
        // connect to MCP server and discover tools dynamically
        await _mcpClient.ConnectAsync();
        var tools = await _mcpClient.ListToolsAsync();

        Console.WriteLine("[MCP] Tools discovered:");
        tools.ForEach(t => Console.WriteLine($"  {t}"));

        // build system prompt from discovered tools — no hardcoding
        var toolList = string.Join("\n", tools.Select((t, i) => $"Tool {i + 1}: {t}"));

        _history.Add(new AgentMessage
        {
            Role = "system",
            Content = "You are a FINRA compliance assistant. You have access to these tools:\n\n" +
                      toolList + "\n\n" +
                      "RULES — FOLLOW EXACTLY:\n" +
                      "- Respond with ONLY a raw JSON object when calling a tool. No explanation, no narration.\n" +
                      "- Example of correct tool call response:\n" +
                      "  {\"tool\":\"get_advisor_activity\",\"parameters\":{\"advisor_id\":\"A12\",\"period\":\"this quarter\"}}\n" +
                      "- Example of correct final response:\n" +
                      "  FINAL: Client C99 holds AAPL. Advisor A12 bought AAPL. This raises Rule 2111 concerns.\n" +
                      "- NEVER write 'Calling Tool', 'I need to', 'Now I will' or any explanation.\n" +
                      "- NEVER invent data. ONLY use data returned in tool results.\n" +
                      "- If the query mentions a rule number, call check_compliance_rule.\n" +
                      "- If the query mentions an advisor, call get_advisor_activity.\n" +
                      "- If the query mentions a client, call get_client_portfolio.\n" +
                      "- Call tools ONE AT A TIME. One JSON object per response, nothing else.\n" +
                      "- Only respond with FINAL: after you have called ALL relevant tools.\n" +
                      "- FINAL: must always be followed by a complete answer, never just 'FINAL' alone."
        });
    }

    public async Task<string> RunAsync(string userQuery)
    {
        var trajectory = new AgentTrajectory { Query = userQuery };

        _history.Add(new AgentMessage { Role = "user", Content = userQuery });
        Console.WriteLine($"\n[USER] {userQuery}\n");

        for (int i = 0; i < MaxIterations; i++)
        {
            Console.WriteLine($"--- Iteration {i + 1} ---");

            await CompressHistoryIfNeededAsync();
            var response = await CallOllamaAsync();
            Console.WriteLine($"[REASON] {response}");

            var toolCall = TryParseToolCall(response);

            if (toolCall != null)
            {
                Console.WriteLine($"[ACT] Calling MCP tool: {toolCall.ToolName} with {JsonSerializer.Serialize(toolCall.Parameters)}");

                // call via MCP — no hardcoded tool references
                var toolResult = await _mcpClient.CallToolAsync(toolCall.ToolName, toolCall.Parameters);
                Console.WriteLine($"[OBSERVE] {toolResult}");

                trajectory.Steps.Add(new AgentStep
                {
                    Iteration = i + 1,
                    Reasoning = response,
                    ToolCalled = toolCall.ToolName,
                    ToolParameters = toolCall.Parameters,
                    ToolResult = toolResult
                });

                _history.Add(new AgentMessage { Role = "assistant", Content = response });
                _history.Add(new AgentMessage { Role = "tool", Content = $"Tool result: {toolResult}" });
            }
            else if (response.Contains("FINAL:") || !response.Trim().StartsWith("{"))
            {
                var answer = response.Replace("FINAL:", "").Trim();

                trajectory.Steps.Add(new AgentStep
                {
                    Iteration = i + 1,
                    Reasoning = response
                });
                trajectory.FinalAnswer = answer;
                trajectory.HitMaxIterations = false;

                await _logger.SaveAsync(trajectory);

                Console.WriteLine($"\n[ANSWER] {answer}");
                return answer;
            }
        }

        trajectory.HitMaxIterations = true;
        await _logger.SaveAsync(trajectory);
        return "Max iterations reached without a final answer.";
    }

    private async Task CompressHistoryIfNeededAsync()
    {
        // only keep last 10 messages + system prompt
        var nonSystemMessages = _history
            .Where(m => m.Role != "system")
            .ToList();

        if (nonSystemMessages.Count < 10) return;

        Console.WriteLine("[AGENT] Compressing history...");

        // ask LLM to summarize conversation so far
        var summaryPrompt = "Summarize this conversation in 3 sentences, " +
                            "keeping key facts: advisor IDs, rule numbers, findings.\n\n" +
                            string.Join("\n", nonSystemMessages
                                .Take(nonSystemMessages.Count - 4)
                                .Select(m => $"{m.Role}: {m.Content}"));

        var summaryMessages = new List<OllamaSharp.Models.Chat.Message>
{
    new() { Role = OllamaSharp.Models.Chat.ChatRole.User, Content = summaryPrompt }
};

        var summaryResult = "";
        await foreach (var chunk in _ollama.ChatAsync(
            new OllamaSharp.Models.Chat.ChatRequest { Messages = summaryMessages }))
            summaryResult += chunk?.Message?.Content ?? "";

        var summary = summaryResult.Trim();

        // replace old history with summary + keep last 4 messages
        var systemMessage = _history.First(m => m.Role == "system");
        var recentMessages = nonSystemMessages.TakeLast(4).ToList();

        _history.Clear();
        _history.Add(systemMessage);
        _history.Add(new AgentMessage
        {
            Role = "user",
            Content = $"[Previous conversation summary]: {summary}"
        });
        _history.AddRange(recentMessages);

        Console.WriteLine($"[AGENT] History compressed to {_history.Count} messages.");
    }

    private async Task<string> CallOllamaAsync()
    {
        var messages = _history.Select(m => new Message
        {
            Role = m.Role switch
            {
                "system" => ChatRole.System,
                "user" => ChatRole.User,
                "tool" => ChatRole.User,
                _ => ChatRole.Assistant
            },
            Content = m.Content
        }).ToList();

        var result = "";
        await foreach (var chunk in _ollama.ChatAsync(new ChatRequest { Messages = messages }))
        {
            result += chunk?.Message?.Content ?? "";
        }

        return result.Trim();
    }

    private ToolCall? TryParseToolCall(string response)
    {
        try
        {
            var trimmed = response.Trim();
            if (trimmed.StartsWith("FINAL:"))
                trimmed = trimmed.Replace("FINAL:", "").Trim();

            var start = trimmed.IndexOf('{');
            var end = trimmed.LastIndexOf('}');
            if (start == -1 || end == -1) return null;

            trimmed = trimmed[start..(end + 1)];

            var doc = JsonDocument.Parse(trimmed);
            var root = doc.RootElement;

            if (!root.TryGetProperty("tool", out var toolProp)) return null;

            var parameters = new Dictionary<string, string>();
            if (root.TryGetProperty("parameters", out var paramsProp))
            {
                foreach (var param in paramsProp.EnumerateObject())
                    parameters[param.Name] = param.Value.GetString() ?? "";
            }

            return new ToolCall
            {
                ToolName = toolProp.GetString() ?? "",
                Parameters = parameters
            };
        }
        catch { return null; }
    }

    public async ValueTask DisposeAsync()
    {
        await _mcpClient.DisposeAsync();
    }
}