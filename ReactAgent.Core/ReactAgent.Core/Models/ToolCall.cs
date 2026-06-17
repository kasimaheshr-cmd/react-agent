namespace ReactAgent.Core.Models;

public class ToolCall
{
    public string ToolName { get; set; } = string.Empty;
    public Dictionary<string, string> Parameters { get; set; } = new();
}