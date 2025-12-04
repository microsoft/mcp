// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Core;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Models;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.Dps.Commands;
using Azure.Mcp.Tools.Dps.Models;
using Azure.Mcp.Tools.Dps.Services.Models;
using Azure.ResourceManager;

namespace Azure.Mcp.Tools.Dps.Services;

/// <summary>
/// Service for Device Provisioning Service operations.
/// </summary>
/// <param name="subscriptionService">Service for Azure subscription operations.</param>
/// <param name="tenantService">Service for Azure tenant operations.</param>
public class DpsService(
    ISubscriptionService subscriptionService,
    ITenantService tenantService) : BaseAzureResourceService(subscriptionService, tenantService), IDpsService
{
    /// <inheritdoc/>
    public async Task<List<DpsInstanceInfo>> ListInstancesAsync(
        string subscription,
        string? resourceGroup = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteResourceQueryAsync(
            "Microsoft.Devices/provisioningServices",
            resourceGroup,
            subscription,
            retryPolicy,
            ConvertToDpsInstanceInfo,
            cancellationToken: cancellationToken);
    }

    private static string ExtractResourceGroupFromId(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return string.Empty;
        }
        var parts = id.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var rgIndex = Array.IndexOf(parts, "resourceGroups");
        if (rgIndex >= 0 && rgIndex < parts.Length - 1)
        {
            return parts[rgIndex + 1];
        }
        return string.Empty;
    }

    /// <inheritdoc/>
    public async Task<DpsInstanceResult> CreateInstanceAsync(
        string instanceName,
        string resourceGroup,
        string location,
        string subscription,
        string? sku = null,
        int? capacity = null,
        string? allocationPolicy = null,
        string? linkedHubConnectionString = null,
        string? linkedHubLocation = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(instanceName), instanceName),
            (nameof(resourceGroup), resourceGroup),
            (nameof(location), location),
            (nameof(subscription), subscription));

        // Validate linked hub parameters
        if (!string.IsNullOrEmpty(linkedHubConnectionString) && string.IsNullOrEmpty(linkedHubLocation))
        {
            throw new ArgumentException("The linked-hub-location parameter is required when linked-hub-connection-string is provided.");
        }

        try
        {
            // Create ArmClient for deployments
            ArmClient armClient = await CreateArmClientWithApiVersionAsync(
                "Microsoft.Devices/provisioningServices",
                "2022-12-12",
                tenant,
                retryPolicy);

            // Prepare resource identifier
            ResourceIdentifier dpsId = new ResourceIdentifier(
                $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.Devices/provisioningServices/{instanceName}");

            // Prepare create content
            var createContent = new DpsInstanceCreateOrUpdateContent
            {
                Location = location,
                Sku = new ResourceSku
                {
                    Name = string.IsNullOrEmpty(sku) ? "S1" : sku,
                    Tier = "Standard",
                    Capacity = capacity ?? 1
                },
                Properties = new DpsInstanceProperties
                {
                    AllocationPolicy = string.IsNullOrEmpty(allocationPolicy) ? "Hashed" : ValidateAllocationPolicy(allocationPolicy)
                }
            };

            // Add linked IoT Hub if provided
            if (!string.IsNullOrEmpty(linkedHubConnectionString))
            {
                createContent.Properties.IotHubs =
                [
                    new IotHubDefinition
                    {
                        ConnectionString = linkedHubConnectionString,
                        Location = linkedHubLocation
                    }
                ];
            }

            var result = await CreateOrUpdateGenericResourceAsync(
                armClient,
                dpsId,
                location,
                createContent,
                DpsJsonContext.Default.DpsInstanceCreateOrUpdateContent);

            if (!result.HasData)
            {
                return new DpsInstanceResult(
                    HasData: false,
                    Id: null,
                    Name: null,
                    Type: null,
                    Location: null,
                    SkuName: null,
                    SkuTier: null,
                    Properties: null);
            }
            else
            {
                return new DpsInstanceResult(
                    HasData: true,
                    Id: result.Data.Id.ToString(),
                    Name: result.Data.Name,
                    Type: result.Data.ResourceType.ToString(),
                    Location: result.Data.Location,
                    SkuName: result.Data.Sku?.Name,
                    SkuTier: result.Data.Sku?.Tier,
                    Properties: result.Data.Properties?.ToObjectFromJson(DpsJsonContext.Default.IDictionaryStringObject));
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error creating DPS instance '{instanceName}': {ex.Message}", ex);
        }
    }

    private static string ValidateAllocationPolicy(string allocationPolicy)
    {
        var validPolicies = new[] { "Hashed", "GeoLatency", "Static" };
        if (!validPolicies.Contains(allocationPolicy, StringComparer.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                $"Invalid allocation policy '{allocationPolicy}'. Valid values are: {string.Join(", ", validPolicies)}.");
        }
        return allocationPolicy;
    }

    private static DpsInstanceInfo ConvertToDpsInstanceInfo(JsonElement item)
    {
        var id = item.GetProperty("id").GetString() ?? string.Empty;
        var name = item.GetProperty("name").GetString() ?? string.Empty;
        var location = item.GetProperty("location").GetString() ?? string.Empty;

        // Extract resource group from ID using helper
        var resourceGroup = ExtractResourceGroupFromId(id);

        var instance = new DpsInstanceInfo
        {
            Name = name,
            Id = id,
            ResourceGroup = resourceGroup,
            Location = location
        };

        // Extract properties if available
        if (item.TryGetProperty("properties", out var properties))
        {
            if (properties.TryGetProperty("provisioningState", out var provisioningState))
            {
                instance.ProvisioningState = provisioningState.GetString();
            }

            if (properties.TryGetProperty("serviceOperationsHostName", out var serviceOpsHost))
            {
                instance.ServiceOperationsHostName = serviceOpsHost.GetString();
            }

            if (properties.TryGetProperty("deviceProvisioningHostName", out var deviceProvHost))
            {
                instance.DeviceProvisioningHostName = deviceProvHost.GetString();
            }

            if (properties.TryGetProperty("idScope", out var idScope))
            {
                instance.IdScope = idScope.GetString();
            }

            if (properties.TryGetProperty("allocationPolicy", out var allocationPolicy))
            {
                instance.AllocationPolicy = allocationPolicy.GetString();
            }

            if (properties.TryGetProperty("state", out var state))
            {
                instance.State = state.GetString();
            }
        }

        // Extract SKU if available
        if (item.TryGetProperty("sku", out var sku))
        {
            if (sku.TryGetProperty("name", out var skuName))
            {
                instance.Sku = skuName.GetString();
            }
        }

        return instance;
    }
}
