using OllamaSharp;
using OllamaSharp.Models.Chat;
using ReactAgent.Workflow.Models;

namespace ReactAgent.Workflow.Nodes;

public class SynthesizeNode
{
    private readonly OllamaApiClient _ollama;

    public SynthesizeNode()
    {
        _ollama = new OllamaApiClient(new Uri("http://localhost:11434"));
        _ollama.SelectedModel = "llama3.2";
    }

    public async Task<WorkflowState> ExecuteAsync(WorkflowState state)
    {
        Console.WriteLine("[NODE] SynthesizeNode executing...");

        // build context from whatever data was fetched
        var context = BuildContext(state);

        var messages = new List<Message>
        {
            new()
            {
                Role    = ChatRole.System,
                Content = "You are a FINRA compliance assistant. " +
                          "Answer the query using ONLY the data provided. " +
                          "Never invent data. Be specific and concise."
            },
            new()
            {
                Role    = ChatRole.User,
                Content = $"Query: {state.Query}\n\nData:\n{context}\n\nProvide a clear compliance answer."
            }
        };

        var result = "";
        await foreach (var chunk in _ollama.ChatAsync(new ChatRequest { Messages = messages }))
            result += chunk?.Message?.Content ?? "";

        state.FinalAnswer = result.Trim();
        state.ExecutedNodes.Add("SynthesizeNode");

        Console.WriteLine($"[NODE] SynthesizeNode → answer ready");
        return state;
    }

    private string BuildContext(WorkflowState state)
    {
        var parts = new List<string>();

        if (state.RuleData != null)
            parts.Add($"Compliance Rule:\n{state.RuleData}");

        if (state.AdvisorData != null)
            parts.Add($"Advisor Activity:\n{state.AdvisorData}");

        if (state.PortfolioData != null)
            parts.Add($"Client Portfolio:\n{state.PortfolioData}");

        return string.Join("\n\n", parts);
    }
}