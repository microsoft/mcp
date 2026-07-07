// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Text.RegularExpressions;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.SreAgent.Commands;
using Azure.Mcp.Tools.SreAgent.Models;
using Azure.Mcp.Tools.SreAgent.Options.Threads;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Options;
namespace Azure.Mcp.Tools.SreAgent.Services;

/// <summary>
/// Provides access to the Azure SRE Agent ARM resources (Microsoft.App/agents)
/// and to the per-resource SRE Agent data-plane REST API (https://*.azuresre.ai).
/// </summary>
/// <remarks>
/// ARM enumeration is performed against Azure Resource Graph via
/// <see cref="BaseAzureResourceService.ExecuteResourceQueryAsync{T}"/>.
/// Data-plane calls acquire a bearer token for the SRE Agent data-plane audience
/// <c>59f0a04a-b322-4310-adc9-39ac41e9631e/.default</c> (matches <c>SRE_API_AUDIENCE</c>
/// in the Node SRE Agent CLI at <c>src/Agent/Agent.Cli.Node/src/services/auth.ts</c>).
/// </remarks>
public sealed class SreAgentService(
    ISubscriptionService subscriptionService,
    ITenantService tenantService,
    IHttpClientFactory httpClientFactory,
    ILogger<SreAgentService> logger)
    : BaseAzureResourceService(subscriptionService, tenantService), ISreAgentService
{
    private const string SreAgentResourceType = "Microsoft.App/agents";
    // Audience for SRE Agent data-plane tokens. Matches SRE_API_AUDIENCE in the Node CLI
    // (src/Agent/Agent.Cli.Node/src/services/auth.ts).
    private static readonly string[] DataPlaneScopes = ["59f0a04a-b322-4310-adc9-39ac41e9631e/.default"];
    private const string ArmApiVersion = "2025-05-01-preview";
    private const string FollowUpPrompt = "Please proceed with the investigation using all available tools and information. Use your best judgment and provide your complete findings including root cause analysis and recommended next steps.";

    private static readonly string[] DirectionPatterns =
    [
        "do you want me to", "would you like me to", "shall i", "should i proceed", "should i continue",
        "do you prefer", "would you prefer", "what would you like me to", "how would you like me to",
        "do you want to proceed", "would you like to proceed"
    ];

    private static readonly string[] DataRequestPatterns =
    [
        "could you provide", "can you provide", "please provide", "could you share", "can you share", "please share",
        "could you clarify", "can you clarify", "please clarify", "please specify", "i need more information",
        "i need additional", "what is the", "what are the", "do you have", "do you know", "subscription id",
        "resource group", "tenant id", "cluster name", "connection string", "credentials", "access key"
    ];

    // ARM endpoint and scope are resolved from the cloud configuration to support sovereign clouds.
    private string ArmHost => TenantService.CloudConfiguration.ArmEnvironment.Endpoint.ToString().TrimEnd('/');
    private string[] ArmScopes => [TenantService.CloudConfiguration.ArmEnvironment.DefaultScope];

    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
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

    public async Task<SreAgentResource?> GetAgentAsync(
        string subscription,
        string? resourceGroup = null,
        string agentName = "",
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subscription);
        ArgumentException.ThrowIfNullOrWhiteSpace(agentName);

        return await ExecuteSingleResourceQueryAsync(
            SreAgentResourceType,
            resourceGroup,
            subscription,
            retryPolicy,
            ConvertToSreAgentResource,
            additionalFilter: $"name =~ '{EscapeKqlString(agentName)}'",
            tenant: tenant,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Calls the per-resource SRE Agent data-plane REST API. Acquires a bearer token
    /// for the SRE Agent data-plane audience using the active credential and forwards the response body.
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

        ValidateDataPlaneEndpoint(endpointUri);

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

        using var http = _httpClientFactory.CreateClient();
        using var response = await http.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "SRE Agent data-plane call failed. Status={Status} Endpoint={Endpoint} Path={Path}",
                (int)response.StatusCode, endpointUri.Host, path);
            throw new HttpRequestException(
                $"SRE Agent data-plane call to {endpointUri.Host}{path} failed with status {(int)response.StatusCode}: {SanitizeForErrorMessage(body, 300)}");
        }

        return body;
    }

    /// <summary>
    /// Validates that the SRE Agent data-plane endpoint host belongs to the trusted
    /// <c>*.azuresre.ai</c> domain to prevent SSRF.
    /// </summary>
    private static void ValidateDataPlaneEndpoint(Uri endpointUri)
    {
        var host = endpointUri.Host;
        if (string.IsNullOrEmpty(host) ||
            !host.EndsWith(".azuresre.ai", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                $"SRE Agent endpoint host must end with '.azuresre.ai'. Got: '{host}'.",
                nameof(endpointUri));
        }
    }

    /// <summary>
    /// Calls the Azure Resource Manager (ARM) control plane. Acquires a bearer token for the
    /// ARM audience and appends the SRE Agent preview API version to the query string.
    /// </summary>
    /// <param name="path">Resource-relative ARM path beginning with <c>/subscriptions/...</c>.</param>
    /// <param name="method">HTTP method.</param>
    /// <param name="jsonBody">Optional JSON request body.</param>
    /// <param name="tenant">Optional tenant to use when acquiring the credential.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Response body as a string. Empty string for 204 No Content responses.</returns>
    internal async Task<string> CallArmAsync(
        string path,
        HttpMethod method,
        string? jsonBody = null,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        var credential = await GetCredential(tenant, cancellationToken);
        var token = await credential.GetTokenAsync(new TokenRequestContext(ArmScopes), cancellationToken);

        var separator = path.Contains('?') ? "&" : "?";
        var requestUri = new Uri($"{ArmHost}{path}{separator}api-version={ArmApiVersion}");
        using var request = new HttpRequestMessage(method, requestUri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        if (jsonBody is not null)
        {
            request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
        }

        using var http = _httpClientFactory.CreateClient();
        using var response = await http.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "SRE Agent ARM call failed. Status={Status} Path={Path}",
                (int)response.StatusCode, path);
            throw new HttpRequestException(
                $"SRE Agent ARM call to {path} failed with status {(int)response.StatusCode}: {SanitizeForErrorMessage(body, 300)}");
        }

        return body;
    }

    private static string BuildConnectorArmPath(string subscription, string resourceGroup, string agentName, string? connectorName = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subscription);
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceGroup);
        ArgumentException.ThrowIfNullOrWhiteSpace(agentName);

        var basePath = $"/subscriptions/{Uri.EscapeDataString(subscription)}/resourceGroups/{Uri.EscapeDataString(resourceGroup)}/providers/Microsoft.App/agents/{Uri.EscapeDataString(agentName)}/connectors";
        return string.IsNullOrWhiteSpace(connectorName)
            ? basePath
            : $"{basePath}/{Uri.EscapeDataString(connectorName)}";
    }

    /// <summary>
    /// Resolves the resource group of a SRE Agent by name, scoped to the subscription.
    /// Used by ARM connector operations when the caller does not supply --resource-group.
    /// Uses a single-resource Resource Graph query filtered by name to avoid fetching all agents.
    /// </summary>
    public async Task<string> ResolveAgentResourceGroupAsync(
        string subscription,
        string agentName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subscription);
        ArgumentException.ThrowIfNullOrWhiteSpace(agentName);

        var agent = await ExecuteSingleResourceQueryAsync(
            SreAgentResourceType,
            resourceGroup: null,
            subscription: subscription,
            retryPolicy: retryPolicy,
            converter: ConvertToSreAgentResource,
            additionalFilter: $"name =~ '{EscapeKqlString(agentName)}'",
            tenant: tenant,
            cancellationToken: cancellationToken);

        if (agent is null || string.IsNullOrWhiteSpace(agent.ResourceGroup))
        {
            throw new InvalidOperationException($"SRE Agent resource '{agentName}' was not found in subscription '{subscription}'.");
        }

        return agent.ResourceGroup!;
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
            // The SRE Agent ARM resource exposes its data-plane URL on properties.agentEndpoint.
            // Older preview API versions used properties.endpoint / properties.fqdn; check all for compat.
            resource.Endpoint = TryGetString(props, "agentEndpoint")
                ?? TryGetString(props, "endpoint")
                ?? TryGetString(props, "fqdn");
            if (resource.Endpoint is { Length: > 0 } ep && !ep.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                resource.Endpoint = $"https://{ep}";
            }
        }

        return resource;
    }

    private static string? TryGetString(JsonElement element, string propertyName)
    {
        if (element.ValueKind != JsonValueKind.Object)
            return null;
        if (!element.TryGetProperty(propertyName, out var prop))
            return null;
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

    public async Task<SreAgentDeleteResult> DeleteSkillAsync(
        string endpoint,
        string name,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(endpoint), endpoint), (nameof(name), name));

        await CallDataPlaneAsync(
            endpoint,
            $"/api/v2/extendedAgent/skills/{Uri.EscapeDataString(name)}",
            HttpMethod.Delete,
            tenant: tenant,
            cancellationToken: cancellationToken);

        return new SreAgentDeleteResult(name, "ExtendedAgentSkill", true);
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


    #region Connectors + Hooks (sub-agent B)

    // Connector CRUD operations route through Azure Resource Manager (ARM) so the records end up
    // in the same store the portal reads from. The data-plane endpoint is intentionally NOT used
    // here — writes against /api/v2/extendedAgent/connectors create per-resource records that the
    // portal Connectors page cannot see. TestConnectorAsync still uses the data plane because the
    // testconnection action has no ARM equivalent.
    public async Task<List<AgentConnector>> ListConnectorsAsync(
        string subscription,
        string resourceGroup,
        string agentName,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        var path = BuildConnectorArmPath(subscription, resourceGroup, agentName);
        var body = await CallArmAsync(path, HttpMethod.Get, tenant: tenant, cancellationToken: cancellationToken);
        return [.. DeserializeArmValueArray(body, SreAgentJsonContext.Default.AgentConnectorEnvelope).Select(ToConnector)];
    }

    public async Task<AgentConnector> GetConnectorAsync(
        string subscription,
        string resourceGroup,
        string agentName,
        string name,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        var path = BuildConnectorArmPath(subscription, resourceGroup, agentName, name);
        var body = await CallArmAsync(path, HttpMethod.Get, tenant: tenant, cancellationToken: cancellationToken);
        var envelope = JsonSerializer.Deserialize(body, SreAgentJsonContext.Default.AgentConnectorEnvelope)
            ?? throw new InvalidOperationException($"Connector '{name}' returned an empty response.");
        return ToConnector(envelope);
    }

    public async Task<AgentConnector> CreateOrUpdateConnectorAsync(
        string subscription,
        string resourceGroup,
        string agentName,
        string name,
        AgentConnectorEnvelope connector,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(connector);
        // ARM expects only the properties wrapper; top-level name/type belong in the URL/route.
        var armEnvelope = new AgentConnectorEnvelope { Properties = connector.Properties };
        var jsonBody = JsonSerializer.Serialize(armEnvelope, SreAgentJsonContext.Default.AgentConnectorEnvelope);
        var path = BuildConnectorArmPath(subscription, resourceGroup, agentName, name);
        var body = await CallArmAsync(path, HttpMethod.Put, jsonBody, tenant, cancellationToken);
        var envelope = JsonSerializer.Deserialize(body, SreAgentJsonContext.Default.AgentConnectorEnvelope);
        if (envelope is not null)
        {
            return ToConnector(envelope);
        }
        // ARM returned an empty body. Fall back to the request envelope but redact secrets
        // before returning, since the caller-supplied ExtendedProperties still contain
        // resolved bearer tokens / passwords from environment variables.
        var fallback = connector.Properties ?? new AgentConnector { Name = name };
        RedactSecretsInPlace(fallback.ExtendedProperties);
        return fallback;
    }

    public async Task DeleteConnectorAsync(
        string subscription,
        string resourceGroup,
        string agentName,
        string name,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        var path = BuildConnectorArmPath(subscription, resourceGroup, agentName, name);
        await CallArmAsync(path, HttpMethod.Delete, tenant: tenant, cancellationToken: cancellationToken);
    }

    private static List<T> DeserializeArmValueArray<T>(string body, JsonTypeInfo<T> typeInfo)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return [];
        }

        using var document = JsonDocument.Parse(body);
        if (!document.RootElement.TryGetProperty("value", out var valueArray) || valueArray.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        var results = new List<T>(valueArray.GetArrayLength());
        foreach (var item in valueArray.EnumerateArray())
        {
            var deserialized = item.Deserialize(typeInfo);
            if (deserialized is not null)
            {
                results.Add(deserialized);
            }
        }

        return results;
    }

    public async Task<ConnectorTestResult> TestConnectorAsync(
        string endpoint,
        string name,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        var encodedName = Uri.EscapeDataString(name);
        var connectorBody = await CallDataPlaneAsync(endpoint, $"/api/v2/extendedAgent/connectors/{encodedName}", HttpMethod.Get, tenant: tenant, cancellationToken: cancellationToken);
        var body = await CallDataPlaneAsync(endpoint, $"/api/v2/extendedAgent/connectors/{encodedName}/testconnection", HttpMethod.Post, connectorBody, tenant, cancellationToken);
        return JsonSerializer.Deserialize(body, SreAgentJsonContext.Default.ConnectorTestResult)
            ?? throw new InvalidOperationException($"Connector '{name}' test returned an empty response.");
    }

    public async Task<List<HookEnvelope>> ListHooksAsync(
        string endpoint,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        var body = await CallDataPlaneAsync(endpoint, "/api/v2/extendedAgent/hooks", HttpMethod.Get, tenant: tenant, cancellationToken: cancellationToken);
        return DeserializeArray(body, SreAgentJsonContext.Default.ListHookEnvelope);
    }

    public async Task<HookEnvelope> GetHookAsync(
        string endpoint,
        string name,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        var body = await CallDataPlaneAsync(endpoint, $"/api/v2/extendedAgent/hooks/{Uri.EscapeDataString(name)}", HttpMethod.Get, tenant: tenant, cancellationToken: cancellationToken);
        return JsonSerializer.Deserialize(body, SreAgentJsonContext.Default.HookEnvelope)
            ?? throw new InvalidOperationException($"Hook '{name}' returned an empty response.");
    }

    public async Task DeleteHookAsync(
        string endpoint,
        string name,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        await CallDataPlaneAsync(endpoint, $"/api/v2/extendedAgent/hooks/{Uri.EscapeDataString(name)}", HttpMethod.Delete, tenant: tenant, cancellationToken: cancellationToken);
    }

    public async Task<ThreadHooksResponse> ListThreadHooksAsync(
        string endpoint,
        string threadId,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(threadId);
        var body = await CallDataPlaneAsync(endpoint, $"/api/v1/threads/{Uri.EscapeDataString(threadId)}/hooks", HttpMethod.Get, tenant: tenant, cancellationToken: cancellationToken);
        return JsonSerializer.Deserialize(body, SreAgentJsonContext.Default.ThreadHooksResponse) ?? new ThreadHooksResponse { Hooks = [] };
    }

    public async Task ActivateThreadHookAsync(
        string endpoint,
        string threadId,
        string hookName,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(threadId);
        ArgumentException.ThrowIfNullOrWhiteSpace(hookName);
        await CallDataPlaneAsync(endpoint, $"/api/v1/threads/{Uri.EscapeDataString(threadId)}/hooks/{Uri.EscapeDataString(hookName)}/activate", HttpMethod.Post, "{}", tenant, cancellationToken);
    }

    public async Task DeactivateThreadHookAsync(
        string endpoint,
        string threadId,
        string hookName,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(threadId);
        ArgumentException.ThrowIfNullOrWhiteSpace(hookName);
        await CallDataPlaneAsync(endpoint, $"/api/v1/threads/{Uri.EscapeDataString(threadId)}/hooks/{Uri.EscapeDataString(hookName)}/deactivate", HttpMethod.Post, "{}", tenant, cancellationToken);
    }

    private static AgentConnector ToConnector(AgentConnectorEnvelope envelope)
    {
        var connector = envelope.Properties ?? new AgentConnector();
        connector.Name ??= envelope.Name;
        RedactSecretsInPlace(connector.ExtendedProperties);
        return connector;
    }

    // Keys whose values should never be returned to MCP clients. Matched case-insensitively.
    private static readonly HashSet<string> SensitiveExtendedPropertyKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "bearerToken",
        "apiKey",
        "password",
        "secret",
        "clientSecret",
        "accessToken",
        "refreshToken",
        "authorization",
        "token",
    };

    private const string RedactedValue = "***";

    /// <summary>
    /// Walks an extended-properties dictionary and replaces values for known secret-bearing
    /// keys with a redaction marker. Recurses into nested objects (e.g. <c>headers</c> maps
    /// that may contain an <c>Authorization</c> entry) and into <see cref="JsonElement"/>
    /// values produced by the source-generated deserializer.
    /// </summary>
    internal static void RedactSecretsInPlace(Dictionary<string, object>? properties)
    {
        if (properties is null || properties.Count == 0)
        {
            return;
        }

        foreach (var key in properties.Keys.ToList())
        {
            if (SensitiveExtendedPropertyKeys.Contains(key))
            {
                properties[key] = RedactedValue;
                continue;
            }

            properties[key] = RedactNestedValue(properties[key]);
        }
    }

    private static object RedactNestedValue(object? value)
    {
        switch (value)
        {
            case null:
                return null!;
            case Dictionary<string, object> nested:
                RedactSecretsInPlace(nested);
                return nested;
            case JsonElement element when element.ValueKind == JsonValueKind.Object:
                var dict = new Dictionary<string, object>();
                foreach (var prop in element.EnumerateObject())
                {
                    if (SensitiveExtendedPropertyKeys.Contains(prop.Name))
                    {
                        dict[prop.Name] = RedactedValue;
                    }
                    else
                    {
                        dict[prop.Name] = RedactNestedValue(prop.Value);
                    }
                }
                return dict;
            default:
                return value;
        }
    }

    // Regexes used to strip secrets out of upstream error response bodies before they are
    // surfaced in exception messages or written to structured logs.
    private static readonly Regex SecretJsonValueRegex = new(
        "(\"(?:bearerToken|apiKey|api_key|password|secret|clientSecret|client_secret|accessToken|access_token|refreshToken|refresh_token|token|authorization)\"\\s*:\\s*)\"[^\"]*\"",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex BasicAuthHeaderRegex = new(
        "(\"Authorization\"\\s*:\\s*\")(?:Bearer|Basic)\\s+[^\"]*\"",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex BearerAuthHeaderRegex = new(
        "(?i)(Authorization\\s*:\\s*)(?:Bearer|Basic)\\s+\\S+",
        RegexOptions.Compiled);

    /// <summary>
    /// Sanitizes an upstream response body for inclusion in an exception message.
    /// Redacts JSON values for well-known secret keys and any Bearer/Basic Authorization
    /// header values, then truncates to <paramref name="max"/> characters. ARM and
    /// data-plane error bodies frequently echo parts of the request, including the
    /// secrets we sent, so this scrub is required before the body is logged or thrown.
    /// </summary>
    internal static string SanitizeForErrorMessage(string? value, int max)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        var scrubbed = SecretJsonValueRegex.Replace(value, "$1\"" + RedactedValue + "\"");
        scrubbed = BasicAuthHeaderRegex.Replace(scrubbed, "$1" + RedactedValue + "\"");
        scrubbed = BearerAuthHeaderRegex.Replace(scrubbed, "$1" + RedactedValue);
        return Truncate(scrubbed, max);
    }
    private static List<T> DeserializeArray<T>(string body, JsonTypeInfo<List<T>> jsonTypeInfo)
    {
        using var document = JsonDocument.Parse(body);
        if (document.RootElement.ValueKind == JsonValueKind.Array)
        {
            return JsonSerializer.Deserialize(body, jsonTypeInfo) ?? [];
        }

        if (document.RootElement.ValueKind == JsonValueKind.Object &&
            document.RootElement.TryGetProperty("value", out var valueElement) &&
            valueElement.ValueKind == JsonValueKind.Array)
        {
            return JsonSerializer.Deserialize(valueElement.GetRawText(), jsonTypeInfo) ?? [];
        }

        return [];
    }

    #endregion




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
        await CallDataPlaneAsync(endpoint, $"/api/v1/scheduledtasks/{Uri.EscapeDataString(taskId)}/pause", HttpMethod.Post, "{}", tenant, cancellationToken);
    }

    public async Task ResumeScheduledTaskAsync(string endpoint, string taskId, string? tenant = null, CancellationToken cancellationToken = default)
    {
        await CallDataPlaneAsync(endpoint, $"/api/v1/scheduledtasks/{Uri.EscapeDataString(taskId)}/resume", HttpMethod.Post, "{}", tenant, cancellationToken);
    }

    public async Task<List<SreAgentThreadMessage>> PollThreadForCompletionAsync(string endpoint, string threadId, string? tenant, TimeSpan timeout, bool autoApprove, CancellationToken cancellationToken = default)
    {
        var endTime = DateTimeOffset.UtcNow + timeout;
        List<SreAgentThreadMessage> messages = [];
        while (DateTimeOffset.UtcNow < endTime)
        {
            cancellationToken.ThrowIfCancellationRequested();
            messages = await GetThreadMessagesAsync(endpoint, threadId, tenant, cancellationToken);

            if (autoApprove)
            {
                await ApprovePendingApprovalsAsync(this, endpoint, messages, tenant, cancellationToken);
            }
            else if (HasPendingInteractiveRequest(messages))
            {
                return messages;
            }

            var thread = await GetThreadAsync(endpoint, threadId, tenant, cancellationToken);
            var last = thread?.LastMessage;
            var isAgentMessage = !string.Equals(last?.Author?.Role, "user", StringComparison.OrdinalIgnoreCase);
            if (isAgentMessage && last?.IsComplete == true)
            {
                return messages.Count > 0
                    ? messages
                    : await GetThreadMessagesAsync(endpoint, threadId, tenant, cancellationToken);
            }

            await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);
        }

        return messages;
    }

    private static async Task ApprovePendingApprovalsAsync(
        ISreAgentService service,
        string endpoint,
        List<SreAgentThreadMessage> messages,
        string? tenant,
        CancellationToken cancellationToken)
    {
        var (userId, _) = SreAgentCommandHelpers.GetUser("mcp-yolo");
        var approvals = messages
            .Where(m => !string.IsNullOrWhiteSpace(m.Approval?.Id) && IsPendingApproval(m.Approval.Status))
            .Select(m => m.Approval!.Id!)
            .Distinct(StringComparer.OrdinalIgnoreCase);

        foreach (var approvalId in approvals)
        {
            await service.ApproveApprovalAsync(endpoint, approvalId, new(userId), tenant, cancellationToken);
        }
    }

    private static bool IsPendingApproval(string? status) =>
        string.Equals(status, "Pending", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(status, "PendingAuthorization", StringComparison.OrdinalIgnoreCase);

    private static bool HasPendingInteractiveRequest(List<SreAgentThreadMessage> messages) =>
        messages.Any(m =>
            (string.Equals(m.MessageType, "UserQuestion", StringComparison.OrdinalIgnoreCase) && string.Equals(m.UserQuestion?.Status, "Pending", StringComparison.OrdinalIgnoreCase)) ||
            (string.Equals(m.MessageType, "Approval", StringComparison.OrdinalIgnoreCase) && IsPendingApproval(m.Approval?.Status)));

    public static async Task<SreAgentInvestigationResult> RunInvestigationAsync(ISreAgentService service, ThreadsInvestigateOptions options, bool autoApprove, CancellationToken cancellationToken = default)
    {
        var endpoint = await SreAgentCommandHelpers.ResolveAgentEndpointAsync(
            service,
            options,
            cancellationToken);
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var timeoutSeconds = Math.Max(1, options.TimeoutSeconds ?? 600);
        timeout.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));

        var thread = await service.CreateThreadAsync(endpoint, SreAgentCommandHelpers.CreateThreadRequest(options.Message, options.Agent), options.Tenant, timeout.Token);
        var threadId = thread?.Id;
        if (string.IsNullOrWhiteSpace(threadId))
        {
            return new(null, "failed", 0, true, "Thread created but no ID was returned.", []);
        }

        var messages = await service.PollThreadForCompletionAsync(endpoint, threadId, options.Tenant, TimeSpan.FromSeconds(timeoutSeconds), autoApprove, timeout.Token);
        var followUps = 0;
        var maxIterations = Math.Max(0, options.MaxIterations ?? 20);
        while (followUps < maxIterations)
        {
            var action = ClassifyFollowUp(messages);
            if (action == FollowUpAction.None)
            {
                break;
            }

            if (action == FollowUpAction.NeedsData && !autoApprove)
            {
                return new(threadId, "needs-data", followUps, true, LastAgentText(messages), messages);
            }

            if (action == FollowUpAction.NeedsData && autoApprove)
            {
                await ApprovePendingApprovalsAsync(service, endpoint, messages, options.Tenant, timeout.Token);
            }

            await service.SendThreadMessageAsync(endpoint, threadId, SreAgentCommandHelpers.CreateMessageRequest(FollowUpPrompt), options.Tenant, timeout.Token);
            messages = await service.PollThreadForCompletionAsync(endpoint, threadId, options.Tenant, TimeSpan.FromSeconds(timeoutSeconds), autoApprove, timeout.Token);
            followUps++;
        }

        var status = followUps >= maxIterations ? "max-iterations-reached" : "completed";
        return new(threadId, status, followUps, false, null, messages);
    }

    private static string? LastAgentText(List<SreAgentThreadMessage> messages) => messages
        .LastOrDefault(m => !string.Equals(m.Author?.Role, "user", StringComparison.OrdinalIgnoreCase))?.Text;

    private static FollowUpAction ClassifyFollowUp(List<SreAgentThreadMessage> messages)
    {
        var agentMessages = messages
            .Where(m => !string.Equals(m.Author?.Role, "user", StringComparison.OrdinalIgnoreCase))
            .ToList();
        if (agentMessages.Count == 0)
        {
            return FollowUpAction.None;
        }

        var last = agentMessages[^1];
        if (string.Equals(last.MessageType, "UserQuestion", StringComparison.OrdinalIgnoreCase) &&
            string.Equals(last.UserQuestion?.Status, "Pending", StringComparison.OrdinalIgnoreCase))
        {
            return FollowUpAction.NeedsData;
        }

        if (string.Equals(last.MessageType, "Approval", StringComparison.OrdinalIgnoreCase) && IsPendingApproval(last.Approval?.Status))
        {
            return FollowUpAction.NeedsData;
        }

        var text = (last.Text ?? string.Empty).ToLowerInvariant().Trim();
        if (DataRequestPatterns.Any(text.Contains))
        {
            return FollowUpAction.NeedsData;
        }

        return DirectionPatterns.Any(text.Contains) ? FollowUpAction.Auto : FollowUpAction.None;
    }

    private enum FollowUpAction
    {
        None,
        Auto,
        NeedsData
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




    #region Incidents + Workflows + Docs + Architecture (sub-agent D)

    public async Task<List<ThreadListItem>> ListIncidentThreadsAsync(string endpoint, string? tenant = null, CancellationToken cancellationToken = default)
    {
        var body = await CallDataPlaneAsync(endpoint, "/api/v1/threads", HttpMethod.Get, tenant: tenant, cancellationToken: cancellationToken);
        return SreAgentPortedCommandHelpers.DeserializeArray(body, SreAgentJsonContext.Default.ListThreadListItem);
    }

    public async Task<IncidentThreadResponse?> CreateIncidentThreadAsync(string endpoint, IncidentThreadCreateRequest request, string? tenant = null, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(request, SreAgentJsonContext.Default.IncidentThreadCreateRequest);
        var body = await CallDataPlaneAsync(endpoint, "/api/v1/threads", HttpMethod.Post, json, tenant, cancellationToken);
        return JsonSerializer.Deserialize(body, SreAgentJsonContext.Default.IncidentThreadResponse);
    }

    public async Task<List<IncidentFilter>> ListIncidentFiltersAsync(string endpoint, string? tenant = null, CancellationToken cancellationToken = default)
    {
        var body = await CallDataPlaneAsync(endpoint, "/api/v1/incidentplayground/filters", HttpMethod.Get, tenant: tenant, cancellationToken: cancellationToken);
        return SreAgentPortedCommandHelpers.DeserializeArray(body, SreAgentJsonContext.Default.ListIncidentFilter);
    }

    public async Task<List<IncidentHandler>> ListIncidentHandlersAsync(string endpoint, string? tenant = null, CancellationToken cancellationToken = default)
    {
        var body = await CallDataPlaneAsync(endpoint, "/api/v1/incidentplayground/handlers", HttpMethod.Get, tenant: tenant, cancellationToken: cancellationToken);
        return SreAgentPortedCommandHelpers.DeserializeArray(body, SreAgentJsonContext.Default.ListIncidentHandler);
    }

    public async Task CreateOrUpdateIncidentFilterAsync(string endpoint, string filterId, IncidentFilterPayload payload, string? tenant = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filterId);
        var json = JsonSerializer.Serialize(payload, SreAgentJsonContext.Default.IncidentFilterPayload);
        await CallDataPlaneAsync(endpoint, $"/api/v1/incidentplayground/filters/{Uri.EscapeDataString(filterId)}", HttpMethod.Put, json, tenant, cancellationToken);
    }

    public async Task DeleteIncidentFilterAsync(string endpoint, string filterId, string? tenant = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filterId);
        await CallDataPlaneAsync(endpoint, $"/api/v1/incidentplayground/filters/{Uri.EscapeDataString(filterId)}", HttpMethod.Delete, tenant: tenant, cancellationToken: cancellationToken);
    }

    public async Task EnableIncidentFilterAsync(string endpoint, string filterId, string? tenant = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filterId);
        await CallDataPlaneAsync(endpoint, $"/api/v1/incidentplayground/filters/{Uri.EscapeDataString(filterId)}/enable", HttpMethod.Post, tenant: tenant, cancellationToken: cancellationToken);
    }

    public async Task CreateOrUpdateIncidentHandlerAsync(string endpoint, string handlerId, IncidentHandler payload, string? tenant = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(handlerId);
        var json = JsonSerializer.Serialize(payload, SreAgentJsonContext.Default.IncidentHandler);
        await CallDataPlaneAsync(endpoint, $"/api/v1/incidentplayground/handlers/{Uri.EscapeDataString(handlerId)}", HttpMethod.Put, json, tenant, cancellationToken);
    }

    public async Task ApplyExtendedAgentResourceAsync(string endpoint, string kind, string name, ExtendedAgentResourceEnvelope payload, string? tenant = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(kind);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        var path = kind switch
        {
            "ExtendedAgentTool" => $"/api/v2/extendedAgent/tools/{Uri.EscapeDataString(name)}",
            "ExtendedAgent" => $"/api/v2/extendedAgent/agents/{Uri.EscapeDataString(name)}",
            _ => throw new ArgumentException($"Unsupported extended agent resource kind '{kind}'.", nameof(kind))
        };
        var json = JsonSerializer.Serialize(payload, SreAgentJsonContext.Default.ExtendedAgentResourceEnvelope);
        await CallDataPlaneAsync(endpoint, path, HttpMethod.Put, json, tenant, cancellationToken);
    }

    public async Task<List<DocumentInfo>> ListMemoriesAsync(string endpoint, string? tenant = null, CancellationToken cancellationToken = default)
    {
        var body = await CallDataPlaneAsync(endpoint, "/api/v1/AgentMemory/files", HttpMethod.Get, tenant: tenant, cancellationToken: cancellationToken);
        return SreAgentPortedCommandHelpers.DeserializeArray(body, SreAgentJsonContext.Default.ListDocumentInfo);
    }

    public async Task DeleteMemoryAsync(string endpoint, string name, string? tenant = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        // The SRE Agent stores uploaded memories as <name>.md files. Append the extension
        // when the caller omits it so DELETE matches the stored filename.
        var fileName = name.EndsWith(".md", StringComparison.OrdinalIgnoreCase) ? name : name + ".md";
        await CallDataPlaneAsync(endpoint, $"/api/v1/AgentMemory/document/{Uri.EscapeDataString(fileName)}", HttpMethod.Delete, tenant: tenant, cancellationToken: cancellationToken);
    }

    public async Task<List<MemorySearchResult>> SearchMemoriesAsync(string endpoint, string query, int k = 10, string? tenant = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(query);
        var body = await CallDataPlaneAsync(endpoint, $"/api/v1/AgentMemory/documents?query={Uri.EscapeDataString(query)}&k={k}", HttpMethod.Get, tenant: tenant, cancellationToken: cancellationToken);

        // The SRE Agent memory search endpoint returns { "results": [ "<json-string>", ... ] }
        // where each element is itself a JSON-serialized MemorySearchResult. Parse the outer
        // envelope and then deserialize each inner string into a model.
        if (string.IsNullOrWhiteSpace(body))
        {
            return [];
        }
        using var document = JsonDocument.Parse(body);
        if (document.RootElement.ValueKind != JsonValueKind.Object ||
            !document.RootElement.TryGetProperty("results", out var resultsElement) ||
            resultsElement.ValueKind != JsonValueKind.Array)
        {
            return SreAgentPortedCommandHelpers.DeserializeArray(body, SreAgentJsonContext.Default.ListMemorySearchResult);
        }
        var list = new List<MemorySearchResult>();
        foreach (var item in resultsElement.EnumerateArray())
        {
            string? innerJson = item.ValueKind switch
            {
                JsonValueKind.String => item.GetString(),
                JsonValueKind.Object => item.GetRawText(),
                _ => null
            };
            if (string.IsNullOrWhiteSpace(innerJson))
            {
                continue;
            }
            try
            {
                var result = JsonSerializer.Deserialize(innerJson, SreAgentJsonContext.Default.MemorySearchResult);
                if (result is not null)
                {
                    list.Add(result);
                }
            }
            catch (JsonException)
            {
                // Skip results that fail to parse rather than failing the entire search.
            }
        }
        return list;
    }

    public async Task ReindexMemoriesAsync(string endpoint, string? tenant = null, CancellationToken cancellationToken = default)
    {
        await CallDataPlaneAsync(endpoint, "/api/v1/AgentMemory/rebuildIndex", HttpMethod.Post, tenant: tenant, cancellationToken: cancellationToken);
    }

    public async Task UploadMemoryAsync(string endpoint, string fileName, string content, string? tenant = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(endpoint);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var endpointUri) ||
            endpointUri.Scheme != Uri.UriSchemeHttps)
        {
            throw new ArgumentException($"SRE Agent endpoint must be an absolute https URL. Got: '{endpoint}'.", nameof(endpoint));
        }
        var requestUri = new Uri(endpointUri, "/api/v1/AgentMemory/upload");
        var credential = await GetCredential(tenant, cancellationToken);
        var token = await credential.GetTokenAsync(new TokenRequestContext(DataPlaneScopes), cancellationToken);

        using var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
        using var multipartContent = new MultipartFormDataContent();
        var fileContent = new StringContent(content ?? string.Empty, Encoding.UTF8, "text/markdown");
        multipartContent.Add(fileContent, "files", fileName);
        request.Content = multipartContent;

        using var http = _httpClientFactory.CreateClient();
        using var response = await http.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"SRE Agent memory upload to {endpointUri.Host} failed with status {(int)response.StatusCode}: {SanitizeForErrorMessage(body, 300)}");
        }
    }

    public async Task<List<CommonPromptEnvelope>> ListCommonPromptsAsync(string endpoint, string? search = null, string? tenant = null, CancellationToken cancellationToken = default)
    {
        var query = string.IsNullOrWhiteSpace(search) ? string.Empty : $"?search={Uri.EscapeDataString(search)}";
        var body = await CallDataPlaneAsync(endpoint, $"/api/v2/extendedAgent/commonprompts{query}", HttpMethod.Get, tenant: tenant, cancellationToken: cancellationToken);
        return SreAgentPortedCommandHelpers.DeserializeArray(body, SreAgentJsonContext.Default.ListCommonPromptEnvelope);
    }

    public async Task<CommonPromptEnvelope?> GetCommonPromptAsync(string endpoint, string name, string? tenant = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        var body = await CallDataPlaneAsync(endpoint, $"/api/v2/extendedAgent/commonprompts/{Uri.EscapeDataString(name)}", HttpMethod.Get, tenant: tenant, cancellationToken: cancellationToken);
        return JsonSerializer.Deserialize(body, SreAgentJsonContext.Default.CommonPromptEnvelope);
    }

    public async Task CreateOrUpdateCommonPromptAsync(string endpoint, string name, string content, string? tenant = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        var envelope = new CommonPromptEnvelope
        {
            Name = name,
            Type = "CommonPrompt",
            Properties = new CommonPromptProperties { Prompt = content }
        };
        var json = JsonSerializer.Serialize(envelope, SreAgentJsonContext.Default.CommonPromptEnvelope);
        await CallDataPlaneAsync(endpoint, $"/api/v2/extendedAgent/commonprompts/{Uri.EscapeDataString(name)}", HttpMethod.Put, json, tenant, cancellationToken);
    }

    public async Task DeleteCommonPromptAsync(string endpoint, string name, string? tenant = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        await CallDataPlaneAsync(endpoint, $"/api/v2/extendedAgent/commonprompts/{Uri.EscapeDataString(name)}", HttpMethod.Delete, tenant: tenant, cancellationToken: cancellationToken);
    }

    #endregion

}
