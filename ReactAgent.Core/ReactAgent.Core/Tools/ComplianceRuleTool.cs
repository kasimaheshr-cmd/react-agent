namespace ReactAgent.Core.Tools;

public class ComplianceRuleTool
{
    public string Name => "check_compliance_rule";

    public string Description =>
        "Looks up a FINRA compliance rule by rule ID. " +
        "Returns the rule description and key requirements an advisor must follow.";

    public Dictionary<string, string> Parameters => new()
    {
        { "rule_id", "The FINRA rule ID to look up e.g. 4511, 3110, 2111" }
    };

    public string Execute(Dictionary<string, string> parameters)
    {
        var ruleId = parameters.GetValueOrDefault("rule_id", "").Trim();

        return ruleId switch
        {
            "4511" => """{"rule_id":"4511","name":"Books and Records","requirement":"Advisors must preserve all records of transactions for a minimum of 6 years. Electronic records must be stored in non-rewritable format."}""",
            "3110" => """{"rule_id":"3110","name":"Supervision","requirement":"Firms must establish a supervisory system to review all advisor activity. Written procedures required. Reviews must be documented."}""",
            "2111" => """{"rule_id":"2111","name":"Suitability","requirement":"Advisors must have reasonable basis to believe a recommendation is suitable for the client based on investment profile."}""",
            _ => """{"error":"Rule not found","rule_id":"unknown"}"""
        };
    }
}