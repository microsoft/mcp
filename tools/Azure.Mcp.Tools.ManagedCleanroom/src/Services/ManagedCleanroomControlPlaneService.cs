// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.ManagedCleanroom.Models;
using Azure.ResourceManager.CleanRoom;
using Azure.ResourceManager.CleanRoom.Models;
using Azure.ResourceManager.Resources;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ManagedCleanroom.Services;

public class ManagedCleanroomControlPlaneService(ISubscriptionService subscriptionService, ITenantService tenantService)
    : BaseAzureResourceService(subscriptionService, tenantService), IManagedCleanroomServiceControlPlane
{
    private readonly ISubscriptionService _subscriptionService = subscriptionService;

    public async Task<CollaborationCreateResult> CreateCollaborationArmResourceAsync(
        string name,
        string resourceGroup,
        string subscription,
        string location,
        string? resourceLocation = null,
        string[]? collaborators = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(name), name),
            (nameof(resourceGroup), resourceGroup),
            (nameof(subscription), subscription),
            (nameof(location), location));

        var armClient = await CreateArmClientAsync(tenant, retryPolicy, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        var subscriptionResource = await _subscriptionService
            .GetSubscription(subscription, tenant, retryPolicy, cancellationToken)
            .ConfigureAwait(false);

        var resourceGroupId = ResourceGroupResource.CreateResourceIdentifier(
            subscriptionResource.Id.SubscriptionId!,
            resourceGroup);
        var resourceGroupResource = armClient.GetResourceGroupResource(resourceGroupId);

        var collaborationData = new CollaborationData(new AzureLocation(location))
        {
            ResourceLocation = new AzureLocation(resourceLocation ?? location)
        };

        foreach (var collaborator in collaborators ?? [])
        {
            collaborationData.Collaborators.Add(new Collaborator
            {
                UserIdentifier = collaborator
            });
        }

        // Fire the ARM PUT and return immediately — provisioning takes ~25 minutes in the background.
        await resourceGroupResource.GetCollaborations()
            .CreateOrUpdateAsync(
                WaitUntil.Started,
                name,
                collaborationData,
                cancellationToken)
            .ConfigureAwait(false);

        var message = $"Collaboration '{name}' creation request accepted. " +
            "Provisioning is running in the background and typically takes ~25 minutes to complete. " +
            $"You can check the status by asking to get the collaboration '{name}' in resource group '{resourceGroup}'.";

        return new CollaborationCreateResult(
            BuildAcceptedCreateResult(name, resourceGroup, subscription, location, resourceLocation, collaborators),
            message);
    }

    private static JsonElement BuildAcceptedCreateResult(
        string name,
        string resourceGroup,
        string subscription,
        string location,
        string? resourceLocation,
        string[]? collaborators)
    {
        using var ms = new System.IO.MemoryStream();
        using (var writer = new Utf8JsonWriter(ms))
        {
            writer.WriteStartObject();
            writer.WriteString("name", name);
            writer.WriteString("resourceGroup", resourceGroup);
            writer.WriteString("subscription", subscription);
            writer.WriteString("location", location);
            writer.WriteString("resourceLocation", resourceLocation ?? location);
            writer.WriteString("provisioningState", "Accepted");
            writer.WritePropertyName("collaborators");
            writer.WriteStartArray();

            foreach (var collaborator in collaborators ?? [])
            {
                writer.WriteStartObject();
                writer.WriteString("userIdentifier", collaborator);
                writer.WriteEndObject();
            }

            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        ms.Position = 0;
        using var doc = JsonDocument.Parse(ms);
        return doc.RootElement.Clone();
    }
}