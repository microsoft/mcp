// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Buffers;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Azure;
using Azure.Core;
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
    private const string CollaborationsListPath = "gets";
    private static readonly TimeSpan ProvisioningPollInterval = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan ProvisioningTimeout = TimeSpan.FromMinutes(40);

    // Note: These constants are retained for future use in a dedicated status-check command.

    public async Task<JsonElement> ListCollaborationsAsync(
        string endpoint,
        bool? activeOnly = null,
        bool allowUntrustedCert = false,
        string? tokenScope = null,
        string? tenant = null,
        CancellationToken cancellationToken = default)
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

        var requestUri = BuildCollaborationsListUri(endpointUri, activeOnly);
        using var client = CreateHttpClient(allowUntrustedCert);
        using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

        var credential = await GetCredential(tenant, cancellationToken).ConfigureAwait(false);
        var scope = ResolveTokenScope(endpointUri, tokenScope);
        var token = await credential.GetTokenAsync(
            new TokenRequestContext([scope]),
            cancellationToken).ConfigureAwait(false);

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);

        using var response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var responseBody = await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            var message = responseBody.Length > 0
                ? Encoding.UTF8.GetString(responseBody)
                : $"Managed Cleanroom list request failed with HTTP {(int)response.StatusCode}.";
            throw new InvalidOperationException(message);
        }

        return responseBody.Length == 0
            ? default
            : JsonSerializer.Deserialize(responseBody, ManagedCleanroomJsonContext.Default.JsonElement);
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

    private HttpClient CreateHttpClient(bool allowUntrustedCert)
    {
        var clientName = allowUntrustedCert
            ? ManagedCleanroomSetup.UnsafeHttpClientName
            : ManagedCleanroomSetup.DefaultHttpClientName;

        return _httpClientFactory.CreateClient(clientName);
    }

    internal static Uri BuildCollaborationsListUri(Uri endpointUri, bool? activeOnly)
    {
        // The current frontend route for listing collaborations is /gets.
        var basePath = endpointUri.AbsolutePath.TrimEnd('/');
        var path = string.IsNullOrEmpty(basePath) || basePath == "/"
            ? $"/{CollaborationsListPath}"
            : $"{basePath}/{CollaborationsListPath}";

        var builder = new UriBuilder(endpointUri)
        {
            Path = path
        };

        if (activeOnly.HasValue)
        {
            var existingQuery = endpointUri.Query.TrimStart('?');
            var activeOnlyParam = $"activeOnly={activeOnly.Value.ToString().ToLowerInvariant()}";
            builder.Query = string.IsNullOrEmpty(existingQuery)
                ? activeOnlyParam
                : $"{existingQuery}&{activeOnlyParam}";
        }

        return builder.Uri;
    }

    internal static string ResolveTokenScope(Uri endpointUri, string? tokenScope)
    {
        if (!string.IsNullOrWhiteSpace(tokenScope))
        {
            return tokenScope;
        }

        return $"{endpointUri.GetLeftPart(UriPartial.Authority)}/.default";
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
