using Octokit;

namespace StandupStatus.McpServer.GitHub;

internal static class GitHubHelper
{
    private static readonly ProductHeaderValue s_productHeaderValue = new ProductHeaderValue("StandupStatusApp");

    public static IGitHubClient CreateClient(string token)
    {
        var client = new GitHubClient(s_productHeaderValue)
        {
            Credentials = new Credentials(token)
        };

        return client;
    }
}
