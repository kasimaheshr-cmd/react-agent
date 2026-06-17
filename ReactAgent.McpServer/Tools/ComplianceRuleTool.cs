using ModelContextProtocol.Server;
using System.ComponentModel;

namespace ReactAgent.McpServer.Tools;

[McpServerToolType]
public class ComplianceRuleTool
{
    [McpServerTool, Description("Looks up a FINRA compliance rule by rule ID. Returns the rule description and key requirements an advisor must follow.")]
    public static string check_compliance_rule(
        [Description("The FINRA rule ID to look up e.g. 4511, 3110, 2111")]
        string rule_id)
    {
        return rule_id.Trim() switch
        {
            "4511" => """{"rule_id":"4511","name":"Books and Records","requirement":"Advisors must preserve all records of transactions for a minimum of 6 years. Electronic records must be stored in non-rewritable format."}""",
            "3110" => """{"rule_id":"3110","name":"Supervision","requirement":"Firms must establish a supervisory system to review all advisor activity. Written procedures required. Reviews must be documented."}""",
            "2111" => """{"rule_id":"2111","name":"Suitability","requirement":"Advisors must have reasonable basis to believe a recommendation is suitable for the client based on investment profile."}""",
            _ => """{"error":"Rule not found"}"""
        };
    }
}