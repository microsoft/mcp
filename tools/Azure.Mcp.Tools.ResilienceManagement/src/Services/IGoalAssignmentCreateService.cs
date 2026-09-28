// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.ResilienceManagement.Models;

namespace Azure.Mcp.Tools.ResilienceManagement.Services;

public interface IGoalAssignmentCreateService
{
    Task<GoalAssignmentInfo> CreateGoalAssignmentAsync(
        string serviceGroup,
        string goalAssignment,
        string goalTemplate,
        string? tenant = null,
        CancellationToken cancellationToken = default);
}
