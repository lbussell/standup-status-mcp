using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StandupStatus.McpServer.GitHub;
using StandupStatus.McpServer.Tools;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddMcpServer()
    .WithStdioServerTransport()
    .WithTools<GitHubEventsTool>();

builder.Logging.AddConsole(options =>
{
    options.LogToStandardErrorThreshold = LogLevel.Trace;
});

// Add GitHub client
builder.Services.AddSingleton(_ =>
{
    string githubToken = Environment.GetEnvironmentVariable("GITHUB_TOKEN")
        ?? throw new InvalidOperationException("Please set GITHUB_TOKEN environment variable.");

    var githubClient = GitHubHelper.CreateClient(githubToken);
    return githubClient;
});

await builder.Build().RunAsync();
