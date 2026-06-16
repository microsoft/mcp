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
using Azure.ResourceManager.Resources;
using Azure.ResourceManager.Resources.Models;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ManagedCleanroom.Services;

public class ManagedCleanroomService(ISubscriptionService subscriptionService, ITenantService tenantService, IHttpClientFactory httpClientFactory)
    : BaseAzureResourceService(subscriptionService, tenantService), IManagedCleanroomService
{
    private readonly ISubscriptionService _subscriptionService = subscriptionService;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));

    private const string CleanRoomApiVersion = "2026-04-30-preview";
    private const string CleanRoomResourceType = "Microsoft.CleanRoom/Collaborations";
    private const string CollaborationsListPath = "gets";
    private static readonly TimeSpan ProvisioningPollInterval = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan ProvisioningTimeout = TimeSpan.FromMinutes(40);

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

        var armClient = await CreateArmClientWithApiVersionAsync(
            CleanRoomResourceType, CleanRoomApiVersion, tenant, retryPolicy, cancellationToken)
            .ConfigureAwait(false);

        var subscriptionResource = await _subscriptionService
            .GetSubscription(subscription, tenant, retryPolicy, cancellationToken)
            .ConfigureAwait(false);

        var resourceId = new ResourceIdentifier(
            $"{subscriptionResource.Id}/resourceGroups/{resourceGroup}/providers/{CleanRoomResourceType}/{name}");

        var payloadBuffer = new ArrayBufferWriter<byte>();
        using (var jsonWriter = new Utf8JsonWriter(payloadBuffer))
        {
            jsonWriter.WriteStartObject();
            jsonWriter.WritePropertyName("collaborators");
            jsonWriter.WriteStartArray();

            foreach (var collaborator in collaborators ?? [])
            {
                jsonWriter.WriteStartObject();
                jsonWriter.WriteString("userIdentifier", collaborator);
                jsonWriter.WriteEndObject();
            }

            jsonWriter.WriteEndArray();
            jsonWriter.WriteString("resourceLocation", resourceLocation ?? location);
            jsonWriter.WriteEndObject();
            jsonWriter.Flush();
        }

        var resourceData = new GenericResourceData(new AzureLocation(location))
        {
            Properties = new BinaryData(payloadBuffer.WrittenMemory)
        };

        await armClient.GetGenericResources()
            .CreateOrUpdateAsync(
                WaitUntil.Started,
                resourceId,
                resourceData,
                cancellationToken)
            .ConfigureAwait(false);

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var timeoutAt = DateTimeOffset.UtcNow + ProvisioningTimeout;
        var resource = armClient.GetGenericResource(resourceId);
        var provisioningState = "Accepted";
        JsonElement properties = default;

        while (provisioningState is not ("Succeeded" or "Failed" or "Canceled"))
        {
            if (DateTimeOffset.UtcNow >= timeoutAt)
            {
                throw new TimeoutException(
                    $"Timed out waiting for collaboration provisioning to reach a terminal state. Last known state: '{provisioningState}'.");
            }

            await Task.Delay(ProvisioningPollInterval, cancellationToken).ConfigureAwait(false);

            Response<GenericResource> getResponse;

            try
            {
                getResponse = await resource.GetAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            }
            catch (RequestFailedException ex) when (ex.Status == (int)HttpStatusCode.NotFound)
            {
                // ARM can briefly return 404 after the initial create starts but before the
                // resource has fully materialized. Keep polling until the timeout or a terminal state.
                continue;
            }

            var propsBytes = getResponse.Value.Data.Properties?.ToArray() ?? [];

            if (propsBytes.Length > 0)
            {
                properties = JsonSerializer.Deserialize(propsBytes, ManagedCleanroomJsonContext.Default.JsonElement);
                provisioningState = properties.TryGetProperty("provisioningState", out var ps)
                    ? ps.GetString() ?? "Unknown"
                    : "Unknown";
            }
        }

        stopwatch.Stop();
        var elapsed = stopwatch.Elapsed;

        if (provisioningState is "Failed" or "Canceled")
        {
            throw new InvalidOperationException(
                $"Collaboration provisioning {provisioningState.ToLowerInvariant()} after " +
                $"{(int)elapsed.TotalMinutes}m {elapsed.Seconds}s. " +
                $"Properties: {properties}");
        }

        var message = $"Collaboration provisioning succeeded after " +
            $"{(int)elapsed.TotalMinutes}m {elapsed.Seconds}s " +
            "(expected ~25 minutes).";

        return new CollaborationCreateResult(properties, message);
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
}
