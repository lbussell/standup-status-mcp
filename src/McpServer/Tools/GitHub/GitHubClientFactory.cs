using GitHub;
using GitHub.Octokit.Client;
using GitHub.Octokit.Client.Authentication;

namespace StandupStatus.McpServer.Tools.GitHub;

public static class GitHubClientFactory
{
    public static GitHubClient Create(string? token = null)
    {
        token = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
        var tokenProvider = new TokenProvider(token ?? "");
        var tokenAuthProvider = new TokenAuthProvider(tokenProvider);
        var requestAdapter = RequestAdapter.Create(tokenAuthProvider);
        var client = new GitHubClient(requestAdapter);
        return client;
    }
}
