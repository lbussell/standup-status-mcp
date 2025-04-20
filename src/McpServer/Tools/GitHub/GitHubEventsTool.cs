using System.ComponentModel;
using System.Globalization;
using System.Text;
using Humanizer;
using ModelContextProtocol.Server;
using Octokit;

namespace StandupStatus.McpServer.Tools.GitHub;


[McpServerToolType]
public class GitHubEventsTool(GitHubClientForMcp client)
{
    private readonly GitHubClientForMcp _client = client;

    [McpServerTool(Name = "GetUserActivity")]
    [Description("Get the current user's recent activity on GitHub")]
    public async Task<string> GetUserActivity(
        [Description("Get events since this date (optional)")]
        DateTime? since = null)
    {
        IEnumerable<Activity> gitHubEvents =
            await _client.GetRecentActivityAsync(since);

        if (!gitHubEvents.Any())
        {
            return since.HasValue
                ? $"No GitHub activity found since {since.Value.Humanize()}."
                : "No recent GitHub activity found.";
        }

        var output = new StringBuilder();

        // Add summary header with time frame information
        if (since.HasValue)
        {
            output.AppendLine($"## GitHub Activity since {since.Value.Humanize()}");
        }
        else
        {
            output.AppendLine("## Recent GitHub Activity");
        }
        output.AppendLine();

        // Group events by repository for better organization
        var eventsByRepo = gitHubEvents
            .GroupBy(e => e.Repo.Name)
            .OrderBy(g => g.Key);

        foreach (var repoGroup in eventsByRepo)
        {
            output.AppendLine($"### {repoGroup.Key}");

            // Group events by type within each repository
            var eventsByType = repoGroup
                .GroupBy(e => e.Type)
                .OrderByDescending(g => g.Max(e => e.CreatedAt));

            foreach (var typeGroup in eventsByType)
            {
                // Format events by type
                FormatEventsByType(output, typeGroup.Key, typeGroup.OrderByDescending(e => e.CreatedAt));
            }

            output.AppendLine();
        }

        // Add a summary count
        output.AppendLine($"**Total: {gitHubEvents.Count()} event{(gitHubEvents.Count() != 1 ? "s" : "")}**");

        return output.ToString();
    }

    private void FormatEventsByType(StringBuilder sb, string eventType, IEnumerable<Activity> events)
    {
        // Create a human-readable heading for this event type
        var typeName = eventType.Humanize(LetterCasing.Title);
        sb.AppendLine($"#### {typeName} ({events.Count()})");

        foreach (var evt in events)
        {
            string description = GetEventDescription(evt);
            sb.AppendLine($"- {description} ({evt.CreatedAt.Humanize()})");
        }

        sb.AppendLine();
    }

    private string GetEventDescription(Activity evt)
    {
        // The payload is available through the RawPayload property, but
        // for simplicity, we'll just use the information already available

        // Different event types could be handled differently for more detailed descriptions
        switch (evt.Type)
        {
            case "PushEvent":
                return $"Pushed to {evt.Repo.Name}";

            case "CreateEvent":
                return $"Created {evt.Repo.Name}";

            case "PullRequestEvent":
                return $"Pull request activity on {evt.Repo.Name}";

            case "IssuesEvent":
                return $"Issue activity on {evt.Repo.Name}";

            case "IssueCommentEvent":
                return $"Commented on an issue in {evt.Repo.Name}";

            case "WatchEvent":
                return $"Starred {evt.Repo.Name}";

            case "ForkEvent":
                return $"Forked {evt.Repo.Name}";

            case "DeleteEvent":
                return $"Deleted something in {evt.Repo.Name}";

            default:
                return $"{evt.Type.Humanize(LetterCasing.Sentence)} on {evt.Repo.Name}";
        }
    }
}
