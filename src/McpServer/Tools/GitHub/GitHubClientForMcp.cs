using GitHub;
using GitHub.Models;

namespace StandupStatus.McpServer.Tools.GitHub;

public class GitHubClientForMcp(GitHubClient innerClient)
{
    private readonly GitHubClient _client = innerClient;

    public async Task<string> GetRecentActivityAsync()
    {
        List<Event>? events = await _client.Events.GetAsync();
        return System.Text.Json.JsonSerializer.Serialize(events, new System.Text.Json.JsonSerializerOptions() { WriteIndented = true });
    }
}
