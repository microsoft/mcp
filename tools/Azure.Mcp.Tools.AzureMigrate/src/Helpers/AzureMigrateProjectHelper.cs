// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.AzureMigrate.Models;
using Azure.ResourceManager;

namespace Azure.Mcp.Tools.AzureMigrate.Helpers;

/// <summary>
/// Helper for creating Azure Migrate projects.
/// </summary>
public sealed class AzureMigrateProjectHelper(
    ISubscriptionService subscriptionService,
    ITenantService tenantService)
    : BaseAzureResourceService(subscriptionService, tenantService)
{
    private readonly ISubscriptionService _subscriptionService = subscriptionService;

    private const string MigrateProjectResourceType = "Microsoft.Migrate/MigrateProjects";
    private const string MigrateProjectApiVersion = "2020-06-01-preview";

    /// <summary>
    /// Creates an Azure Migrate project in the specified resource group.
    /// </summary>
    public async Task<MigrateProjectResult> CreateAzureMigrateProjectAsync(
        string projectName,
        string resourceGroup,
        string location,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(projectName), projectName),
            (nameof(resourceGroup), resourceGroup),
            (nameof(location), location),
            (nameof(subscription), subscription));

        try
        {
            ArmClient armClient = await CreateArmClientWithApiVersionAsync(
                MigrateProjectResourceType,
                MigrateProjectApiVersion,
                null,
                retryPolicy,
                cancellationToken);

            var subscriptionResource = await _subscriptionService.GetSubscription(subscription, cancellationToken: cancellationToken);
            ResourceIdentifier projectId = new ResourceIdentifier(
                $"/subscriptions/{subscriptionResource.Data.SubscriptionId}/resourceGroups/{resourceGroup}/providers/{MigrateProjectResourceType}/{projectName}");

            var createContent = new MigrateProjectCreateContent
            {
                Location = location,
                Properties = new MigrateProjectProperties()
            };

            var result = await CreateOrUpdateGenericResourceAsync(
                armClient,
                projectId,
                location,
                createContent,
                AzureMigrateSerializerContext.Default.MigrateProjectCreateContent,
                cancellationToken);

            if (!result.HasData)
            {
                return new MigrateProjectResult(
                    HasData: false,
                    Id: null,
                    Name: null,
                    Type: null,
                    Location: null,
                    Properties: null);
            }

            return new MigrateProjectResult(
                HasData: true,
                Id: result.Data.Id.ToString(),
                Name: result.Data.Name,
                Type: result.Data.ResourceType.ToString(),
                Location: result.Data.Location,
                Properties: null);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error creating Azure Migrate project '{projectName}': {ex.Message}", ex);
        }
    }
}
