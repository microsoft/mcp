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
    /// The subscription ID or name to query. Must be <see langword="null"/> when <paramref name="scope"/> is a
    /// management group scope because those assignments live outside any subscription.
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

    /// <summary>
    /// Lists pending Azure RBAC PIM role assignment approvals for the current approver.
    /// </summary>
    /// <param name="subscription">The subscription ID or name to use for authentication context.</param>
    /// <param name="scope">The scope where pending approvals should be queried.</param>
    /// <param name="tenantId">Optional tenant ID for cross-tenant operations.</param>
    /// <param name="cancellationToken">Optional cancellation token for the operation.</param>
    /// <returns>Pending role assignment approvals assigned to the current approver.</returns>
    Task<List<RoleAssignmentApproval>> ListPendingRoleAssignmentApprovalsAsync(
        string subscription,
        string scope,
        string? tenantId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Approves a pending Azure RBAC PIM role assignment approval stage.
    /// </summary>
    /// <param name="subscription">The subscription ID or name to use for authentication context.</param>
    /// <param name="scope">The scope where the approval exists.</param>
    /// <param name="approval">The approval name or full approval resource ID.</param>
    /// <param name="stage">The approval stage name or full stage resource ID.</param>
    /// <param name="justification">The business justification for approval.</param>
    /// <param name="tenantId">Optional tenant ID for cross-tenant operations.</param>
    /// <param name="cancellationToken">Optional cancellation token for the operation.</param>
    /// <returns>The updated approval stage.</returns>
    Task<RoleAssignmentApprovalStage> ApproveRoleAssignmentApprovalAsync(
        string subscription,
        string scope,
        string approval,
        string stage,
        string justification,
        string? tenantId = null,
        CancellationToken cancellationToken = default);
}
