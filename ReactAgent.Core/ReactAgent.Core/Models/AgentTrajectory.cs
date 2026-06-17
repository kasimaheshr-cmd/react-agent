namespace ReactAgent.Core.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class AgentStep
{
    public int Iteration { get; set; }
    public string Reasoning { get; set; } = string.Empty;   // what LLM returned
    public string? ToolCalled { get; set; }                  // null if FINAL
    public Dictionary<string, string>? ToolParameters { get; set; }
    public string? ToolResult { get; set; }                  // null if FINAL
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class AgentTrajectory
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Query { get; set; } = string.Empty;
    public List<AgentStep> Steps { get; set; } = new();
    public string? FinalAnswer { get; set; }
    public bool HitMaxIterations { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}