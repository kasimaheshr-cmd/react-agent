namespace ReactAgent.Workflow.Models;

public class WorkflowState
{
    // input
    public string Query { get; set; } = string.Empty;

    // set by ClassifyNode — drives routing decisions
    public string QueryType { get; set; } = string.Empty;  // "rule_lookup" | "advisor_review" | "full_audit"

    // populated by fetch nodes as they execute
    public string? RuleData { get; set; }
    public string? AdvisorData { get; set; }
    public string? PortfolioData { get; set; }

    // set by SynthesizeNode — the final answer
    public string? FinalAnswer { get; set; }

    // audit trail — every node appends its name here
    public List<string> ExecutedNodes { get; set; } = new();

    // error handling — any node can set this to short-circuit
    public string? Error { get; set; }
}