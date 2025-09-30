// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Threading;
using Azure.Core;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.VirtualDesktop.Models;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.VirtualDesktop.Services;

public class VirtualDesktopService(ISubscriptionService subscriptionService, ITenantService tenantService, ILogger<VirtualDesktopService> logger) : BaseAzureResourceService(subscriptionService, tenantService), IVirtualDesktopService
{
    private readonly ISubscriptionService _subscriptionService = subscriptionService;
    private readonly ILogger<VirtualDesktopService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<IReadOnlyList<HostPool>> ListHostpoolsAsync(string subscription, string? tenant = null, RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters((nameof(subscription), subscription));

        try
        {
            return await ExecuteResourceQueryAsync(
                "Microsoft.DesktopVirtualization/hostPools",
                null,
                subscription,
                retryPolicy,
                ConvertToHostPoolModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing Host Pools in Subscription: {Subscription}", subscription);
            throw;
        }
    }

    public async Task<IReadOnlyList<HostPool>> ListHostpoolsByResourceGroupAsync(string subscription, string resourceGroup, string? tenant = null, RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters((nameof(subscription), subscription));
        ValidateRequiredParameters((nameof(resourceGroup), resourceGroup));

        try
        {
            return await ExecuteResourceQueryAsync(
                "Microsoft.DesktopVirtualization/hostPools",
                resourceGroup,
                subscription,
                retryPolicy,
                ConvertToHostPoolModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing Host Pools in Subscription: {Subscription}", subscription);
            throw;
        }
    }

    public async Task<IReadOnlyList<SessionHost>> ListSessionHostsAsync(string subscription, string hostPoolName, string? tenant = null, RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters((nameof(subscription), subscription));
        ValidateRequiredParameters((nameof(hostPoolName), hostPoolName));

        try
        {
            hostPoolName = hostPoolName.Trim('"', '\'');
            var results = await ExecuteResourceQueryAsync(
                "Microsoft.DesktopVirtualization/hostPools/sessionHosts",
                null,
                subscription,
                retryPolicy,
                ConvertToSessionHostModel,
                additionalFilter: $"id contains '/hostPools/{EscapeKqlString(hostPoolName)}'");

            foreach (var session in results)
            {
                session.HostPoolName = hostPoolName;
            }
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing Session Hosts in host Pool: {HostPoolName}, Subscription: {Subscription}", hostPoolName, subscription);
            throw;
        }
    }

    public async Task<IReadOnlyList<UserSession>> ListUserSessionsAsync(string subscription, string hostPoolName, string sessionHostName, string? tenant = null, RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters((nameof(subscription), subscription));
        ValidateRequiredParameters((nameof(hostPoolName), hostPoolName));
        ValidateRequiredParameters((nameof(sessionHostName), sessionHostName));

        try
        {
            hostPoolName = hostPoolName.Trim('"', '\'');
            sessionHostName = sessionHostName.Trim('"', '\'');
            var results = await ExecuteResourceQueryAsync(
                "Microsoft.DesktopVirtualization/hostPools/sessionHosts/userSessions",
                null,
                subscription,
                retryPolicy,
                ConvertToUserSessionModel,
                additionalFilter: $"id contains '/hostPools/{EscapeKqlString(hostPoolName)}/sessionHosts/{EscapeKqlString(sessionHostName)}'");

            foreach (var session in results)
            {
                session.HostPoolName = hostPoolName;
                session.SessionHostName = sessionHostName;
            }
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing Session Hosts in host Pool: {HostPoolName}, Session Host: {SessionHostName}, Subscription: {Subscription}", hostPoolName, sessionHostName, subscription);
            throw;
        }
    }

    public async Task<IReadOnlyList<SessionHost>> ListSessionHostsByResourceIdAsync(string subscription, string hostPoolResourceId, string? tenant = null, RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters((nameof(subscription), subscription));
        ValidateRequiredParameters((nameof(hostPoolResourceId), hostPoolResourceId));

        var resourceId = new ResourceIdentifier(hostPoolResourceId);

        try
        {
            var results = await ExecuteResourceQueryAsync(
                "Microsoft.DesktopVirtualization/hostPools/sessionHosts",
                null,
                subscription,
                retryPolicy,
                ConvertToSessionHostModel,
                additionalFilter: $"id contains '/hostPools/{resourceId.Name}'");

            foreach (var session in results)
            {
                session.HostPoolName = resourceId.Name;
            }
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing Session Hosts in host Pool: {HostPoolResourceId}, Subscription: {Subscription}", hostPoolResourceId, subscription);
            throw;
        }
    }

    public async Task<IReadOnlyList<UserSession>> ListUserSessionsByResourceIdAsync(string subscription, string hostPoolResourceId, string sessionHostName, string? tenant = null, RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters((nameof(subscription), subscription));
        ValidateRequiredParameters((nameof(hostPoolResourceId), hostPoolResourceId));
        ValidateRequiredParameters((nameof(sessionHostName), sessionHostName));

        var resourceId = new ResourceIdentifier(hostPoolResourceId);

        try
        {
            sessionHostName = sessionHostName.Trim('"', '\'');
            var results = await ExecuteResourceQueryAsync(
                "Microsoft.DesktopVirtualization/hostPools/sessionHosts/userSessions",
                null,
                subscription,
                retryPolicy,
                ConvertToUserSessionModel,
                additionalFilter: $"id contains '/hostPools/{EscapeKqlString(resourceId.Name)}/sessionHosts/{EscapeKqlString(sessionHostName)}'");

            foreach (var session in results)
            {
                session.HostPoolName = resourceId.Name;
                session.SessionHostName = sessionHostName;
            }
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing Session Hosts in host Pool: {HostPoolResourceId}, Session Host: {SessionHostName}, Subscription: {Subscription}", hostPoolResourceId, sessionHostName, subscription);
            throw;
        }
    }

    public async Task<IReadOnlyList<SessionHost>> ListSessionHostsByResourceGroupAsync(string subscription, string resourceGroup, string hostPoolName, string? tenant = null, RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters((nameof(subscription), subscription));
        ValidateRequiredParameters((nameof(hostPoolName), hostPoolName));
        ValidateRequiredParameters((nameof(resourceGroup), resourceGroup));

        try
        {
            hostPoolName = hostPoolName.Trim('"', '\'');
            var results = await ExecuteResourceQueryAsync(
                "Microsoft.DesktopVirtualization/hostPools/sessionHosts",
                resourceGroup,
                subscription,
                retryPolicy,
                ConvertToSessionHostModel,
                additionalFilter: $"id contains '/hostPools/{EscapeKqlString(hostPoolName)}'");

            foreach (var session in results)
            {
                session.HostPoolName = hostPoolName;
            }
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing Session Hosts in host Pool: {HostPoolName}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}", hostPoolName, resourceGroup, subscription);
            throw;
        }
    }

    public async Task<IReadOnlyList<UserSession>> ListUserSessionsByResourceGroupAsync(string subscription, string resourceGroup, string hostPoolName, string sessionHostName, string? tenant = null, RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters((nameof(subscription), subscription));
        ValidateRequiredParameters((nameof(hostPoolName), hostPoolName));
        ValidateRequiredParameters((nameof(resourceGroup), resourceGroup));
        ValidateRequiredParameters((nameof(sessionHostName), sessionHostName));

        try
        {
            hostPoolName = hostPoolName.Trim('"', '\'');
            sessionHostName = sessionHostName.Trim('"', '\'');
            var results = await ExecuteResourceQueryAsync(
                "Microsoft.DesktopVirtualization/hostPools/sessionHosts/userSessions",
                resourceGroup,
                subscription,
                retryPolicy,
                ConvertToUserSessionModel,
                additionalFilter: $"id contains '/hostPools/{EscapeKqlString(hostPoolName)}/sessionHosts/{EscapeKqlString(sessionHostName)}'");

            foreach (var session in results)
            {
                session.HostPoolName = hostPoolName;
                session.SessionHostName = sessionHostName;
            }
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing Session Hosts in host Pool: {HostPoolName}, Session Host: {SessionHostName}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}", hostPoolName, sessionHostName, resourceGroup, subscription);
            throw;
        }
    }

