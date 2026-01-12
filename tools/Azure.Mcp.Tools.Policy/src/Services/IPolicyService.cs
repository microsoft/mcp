// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Policy.Models;

namespace Azure.Mcp.Tools.Policy.Services;

public interface IPolicyService
{
    /// <summary>
    /// Lists policy assignments in a subscription or scope.
    /// </summary>
    /// <param name="subscription">The subscription ID or name.</param>
    /// <param name="scope">Optional scope to filter policy assignments. If not provided, lists all assignments in the subscription.</param>
    /// <param name="tenantId">Optional tenant ID for cross-tenant operations.</param>
    /// <param name="retryPolicy">Optional retry policy for the operation.</param>
    /// <param name="cancellationToken">Optional cancellation token for the operation.</param>
    /// <returns>A list of policy assignments.</returns>
    Task<List<PolicyAssignment>> ListPolicyAssignmentsAsync(
        string subscription,
        string? scope = null,
        string? tenantId = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);
}
