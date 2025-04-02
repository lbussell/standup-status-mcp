using StandupStatus.GitHub;

Console.Write("Enter your GitHub Personal Access Token (PAT): ");

string? pat = Console.ReadLine();
if (string.IsNullOrWhiteSpace(pat))
{
    Console.WriteLine("GitHub PAT cannot be empty.");
    return;
}

var client = new GitHubHelper(pat);

var events = await client.GetUserEventsAsync();

Console.WriteLine($"Total events: {events.Count}");
