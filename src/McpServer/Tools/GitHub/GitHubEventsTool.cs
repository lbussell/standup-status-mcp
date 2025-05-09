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
        [Description("Get events since this date in local time (optional)")]
        DateTime? since = null)
    {
        // Ensure the input DateTime has the correct Kind value (Local) if provided
        if (since.HasValue && since.Value.Kind == DateTimeKind.Unspecified)
        {
            since = DateTime.SpecifyKind(since.Value, DateTimeKind.Local);
        }

        IEnumerable<Activity> gitHubEvents =
            await _client.GetRecentActivityAsync(since);

        if (!gitHubEvents.Any())
        {
            return since.HasValue
                ? $"No GitHub activity found since {Format(since.Value)}."
                : "No recent GitHub activity found.";
        }

        var output = new StringBuilder();

        // Add summary header with time frame information
        if (since.HasValue)
        {
            output.AppendLine($"# GitHub Activity since {Format(since.Value)}");
        }
        else
        {
            output.AppendLine("# Recent GitHub Activity");
        }
        output.AppendLine();

        // Format events by type
        foreach (var gitHubEvent in gitHubEvents)
        {
            string description = GetEventDescription(gitHubEvent);
            output.AppendLine($"## {description} ({Format(gitHubEvent.CreatedAt)})");
            output.AppendLine();
        }

        return output.ToString();
    }

    private static string GetEventDescription(Activity evt)
    {
        // Different event types could be handled differently for more detailed descriptions
        switch (evt.Type)
        {
            case "PushEvent":
                if (evt.Payload is PushEventPayload pushPayload)
                {
                    var commitCount = pushPayload.Size;
                    var branchRef = pushPayload.Ref?.Replace("refs/heads/", "");

                    var commitText = "commit".ToQuantity(commitCount);

                    var branchText = !string.IsNullOrEmpty(branchRef)
                        ? $" to branch '{branchRef}'"
                        : "";

                    var messageText = "";
                    if (pushPayload.Commits != null && pushPayload.Commits.Any())
                    {
                        var sb = new StringBuilder();
                        sb.AppendLine($": ");
                        foreach (var commit in pushPayload.Commits)
                        {
                            sb.AppendLine($"- \"{commit.Message.Split('\n')[0]}\"");
                        }
                        messageText = sb.ToString();
                    }

                    return $"Pushed {commitText}{branchText} in {evt.Repo.Name}{messageText}";
                }
                return $"Pushed to {evt.Repo.Name}";

            case "CreateEvent":
                if (evt.Payload is CreateEventPayload createPayload)
                {
                    var refType = createPayload.RefType.ToString();
                    var refName = !string.IsNullOrEmpty(createPayload.Ref)
                        ? $" '{createPayload.Ref}'"
                        : "";

                    return $"Created {refType}{refName} in {evt.Repo.Name}";
                }
                return $"Created {evt.Repo.Name}";

            case "PullRequestEvent":
                if (evt.Payload is PullRequestEventPayload payload)
                {
                    var action = payload.Action;
                    var prNumber = payload.Number;
                    var prTitle = payload.PullRequest.Title;
                    var prBody = payload.PullRequest.Body ?? "";

                    return
                        $"""
                        {action.Humanize(LetterCasing.Title)} pull request '{prTitle}' ({evt.Repo.Name}#{prNumber})
                        {prBody}
                        """;
                }
                return $"Pull request activity on {evt.Repo.Name}";

            case "IssuesEvent":
                if (evt.Payload is IssueEventPayload issuePayload)
                {
                    var action = issuePayload.Action ?? "updated";
                    var issueNumber = issuePayload.Issue.Number;
                    var issueTitle = issuePayload.Issue.Title;
                    var issueBody = issuePayload.Issue.Body ?? "";

                    return $"""
                        {action.Humanize(LetterCasing.Title)} issue '{issueTitle}' ({evt.Repo.Name}#{issueNumber})

                        {issueBody}
                        """;
                }
                return $"Issue activity on {evt.Repo.Name}";

            case "IssueCommentEvent":
                if (evt.Payload is IssueCommentPayload commentPayload)
                {
                    var action = commentPayload.Action;
                    var issueNumber = commentPayload.Issue.Number;
                    var issueTitle = commentPayload.Issue.Title;
                    var commentBody = commentPayload.Comment.Body ?? "";

                    // Determine if it's a PR or issue comment
                    var isPullRequest = commentPayload.Issue.PullRequest != null;
                    var itemType = isPullRequest ? "pull request" : "issue";

                    return
                        $"""
                        {action.Humanize(LetterCasing.Title)} comment on {itemType} '{issueTitle}' ({evt.Repo.Name}#{issueNumber})

                        "{commentBody}"
                        """;
                }
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

    private static string Format(DateTime dateTime)
    {
        // Convert to local time if not already and format with pattern
        DateTime localTime = dateTime.Kind == DateTimeKind.Utc
            ? dateTime.ToLocalTime()
            : dateTime;

        return localTime.ToString("g", CultureInfo.CurrentCulture);
    }

    private static string Format(DateTimeOffset dateTimeOffset)
    {
        // Convert to local time and format consistently
        return dateTimeOffset.ToLocalTime().Humanize(culture: CultureInfo.CurrentCulture);
    }
}
