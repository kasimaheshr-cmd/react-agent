namespace ReactAgent.Core.Models;

public class AgentMessage
{
    public string Role { get; set; } = string.Empty;   // "system" | "user" | "assistant" | "tool"
    public string Content { get; set; } = string.Empty;
}