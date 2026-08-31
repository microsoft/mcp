// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.Authorization.Models;
using Azure.Mcp.Tools.Authorization.Services.Models;

namespace Azure.Mcp.Tools.Authorization.Services;

public class AuthorizationService(IAzureService azureService)
    : BaseAzureResourceService(azureService), IAuthorizationService
{
    public async Task<ResourceQueryResults<RoleAssignment>> ListRoleAssignmentsAsync(
        string subscription,
        string scope,
        string? tenantId = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(scope), scope));

        var scopeId = new ResourceIdentifier(scope!);
        return await ExecuteResourceQueryAsync(
            "Microsoft.Authorization/roleAssignments",
            null, // all resource groups
            subscription,
            ConvertToRoleAssignmentModel,
            "authorizationresources",
            additionalFilter: $"id contains '{EscapeKqlString(scope)}'",
            tenant: tenantId,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Converts a JsonElement from Azure Resource Graph query to a role assignment model.
    /// </summary>
    /// <param name="item">The JsonElement containing role assignment data</param>
    /// <returns>The role assignment model</returns>
    private static RoleAssignment ConvertToRoleAssignmentModel(JsonElement item)
    {
        RoleAssignmentData? roleAssignmentData = RoleAssignmentData.FromJson(item)
            ?? throw new InvalidOperationException("Failed to parse role assignment data");

        return new()
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
