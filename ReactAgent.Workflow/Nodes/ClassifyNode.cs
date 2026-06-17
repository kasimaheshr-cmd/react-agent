using OllamaSharp;
using OllamaSharp.Models.Chat;
using ReactAgent.Workflow.Models;

namespace ReactAgent.Workflow.Nodes;

public class ClassifyNode
{
    private readonly OllamaApiClient _ollama;

    public ClassifyNode()
    {
        _ollama = new OllamaApiClient(new Uri("http://localhost:11434"));
        _ollama.SelectedModel = "llama3.2";
    }

    public async Task<WorkflowState> ExecuteAsync(WorkflowState state)
    {
        Console.WriteLine("[NODE] ClassifyNode executing...");

        var prompt = $"""
            Classify this compliance query into exactly one category.
            Query: {state.Query}

            Categories:
            - rule_lookup: user wants to know about a specific FINRA rule
            - advisor_review: user wants to see advisor trade activity only
            - full_audit: user wants to cross-reference advisor trades, client portfolio, and compliance rules together

            Respond with ONLY one of these exact words: rule_lookup, advisor_review, full_audit
            """;

        var messages = new List<Message>
        {
            new() { Role = ChatRole.User, Content = prompt }
        };

        var result = "";
        await foreach (var chunk in _ollama.ChatAsync(new ChatRequest { Messages = messages }))
            result += chunk?.Message?.Content ?? "";

        // extract just the category word
        var classified = result.Trim().ToLower();
        if (classified.Contains("full_audit")) classified = "full_audit";
        else if (classified.Contains("advisor_review")) classified = "advisor_review";
        else classified = "rule_lookup";

        state.QueryType = classified;
        state.ExecutedNodes.Add("ClassifyNode");

        Console.WriteLine($"[NODE] ClassifyNode → QueryType: {state.QueryType}");
        return state;
    }
}