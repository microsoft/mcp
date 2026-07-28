// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.IoTOperations.Models;
using Azure.Mcp.Tools.IoTOperations.Services.Models;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.IoTOperations.Services;

public sealed class IoTOperationsService(ISubscriptionService subscriptionService, ITenantService tenantService)
    : BaseAzureResourceService(subscriptionService, tenantService), IIoTOperationsService
{
    private const string InstanceResourceType = "Microsoft.IoTOperations/instances";

    public async Task<ResourceQueryResults<IoTOperationsInstanceInfo>> ListInstancesAsync(
        string subscription,
        string? resourceGroup = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(subscription), subscription));

        return await ExecuteResourceQueryAsync(
            InstanceResourceType,
            resourceGroup,
            subscription,
            retryPolicy,
            ConvertToInstanceInfoModel,
            tenant: tenant,
            cancellationToken: cancellationToken);
    }

    public async Task<IoTOperationsInstanceInfo> GetInstanceAsync(
        string subscription,
        string resourceGroup,
        string instance,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(instance), instance));

        var result = await ExecuteSingleResourceQueryAsync(
            InstanceResourceType,
            resourceGroup: resourceGroup,
            subscription: subscription,
            retryPolicy: retryPolicy,
            converter: ConvertToInstanceInfoModel,
            additionalFilter: $"name =~ '{EscapeKqlString(instance)}'",
            tenant: tenant,
            cancellationToken: cancellationToken);

        if (result == null)
        {
            throw new KeyNotFoundException(
                $"IoT Operations instance '{instance}' not found in resource group '{resourceGroup}' for subscription '{subscription}'.");
        }

        return result;
    }

    private static IoTOperationsInstanceInfo ConvertToInstanceInfoModel(JsonElement item)
    {
        var data = IoTOperationsInstanceData.FromJson(item)
            ?? throw new InvalidOperationException("Failed to deserialize IoT Operations Instance data.");

        return new IoTOperationsInstanceInfo(
            Name: data.Name ?? string.Empty,
            Id: data.Id,
            Location: data.Location,
            ResourceGroup: data.ResourceGroup,
            Type: data.Type,
            ProvisioningState: data.Properties?.ProvisioningState,
            Version: data.Properties?.Version,
            Description: data.Properties?.Description,
            SchemaRegistryResourceId: data.Properties?.SchemaRegistryRef?.ResourceId);
    }
}
