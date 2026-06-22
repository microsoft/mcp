// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.Authorization.Models;
using Microsoft.Mcp.Core.Options;

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
    Task<ResourceQueryResults<RoleAssignment>> ListRoleAssignmentsAsync(
        string subscription,
        string scope,
        string? tenantId = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists pending Azure RBAC PIM role assignment approvals for the current approver.
    /// </summary>
    /// <param name="subscription">The subscription ID or name to use for authentication context.</param>
    /// <param name="scope">The scope where pending approvals should be queried.</param>
    /// <param name="tenantId">Optional tenant ID for cross-tenant operations.</param>
    /// <param name="retryPolicy">Optional retry policy for the operation.</param>
    /// <param name="cancellationToken">Optional cancellation token for the operation.</param>
    /// <returns>Pending role assignment approvals assigned to the current approver.</returns>
    Task<List<RoleAssignmentApproval>> ListPendingRoleAssignmentApprovalsAsync(
        string subscription,
        string scope,
        string? tenantId = null,
        RetryPolicyOptions? retryPolicy = null,
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
    /// <param name="retryPolicy">Optional retry policy for the operation.</param>
    /// <param name="cancellationToken">Optional cancellation token for the operation.</param>
    /// <returns>The updated approval stage.</returns>
    Task<RoleAssignmentApprovalStage> ApproveRoleAssignmentApprovalAsync(
        string subscription,
        string scope,
        string approval,
        string stage,
        string justification,
        string? tenantId = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);
}
