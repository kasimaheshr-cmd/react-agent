using ReactAgent.Core.Agent;

// --- Existing Week 5: ReAct Agent ---
Console.WriteLine("=== Week 5: ReAct Agent ===\n");
await using var agent = new ReActAgent();
await agent.InitAsync();

await agent.RunAsync(
    "Does client C99's portfolio have any positions that advisor A12 traded " +
    "this quarter, and does that raise any Rule 2111 suitability concerns?"
);

// --- Week 14: Semantic Kernel Agent ---
Console.WriteLine("\n=== Week 14: Semantic Kernel Agent ===\n");
var skAgent = new SemanticKernelAgent();

Console.WriteLine("--- SK Query 1: rule lookup ---");
var r1 = await skAgent.RunAsync("Check compliance rule 4511");
Console.WriteLine(r1);

Console.WriteLine("\n--- SK Query 2: advisor activity ---");
var r2 = await skAgent.RunAsync("Get advisor activity for A12");
Console.WriteLine(r2);

Console.WriteLine("\n--- SK Query 3: combined ---");
var r3 = await skAgent.RunAsync(
    "Did advisor A12 make any trades that might violate Rule 2111 for client C99?"
);
Console.WriteLine(r3);

Console.WriteLine("\n=== Week 14 Complete ===");