using GitHub;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StandupStatus.McpServer.Tools.GitHub;

var builder = Host.CreateApplicationBuilder(args);

// Configure logging
builder.Logging.AddConsole(options =>
{
    options.LogToStandardErrorThreshold = LogLevel.Trace;
});

builder.Services.AddSingleton<GitHubClient>(_ => GitHubClientFactory.Create());
builder.Services.AddSingleton<GitHubClientForMcp>();

// Add MCP Server
builder.Services.AddMcpServer()
    .WithStdioServerTransport()
    .WithTools<GitHubEventsTool>();

await builder.Build().RunAsync();
