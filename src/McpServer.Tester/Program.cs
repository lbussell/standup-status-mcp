using Microsoft.Extensions.DependencyInjection;
using StandupStatus.McpServer.Tools.GitHub;

var services = new ServiceCollection();

services.AddSingleton(_ => GitHubClientFactory.Create("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"));
services.AddSingleton<GitHubClientForMcp>();
services.AddSingleton<GitHubEventsTool>();

var serviceProvider = services.BuildServiceProvider();

GitHubEventsTool? tool = serviceProvider.GetService<GitHubEventsTool>()
    ?? throw new InvalidOperationException(
        $"Failed to resolve {nameof(GitHubEventsTool)}");

// Get yesterday at 1 PM LOCAL time - our API now handles the conversion to UTC internally
var yesterday = DateTime.Now.AddDays(-1);
var yesterdayAt1PM = new DateTime(yesterday.Year, yesterday.Month, yesterday.Day, 13, 0, 0, DateTimeKind.Local);
Console.WriteLine($"Getting events since {yesterdayAt1PM}");

// Pass local time directly - the API will handle conversion to UTC when needed
var activitySummary = await tool.GetUserActivity(since: yesterdayAt1PM);
Console.WriteLine(activitySummary);
