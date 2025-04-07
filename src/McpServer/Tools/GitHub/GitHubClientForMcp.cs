using GitHub;
using GitHub.Models;
using GitHub.Repos.Item;
using static GitHub.User.UserRequestBuilder;

namespace StandupStatus.McpServer.Tools.GitHub;

public class GitHubClientForMcp(GitHubClient innerClient)
{
    private readonly GitHubClient _client = innerClient;

    public async Task<List<Event>> GetRecentActivityAsync(DateTime? since = null)
    {
        UserGetResponse user = await _client.User.GetAsync()
            ?? throw new Exception("No user found");

        string userName = user.PublicUser?.Login
            ?? throw new Exception("Unable to get current user");

        List<Event> events = await _client.Users[userName].Events.GetAsync()
            ?? throw new Exception("Unable to get events for user " + userName);

        // Get events since the specified date
        if (since is not null)
        {
            events = events
                .Where(e => e.CreatedAt >= since)
                .ToList();
        }

        return events;
    }
}
