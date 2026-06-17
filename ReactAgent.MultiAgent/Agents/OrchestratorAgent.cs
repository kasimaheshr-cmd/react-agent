using OllamaSharp;
using OllamaSharp.Models.Chat;

namespace ReactAgent.MultiAgent.Agents;

public class OrchestratorAgent
{
    private readonly OllamaApiClient _ollama;
    private readonly ComplianceSpecialist _complianceSpecialist;
    private readonly AdvisorSpecialist _advisorSpecialist;

    public OrchestratorAgent()
    {
        _ollama = new OllamaApiClient(new Uri("http://localhost:11434"));
        _ollama.SelectedModel = "llama3.2";
        _complianceSpecialist = new ComplianceSpecialist();
        _advisorSpecialist = new AdvisorSpecialist();
    }

    public async Task<string> RunAsync(string userQuery)
    {
        Console.WriteLine($"\n[ORCHESTRATOR] Query: {userQuery}");
        Console.WriteLine("─────────────────────────────────────");

        // Step 1 — decompose query into specialist sub-queries
        var (complianceSubQuery, advisorSubQuery) = await DecomposeQueryAsync(userQuery);
        Console.WriteLine($"\n[ORCHESTRATOR] Compliance sub-query: {complianceSubQuery}");
        Console.WriteLine($"[ORCHESTRATOR] Advisor sub-query:     {advisorSubQuery}");

        // Step 2 — delegate to specialists in parallel
        Console.WriteLine("\n[ORCHESTRATOR] Delegating to specialists in parallel...");
        var complianceTask = _complianceSpecialist.ExecuteAsync(complianceSubQuery);
        var advisorTask = _advisorSpecialist.ExecuteAsync(advisorSubQuery);
        await Task.WhenAll(complianceTask, advisorTask);

        var complianceAnswer = complianceTask.Result;
        var advisorAnswer = advisorTask.Result;

        Console.WriteLine($"\n[ORCHESTRATOR] Compliance answer received.");
        Console.WriteLine($"[ORCHESTRATOR] Advisor answer received.");

        // Step 3 — synthesize both specialist answers into final answer
        var finalAnswer = await SynthesizeAsync(userQuery, complianceAnswer, advisorAnswer);

        Console.WriteLine("─────────────────────────────────────");
        Console.WriteLine($"[ORCHESTRATOR] Final answer:\n{finalAnswer}");

        return finalAnswer;
    }

    private async Task<(string complianceSubQuery, string advisorSubQuery)> DecomposeQueryAsync(
        string userQuery)
    {
        Console.WriteLine("[ORCHESTRATOR] Decomposing query...");

        var messages = new List<Message>
        {
            new()
            {
                Role    = ChatRole.System,
                Content = "You are an orchestrator that decomposes compliance queries into specialist sub-queries.\n" +
                          "Given a user query, produce exactly two sub-queries:\n" +
                          "1. COMPLIANCE: focused on the specific FINRA rule requirements\n" +
                          "2. ADVISOR: focused on the advisor trades and client portfolio\n\n" +
                          "Respond in this exact format, nothing else:\n" +
                          "COMPLIANCE: <sub-query here>\n" +
                          "ADVISOR: <sub-query here>"
            },
            new()
            {
                Role    = ChatRole.User,
                Content = $"Decompose this query: {userQuery}"
            }
        };

        var result = "";
        await foreach (var chunk in _ollama.ChatAsync(new ChatRequest { Messages = messages }))
            result += chunk?.Message?.Content ?? "";

        return ParseSubQueries(result.Trim());
    }

    private (string compliance, string advisor) ParseSubQueries(string response)
    {
        var compliance = "What are the requirements of FINRA Rule 2111?";
        var advisor = "What trades has advisor A12 made this quarter for client C99?";

        var lines = response.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            if (line.StartsWith("COMPLIANCE:", StringComparison.OrdinalIgnoreCase))
                compliance = line.Replace("COMPLIANCE:", "").Trim();
            else if (line.StartsWith("ADVISOR:", StringComparison.OrdinalIgnoreCase))
                advisor = line.Replace("ADVISOR:", "").Trim();
        }

        return (compliance, advisor);
    }

    private async Task<string> SynthesizeAsync(
        string originalQuery, string complianceAnswer, string advisorAnswer)
    {
        Console.WriteLine("\n[ORCHESTRATOR] Synthesizing final answer...");

        var messages = new List<Message>
        {
            new()
            {
                Role    = ChatRole.System,
                Content = "You are a senior compliance officer. " +
                          "Synthesize the specialist reports into a final compliance determination. " +
                          "Be specific, reference the rule, reference the trades. " +
                          "State clearly whether a violation occurred or may have occurred."
            },
            new()
            {
                Role    = ChatRole.User,
                Content = $"Original query: {originalQuery}\n\n" +
                          $"Compliance Specialist Report:\n{complianceAnswer}\n\n" +
                          $"Advisor Specialist Report:\n{advisorAnswer}\n\n" +
                          $"Final compliance determination:"
            }
        };

        var result = "";
        await foreach (var chunk in _ollama.ChatAsync(new ChatRequest { Messages = messages }))
            result += chunk?.Message?.Content ?? "";

        return result.Trim();
    }
}