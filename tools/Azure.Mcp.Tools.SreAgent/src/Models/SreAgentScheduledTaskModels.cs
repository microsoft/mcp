// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.SreAgent.Models;

public sealed record SreAgentScheduledTask
{
    public string? Id { get; init; }
    public string? Name { get; init; }
    public string? Description { get; init; }
    public string? CronExpression { get; init; }
    public string? Status { get; init; }
    public string? AgentName { get; init; }
    public string? LastRun { get; init; }
    public string? NextRun { get; init; }
}

public sealed record SreAgentScheduledTaskCreateRequest(
    string Name,
    string Agent,
    string CronExpression,
    string AgentPrompt,
    string Description);
