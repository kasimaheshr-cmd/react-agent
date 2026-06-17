using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.ComponentModel;

namespace ReactAgent.Core.Agent;

public class SemanticKernelAgent
{
    private readonly Kernel _kernel;


    // Ollama local constructor — no Azure credentials needed
    public SemanticKernelAgent()
    {
        var builder = Kernel.CreateBuilder();

        builder.AddOpenAIChatCompletion(
            modelId: "llama3.2",
            apiKey: "ollama",
            endpoint: new Uri("http://localhost:11434/v1")
        );

        _kernel = builder.Build();
        _kernel.Plugins.AddFromObject(new CompliancePlugin(), "Compliance");
    }

    public SemanticKernelAgent(string endpoint, string apiKey, string deployment)
    {
        _kernel = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(deployment, endpoint, apiKey)
            .Build();

        // register your compliance tools as SK plugins
        _kernel.Plugins.AddFromObject(new CompliancePlugin(), "Compliance");
    }

    public async Task<string> RunAsync(string query)
    {
        Console.WriteLine($"[SK] Query: {query}");

        var settings = new OpenAIPromptExecutionSettings
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
        };

        var result = await _kernel.InvokePromptAsync(query,
            new KernelArguments(settings));

        return result.ToString();
    }
}

// your tools become SK plugin methods
public class CompliancePlugin
{
    [KernelFunction, Description("Looks up a FINRA compliance rule by rule ID")]
    public string CheckComplianceRule(
        [Description("The FINRA rule ID e.g. 4511, 3110, 2111")]
        string rule_id)
    {
        return rule_id switch
        {
            "4511" => """{"rule_id":"4511","name":"Books and Records","requirement":"Preserve records 6 years."}""",
            "3110" => """{"rule_id":"3110","name":"Supervision","requirement":"Establish supervisory system."}""",
            "2111" => """{"rule_id":"2111","name":"Suitability","requirement":"Reasonable suitability basis."}""",
            _ => """{"error":"Rule not found"}"""
        };
    }

    [KernelFunction, Description("Gets advisor trade activity")]
    public string GetAdvisorActivity(
        [Description("Advisor ID e.g. A12")]
        string advisor_id)
    {
        return """{"advisor_id":"A12","trades":[{"security":"AAPL","action":"BUY","amount":50000}]}""";
    }
}