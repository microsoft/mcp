// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Core;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.Authorization.Models;
using Azure.Mcp.Tools.Authorization.Services.Models;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Authorization.Services;

public class AuthorizationService(ISubscriptionService subscriptionService, ITenantService tenantService, ILogger<AuthorizationService> logger)
    : BaseAzureResourceService(subscriptionService, tenantService), IAuthorizationService
{
    private readonly ISubscriptionService _subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));
    private readonly ILogger<AuthorizationService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<List<RoleAssignment>> ListRoleAssignmentsAsync(
        string subscription,
        string scope,
        string? tenantId = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(scope), scope));

        try
        {
            var scopeId = new ResourceIdentifier(scope!);
            var roleAssignments = await ExecuteResourceQueryAsync(
                "Microsoft.Authorization/roleAssignments",
                null, // all resource groups
                subscription,
                retryPolicy,
                ConvertToRoleAssignmentModel,
                "authorizationresources",
                additionalFilter: $"id contains '{EscapeKqlString(scope)}'",
                cancellationToken: cancellationToken);

            return roleAssignments;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error retrieving role assignments for scope '{Scope}' and tenant '{Tenant}'",
                scope, tenantId);
            throw;
        }
    }

    /// <summary>
    /// Converts a JsonElement from Azure Resource Graph query to a role assignment model.
    /// </summary>
    /// <param name="item">The JsonElement containing role assignment data</param>
    /// <returns>The role assignment model</returns>
    private static RoleAssignment ConvertToRoleAssignmentModel(JsonElement item)
    {
        RoleAssignmentData? roleAssignmentData = RoleAssignmentData.FromJson(item);
        if (roleAssignmentData == null)
            throw new InvalidOperationException("Failed to parse role assignment data");

        return new RoleAssignment
        {
            Id = roleAssignmentData.ResourceId,
            Name = roleAssignmentData.ResourceName,
            PrincipalId = roleAssignmentData.Properties?.PrincipalId,
            PrincipalType = roleAssignmentData.Properties?.PrincipalType,
            RoleDefinitionId = roleAssignmentData.Properties?.RoleDefinitionId,
            Scope = roleAssignmentData.Properties?.Scope,
            Description = roleAssignmentData.Properties?.Description,
            DelegatedManagedIdentityResourceId = roleAssignmentData.Properties?.DelegatedManagedIdentityResourceId,
            Condition = roleAssignmentData.Properties?.Condition
        };
    }
}
