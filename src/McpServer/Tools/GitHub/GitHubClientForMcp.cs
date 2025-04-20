using Octokit;

namespace StandupStatus.McpServer.Tools.GitHub;

public class GitHubClientForMcp(GitHubClient innerClient)
{
    private readonly GitHubClient _client = innerClient;

    public async Task<IEnumerable<Activity>> GetRecentActivityAsync(
        DateTime? since = null)
    {
        var currentUser = await _client.User.Current();

        // By default, fetches the last 30 events or the first page. For more
        // control (e.g., fetching more pages or handling pagination
        // explicitly), you might need ApiOptions.
        IEnumerable<Activity> events =
            await _client.Activity.Events
                .GetAllUserPerformed(currentUser.Login);

        if (since.HasValue)
        {
            // Filter events that occurred after the 'since' date
            events = events.Where(e => e.CreatedAt > since.Value);
        }

        return events;
    }
}
