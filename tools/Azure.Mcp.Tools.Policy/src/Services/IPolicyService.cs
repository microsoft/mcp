// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Policy.Models;

namespace Azure.Mcp.Tools.Policy.Services;

public interface IPolicyService
{
    Task<List<PolicyAssignment>> GetAssignmentsAsync(
        string? subscription = null,
        string? assignment = null,
        string? tenant = null,
        CancellationToken cancellationToken = default);
}
