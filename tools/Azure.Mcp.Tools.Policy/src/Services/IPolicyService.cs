// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Policy.Models;

namespace Azure.Mcp.Tools.Policy.Services;

public interface IPolicyService
{
    /// <summary>
    /// Gets a policy assignment by name and scope.
    /// </summary>
    /// <param name="assignmentName">The name of the policy assignment.</param>
    /// <param name="scope">The scope of the policy assignment.</param>
    /// <param name="subscription">The subscription ID or name.</param>
    /// <param name="tenantId">Optional tenant ID for cross-tenant operations.</param>
    /// <param name="retryPolicy">Optional retry policy for the operation.</param>
    /// <param name="cancellationToken">Optional cancellation token for the operation.</param>
    /// <returns>The policy assignment details.</returns>
    Task<PolicyAssignment?> GetPolicyAssignmentAsync(
        string assignmentName,
        string scope,
        string subscription,
        string? tenantId = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

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

    /// <summary>
    /// Gets a policy definition by name.
    /// </summary>
    /// <param name="definitionName">The name of the policy definition.</param>
    /// <param name="subscription">The subscription ID or name. Optional if managementGroup is specified.</param>
    /// <param name="managementGroup">The management group ID. If specified, retrieves management group-level policy definition.</param>
    /// <param name="tenantId">Optional tenant ID for cross-tenant operations.</param>
    /// <param name="retryPolicy">Optional retry policy for the operation.</param>
    /// <param name="cancellationToken">Optional cancellation token for the operation.</param>
    /// <returns>The policy definition details.</returns>
    Task<PolicyDefinition?> GetPolicyDefinitionAsync(
        string definitionName,
        string? subscription = null,
        string? managementGroup = null,
        string? tenantId = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);
}
