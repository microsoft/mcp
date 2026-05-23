// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.SreAgent.Models;

public sealed record SreAgentScheduledTask
{
    public string? Id { get; init; }
    public string? Name { get; init; }
    public string? Description { get; init; }
    public string? Status { get; init; }
    public string? CronExpression { get; init; }
    public string? AgentPrompt { get; init; }
    public string? Agent { get; init; }
    public string? CreatedBy { get; init; }
    public string? CreatedAt { get; init; }
    public string? LastExecutionTime { get; init; }
    public string? NextExecutionTime { get; init; }
    public int? ExecutionCount { get; init; }
    public int? MaxExecutions { get; init; }
    public string? ThreadId { get; init; }
    public string? AgentMode { get; init; }
    public string? ModelTier { get; init; }
}

public sealed record SreAgentScheduledTaskCreateRequest(
    string Name,
    string Agent,
    string CronExpression,
    string AgentPrompt,
    string Description);
