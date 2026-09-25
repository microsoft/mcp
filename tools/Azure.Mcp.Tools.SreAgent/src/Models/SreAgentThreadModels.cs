// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.SreAgent.Models;

public sealed record SreAgentThread
{
    public string? Id { get; init; }
    public string? Title { get; init; }
    public JsonElement? Source { get; init; }
    public SreAgentThreadStatus? Status { get; init; }
    public string? CreatedTimestamp { get; init; }
    public string? ModifiedTimestamp { get; init; }
    public SreAgentThreadStartMessage? StartMessage { get; init; }
    public SreAgentThreadLastMessage? LastMessage { get; init; }
}

public sealed record SreAgentThreadStartMessage
{
    public SreAgentMessageAuthor? Author { get; init; }
    public string? Text { get; init; }
}

public sealed record SreAgentThreadLastMessage
{
    public bool? IsComplete { get; init; }
    public SreAgentMessageAuthor? Author { get; init; }
    public string? MessageType { get; init; }
    public string? Text { get; init; }
}

public sealed record SreAgentThreadMessage
{
    public string? Id { get; init; }
    public string? TimeStamp { get; init; }
    public SreAgentMessageAuthor? Author { get; init; }
    public string? Text { get; init; }
    public string? MessageType { get; init; }
    public bool? IsComplete { get; init; }
    public SreAgentMessageApproval? Approval { get; init; }
    public SreAgentMessageUserQuestion? UserQuestion { get; init; }
}

public sealed record SreAgentMessageAuthor
{
    public string? Role { get; init; }
    public string? UserId { get; init; }
    public string? DisplayName { get; init; }
}

public sealed record SreAgentMessageApproval
{
    public string? Id { get; init; }
    public string? Title { get; init; }
    public string? Description { get; init; }
    public string? Status { get; init; }
}

public sealed record SreAgentMessageUserQuestion
{
    public string? QuestionId { get; init; }
    public string? Question { get; init; }
    public string? Header { get; init; }
    public List<SreAgentQuestionOption>? Options { get; init; }
    public bool? AllowFreeText { get; init; }
    public string? Status { get; init; }
}

public sealed record SreAgentQuestionOption
{
    public string? Label { get; init; }
    public string? Description { get; init; }
}

public sealed record SreAgentPagedResponse<T>
{
    public List<T>? Value { get; init; }
}

public sealed record SreAgentThreadStatus
{
    public SreAgentThreadActionsStatus? ActionsStatus { get; init; }
    public SreAgentThreadIncidentStatus? IncidentStatus { get; init; }
}

public sealed record SreAgentThreadActionsStatus
{
    public bool? HasCriticalActions { get; init; }
    public bool? HasWarningActions { get; init; }
}

public sealed record SreAgentThreadIncidentStatus
{
    public string? IncidentId { get; init; }
    public string? Status { get; init; }
}

public sealed record SreAgentThreadCreateRequest(SreAgentThreadStartMessageRequest StartMessage);

public sealed record SreAgentThreadStartMessageRequest(string Text, string UserId, string DisplayName, string Agent);

public sealed record SreAgentThreadMessageRequest(string Text, string UserId, string DisplayName, string? Agent = null);

public sealed record SreAgentApprovalRequest(string User);

public sealed record SreAgentThreadOperationResult(string? ThreadId, string Status, List<SreAgentThreadMessage> Messages, string? Message = null);

public sealed record SreAgentInvestigationResult(
    string? ThreadId,
    string Status,
    int FollowUps,
    bool ActionNeeded,
    string? ActionMessage,
    List<SreAgentThreadMessage> Messages);
