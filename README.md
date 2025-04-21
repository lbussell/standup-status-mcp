# standup-status-mcp

MCP Tool to summarize your recent GitHub activity.

## Usage

### GitHub Copilot via VS Code

1. Clone this repo
2. Open [.vscode/mcp.json](.vscode/mcp.json)
3. Start the server by clicking "Start" above the server definition.
4. Input your GitHub token in the prompt. The token needs **Read** access to
   user events, issues, metadata, and pull requests.
5. In GitHub Copilot chat, enter a prompt like:

    > Write a summary of what I did on Github since last Friday at 1 PM.
    > Include specific details about issues, comments, pull requests, and
    > commits. Format your response in a way that is easy read out-loud.

6. Make sure to reference the `GetUserActivity` tool as context before sending
   your message.
7. Sometimes GitHub Copilot likes to try to use the official GitHub MCP Server
   instead of this one. That one doesn't have GitHub Events API support, so it
   won't do what you want. If you see that happen, just click "Rerun without"
   and it should call this tool instead.
