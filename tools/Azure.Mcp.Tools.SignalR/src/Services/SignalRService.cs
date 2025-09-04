// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Core;
using Azure.Core.Pipeline;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.SignalR.Commands;
using Azure.Mcp.Tools.SignalR.Models;
using Azure.ResourceManager;
using Microsoft.Extensions.Logging;
using SignalRIdentity = Azure.Mcp.Tools.SignalR.Models.Identity;

namespace Azure.Mcp.Tools.SignalR.Services;

/// <summary>
/// Service for Azure SignalR operations using Resource Graph API.
/// </summary>
public class SignalRService(
    ISubscriptionService subscriptionService,
    ITenantService tenantService,
    ILoggerFactory? loggerFactory = null)
    : BaseAzureResourceService(subscriptionService, tenantService, loggerFactory), ISignalRService
{
    private readonly ILogger<SignalRService>? _logger = loggerFactory?.CreateLogger<SignalRService>();
    private const int TokenExpirationBuffer = 300;
    private const string ManagementApiBaseUrl = "https://management.azure.com";
    private const string SignalRResourceType = "Microsoft.SignalRService/SignalR";
    private const string ApiVersion = "2024-03-01";

    private string? _cachedAccessToken;
    private DateTimeOffset _tokenExpiryTime;
    private readonly ISubscriptionService _subscriptionService = subscriptionService;

    public async Task<IEnumerable<Runtime>> ListRuntimesAsync(
        string subscription,
        string? tenant = null,
        AuthMethod? authMethod = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(subscription);
        try
        {
            var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy)
                                       ?? throw new Exception($"Subscription '{subscription}' not found");
            var clientOptions = AddDefaultPolicies(new ArmClientOptions());

            if (retryPolicy != null)
            {
                clientOptions.Retry.MaxRetries = retryPolicy.MaxRetries;
                clientOptions.Retry.Mode = retryPolicy.Mode;
                clientOptions.Retry.Delay = TimeSpan.FromSeconds(retryPolicy.DelaySeconds);
                clientOptions.Retry.MaxDelay = TimeSpan.FromSeconds(retryPolicy.MaxDelaySeconds);
                clientOptions.Retry.NetworkTimeout = TimeSpan.FromSeconds(retryPolicy.NetworkTimeoutSeconds);
            }

            var pipeline = HttpPipelineBuilder.Build(clientOptions);
            var token = await GetAccessTokenAsync(tenant);

            var runtimes = new List<Runtime>();
            string? nextLink = BuildListSignalrUrl(subscriptionResource.Data.SubscriptionId);

            while (!string.IsNullOrEmpty(nextLink))
            {
                var request = pipeline.CreateRequest();
                request.Method = RequestMethod.Get;
                request.Uri.Reset(new Uri(nextLink));
                request.Headers.Add("Authorization", $"Bearer {token}");

                var response = await pipeline.SendRequestAsync(request, CancellationToken.None);
                if (response.IsError)
                {
                    throw new HttpRequestException($"Request failed with status {response.Status}: {response.ReasonPhrase}");
                }

                using var doc = await JsonDocument.ParseAsync(response.Content.ToStream());
                var root = doc.RootElement;

                if (root.TryGetProperty("value", out var valueElement) && valueElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in valueElement.EnumerateArray())
                    {
                        runtimes.Add(ConvertToRuntimeModel(item));
                    }
                }

                if (root.TryGetProperty("nextLink", out var nextLinkElement) &&
                    nextLinkElement.ValueKind == JsonValueKind.String)
                {
                    nextLink = nextLinkElement.GetString();
                }
                else
                {
                    nextLink = null;
                }
            }

            return runtimes;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to list SignalR services for subscription {Subscription}", subscription);
            throw new Exception($"Failed to list SignalR services: {ex.Message}", ex);
        }
    }

    public async Task<Runtime?> GetRuntimeAsync(
        string subscription,
        string resourceGroup,
        string signalRName,
        string? tenant = null,
        AuthMethod? authMethod = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(subscription, resourceGroup, signalRName);

        try
        {
            var result = await ExecuteSingleResourceQueryAsync(
                "Microsoft.SignalRService/SignalR",
                resourceGroup,
                subscription,
                retryPolicy,
                ConvertToRuntimeModel,
                $"name =~ '{EscapeKqlString(signalRName)}'");

            if (result == null)
            {
                throw new KeyNotFoundException(
                    $"SignalR service '{signalRName}' not found in resource group '{resourceGroup}' for subscription '{subscription}'.");
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex,
                "Failed to get SignalR service {SignalRName} in resource group {ResourceGroup} for subscription {Subscription}",
                signalRName, resourceGroup, subscription);
            throw new Exception($"Failed to get SignalR service '{signalRName}': {ex.Message}", ex);
        }
    }

    public async Task<Key?> ListKeysAsync(
        string subscription,
        string resourceGroup,
        string signalRName,
        string? tenant = null,
        AuthMethod? authMethod = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(subscription, resourceGroup, signalRName);

        try
        {
            var localAuth = await ExecuteSingleResourceQueryAsync(
                "Microsoft.SignalRService/SignalR",
                resourceGroup,
                subscription,
                retryPolicy,
                r =>
                {
                    bool? disableLocalAuth = null;
                    if (r.TryGetProperty("properties", out var props) &&
                        props.TryGetProperty("disableLocalAuth", out var d) &&
                        (d.ValueKind == JsonValueKind.True || d.ValueKind == JsonValueKind.False))
                    {
                        disableLocalAuth = d.GetBoolean();
                    }
                    return new { DisableLocalAuth = disableLocalAuth };
                },
                $"name =~ '{EscapeKqlString(signalRName)}'");
            if (localAuth?.DisableLocalAuth == true)
            {
                throw new RequestFailedException(403, "Access keys are disabled for this SignalR service.");
            }

            var clientOptions = AddDefaultPolicies(new ArmClientOptions());

            if (retryPolicy != null)
            {
                clientOptions.Retry.MaxRetries = retryPolicy.MaxRetries;
                clientOptions.Retry.Mode = retryPolicy.Mode;
                clientOptions.Retry.Delay = TimeSpan.FromSeconds(retryPolicy.DelaySeconds);
                clientOptions.Retry.MaxDelay = TimeSpan.FromSeconds(retryPolicy.MaxDelaySeconds);
                clientOptions.Retry.NetworkTimeout = TimeSpan.FromSeconds(retryPolicy.NetworkTimeoutSeconds);
            }

            var pipeline = HttpPipelineBuilder.Build(clientOptions);
            var signalrUrl = BuildPostSignalrUrl(subscription, resourceGroup, signalRName, "listKeys");
            var token = await GetAccessTokenAsync(tenant);

            var request = pipeline.CreateRequest();
            request.Method = RequestMethod.Post;
            request.Uri.Reset(new Uri(signalrUrl));

            request.Headers.Add("Authorization", $"Bearer {token}");

            var response = await pipeline.SendRequestAsync(request, CancellationToken.None);
            if (!response.IsError)
            {
                var keys = JsonSerializer.Deserialize(response.Content.ToStream(), SignalRJsonContext.Default.Key);
                return keys ?? throw new JsonException("Failed to deserialize SignalR keys response.");
            }

            throw new HttpRequestException($"Request failed with status {response.Status}: {response.ReasonPhrase}");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex,
                "Failed to list keys for SignalR service {SignalRName} in resource group {ResourceGroup} for subscription {Subscription}",
                signalRName, resourceGroup, subscription);
            throw new Exception($"Failed to list keys for SignalR service '{signalRName}': {ex.Message}", ex);
        }
    }

    public async Task<SignalRIdentity?> GetSignalRIdentityAsync(
        string subscription,
        string resourceGroup,
        string signalRName,
        string? tenant = null,
        AuthMethod? authMethod = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(subscription, resourceGroup, signalRName);

        try
        {
            var result = await ExecuteSingleResourceQueryAsync(
                "Microsoft.SignalRService/SignalR",
                resourceGroup,
                subscription,
                retryPolicy,
                ConvertToIdentityModel,
                $"name =~ '{EscapeKqlString(signalRName)}'");

            return result;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex,
                "Failed to get identity for SignalR service {SignalRName} in resource group {ResourceGroup} for subscription {Subscription}",
                signalRName, resourceGroup, subscription);
            throw new Exception($"Failed to get SignalR service identity: {ex.Message}", ex);
        }
    }

    public async Task<NetworkRule?> GetNetworkRulesAsync(
        string subscription,
        string resourceGroup,
        string signalRName,
        string? tenant = null,
        AuthMethod? authMethod = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(subscription, resourceGroup, signalRName);

        try
        {
            var result = await ExecuteSingleResourceQueryAsync(
                "Microsoft.SignalRService/SignalR",
                resourceGroup,
                subscription,
                retryPolicy,
                ConvertToNetworkRuleModel,
                $"name =~ '{EscapeKqlString(signalRName)}'");

            return result;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex,
                "Failed to get network rules for SignalR service {SignalRName} in resource group {ResourceGroup} for subscription {Subscription}",
                signalRName, resourceGroup, subscription);
            throw new Exception($"Failed to get SignalR service network rules: {ex.Message}", ex);
        }
    }

    private static Runtime ConvertToRuntimeModel(JsonElement resource)
    {
        return new Runtime
        {
            Name = resource.TryGetProperty("name", out var name) ? name.GetString() : null,
            ResourceGroupName = resource.TryGetProperty("resourceGroup", out var rg) ? rg.GetString() : null,
            Location = resource.TryGetProperty("location", out var location) ? location.GetString() : null,
            SkuName =
                resource.TryGetProperty("sku", out var sku) && sku.TryGetProperty("name", out var skuName)
                    ? skuName.GetString()
                    : null,
            SkuTier =
                resource.TryGetProperty("sku", out var skuTier) && skuTier.TryGetProperty("tier", out var tier)
                    ? tier.GetString()
                    : null,
            ProvisioningState =
                resource.TryGetProperty("properties", out var props) &&
                props.TryGetProperty("provisioningState", out var state)
                    ? state.GetString()
                    : null,
            HostName =
                resource.TryGetProperty("properties", out var hostProps) &&
                hostProps.TryGetProperty("hostName", out var hostName)
                    ? hostName.GetString()
                    : null,
            PublicPort =
                resource.TryGetProperty("properties", out var portProps) &&
                portProps.TryGetProperty("publicPort", out var publicPort) && publicPort.TryGetInt32(out var pPort)
                    ? pPort
                    : null,
            ServerPort =
                resource.TryGetProperty("properties", out var serverProps) &&
                serverProps.TryGetProperty("serverPort", out var serverPort) &&
                serverPort.TryGetInt32(out var sPort)
                    ? sPort
                    : null
        };
    }

    private static SignalRIdentity ConvertToIdentityModel(JsonElement resource)
    {
        var identity = new SignalRIdentity();

        if (resource.TryGetProperty("identity", out var identityElement))
        {
            if (identityElement.TryGetProperty("type", out var identityType))
            {
                identity.Type = identityType.GetString();
            }

            if (identityElement.TryGetProperty("principalId", out var principalId))
            {
                identity.PrincipalId = principalId.GetString();
            }

            if (identityElement.TryGetProperty("tenantId", out var tenantId))
            {
                identity.TenantId = tenantId.GetString();
            }

            if (identityElement.TryGetProperty("userAssignedIdentities", out var userAssignedIdentities) &&
                userAssignedIdentities.ValueKind == JsonValueKind.Object)
            {
                identity.UserAssignedIdentities = new Dictionary<string, UserAssignedIdentity>();

                foreach (var property in userAssignedIdentities.EnumerateObject())
                {
                    var userIdentity = new UserAssignedIdentity();

                    if (property.Value.TryGetProperty("principalId", out var userPrincipalId))
                    {
                        userIdentity.PrincipalId = userPrincipalId.GetString();
                    }

                    if (property.Value.TryGetProperty("clientId", out var clientId))
                    {
                        userIdentity.ClientId = clientId.GetString();
                    }

                    identity.UserAssignedIdentities[property.Name] = userIdentity;
                }
            }
        }

        return identity;
    }

    private static NetworkRule ConvertToNetworkRuleModel(JsonElement resource)
    {
        var networkRule = new NetworkRule();

        if (resource.TryGetProperty("properties", out var properties) &&
            properties.TryGetProperty("networkACLs", out var networkAcls))
        {
            if (networkAcls.TryGetProperty("defaultAction", out var defaultAction))
            {
                networkRule.DefaultAction = defaultAction.GetString();
            }

            if (networkAcls.TryGetProperty("privateEndpoints", out var privateEndpoints) &&
                privateEndpoints.ValueKind == JsonValueKind.Array)
            {
                var endpoints = new List<PrivateEndpointNetworkAcl>();

                foreach (var endpoint in privateEndpoints.EnumerateArray())
                {
                    var endpointAcl = new PrivateEndpointNetworkAcl();

                    if (endpoint.TryGetProperty("name", out var endpointName))
                    {
                        endpointAcl.Name = endpointName.GetString();
                    }

                    if (endpoint.TryGetProperty("allow", out var allow) && allow.ValueKind == JsonValueKind.Array)
                    {
                        endpointAcl.Allow = allow
                            .EnumerateArray()
                            .Select(x => x.GetString())
                            .Where(x => x != null)
                            .Select(x => x!)
                            .ToList();
                    }

                    if (endpoint.TryGetProperty("deny", out var deny) && deny.ValueKind == JsonValueKind.Array)
                    {
                        endpointAcl.Deny = deny
                            .EnumerateArray()
                            .Select(x => x.GetString())
                            .Where(x => x != null)
                            .Select(x => x!)
                            .ToList();
                    }

                    endpoints.Add(endpointAcl);
                }

                networkRule.PrivateEndpoints = endpoints;
            }

            if (networkAcls.TryGetProperty("publicNetwork", out var publicNetwork))
            {
                var publicAcl = new NetworkAcl();

                if (publicNetwork.TryGetProperty("allow", out var publicAllow) &&
                    publicAllow.ValueKind == JsonValueKind.Array)
                {
                    publicAcl.Allow = publicAllow
                        .EnumerateArray()
                        .Select(x => x.GetString())
                        .Where(x => x != null)
                        .Select(x => x!)
                        .ToList();
                }

                if (publicNetwork.TryGetProperty("deny", out var publicDeny) &&
                    publicDeny.ValueKind == JsonValueKind.Array)
                {
                    publicAcl.Deny = publicDeny
                        .EnumerateArray()
                        .Select(x => x.GetString())
                        .Where(x => x != null)
                        .Select(x => x!)
                        .ToList();
                }

                networkRule.PublicNetwork = publicAcl;
            }
        }

        return networkRule;
    }

    private static string BuildPostSignalrUrl(string subscription, string resourceGroup, string signalRName,
        string action)
    {
        var queryParams = new List<string> { $"api-version={ApiVersion}" };
        string queryString = string.Join("&", queryParams);
        return
            $"{ManagementApiBaseUrl}/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/{SignalRResourceType}/{signalRName}/{action}?{queryString}";
    }

    private static string BuildListSignalrUrl(string subscription)
    {
        return $"{ManagementApiBaseUrl}/subscriptions/{subscription}/providers/{SignalRResourceType}?api-version={ApiVersion}";
    }

    private async Task<string> GetAccessTokenAsync(string? tenant = null)
    {
        if (_cachedAccessToken != null && DateTimeOffset.UtcNow < _tokenExpiryTime)
        {
            return _cachedAccessToken;
        }

        AccessToken accessToken = await GetEntraIdAccessTokenAsync(ManagementApiBaseUrl, tenant);
        _cachedAccessToken = accessToken.Token;
        _tokenExpiryTime = accessToken.ExpiresOn.AddSeconds(-TokenExpirationBuffer);

        return _cachedAccessToken;
    }

    private async Task<AccessToken> GetEntraIdAccessTokenAsync(string resource, string? tenant = null)
    {
        var tokenRequestContext = new TokenRequestContext([$"{resource}/.default"]);
        var tokenCredential = await GetCredential(tenant);
        return await tokenCredential
            .GetTokenAsync(tokenRequestContext, CancellationToken.None);
    }
}
