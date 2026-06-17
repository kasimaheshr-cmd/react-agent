using Azure;
using Azure.AI.OpenAI;
using OpenAI.Chat;

namespace ReactAgent.Core.Agent;

public class AzureOpenAIService
{
    private readonly ChatClient _client;

    public AzureOpenAIService(string endpoint, string apiKey, string deploymentName)
    {
        var azureClient = new AzureOpenAIClient(
            new Uri(endpoint),
            new AzureKeyCredential(apiKey)
        );
        _client = azureClient.GetChatClient(deploymentName);
    }

    public async Task<string> ChatAsync(string systemPrompt, string userMessage)
    {
        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(systemPrompt),
            new UserChatMessage(userMessage)
        };

        var response = await _client.CompleteChatAsync(messages);
        return response.Value.Content[0].Text;
    }

    public async Task<string> ChatWithHistoryAsync(
        List<(string role, string content)> history)
    {
        var messages = history.Select<(string role, string content), ChatMessage>(h =>
            h.role switch
            {
                "system" => new SystemChatMessage(h.content),
                "assistant" => new AssistantChatMessage(h.content),
                _ => new UserChatMessage(h.content)
            }
        ).ToList();

        var response = await _client.CompleteChatAsync(messages);
        return response.Value.Content[0].Text;
    }
}