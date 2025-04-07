using System.ComponentModel;
using ModelContextProtocol.Server;

namespace StandupStatus.McpServer.Tools.GitHub;

[McpServerToolType]
public class GitHubEventsTool(GitHubClientForMcp client)
{
    private readonly GitHubClientForMcp _client = client;

    [McpServerTool(Name = "GetUserActivity")]
    [Description("Get the current user's recent activity on GitHub")]
    public async Task<string> GetUserActivity()
    {
        string events = await _client.GetRecentActivityAsync();
        return events;
    }
}
