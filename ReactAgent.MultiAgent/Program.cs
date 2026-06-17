using ReactAgent.MultiAgent.Agents;

var orchestrator = new OrchestratorAgent();

Console.WriteLine("=== Query 1 — single domain ===");
await orchestrator.RunAsync("What are the requirements of FINRA Rule 2111?");

Console.WriteLine("\n\n=== Query 2 — full audit ===");
await orchestrator.RunAsync(
    "Did advisor A12 make any trades this quarter that might violate " +
    "Rule 2111 for client C99?"
);