    private static HostPool ConvertToHostPoolModel(JsonElement item)
    {
        var hostPoolData = Models.HostPoolData.FromJson(item);
        if (hostPoolData == null)
            throw new InvalidOperationException("Failed to parse host pool data");

        if (string.IsNullOrEmpty(hostPoolData.ResourceId))
            throw new InvalidOperationException("Resource ID is missing");
        var id = new ResourceIdentifier(hostPoolData.ResourceId);

        return new HostPool
        {
            Name = hostPoolData.ResourceName,
            ResourceGroupName = id.ResourceGroupName,
            SubscriptionId = id.SubscriptionId,
            Location = hostPoolData.Location,
            HostPoolType = hostPoolData.Properties?.HostPoolType,
            LoadBalancerType = hostPoolData.Properties?.LoadBalancerType,
            MaxSessionLimit = hostPoolData.Properties?.MaxSessionLimit,
            FriendlyName = hostPoolData.Properties?.FriendlyName,
            Description = hostPoolData.Properties?.Description,
            ValidationEnvironment = hostPoolData.Properties?.IsValidationEnvironment,
            PreferredAppGroupType = hostPoolData.Properties?.PreferredAppGroupType,
            StartVMOnConnect = hostPoolData.Properties?.StartVmOnConnect,
            RegistrationEnabled = hostPoolData.Properties?.RegistrationInfo?.RegistrationTokenOperation != null,
            CustomRdpProperty = hostPoolData.Properties?.CustomRdpProperty,
            ResourceIdentifier = hostPoolData.ResourceId
        };
    }

