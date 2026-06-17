namespace ReactAgent.Core.Tools;

public class ClientPortfolioTool
{
    public string Name => "get_client_portfolio";

    public string Description =>
        "Retrieves the current portfolio holdings for a specific client. " +
        "Returns a list of securities, quantities, and current values. " +
        "Use this when asked about what a client owns or holds.";

    public Dictionary<string, string> Parameters => new()
    {
        { "client_id", "The client ID to look up e.g. C99, C45, C12" }
    };

    public string Execute(Dictionary<string, string> parameters)
    {
        var clientId = parameters.GetValueOrDefault("client_id", "").ToUpper();

        return clientId switch
        {
            "C99" => """
                {
                  "client_id": "C99",
                  "risk_profile": "conservative",
                  "holdings": [
                    {"security":"AAPL","quantity":200,"value":35000},
                    {"security":"TSLA","quantity":50,"value":8000},
                    {"security":"BONDS","quantity":1000,"value":100000}
                  ]
                }
                """,
            "C45" => """
                {
                  "client_id": "C45",
                  "risk_profile": "aggressive",
                  "holdings": [
                    {"security":"MSFT","quantity":300,"value":90000},
                    {"security":"NVDA","quantity":100,"value":45000}
                  ]
                }
                """,
            _ => """{"error":"Client not found"}"""
        };
    }
}