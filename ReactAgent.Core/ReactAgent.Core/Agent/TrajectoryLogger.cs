using MongoDB.Driver;
using ReactAgent.Core.Models;

namespace ReactAgent.Core.Agent;

public class TrajectoryLogger
{
    private readonly IMongoCollection<AgentTrajectory> _collection;

    public TrajectoryLogger()
    {
        var client = new MongoClient("mongodb://admin:LPLMongo2024!@localhost:27017");
        var db = client.GetDatabase("react_agent");
        _collection = db.GetCollection<AgentTrajectory>("trajectories");
    }

    public async Task SaveAsync(AgentTrajectory trajectory)
    {
        try
        {
            trajectory.Id = Guid.NewGuid().ToString(); // force fresh id every save
            await _collection.InsertOneAsync(trajectory);
            Console.WriteLine($"[LOG] Trajectory saved → MongoDB id: {trajectory.Id}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[LOG ERROR] {ex.Message}");
        }
    }
}