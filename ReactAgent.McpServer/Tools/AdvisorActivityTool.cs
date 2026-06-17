using ModelContextProtocol.Server;
using System.ComponentModel;

namespace ReactAgent.McpServer.Tools;

[McpServerToolType]
public class AdvisorActivityTool
{
    [McpServerTool, Description("Retrieves recent trade activity for a specific advisor. Returns a list of trades including date, security, action, and amount. Use this when asked about what an advisor has traded or done recently.")]
    public static string get_advisor_activity(
        [Description("The advisor ID to look up e.g. A12, A99")]
        string advisor_id,
        [Description("Time period to search e.g. this quarter, last month, 2025")]
        string period)
    {
        return advisor_id.ToUpper() switch
        {
            "A12" => """
                {
                  "advisor_id": "A12",
                  "trades": [
                    {"date":"2025-01-15","security":"AAPL","action":"BUY","amount":50000,"client":"C99"},
                    {"date":"2025-02-03","security":"TSLA","action":"SELL","amount":12000,"client":"C99"},
                    {"date":"2025-03-10","security":"MSFT","action":"BUY","amount":30000,"client":"C45"}
                  ]
                }
                """,
            "A99" => """
                {
                  "advisor_id": "A99",
                  "trades": [
                    {"date":"2025-01-20","security":"AMZN","action":"BUY","amount":75000,"client":"C12"}
                  ]
                }
                """,
            _ => """{"error":"Advisor not found"}"""
        };
    }
}