using System.ComponentModel;
using ModelContextProtocol.Server;
using Octokit;

namespace StandupStatus.McpServer.Tools;

[McpServerToolType]
public class GitHubEventsTool(IGitHubClient githubClient)
{
    private readonly GitHubClientForMcp _githubClient = new(githubClient);

    [McpServerTool(Title = "Get user events")]
    [Description("See the latest GitHub activity of the current user.")]
    public async Task<IReadOnlyList<string>> GetUserActivity()
    {
        var activities = await _githubClient.GetUserActivity();
        return activities.Select(activity => activity.ToHumanReadableDescription()).ToList();
    }

    private class GitHubClientForMcp(IGitHubClient githubClient)
    {
        private readonly IGitHubClient _githubClient = githubClient;

        // Cache GitHub user information, since it won't change
        private User? _user = null;

        public async Task<string> GetCurrentUser()
        {
            User user = await GetUser();
            return user.Name;
        }

        public async Task<IReadOnlyList<Activity>> GetUserActivity()
        {
            User user = await GetUser();
            string userLogin = user.Login;

            var events = await _githubClient.Activity.Events.GetAllUserPerformed(userLogin);
            return events;
        }

        private async Task<User> GetUser()
        {
            _user ??= await _githubClient.User.Current();
            return _user;
        }
    }
}
