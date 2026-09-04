// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.Authorization.Models;

namespace Azure.Mcp.Tools.Authorization.Services;

public interface IAuthorizationService
{
    /// <summary>
    /// Lists the role assignments at or below the requested scope.
    /// </summary>
    /// <param name="subscription">
    /// The subscription ID to query. Not required when <paramref name="scope"/> is a management group scope,
    /// because those assignments live outside any subscription.
    /// </param>
    /// <param name="scope">The scope that the role assignments apply against.</param>
    /// <param name="tenantId">Optional tenant ID for cross-tenant operations.</param>
    /// <param name="cancellationToken">Optional cancellation token for the operation.</param>
    /// <returns>The role assignments found at or below the scope.</returns>
    Task<ResourceQueryResults<RoleAssignment>> ListRoleAssignmentsAsync(
        string? subscription,
        string scope,
        string? tenantId = null,
        CancellationToken cancellationToken = default);
}
