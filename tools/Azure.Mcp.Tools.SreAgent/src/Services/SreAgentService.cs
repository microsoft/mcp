// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
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

    #region Agents + Skills (sub-agent A)

    public async Task<SreSubAgent> GetSubAgentAsync(
        string endpoint,
        string name,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(endpoint), endpoint), (nameof(name), name));

        var body = await CallDataPlaneAsync(
            endpoint,
            $"/api/v2/extendedAgent/agents/{Uri.EscapeDataString(name)}",
            HttpMethod.Get,
            tenant: tenant,
            cancellationToken: cancellationToken);

        return DeserializeRequired(body, SreAgentJsonContext.Default.SreSubAgent, $"sub-agent '{name}'");
    }

    public async Task<SreSubAgent> CreateSubAgentAsync(
        string endpoint,
        SreSubAgentCreateRequest request,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(endpoint), endpoint), (nameof(request.Name), request.Name));

        var jsonBody = JsonSerializer.Serialize(request, SreAgentJsonContext.Default.SreSubAgentCreateRequest);
        var body = await CallDataPlaneAsync(
            endpoint,
            $"/api/v2/extendedAgent/agents/{Uri.EscapeDataString(request.Name)}",
            HttpMethod.Put,
            jsonBody,
            tenant,
            cancellationToken);

        return DeserializeOrDefault(
            body,
            SreAgentJsonContext.Default.SreSubAgent,
            new SreSubAgent { Name = request.Name, Type = request.Type, Properties = request.Properties });
    }

    public async Task<SreAgentDeleteResult> DeleteSubAgentAsync(
        string endpoint,
        string name,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(endpoint), endpoint), (nameof(name), name));

        await CallDataPlaneAsync(
            endpoint,
            $"/api/v2/extendedAgent/agents/{Uri.EscapeDataString(name)}",
            HttpMethod.Delete,
            tenant: tenant,
            cancellationToken: cancellationToken);

        return new SreAgentDeleteResult(name, "ExtendedAgent", true);
    }

    public async Task<SreAgentTool> GetAgentToolAsync(
        string endpoint,
        string name,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(endpoint), endpoint), (nameof(name), name));

        var body = await CallDataPlaneAsync(
            endpoint,
            $"/api/v2/extendedAgent/tools/{Uri.EscapeDataString(name)}",
            HttpMethod.Get,
            tenant: tenant,
            cancellationToken: cancellationToken);

        return DeserializeRequired(body, SreAgentJsonContext.Default.SreAgentTool, $"agent tool '{name}'");
    }

    public async Task<SreAgentTool> CreateAgentToolAsync(
        string endpoint,
        SreAgentToolCreateRequest request,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(endpoint), endpoint), (nameof(request.Name), request.Name));

        var jsonBody = JsonSerializer.Serialize(request, SreAgentJsonContext.Default.SreAgentToolCreateRequest);
        var body = await CallDataPlaneAsync(
            endpoint,
            $"/api/v2/extendedAgent/tools/{Uri.EscapeDataString(request.Name)}",
            HttpMethod.Put,
            jsonBody,
            tenant,
            cancellationToken);

        return DeserializeOrDefault(
            body,
            SreAgentJsonContext.Default.SreAgentTool,
            new SreAgentTool { Name = request.Name, Type = request.Type, Properties = request.Properties });
    }

    public async Task<List<SreAgentTool>> ListAgentToolsAsync(
        string endpoint,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(endpoint), endpoint));

        var body = await CallDataPlaneAsync(
            endpoint,
            "/api/v2/extendedAgent/tools",
            HttpMethod.Get,
            tenant: tenant,
            cancellationToken: cancellationToken);

        return DeserializeList(body, SreAgentJsonContext.Default.SreAgentTool);
    }

    public async Task<SreAgentDeleteResult> DeleteAgentToolAsync(
        string endpoint,
        string name,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(endpoint), endpoint), (nameof(name), name));

        await CallDataPlaneAsync(
            endpoint,
            $"/api/v2/extendedAgent/tools/{Uri.EscapeDataString(name)}",
            HttpMethod.Delete,
            tenant: tenant,
            cancellationToken: cancellationToken);

        return new SreAgentDeleteResult(name, "ExtendedAgentTool", true);
    }

    public async Task<List<SreSkill>> ListSkillsAsync(
        string endpoint,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(endpoint), endpoint));

        var body = await CallDataPlaneAsync(
            endpoint,
            "/api/v2/extendedAgent/skills",
            HttpMethod.Get,
            tenant: tenant,
            cancellationToken: cancellationToken);

        return DeserializeList(body, SreAgentJsonContext.Default.SreSkill);
    }

    public async Task<SreSkill> CreateSkillAsync(
        string endpoint,
        SreSkillCreateRequest request,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(endpoint), endpoint), (nameof(request.Name), request.Name));

        var jsonBody = JsonSerializer.Serialize(request, SreAgentJsonContext.Default.SreSkillCreateRequest);
        var body = await CallDataPlaneAsync(
            endpoint,
            $"/api/v2/extendedAgent/skills/{Uri.EscapeDataString(request.Name)}",
            HttpMethod.Put,
            jsonBody,
            tenant,
            cancellationToken);

        return DeserializeOrDefault(
            body,
            SreAgentJsonContext.Default.SreSkill,
            new SreSkill { Name = request.Name, Type = request.Type, Properties = request.Properties });
    }

    private static T DeserializeRequired<T>(string body, System.Text.Json.Serialization.Metadata.JsonTypeInfo<T> jsonTypeInfo, string resourceName)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            throw new InvalidOperationException($"The SRE Agent data-plane response for {resourceName} was empty.");
        }

        return JsonSerializer.Deserialize(body, jsonTypeInfo)
            ?? throw new InvalidOperationException($"The SRE Agent data-plane response for {resourceName} could not be deserialized.");
    }

    private static T DeserializeOrDefault<T>(string body, System.Text.Json.Serialization.Metadata.JsonTypeInfo<T> jsonTypeInfo, T defaultValue)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return defaultValue;
        }

        return JsonSerializer.Deserialize(body, jsonTypeInfo) ?? defaultValue;
    }

    private static List<T> DeserializeList<T>(string body, System.Text.Json.Serialization.Metadata.JsonTypeInfo<T> jsonTypeInfo)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return [];
        }

        using var document = JsonDocument.Parse(body);
        if (!TryResolveArray(document.RootElement, out var arrayElement))
        {
            return [];
        }

        var results = new List<T>();
        foreach (var item in arrayElement.EnumerateArray())
        {
            var value = JsonSerializer.Deserialize(item.GetRawText(), jsonTypeInfo);
            if (value is not null)
            {
                results.Add(value);
            }
        }

        return results;
    }

    private static bool TryResolveArray(JsonElement root, out JsonElement arrayElement)
    {
        if (root.ValueKind == JsonValueKind.Array)
        {
            arrayElement = root;
            return true;
        }

        if (root.ValueKind == JsonValueKind.Object)
        {
            foreach (var propertyName in new[] { "value", "data", "skills", "tools" })
            {
                if (root.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Array)
                {
                    arrayElement = property;
                    return true;
                }
            }

            if (root.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Object)
            {
                foreach (var wrapperName in new[] { "tools", "skills", "agents" })
                {
                    if (data.TryGetProperty(wrapperName, out var wrapper) &&
                        wrapper.ValueKind == JsonValueKind.Object &&
                        wrapper.TryGetProperty("data", out var nestedData) &&
                        nestedData.ValueKind == JsonValueKind.Array)
                    {
                        arrayElement = nestedData;
                        return true;
                    }
                }
            }
        }

        arrayElement = default;
        return false;
    }

    #endregion
}
