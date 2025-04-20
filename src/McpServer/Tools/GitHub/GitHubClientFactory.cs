using Octokit;

namespace StandupStatus.McpServer.Tools.GitHub;

public static class GitHubClientFactory
{
    private static readonly ProductHeaderValue s_productHeaderValue =
        new("standup-status-mcp");

    public static GitHubClient Create(string? token = null)
    {
        if (string.IsNullOrEmpty(token))
        {
            token = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
        }

        var credentials = new Credentials(token);

        var client = new GitHubClient(s_productHeaderValue)
        {
            Credentials = credentials,
        };

        return client;
    }
}
