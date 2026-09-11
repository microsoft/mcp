// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.ResiliencyAgent.Models;

/// <summary>
/// One artifact the agent produced during a conversation, already decoded into text.
/// The A2A response carries artifacts inline, so nothing is fetched separately.
/// </summary>
/// <param name="Name">File name as the agent named it.</param>
/// <param name="Description">The agent's own description of the artifact, when it supplied one.</param>
/// <param name="MimeType">Reported media type. Unreliable - the agent labels Bicep as JSON - so it is
/// carried for information only and never used to decide how content is handled.</param>
/// <param name="Content">Decoded text content.</param>
/// <param name="Format">Artifact family from the agent's metadata, such as arm, bicep or terraform.</param>
public sealed record AgentArtifact(
    string Name,
    string? Description,
    string? MimeType,
    string? Content,
    string? Format = null);

/// <summary>
/// An artifact after it has been written to the caller's working directory.
/// </summary>
public sealed record WrittenArtifact(
    string Name,
    string Path,
    string? Format,
    string? Description);

/// <summary>
/// The state of an agent turn after a send or a poll.
/// </summary>
/// <param name="ConversationId">The A2A context id. Returned to the caller so a later turn can continue this conversation.</param>
/// <param name="TaskId">The A2A task id. Internal only - it never leaves the tool.</param>
/// <param name="State">The raw A2A task state, for logging and telemetry.</param>
/// <param name="IsTerminal">True when the agent has finished this turn.</param>
/// <param name="IsAwaitingUser">True when the agent is waiting on the user before it can continue.</param>
/// <param name="Reply">The agent's text for the user - either its question or its answer.</param>
/// <param name="Artifacts">Artifacts carried on the final response.</param>
/// <param name="ReasoningStep">
/// The most recent reasoning step the agent published while working, if any. The backend appends
/// these to the task's status metadata for as long as the task is running, so this is live progress
/// text rather than a guess.
/// </param>
/// <param name="SuggestedNextSteps">
/// The agent's own suggestions for what the user could do next, in the user's voice. Emitted only
/// when the agent gave a definitive answer, so an empty list is normal mid-conversation.
/// </param>
public sealed record AgentTurn(
    string ConversationId,
    string TaskId,
    string State,
    bool IsTerminal,
    bool IsAwaitingUser,
    string? Reply,
    IReadOnlyList<AgentArtifact> Artifacts,
    string? ReasoningStep = null,
    IReadOnlyList<string>? SuggestedNextSteps = null);
