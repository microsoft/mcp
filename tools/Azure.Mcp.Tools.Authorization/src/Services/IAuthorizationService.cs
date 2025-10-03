// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Authorization.Models;

namespace Azure.Mcp.Tools.Authorization.Services;

public interface IAuthorizationService
{
    /// <summary>
    /// Lists all role assignments in the subscription.
    /// </summary>
    /// <param name="subscription">The subscription ID to query.</param>
    /// <param name="scope">The scope that the resource will apply against.</param>
    /// <param name="tenantId">Optional tenant ID for cross-tenant operations.</param>
    /// <param name="retryPolicy">Optional retry policy for the operation.</param>
    /// <param name="cancellationToken">Optional cancellation token for the operation.</param>
    /// <returns>List of role assignments in the format "Role Definition ID: Principal ID"</returns>
    Task<List<RoleAssignment>> ListRoleAssignmentsAsync(
        string subscription,
        string scope,
        string? tenantId = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);
}
