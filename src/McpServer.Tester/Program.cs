using Microsoft.Extensions.DependencyInjection;
using StandupStatus.McpServer.Tools.GitHub;

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");

var services = new ServiceCollection();

services.AddSingleton(_ => GitHubClientFactory.Create(null));
services.AddSingleton<GitHubClientForMcp>();
services.AddSingleton<GitHubEventsTool>();

var serviceProvider = services.BuildServiceProvider();

GitHubEventsTool? tool = serviceProvider.GetService<GitHubEventsTool>()
    ?? throw new InvalidOperationException(
        $"Failed to resolve {nameof(GitHubEventsTool)}");

// Get yesterday at 1 PM Pacific Time
var pacificTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
var yesterday = DateTime.Now.AddDays(-1);
var yesterdayAt1PM = new DateTime(yesterday.Year, yesterday.Month, yesterday.Day, 13, 0, 0);
var yesterdayAt1PMPacific = TimeZoneInfo.ConvertTimeToUtc(yesterdayAt1PM, pacificTimeZone);

var activitySummary = await tool.GetUserActivity(since: yesterdayAt1PMPacific);
Console.WriteLine(activitySummary);
