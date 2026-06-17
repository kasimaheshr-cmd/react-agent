using ReactAgent.Workflow.Models;
using ReactAgent.Workflow.Nodes;

namespace ReactAgent.Workflow.Engine;

public class WorkflowEngine
{
    private readonly ClassifyNode _classifyNode;
    private readonly FetchRuleNode _fetchRuleNode;
    private readonly FetchAdvisorNode _fetchAdvisorNode;
    private readonly FetchPortfolioNode _fetchPortfolioNode;
    private readonly SynthesizeNode _synthesizeNode;

    public WorkflowEngine()
    {
        _classifyNode = new ClassifyNode();
        _fetchRuleNode = new FetchRuleNode();
        _fetchAdvisorNode = new FetchAdvisorNode();
        _fetchPortfolioNode = new FetchPortfolioNode();
        _synthesizeNode = new SynthesizeNode();
    }

    public async Task<WorkflowState> RunAsync(string query)
    {
        var state = new WorkflowState { Query = query };

        Console.WriteLine($"\n[WORKFLOW] Starting for: {query}");
        Console.WriteLine("─────────────────────────────────────");

        // Step 1 — always classify first
        state = await _classifyNode.ExecuteAsync(state);

        // Step 2 — conditional edges based on QueryType
        state = state.QueryType switch
        {
            "rule_lookup" => await RunRuleLookupPath(state),
            "advisor_review" => await RunAdvisorReviewPath(state),
            "full_audit" => await RunFullAuditPath(state),
            _ => await RunRuleLookupPath(state)   // default fallback
        };

        // Step 3 — always synthesize last
        state = await _synthesizeNode.ExecuteAsync(state);

        Console.WriteLine("─────────────────────────────────────");
        Console.WriteLine($"[WORKFLOW] Nodes executed: {string.Join(" → ", state.ExecutedNodes)}");
        Console.WriteLine($"[WORKFLOW] Answer: {state.FinalAnswer}");

        return state;
    }

    // path 1 — rule lookup only
    private async Task<WorkflowState> RunRuleLookupPath(WorkflowState state)
    {
        Console.WriteLine("[WORKFLOW] Path: rule_lookup");
        state = await _fetchRuleNode.ExecuteAsync(state);
        return state;
    }

    // path 2 — advisor review only
    private async Task<WorkflowState> RunAdvisorReviewPath(WorkflowState state)
    {
        Console.WriteLine("[WORKFLOW] Path: advisor_review");
        state = await _fetchAdvisorNode.ExecuteAsync(state);
        return state;
    }

    // path 3 — full audit: all three fetch nodes
    private async Task<WorkflowState> RunFullAuditPath(WorkflowState state)
    {
        Console.WriteLine("[WORKFLOW] Path: full_audit");
        state = await _fetchRuleNode.ExecuteAsync(state);
        state = await _fetchAdvisorNode.ExecuteAsync(state);
        state = await _fetchPortfolioNode.ExecuteAsync(state);
        return state;
    }
}