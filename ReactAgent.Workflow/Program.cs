using ReactAgent.Workflow.Engine;

var engine = new WorkflowEngine();

Console.WriteLine("=== Query 1 — rule lookup ===");
await engine.RunAsync("What are the requirements of FINRA Rule 4511?");

Console.WriteLine("\n=== Query 2 — advisor review ===");
await engine.RunAsync("What trades has advisor A12 made this quarter?");

Console.WriteLine("\n=== Query 3 — full audit ===");
await engine.RunAsync(
    "Did advisor A12 make any trades this quarter that might violate " +
    "Rule 2111 for client C99?"
);