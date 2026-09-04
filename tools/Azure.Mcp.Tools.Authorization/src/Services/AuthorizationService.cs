// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.Authorization.Models;
using Azure.Mcp.Tools.Authorization.Services.Models;

namespace Azure.Mcp.Tools.Authorization.Services;

public class AuthorizationService(IAzureService azureService)
    : BaseAzureResourceService(azureService), IAuthorizationService
{
    private const string RoleAssignmentsTable = "authorizationresources";
    private const string RoleAssignmentResourceType = "Microsoft.Authorization/roleAssignments";

    public async Task<ResourceQueryResults<RoleAssignment>> ListRoleAssignmentsAsync(
        string? subscription,
        string scope,
        string? tenantId = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(scope), scope));

        // Match the scope itself, plus anything nested beneath it. The trailing separator keeps a scope
        // ending in "rg1" from also matching "rg10".
        var escapedScope = EscapeKqlString(scope.TrimEnd('/'));
        var scopeFilter = $"(properties.scope =~ '{escapedScope}' or properties.scope startswith '{escapedScope}/')";

        if (ManagementGroupScope.TryParse(scope, out var managementGroup))
        {
            // Role assignments on a management group are not part of any subscription, so a
            // subscription-scoped Resource Graph query can never return them.
            return await ExecuteManagementGroupResourceQueryAsync(
                RoleAssignmentResourceType,
                managementGroup,
                ConvertToRoleAssignmentModel,
                RoleAssignmentsTable,
                additionalFilter: scopeFilter,
                tenant: tenantId,
                cancellationToken: cancellationToken);
        }

        ValidateRequiredParameters((nameof(subscription), subscription));

        return await ExecuteResourceQueryAsync(
            RoleAssignmentResourceType,
            null, // all resource groups
            subscription!,
            ConvertToRoleAssignmentModel,
            RoleAssignmentsTable,
            additionalFilter: scopeFilter,
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
