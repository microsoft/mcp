// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Buffers;
using System.Text.Json;
using AnalyticsFrontendAPI;
using Azure;
using Azure.Core;
using Azure.Core.Pipeline;
using Azure.Mcp.Tools.ManagedCleanroom.Commands;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.ResourceManager;
using Azure.ResourceManager.CleanRoom;
using Azure.ResourceManager.CleanRoom.Models;
using Azure.ResourceManager.Resources;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ManagedCleanroom.Services;

public class ManagedCleanroomService(ISubscriptionService subscriptionService, ITenantService tenantService, IHttpClientFactory httpClientFactory)
    : BaseAzureResourceService(subscriptionService, tenantService), IManagedCleanroomServiceDataPlane, IManagedCleanroomServiceControlPlane
{
    private readonly ISubscriptionService _subscriptionService = subscriptionService;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    private static readonly TimeSpan ProvisioningPollInterval = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan ProvisioningTimeout = TimeSpan.FromMinutes(40);

    // Note: These constants are retained for future use in a dedicated status-check command.

    public async Task<JsonElement> ListCollaborationsAsync(
        string endpoint,
        bool? activeOnly = null,
        string? tokenScope = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        var client = await BuildClientAsync(endpoint, tokenScope, tenant, cancellationToken)
            .ConfigureAwait(false);

        var requestContext = new RequestContext { CancellationToken = cancellationToken };
        Response response = await client.GetGetsAsync(activeOnly, requestContext).ConfigureAwait(false);

        return ParseResponse(response);
    }

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

        var collaborationData = new CollaborationData(new Azure.Core.AzureLocation(location))
        {
            ResourceLocation = new Azure.Core.AzureLocation(resourceLocation ?? location)
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

        return new CollaborationCreateResult(default, message);
    }

    private async Task<CollaborationClient> BuildClientAsync(
        string endpoint,
        string? tokenScope,
        string? tenant,
        CancellationToken cancellationToken)
    {
        ValidateRequiredParameters((nameof(endpoint), endpoint));

        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var endpointUri))
        {
            throw new ArgumentException($"Endpoint '{endpoint}' is not a valid absolute URI.", nameof(endpoint));
        }

        if (endpointUri.Scheme != Uri.UriSchemeHttps)
        {
            throw new ArgumentException("Endpoint must use HTTPS.", nameof(endpoint));
        }

        var credential = await GetCredential(tenant, cancellationToken).ConfigureAwait(false);
        var options = new CollaborationClientOptions();
        var scope = ResolveTokenScope(endpointUri, tokenScope);
        options.AddPolicy(
            new BearerTokenAuthenticationPolicy(credential, scope),
            HttpPipelinePosition.PerCall);

        var testProxyUrl = Environment.GetEnvironmentVariable("TEST_PROXY_URL");
        if (!string.IsNullOrWhiteSpace(testProxyUrl))
        {
            options.Transport = new HttpClientTransport(_httpClientFactory.CreateClient());
        }
        else
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };
            options.Transport = new HttpClientTransport(handler);
        }

        return new CollaborationClient(endpointUri, options);
    }

    internal static string ResolveTokenScope(Uri endpointUri, string? tokenScope)
    {
        if (!string.IsNullOrWhiteSpace(tokenScope))
        {
            return tokenScope;
        }

        return $"{endpointUri.GetLeftPart(UriPartial.Authority)}/.default";
    }

    internal static Uri BuildCollaborationsListUri(Uri endpointUri, bool? activeOnly)
    {
        var builder = new UriBuilder(endpointUri);
        var path = builder.Path;
        if (string.IsNullOrEmpty(path) || path == "/")
        {
            builder.Path = "/gets";
        }
        else
        {
            builder.Path = path.TrimEnd('/') + "/gets";
        }

        if (activeOnly.HasValue)
        {
            var activeOnlyQuery = $"activeOnly={(activeOnly.Value ? "true" : "false")}";
            builder.Query = string.IsNullOrEmpty(builder.Query)
                ? activeOnlyQuery
                : builder.Query.TrimStart('?') + "&" + activeOnlyQuery;
        }

        return builder.Uri;
    }

    private static JsonElement ParseResponse(Response response)
    {
        if (response.Content is null)
        {
            return default;
        }

        return JsonSerializer.Deserialize(
            response.Content.ToMemory().Span,
            ManagedCleanroomJsonContext.Default.JsonElement);
    }

    private static JsonElement SerializeCollaborationData(CollaborationData data)
    {
        // Create a minimal JSON representation with the key properties
        // to avoid AOT-incompatible serialization
        using var ms = new System.IO.MemoryStream();
        using (var writer = new Utf8JsonWriter(ms))
        {
            writer.WriteStartObject();
            writer.WriteString("provisioningState", data.ProvisioningState?.ToString());
            writer.WriteString("resourceLocation", data.ResourceLocation?.Name);
            writer.WritePropertyName("collaborators");
            writer.WriteStartArray();
            foreach (var collaborator in data.Collaborators)
            {
                writer.WriteStartObject();
                writer.WriteString("userIdentifier", collaborator.UserIdentifier);
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
