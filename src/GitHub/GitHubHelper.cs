using Octokit;

namespace StandupStatus.GitHub;

public class GitHubHelper
{
    private static readonly ProductHeaderValue s_productHeaderValue = new ProductHeaderValue("StandupStatusApp");

    private readonly GitHubClient _client;

    public GitHubHelper(string token)
    {
        _client = new GitHubClient(s_productHeaderValue)
        {
            Credentials = new Credentials(token)
        };
    }

    public async Task<IReadOnlyList<Activity>> GetUserEventsAsync()
    {
        var user = await GetCurrentUserAsync();
        var events = await _client.Activity.Events.GetAllUserPerformed(user);
        return events;
    }

    private async Task<string> GetCurrentUserAsync()
    {
        var user = await _client.User.Current();
        return user.Login;
    }
}
