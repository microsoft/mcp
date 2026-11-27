// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.ResourceHealth.Models;
using Azure.Mcp.Tools.ResourceHealth.Models.Internal;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using Microsoft.Mcp.Core.Helpers;

namespace Azure.Mcp.Tools.ResourceHealth.Services;

public class ResourceHealthService(IAzureService azureService)
    : BaseAzureService(azureService), IResourceHealthService
{
    private const string ResourceHealthApiVersion = "2025-05-01";

    public async Task<AvailabilityStatus> GetAvailabilityStatusAsync(
        string resourceId,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(resourceId), resourceId));

        // Parse and validate resource ID format using Azure SDK
        ResourceIdentifier parsedResourceId = ResourceIdentifier.Parse(resourceId);
        string relativePath = $"{parsedResourceId}/providers/Microsoft.ResourceHealth/availabilityStatuses/current?api-version={ResourceHealthApiVersion}";
        Uri requestUri = CreateAndValidateRequestUri(AzureService, relativePath);

        AccessToken token = await GetArmAccessTokenAsync(null, cancellationToken);

        HttpClient client = AzureService.GetClient();
        client.DefaultRequestHeaders.Authorization = new("Bearer", token.Token);

        using HttpResponseMessage response = await client.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        await EnsureResourceHealthSuccessAsync(response, cancellationToken, resourceId, parsedResourceId.ResourceType.ToString());

        AvailabilityStatusResponse apiResponse = await response.Content.ReadFromJsonAsync(ResourceHealthJsonContext.Default.AvailabilityStatusResponse, cancellationToken)
            ?? throw new InvalidOperationException($"Failed to deserialize availability status response for resource '{resourceId}'");

        return apiResponse.ToAvailabilityStatus();
    }

    public async Task<List<AvailabilityStatus>> ListAvailabilityStatusesAsync(
        string subscription,
        string? resourceGroup = null,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(subscription), subscription));

        SubscriptionResource subscriptionResource = await AzureService.GetSubscription(subscription, tenant, cancellationToken: cancellationToken);
        string subscriptionId = subscriptionResource.Id.SubscriptionId
            ?? throw new InvalidOperationException("The resolved subscription does not have a subscription ID.");
        string escapedSubscriptionId = Uri.EscapeDataString(subscriptionId);
        string relativePath = resourceGroup != null
            ? $"/subscriptions/{escapedSubscriptionId}/resourceGroups/{Uri.EscapeDataString(resourceGroup)}/providers/Microsoft.ResourceHealth/availabilityStatuses?api-version={ResourceHealthApiVersion}"
            : $"/subscriptions/{escapedSubscriptionId}/providers/Microsoft.ResourceHealth/availabilityStatuses?api-version={ResourceHealthApiVersion}";
        Uri requestUri = CreateAndValidateRequestUri(AzureService, relativePath);

        AccessToken token = await GetArmAccessTokenAsync(tenant, cancellationToken);

        HttpClient client = AzureService.GetClient();
        client.DefaultRequestHeaders.Authorization = new("Bearer", token.Token);

        using HttpResponseMessage response = await client.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        await EnsureResourceHealthSuccessAsync(response, cancellationToken);

        AvailabilityStatusListResponse? apiResponse = await response.Content.ReadFromJsonAsync(ResourceHealthJsonContext.Default.AvailabilityStatusListResponse, cancellationToken);

        if (apiResponse?.Value == null)
        {
            return [];
        }

        return [.. apiResponse.Value.Select(item => item.ToAvailabilityStatus())];
    }

    public async Task<List<ServiceHealthEvent>> ListServiceHealthEventsAsync(
        string subscription,
        string? eventType = null,
        string? status = null,
        string? trackingId = null,
        string? filter = null,
        string? queryStartTime = null,
        string? queryEndTime = null,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(subscription), subscription));

        SubscriptionResource subscriptionResource = await AzureService.GetSubscription(subscription, tenant, cancellationToken: cancellationToken);
        string subscriptionId = subscriptionResource.Id.SubscriptionId
            ?? throw new InvalidOperationException("The resolved subscription does not have a subscription ID.");
        string escapedSubscriptionId = Uri.EscapeDataString(subscriptionId);

        // OData string literals escape apostrophes by doubling them. The completed expression is URI-encoded
        // separately below because OData grammar escaping and URI transport encoding protect different layers.
        static string EscapeODataStringLiteral(string value) =>
            value.Replace("'", "''", StringComparison.Ordinal);

        List<string> filterParts = [];

        if (!string.IsNullOrWhiteSpace(eventType))
        {
            filterParts.Add($"properties/eventType eq '{EscapeODataStringLiteral(eventType)}'");
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            filterParts.Add($"properties/status eq '{EscapeODataStringLiteral(status)}'");
        }

        if (!string.IsNullOrWhiteSpace(trackingId))
        {
            filterParts.Add($"properties/trackingId eq '{EscapeODataStringLiteral(trackingId)}'");
        }

        if (!string.IsNullOrWhiteSpace(filter))
        {
            filterParts.Add(filter);
        }

        // Use Service Health Events API with 2025-05-01 version
        string relativePath = $"/subscriptions/{escapedSubscriptionId}/providers/Microsoft.ResourceHealth/events?api-version={ResourceHealthApiVersion}";

        // Add time range query parameters if provided (not as OData filters)
        if (!string.IsNullOrWhiteSpace(queryStartTime))
        {
            relativePath += $"&queryStartTime={Uri.EscapeDataString(queryStartTime)}";
        }

        if (!string.IsNullOrWhiteSpace(queryEndTime))
        {
            relativePath += $"&queryEndTime={Uri.EscapeDataString(queryEndTime)}";
        }

        // Add OData filters if provided
        if (filterParts.Count > 0)
        {
            string combinedFilter = string.Join(" and ", filterParts);
            relativePath += $"&$filter={Uri.EscapeDataString(combinedFilter)}";
        }

        Uri requestUri = CreateAndValidateRequestUri(AzureService, relativePath);

        AccessToken token = await GetArmAccessTokenAsync(tenant, cancellationToken);

        HttpClient client = AzureService.GetClient();
        client.DefaultRequestHeaders.Authorization = new("Bearer", token.Token);

        using HttpResponseMessage response = await client.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        await EnsureResourceHealthSuccessAsync(response, cancellationToken);

        ServiceHealthEventListResponse? apiResponse = await response.Content.ReadFromJsonAsync(ResourceHealthJsonContext.Default.ServiceHealthEventListResponse, cancellationToken);

        if (apiResponse?.Value == null)
        {
            return [];
        }

        return apiResponse.Value
            .Select(item => item.ToServiceHealthEvent(subscriptionId))
            .Where(evt => !string.IsNullOrEmpty(evt.Id)) // Filter out any invalid entries
            .ToList();
    }

    /// <summary>
    /// Creates and authorizes a Resource Health request URI under the configured Azure Resource Manager endpoint.
    /// </summary>
    /// <param name="azureService">The Azure service using this host's immutable endpoint policy.</param>
    /// <param name="relativePath">
    /// The rooted or relative ARM request path. Absolute and network-path values are rejected unless they resolve
    /// to the configured cloud's exact ARM host.
    /// </param>
    /// <returns>The completed and authorized request URI.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="relativePath"/> is empty or the ARM endpoint allow-list is unavailable.
    /// </exception>
    /// <exception cref="System.Security.SecurityException">
    /// Thrown when the completed request URI is not an HTTPS Azure Resource Manager endpoint for the configured cloud.
    /// </exception>
    internal static Uri CreateAndValidateRequestUri(IAzureService azureService, string relativePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(relativePath);

        Uri requestUri = new(azureService.CloudConfiguration.ArmEnvironment.Endpoint, relativePath);

        // Uri resolution accepts absolute and network-path inputs that can replace the configured ARM authority.
        // Validate the completed URI before acquiring the raw request's token so input cannot redirect that token.
        azureService.ValidateAzureServiceEndpoint(
            endpoint: requestUri.AbsoluteUri,
            serviceType: "arm");

        return requestUri;
    }

    private static async Task EnsureResourceHealthSuccessAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken,
        string? resourceId = null,
        string? resourceType = null)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        var (errorCode, errorMessage) = ParseErrorResponse(responseContent);

        if (response.StatusCode == HttpStatusCode.UnprocessableEntity && resourceId is not null && resourceType is not null)
        {
            throw new ResourceHealthUnprocessableEntityException(
                resourceId,
                resourceType,
                errorCode,
                errorMessage,
                responseContent);
        }

        throw new ResourceHealthRequestFailedException(
            response.StatusCode,
            errorCode,
            errorMessage,
            responseContent);
    }

    private static (string? Code, string? Message) ParseErrorResponse(string responseContent)
    {
        if (string.IsNullOrWhiteSpace(responseContent))
        {
            return (null, null);
        }

        try
        {
            using var jsonDoc = JsonDocument.Parse(responseContent);
            var root = jsonDoc.RootElement;

            if (root.TryGetProperty("error", out var errorElement) && errorElement.ValueKind == JsonValueKind.Object)
            {
                return (GetStringProperty(errorElement, "code"), GetStringProperty(errorElement, "message"));
            }

            return (GetStringProperty(root, "code"), GetStringProperty(root, "message"));
        }
        catch (JsonException)
        {
            return (null, null);
        }
    }

    private static string? GetStringProperty(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;
    }
}
