// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.ResiliencyAgent.Models;

namespace Azure.Mcp.Tools.ResiliencyAgent.Services;

/// <summary>
/// A2A client for the Azure Resiliency Agent.
/// </summary>
/// <remarks>
/// This is a protocol client only. It sends a turn and reads task state; it does not decide when to
/// ask the user anything. The conversation loop, elicitation and progress reporting live in the
/// command, because they need the MCP server for the current request.
/// </remarks>
public interface IResiliencyAgentService
{
    /// <summary>
    /// Sends one user message into a conversation, starting it if <paramref name="taskId"/> is null.
    /// </summary>
    Task<AgentTurn> SendAsync(
        string conversationId,
        string? taskId,
        string text,
        CancellationToken cancellationToken);

    /// <summary>
    /// Polls a task until it is terminal, is waiting on the user, or the budget elapses.
    /// </summary>
    Task<AgentTurn> PollAsync(
        string conversationId,
        string taskId,
        TimeSpan budget,
        CancellationToken cancellationToken);
}
