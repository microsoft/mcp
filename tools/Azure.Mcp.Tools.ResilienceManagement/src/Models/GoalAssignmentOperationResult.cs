// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.ResilienceManagement.Models;

public sealed record GoalAssignmentOperationResult(
    string ServiceGroup,
    string GoalAssignment,
    string? OperationId,
    string Status,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.Never)] bool HasCompleted);
