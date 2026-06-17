using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReactAgent.McpServer.Tools;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

await builder.Build().RunAsync();