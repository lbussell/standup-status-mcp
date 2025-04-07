using System.IO.Pipelines;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol.Transport;
using ModelContextProtocol.Server;
using StandupStatus.McpServer.Tools.GitHub;
using static System.Console;

// This project is going to set up both a server and client in the same process
// using in-memory streams for transport. The purpose of this is so that we can
// debug the server without using an LLM or another client.

var clientToServerPipe = new Pipe();
var serverToClientPipe = new Pipe();

// Create and run server using streams for transport
var services = new ServiceCollection();

services.AddSingleton(_ => GitHubClientFactory.Create());
services.AddSingleton<GitHubClientForMcp>();
services.AddSingleton<GitHubEventsTool>();

services.AddMcpServer()
    .WithStdioServerTransport()
    .WithTools<GitHubEventsTool>();

var transport = new StreamServerTransport(
    inputStream: clientToServerPipe.Reader.AsStream(),
    outputStream: serverToClientPipe.Writer.AsStream());
services.AddSingleton<ITransport>(transport);

var serviceProvider = services.BuildServiceProvider();

var server = serviceProvider.GetRequiredService<IMcpServer>();
var serverTask = server.RunAsync(CancellationToken.None);

// Now we can create a client to connect to the server
var client = await McpClientFactory.CreateAsync(
    new McpServerConfig()
    {
        Id = "standup-status",
        Name = "Standup Status",
        TransportType = "ignored",
    },
    createTransportFunc: (_, _) => new StreamClientTransport(
        serverInput: clientToServerPipe.Writer.AsStream(),
        serverOutput: serverToClientPipe.Reader.AsStream()));

IList<McpClientTool> tools = await client.ListToolsAsync();
WriteLine("Available tools: " + string.Join(", ", tools.Select(t => t.Name)));

var yesterdayAt1PM = DateTime.Today.AddDays(-3).AddHours(13);
WriteLine($"Getting user activity since {yesterdayAt1PM} {yesterdayAt1PM.Kind}");

var result = await client.CallToolAsync(
    nameof(GitHubEventsTool.GetUserActivity),
    arguments: new Dictionary<string, object?>()
        {
            { "since", yesterdayAt1PM }
        });
WriteLine(result.Content.First(c => c.Type == "text").Text);
