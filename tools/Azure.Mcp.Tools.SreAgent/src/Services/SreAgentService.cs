// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.SreAgent.Models;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Core.Services.Caching;

namespace Azure.Mcp.Tools.SreAgent.Services;

/// <summary>
/// Provides access to the Azure SRE Agent control plane (ARM) and data plane.
/// </summary>
/// <remarks>
/// Scaffolding implementation. Subsequent commits will port the SRE Agent REST surface
/// from src/Agent/Agent.Cli/Services/ApiService.cs in the SRE Agent runtime repo.
/// </remarks>
public sealed class SreAgentService(ISubscriptionService subscriptionService, ITenantService tenantService, ICacheService cacheService)
    : BaseAzureService(tenantService), ISreAgentService
{
    private readonly ISubscriptionService _subscriptionService = subscriptionService;
    private readonly ICacheService _cacheService = cacheService;

    public Task<List<SreAgentResource>> ListAgentsAsync(
        string subscription,
        string? resourceGroup = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(subscription);

        // Scaffolding placeholder. The real implementation will enumerate
        // Microsoft.App/sreAgents resources via Azure.ResourceManager and is added in a follow-up commit.
        return Task.FromResult(new List<SreAgentResource>());
    }
}
