using Octokit;

namespace StandupStatus.McpServer.Tools;

public static class ActivityHelper
{
    public static string ToHumanReadableDescription(this Activity activity)
    {
        return activity.Type switch
        {
            "CreateEvent" => CreateEventDescription(activity),
            "DeleteEvent" => DeleteEventDescription(activity),
            // Continue with other event types
            // ForkEvent
            // GollumEvent
            // IssueCommentEvent
            // IssuesEvent
            // MemberEvent
            // PublicEvent
            // PullRequestEvent
            // PullRequestReviewEvent
            // PullRequestReviewCommentEvent
            // PullRequestReviewThreadEvent
            // PushEvent
            // ReleaseEvent
            // SponsorshipEvent
            // WatchEvent
            _ => $"{activity.Type} on {activity.Repo.Name}",
        };
    }

    private static string CreateEventDescription(Activity activity)
    {
        CreateEventPayload payload = (CreateEventPayload) activity.Payload;
        return $"Created {payload.RefType} {payload.Ref} on {activity.Repo.Name}";
    }

    private static string DeleteEventDescription(Activity activity)
    {
        DeleteEventPayload payload = (DeleteEventPayload) activity.Payload;
        return $"Deleted {payload.RefType} {payload.Ref} on {activity.Repo.Name}";
    }
}
