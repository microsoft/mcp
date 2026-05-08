// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.SreAgent.Commands;
using Azure.Mcp.Tools.SreAgent.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.SreAgent.Services;

/// <summary>
/// Provides access to the Azure SRE Agent ARM resources (Microsoft.App/SREAgentPreview)
/// and to the per-resource SRE Agent data-plane REST API (https://*.azuresre.ai).
/// </summary>
/// <remarks>
/// ARM enumeration is performed against Azure Resource Graph via
/// <see cref="BaseAzureResourceService.ExecuteResourceQueryAsync{T}"/>.
/// Data-plane calls acquire a bearer token for the SRE Agent data-plane audience
/// <c>https://azuresre.dev/.default</c> (matches the audience the existing .NET CLI
/// in the SRE Agent runtime repo uses, see Agent.Cli/Services/TokenService.cs).
/// </remarks>
public sealed class SreAgentService(
    ISubscriptionService subscriptionService,
    ITenantService tenantService,
    ILogger<SreAgentService> logger)
    : BaseAzureResourceService(subscriptionService, tenantService), ISreAgentService
{
    private const string SreAgentResourceType = "Microsoft.App/SREAgentPreview";
    private static readonly string[] DataPlaneScopes = ["https://azuresre.dev/.default"];

    private readonly ILogger<SreAgentService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<List<SreAgentResource>> ListAgentsAsync(
        string subscription,
        string? resourceGroup = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(subscription), subscription));

        var results = await ExecuteResourceQueryAsync(
            SreAgentResourceType,
            resourceGroup,
            subscription,
            retryPolicy,
            ConvertToSreAgentResource,
            tenant: tenant,
            cancellationToken: cancellationToken);

        return results.Results ?? [];
    }

    /// <summary>
    /// Calls the per-resource SRE Agent data-plane REST API. Acquires a bearer token
    /// for <c>https://azuresre.dev/.default</c> using the active credential and forwards the response body.
    /// </summary>
    /// <param name="endpoint">The agent's data-plane endpoint (e.g. <c>https://my-agent--abc.def.eastus2.azuresre.ai</c>).</param>
    /// <param name="path">Request path beginning with <c>/</c>.</param>
    /// <param name="method">HTTP method.</param>
    /// <param name="jsonBody">Optional JSON body to send.</param>
    /// <param name="tenant">Optional tenant to use when acquiring the credential.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The response body as a string (caller deserializes into the appropriate model).</returns>
    internal async Task<string> CallDataPlaneAsync(
        string endpoint,
        string path,
        HttpMethod method,
        string? jsonBody = null,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(endpoint);
        ArgumentException.ThrowIfNullOrEmpty(path);

        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var endpointUri) ||
            (endpointUri.Scheme != Uri.UriSchemeHttps))
        {
            throw new ArgumentException($"SRE Agent endpoint must be an absolute https URL. Got: '{endpoint}'.", nameof(endpoint));
        }

        var credential = await GetCredential(tenant, cancellationToken);
        var token = await credential.GetTokenAsync(new TokenRequestContext(DataPlaneScopes), cancellationToken);

        var requestUri = new Uri(endpointUri, path);
        using var request = new HttpRequestMessage(method, requestUri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        if (jsonBody is not null)
        {
            request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
        }

        using var http = TenantService.GetClient();
        using var response = await http.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "SRE Agent data-plane call failed. Status={Status} Endpoint={Endpoint} Path={Path}",
                (int)response.StatusCode, endpointUri.Host, path);
            throw new HttpRequestException(
                $"SRE Agent data-plane call to {endpointUri.Host}{path} failed with status {(int)response.StatusCode}: {Truncate(body, 300)}");
        }

        return body;
    }

    private static SreAgentResource ConvertToSreAgentResource(JsonElement item)
    {
        var resource = new SreAgentResource
        {
            Name = TryGetString(item, "name"),
            Id = TryGetString(item, "id"),
            Location = TryGetString(item, "location"),
            ResourceGroup = TryGetString(item, "resourceGroup"),
        };

        if (item.TryGetProperty("properties", out var props) && props.ValueKind == JsonValueKind.Object)
        {
            resource.ProvisioningState = TryGetString(props, "provisioningState");
            // The SRE Agent ARM resource exposes its data-plane URL on properties.endpoint.
            // Some preview API versions used properties.fqdn; check both for forward/back compat.
            resource.Endpoint = TryGetString(props, "endpoint") ?? TryGetString(props, "fqdn");
            if (resource.Endpoint is { Length: > 0 } ep && !ep.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                resource.Endpoint = $"https://{ep}";
            }
        }

        return resource;
    }

    private static string? TryGetString(JsonElement element, string propertyName)
    {
        if (element.ValueKind != JsonValueKind.Object) return null;
        if (!element.TryGetProperty(propertyName, out var prop)) return null;
        return prop.ValueKind == JsonValueKind.String ? prop.GetString() : null;
    }

    private static string Truncate(string? value, int max) =>
        string.IsNullOrEmpty(value) ? string.Empty :
        value.Length <= max ? value : value[..max] + "...";

    #region Threads + ScheduledTasks (sub-agent C)

    public async Task<List<SreAgentThread>> ListThreadsAsync(string endpoint, string? tenant = null, CancellationToken cancellationToken = default)
    {
        var body = await CallDataPlaneAsync(endpoint, "/api/v1/threads", HttpMethod.Get, tenant: tenant, cancellationToken: cancellationToken);
        return DeserializeList(body, SreAgentJsonContext.Default.SreAgentPagedResponseSreAgentThread, SreAgentJsonContext.Default.ListSreAgentThread);
    }

    public async Task<SreAgentThread?> GetThreadAsync(string endpoint, string threadId, string? tenant = null, CancellationToken cancellationToken = default)
    {
        var body = await CallDataPlaneAsync(endpoint, $"/api/v1/threads/{Uri.EscapeDataString(threadId)}", HttpMethod.Get, tenant: tenant, cancellationToken: cancellationToken);
        return JsonSerializer.Deserialize(body, SreAgentJsonContext.Default.SreAgentThread);
    }

    public async Task<List<SreAgentThreadMessage>> GetThreadMessagesAsync(string endpoint, string threadId, string? tenant = null, CancellationToken cancellationToken = default)
    {
        var body = await CallDataPlaneAsync(endpoint, $"/api/v1/threads/{Uri.EscapeDataString(threadId)}/messages", HttpMethod.Get, tenant: tenant, cancellationToken: cancellationToken);
        return DeserializeList(body, SreAgentJsonContext.Default.SreAgentPagedResponseSreAgentThreadMessage, SreAgentJsonContext.Default.ListSreAgentThreadMessage);
    }

    public async Task<SreAgentThread?> CreateThreadAsync(string endpoint, SreAgentThreadCreateRequest request, string? tenant = null, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(request, SreAgentJsonContext.Default.SreAgentThreadCreateRequest);
        var body = await CallDataPlaneAsync(endpoint, "/api/v1/threads", HttpMethod.Post, json, tenant, cancellationToken);
        return JsonSerializer.Deserialize(body, SreAgentJsonContext.Default.SreAgentThread);
    }

    public async Task<SreAgentThreadMessage?> SendThreadMessageAsync(string endpoint, string threadId, SreAgentThreadMessageRequest request, string? tenant = null, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(request, SreAgentJsonContext.Default.SreAgentThreadMessageRequest);
        var body = await CallDataPlaneAsync(endpoint, $"/api/v1/threads/{Uri.EscapeDataString(threadId)}/messages", HttpMethod.Post, json, tenant, cancellationToken);
        return JsonSerializer.Deserialize(body, SreAgentJsonContext.Default.SreAgentThreadMessage);
    }

    public async Task DeleteThreadAsync(string endpoint, string threadId, string? tenant = null, CancellationToken cancellationToken = default)
    {
        await CallDataPlaneAsync(endpoint, $"/api/v1/threads/{Uri.EscapeDataString(threadId)}", HttpMethod.Delete, tenant: tenant, cancellationToken: cancellationToken);
    }

    public async Task ApproveApprovalAsync(string endpoint, string approvalId, SreAgentApprovalRequest request, string? tenant = null, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(request, SreAgentJsonContext.Default.SreAgentApprovalRequest);
        await CallDataPlaneAsync(endpoint, $"/api/v1/approvals/{Uri.EscapeDataString(approvalId)}/approve", HttpMethod.Post, json, tenant, cancellationToken);
    }

    public async Task<List<SreAgentScheduledTask>> ListScheduledTasksAsync(string endpoint, string? tenant = null, CancellationToken cancellationToken = default)
    {
        var body = await CallDataPlaneAsync(endpoint, "/api/v1/scheduledtasks", HttpMethod.Get, tenant: tenant, cancellationToken: cancellationToken);
        return DeserializeList(body, SreAgentJsonContext.Default.SreAgentPagedResponseSreAgentScheduledTask, SreAgentJsonContext.Default.ListSreAgentScheduledTask);
    }

    public async Task<SreAgentScheduledTask?> GetScheduledTaskAsync(string endpoint, string taskId, string? tenant = null, CancellationToken cancellationToken = default)
    {
        var body = await CallDataPlaneAsync(endpoint, $"/api/v1/scheduledtasks/{Uri.EscapeDataString(taskId)}", HttpMethod.Get, tenant: tenant, cancellationToken: cancellationToken);
        return JsonSerializer.Deserialize(body, SreAgentJsonContext.Default.SreAgentScheduledTask);
    }

    public async Task<SreAgentScheduledTask?> CreateScheduledTaskAsync(string endpoint, SreAgentScheduledTaskCreateRequest request, string? tenant = null, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(request, SreAgentJsonContext.Default.SreAgentScheduledTaskCreateRequest);
        var body = await CallDataPlaneAsync(endpoint, "/api/v1/scheduledtasks", HttpMethod.Post, json, tenant, cancellationToken);
        return JsonSerializer.Deserialize(body, SreAgentJsonContext.Default.SreAgentScheduledTask);
    }

    public async Task DeleteScheduledTaskAsync(string endpoint, string taskId, string? tenant = null, CancellationToken cancellationToken = default)
    {
        await CallDataPlaneAsync(endpoint, $"/api/v1/scheduledtasks/{Uri.EscapeDataString(taskId)}", HttpMethod.Delete, tenant: tenant, cancellationToken: cancellationToken);
    }

    public async Task PauseScheduledTaskAsync(string endpoint, string taskId, string? tenant = null, CancellationToken cancellationToken = default)
    {
        await CallDataPlaneAsync(endpoint, $"/api/v1/scheduledtasks/{Uri.EscapeDataString(taskId)}/pause", HttpMethod.Post, tenant: tenant, cancellationToken: cancellationToken);
    }

    public async Task ResumeScheduledTaskAsync(string endpoint, string taskId, string? tenant = null, CancellationToken cancellationToken = default)
    {
        await CallDataPlaneAsync(endpoint, $"/api/v1/scheduledtasks/{Uri.EscapeDataString(taskId)}/resume", HttpMethod.Post, tenant: tenant, cancellationToken: cancellationToken);
    }

    private static List<T> DeserializeList<T>(string body, JsonTypeInfo<SreAgentPagedResponse<T>> pagedTypeInfo, JsonTypeInfo<List<T>> listTypeInfo)
    {
        using var document = JsonDocument.Parse(body);
        if (document.RootElement.ValueKind == JsonValueKind.Object)
        {
            var paged = JsonSerializer.Deserialize(body, pagedTypeInfo);
            return paged?.Value ?? [];
        }

        return document.RootElement.ValueKind == JsonValueKind.Array
            ? JsonSerializer.Deserialize(body, listTypeInfo) ?? []
            : [];
    }

    #endregion
}
