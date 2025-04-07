using System.ComponentModel;
using System.Globalization;
using System.Text;
using GitHub.Models;
using Humanizer;
using ModelContextProtocol.Server;

using GitHubEvent = GitHub.Models.Event;

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
        var gitHubEvents = await _client.GetRecentActivityAsync(since);
        var events = gitHubEvents.Select(e => new Event(e)).ToList();

        // Create a summary of events per day
        var output = new StringBuilder();
        var eventsByDate = events.GroupBy(e => e.Created);
        foreach (var group in eventsByDate)
        {
            output.AppendLine(FormatDate(group.Key));
            foreach (var @event in group)
            {
                output.AppendLine(" - " + @event.ToString());
            }
        }

        return output.ToString();
    }

    private static StringBuilder FormatEvent(GitHubEvent @event)
    {
        var type = @event.Type;
        var payload = @event.Payload;

        // var date = (DateTimeOffset)@event.CreatedAt;
        // var time = date.ToLocalTime().ToString("hh:mm tt", CultureInfo.InvariantCulture);

        var data = payload?.AdditionalData ?? new Dictionary<string, object>();

        var output = new StringBuilder();
        switch (type)
        {
            case "CreateEvent":
                var refType = data["ref_type"] ?? "unknown";
                var @ref = data["ref"] ?? "unknown";
                output.AppendLine($"Created {refType} {@ref} on {@event.Repo!.Name}");
                break;
            // case "PushEvent":
            //     var pushPayload = payload as PushEventPayload;
            //     return $"{type} to {pushPayload?.Ref} at {time}";
            // case "PullRequestEvent":
            //     var prPayload = payload as PullRequestEventPayload;
            //     return $"{type} {prPayload?.Action} at {time}";
            // case "IssuesEvent":
            //     var issuePayload = payload as IssuesEventPayload;
            //     return $"{type} {issuePayload?.Action} at {time}";
            // case "IssueCommentEvent":
            //     var commentPayload = payload as IssueCommentEventPayload;
            //     return $"{type} {commentPayload?.Action} at {time}";
            default:
                output.AppendLine(@event.Type);
                break;
        }

        return output;
    }

    private static string FormatDate(DateTime? date)
    {
        if (date is null)
        {
            return "Unknown date";
        }

        return ((DateTime)date)
            .ToLocalTime()
            .ToString("dddd, MMMM d, yyyy", CultureInfo.InvariantCulture);
    }

    private class Event(GitHubEvent gitHubEvent)
    {
        private readonly GitHubEvent _event = gitHubEvent;

        public string Type => _event.Type ?? "Unknown event type";

        public DateTime Created => _event.CreatedAt?.Date.ToLocalTime() ?? DateTime.MinValue;

        public override string ToString()
        {
            return Type switch
            {
                "CreateEvent" => FormatCreateEvent(),
                "IssueCommentEvent" => FormatIssueCommentEvent(),
                _ => Type,
            };
        }

        private string FormatIssueCommentEvent(bool includeComment = false)
        {
            string action = _event.Payload?.Action ?? "unknown";
            IssueComment? comment = _event.Payload?.Comment;
            Issue? issue = _event.Payload?.Issue;

            if (includeComment && issue is not null && comment is not null)
            {
                return $"""

                    EVENT: {action.Transform(To.SentenceCase)} comment on {FormatIssue(issue)}:

                    {comment.Body}

                    END EVENT

                    """;
            }

            return $"{action.Transform(To.SentenceCase)} comment on {FormatIssue(issue!)}";
        }

        private string FormatCreateEvent()
        {
            string type = (string?)GetData("ref_type") ?? "unknown";
            string reference = (string?)GetData("ref") ?? "unknown";
            return type switch
            {
                "repository" => $"Created {type} {_event.Repo?.Name}",
                _ => $"Created {type} {reference} on {_event.Repo?.Name}"
            };
        }

        /// <summary>
        /// Get the value of a property in the event payload.
        /// If the key does not exist, return "unknown".
        /// </summary>
        /// <param name="key">The name of the value to get from the payload</param>
        /// <returns>The value of the property, or "unknown" if it does not exist</returns>
        private object? GetData(string key)
        {
            IDictionary<string, object>? additionalData = _event.Payload?.AdditionalData;

            if (additionalData is null)
            {
                return null;
            }

            if (!additionalData.TryGetValue(key, out object? value))
            {
                return null;
            }

            return value;
        }

        private string FormatIssue(Issue issue)
        {
            return $"issue \"{issue.Title}\" ({_event.Repo?.Name}#{issue.Number})";
        }
    }
}