    private static SessionHost ConvertToSessionHostModel(JsonElement item)
    {
        var sessionHostData = Models.SessionHostData.FromJson(item);
        if (sessionHostData == null)
            throw new InvalidOperationException("Failed to parse session host data");

        if (string.IsNullOrEmpty(sessionHostData.ResourceId))
            throw new InvalidOperationException("Resource ID is missing");
        var id = new ResourceIdentifier(sessionHostData.ResourceId);

        return new SessionHost
        {
            Name = sessionHostData.ResourceName,
            ResourceGroupName = id.ResourceGroupName,
            SubscriptionId = id.SubscriptionId,
            Status = sessionHostData.Properties?.Status,
            Sessions = sessionHostData.Properties?.Sessions,
            AgentVersion = sessionHostData.Properties?.AgentVersion,
            AllowNewSession = sessionHostData.Properties?.AllowNewSession,
            AssignedUser = sessionHostData.Properties?.AssignedUser,
            FriendlyName = sessionHostData.Properties?.FriendlyName,
            OsVersion = sessionHostData.Properties?.OSVersion,
            UpdateState = sessionHostData.Properties?.UpdateState,
            UpdateErrorMessage = sessionHostData.Properties?.UpdateErrorMessage,
            SessionHostHealthCheckResults = sessionHostData.Properties?.SessionHostHealthCheckResults?
                .Select(report => new SessionHostHealthCheckResult
                {
                    HealthCheckName = report.HealthCheckName,
                    HealthCheckResult = report.HealthCheckResult,
                    AdditionalFailureDetails = report.AdditionalFailureDetails == null ? null : new SessionHostHealthCheckFailureDetails
                    {
                        Message = report.AdditionalFailureDetails.Message,
                        ErrorCode = report.AdditionalFailureDetails.ErrorCode,
                        LastHealthCheckOn = report.AdditionalFailureDetails.LastHealthCheckOn
                    }
                }
                ).ToList()
        };
    }

    private static UserSession ConvertToUserSessionModel(JsonElement item)
    {
        var userSessionData = Models.UserSessionData.FromJson(item);
        if (userSessionData == null)
            throw new InvalidOperationException("Failed to parse user session data");

        if (string.IsNullOrEmpty(userSessionData.ResourceId))
            throw new InvalidOperationException("Resource ID is missing");
        var id = new ResourceIdentifier(userSessionData.ResourceId);

        return new UserSession
        {
            Name = userSessionData.ResourceName,
            Id = userSessionData.ResourceId,
            ResourceGroupName = id.ResourceGroupName,
            SubscriptionId = id.SubscriptionId,
            UserPrincipalName = userSessionData.Properties?.UserPrincipalName,
            ApplicationType = userSessionData.Properties?.ApplicationType,
            SessionState = userSessionData.Properties?.SessionState,
            ActiveDirectoryUserName = userSessionData.Properties?.ActiveDirectoryUserName,
            CreateTime = userSessionData.Properties?.CreateOn
        };
    }
}
