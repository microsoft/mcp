// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.ResilienceManagement.Models;

public sealed record GoalAssignmentOperationResult
{
    public string? OperationId { get; init; }
    public string Status { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public bool HasCompleted { get; init; }

    public string? ServiceGroup { get; init; }
    public string? GoalAssignment { get; init; }

    public GoalAssignmentOperationResult(string? operationId, string status, bool hasCompleted)
        : this(null, null, operationId, status, hasCompleted)
    {
    }

    [JsonConstructor]
    public GoalAssignmentOperationResult(
        string? serviceGroup,
        string? goalAssignment,
        string? operationId,
        string status,
        bool hasCompleted)
    {
        ServiceGroup = serviceGroup;
        GoalAssignment = goalAssignment;
        OperationId = operationId;
        Status = status;
        HasCompleted = hasCompleted;
    }
}
