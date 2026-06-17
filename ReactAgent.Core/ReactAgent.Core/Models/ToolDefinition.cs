namespace ReactAgent.Core.Models;

public class ToolDefinition
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;  // LLM reads this to decide when to call it
    public Dictionary<string, string> Parameters { get; set; } = new();  // param name → description
}