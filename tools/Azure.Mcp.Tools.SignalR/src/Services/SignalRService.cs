// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Models.Identity;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Core.Services.Caching;
using Azure.Mcp.Tools.SignalR.Models;
using Azure.ResourceManager.Models;
using Azure.ResourceManager.SignalR;
using Azure.ResourceManager.SignalR.Models;

namespace Azure.Mcp.Tools.SignalR.Services;

/// <summary>
/// Service for Azure SignalR operations using Resource Graph API.
/// </summary>
public sealed class SignalRService(
    ISubscriptionService subscriptionService,
    ITenantService tenantService,
    ICacheService cacheService) : BaseAzureService(tenantService), ISignalRService
{
    private readonly ISubscriptionService _subscriptionService =
        subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));

    private readonly ICacheService
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));

    private const string CacheGroup = "signalr";
    private static readonly TimeSpan s_cacheDuration = TimeSpan.FromHours(1);

    public async Task<IEnumerable<Runtime>> GetRuntimeAsync(
        string subscription,
        string? resourceGroup,
        string? signalRName,
        string? tenant = null,
        AuthMethod? authMethod = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(subscription);
        var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy);
        var runtimes = new List<Runtime>();
        if (string.IsNullOrEmpty(signalRName))
        {
            var cacheKey = string.IsNullOrEmpty(tenant) ? subscription : $"{subscription}_{tenant}";
            cacheKey = string.IsNullOrEmpty(resourceGroup) ? cacheKey : $"{cacheKey}_{resourceGroup}";
            var cachedResults = await _cacheService.GetAsync<List<Runtime>>(CacheGroup, cacheKey, s_cacheDuration);
            if (cachedResults != null)
            {
                return cachedResults;
            }

            try
            {
                if (string.IsNullOrEmpty(resourceGroup))
                {
                    var signalRResources = subscriptionResource.GetSignalRsAsync();
                    await foreach (var runtime in signalRResources)
                    {
                        runtimes.Add(ConvertToRuntimeModel(runtime));
                    }
                }
                else
                {
                    var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup);
                    if (!resourceGroupResource.HasValue)
                    {
                        throw new Exception($"Resource group '{resourceGroup}' not found in subscription '{subscription}'");
                    }

                    var signalRResources = resourceGroupResource.Value.GetSignalRs().GetAllAsync();
                    await foreach (var runtime in signalRResources)
                    {
                        runtimes.Add(ConvertToRuntimeModel(runtime));
                    }

                    await _cacheService.SetAsync(CacheGroup, cacheKey, signalRName, s_cacheDuration);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error get SignalR Runtimes: {ex.Message}", ex);
            }
        }
        else
        {
            ValidateRequiredParameters(signalRName, resourceGroup);
            var cacheKey = string.IsNullOrEmpty(tenant)
                ? $"{subscription}_{resourceGroup}_{signalRName}"
                : $"{subscription}_{tenant}_{resourceGroup}_{signalRName}";

            var cachedResults = await _cacheService.GetAsync<List<Runtime>>(CacheGroup, cacheKey, s_cacheDuration);
            if (cachedResults != null)
            {
                return cachedResults;
            }

            try
            {
                var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup);
                if (!resourceGroupResource.HasValue)
                {
                    throw new Exception(
                        $"Resource group '{resourceGroup}' not found in subscription '{subscription}'");
                }

                var signalRResource = await resourceGroupResource.Value.GetSignalRs().GetAsync(signalRName);
                if (!signalRResource.HasValue)
                {
                    throw new Exception(
                        $"SignalR '{signalRName}' not found in resource group '{resourceGroup}'");
                }

                runtimes.Add(ConvertToRuntimeModel(signalRResource.Value));
                await _cacheService.SetAsync(CacheGroup, cacheKey, signalRName, s_cacheDuration);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error get SignalR Runtime: {ex.Message}", ex);
            }
        }

        return runtimes;
    }

    private static Runtime ConvertToRuntimeModel(SignalRResource resource)
    {
        var runtime = new Runtime
        {
            Id = resource.Id.ToString(),
            Identity = ConvertToIdentityModel(resource.Data.Identity),
            Kind = resource.Data.Kind?.ToString(),
            Location = resource.Data.Location,
            Name = resource.Data.Name,
            Properties = new RuntimeProperties
            {
                ExternalIP = resource.Data?.ExternalIP,
                HostName = resource.Data?.HostName,
                NetworkAcls = ConvertToNetworkAclsModel(resource.Data?.NetworkACLs),
                ProvisioningState = resource.Data?.ProvisioningState.ToString(),
                PublicNetworkAccess = resource.Data?.PublicNetworkAccess,
                PublicPort = resource.Data?.PublicPort,
                ServerPort = resource.Data?.ServerPort,
                UpstreamTemplates = ConvertToUpstreamTemplatesModel(resource.Data?.UpstreamTemplates)
            },
            Sku = new Sku
            {
                Capacity = resource.Data?.Sku?.Capacity,
                Name = resource.Data?.Sku?.Name,
                Size = resource.Data?.Sku?.Size,
                Tier = resource.Data?.Sku?.Tier.ToString()
            },
            Tags = resource.Data?.Tags
        };
        return runtime ?? throw new InvalidOperationException("Failed to parse SignalR runtime data");
    }

    private static NetworkAcls? ConvertToNetworkAclsModel(SignalRNetworkAcls? networkAcls)
    {
        if (networkAcls is null)
        {
            return null;
        }

        PublicNetwork? publicNetwork = null;
        if (networkAcls.PublicNetwork is not null)
        {
            var allow = networkAcls.PublicNetwork.Allow?.Select(a => a.ToString()).ToList();
            var deny = networkAcls.PublicNetwork.Deny?.Select(d => d.ToString()).ToList();
            if (allow != null || deny != null)
            {
                publicNetwork = new PublicNetwork { Allow = allow, Deny = deny };
            }
        }

        var privateEndpoints = networkAcls.PrivateEndpoints?.Select(pe => new PrivateEndpoint
        {
            Name = pe.Name,
            Allow = pe.Allow?.Select(a => a.ToString()).ToList(),
            Deny = pe.Deny?.Select(d => d.ToString()).ToList()
        }).ToList();

        return new NetworkAcls
        {
            DefaultAction = networkAcls.DefaultAction?.ToString(),
            PublicNetwork = publicNetwork,
            PrivateEndpoints = privateEndpoints
        };
    }

    private static Models.Identity? ConvertToIdentityModel(ManagedServiceIdentity? identity)
    {
        if (identity is null)
        {
            return null;
        }

        SystemAssignedIdentityInfo? systemAssigned =
            identity.ManagedServiceIdentityType == ManagedServiceIdentityType.SystemAssigned
                ? new SystemAssignedIdentityInfo
                {
                    PrincipalId = identity.PrincipalId.ToString(),
                    TenantId = identity.TenantId.ToString()
                }
                : null;

        UserAssignedIdentityInfo[]? userAssigned =
            identity.ManagedServiceIdentityType == ManagedServiceIdentityType.UserAssigned
            && identity.UserAssignedIdentities is not null
                ? [.. identity.UserAssignedIdentities.Select(kv => new UserAssignedIdentityInfo
                {
                    ClientId = kv.Key.ToString(),
                    PrincipalId = kv.Value.PrincipalId.ToString()
                })]
                : null;

        var managedIdentityInfo = new ManagedIdentityInfo
        {
            SystemAssignedIdentity = systemAssigned,
            UserAssignedIdentities = userAssigned
        };

        return new Models.Identity
        {
            Type = identity.ManagedServiceIdentityType.ToString(),
            ManagedIdentityInfo = managedIdentityInfo
        };
    }

    private static List<UpstreamTemplate>? ConvertToUpstreamTemplatesModel(
        IList<SignalRUpstreamTemplate>? upstreamTemplates)
    {
        return upstreamTemplates?.Select(ut => new UpstreamTemplate
        {
            Auth = new AuthSettings
            {
                Type = ut.Auth?.AuthType?.ToString(),
                Resource = ut.Auth?.ManagedIdentityResource
            },
            CategoryPattern = ut.CategoryPattern,
            EventPattern = ut.EventPattern,
            HubPattern = ut.HubPattern,
            UrlTemplate = ut.UrlTemplate
        }).ToList();
    }
}
