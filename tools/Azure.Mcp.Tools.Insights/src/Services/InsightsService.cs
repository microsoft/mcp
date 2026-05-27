// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.Insights.Services.Models;
using Azure.ResourceManager.ResourceGraph;
using Azure.ResourceManager.ResourceGraph.Models;
using Azure.ResourceManager.Resources;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Insights.Services;

/// <inheritdoc cref="IInsightsService"/>
public sealed class InsightsService(
    ISubscriptionService subscriptionService,
    ITenantService tenantService,
    ILogger<InsightsService> logger)
    : BaseAzureService(tenantService), IInsightsService
{
    private readonly ISubscriptionService _subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));

    private readonly ILogger<InsightsService> _logger = logger;

    private const int PageSize = 1000;

    private const int MaxPages = 100;

    // Filters out portal/test/managed/system resource groups
    private const string KqlQuery = """
        Resources
        | where type !startswith "microsoft.portal/"
        | where type !startswith "providers.test/"
        | where isempty(managedBy)
        | where not (tags contains "hidden-") and not (tags contains "link:")
        | where resourceGroup !startswith "mc_"
            and resourceGroup !startswith "databricks-rg-"
            and resourceGroup !startswith "azurebackuprg_"
            and resourceGroup !startswith "defaultresourcegroup-"
            and resourceGroup != "networkwatcherrg"
        | project id, name, type, kind, location, resourceGroup, subscriptionId, sku, identity, tags, properties
        """;

    public async Task<SubscriptionAggregation> AggregateSubscriptionAsync(
        string subscription,
        string? tenant,
        RetryPolicyOptions? retryPolicy,
        CancellationToken cancellationToken)
    {
        ValidateRequiredParameters((nameof(subscription), subscription));

        var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy, cancellationToken)
            ?? throw new InvalidOperationException($"Subscription '{subscription}' could not be resolved.");
        var tenantResource = await GetTenantResourceAsync(subscriptionResource.Data.TenantId, cancellationToken);

        return await RunQueryAsync(
            tenantResource,
            new[] { subscriptionResource.Data.SubscriptionId },
            subscriptionCount: 1,
            scopeLabel: subscription,
            cancellationToken);
    }

    public async Task<SubscriptionAggregation> AggregateTenantAsync(
        string? tenant,
        RetryPolicyOptions? retryPolicy,
        CancellationToken cancellationToken)
    {
        var subscriptions = await _subscriptionService.GetSubscriptions(tenant, retryPolicy, cancellationToken);
        if (subscriptions.Count == 0)
        {
            throw new InvalidOperationException("No accessible subscriptions were found in the tenant.");
        }

        var tenantId = subscriptions[0].TenantId
            ?? throw new InvalidOperationException("Could not determine tenant ID from accessible subscriptions.");
        var tenantResource = await GetTenantResourceAsync(tenantId, cancellationToken);

        var subscriptionIds = subscriptions
            .Select(s => s.SubscriptionId)
            .Where(id => !string.IsNullOrEmpty(id))
            .ToArray();

        return await RunQueryAsync(
            tenantResource,
            subscriptionIds,
            subscriptionCount: subscriptionIds.Length,
            scopeLabel: $"tenant:{tenantId}",
            cancellationToken);
    }

    private async Task<SubscriptionAggregation> RunQueryAsync(
        TenantResource tenantResource,
        IReadOnlyList<string> subscriptionIds,
        int subscriptionCount,
        string scopeLabel,
        CancellationToken cancellationToken)
    {
        var rows = new List<JsonElement>();
        string? skipToken = null;
        var pages = 0;
        var documents = new List<JsonDocument>();

        try
        {
            while (true)
            {
                var queryContent = new ResourceQueryContent(KqlQuery)
                {
                    Options = new ResourceQueryRequestOptions
                    {
                        Top = PageSize,
                        SkipToken = skipToken,
                    },
                };
                foreach (var id in subscriptionIds)
                {
                    queryContent.Subscriptions.Add(id);
                }

                ResourceQueryResult result = await tenantResource.GetResourcesAsync(queryContent, cancellationToken);
                if (result is null)
                {
                    break;
                }

                pages++;

                if (result.Count > 0 && result.Data is not null)
                {
                    var doc = JsonDocument.Parse(result.Data);
                    documents.Add(doc);
                    if (doc.RootElement.ValueKind == JsonValueKind.Array)
                    {
                        rows.AddRange(doc.RootElement.EnumerateArray());
                    }
                }

                skipToken = result.SkipToken;
                if (string.IsNullOrEmpty(skipToken))
                {
                    break;
                }

                if (pages >= MaxPages)
                {
                    _logger.LogWarning(
                        "Reached pagination cap of {MaxPages} pages for scope {Scope}; results may be truncated.",
                        MaxPages, scopeLabel);
                    break;
                }
            }

            var aggregation = PropertyAggregator.Aggregate(rows, subscriptionCount);
            return AggregationFilter.Filter(aggregation);
        }
        finally
        {
            foreach (var doc in documents)
            {
                doc.Dispose();
            }
        }
    }

    private async Task<TenantResource> GetTenantResourceAsync(Guid? tenantId, CancellationToken cancellationToken)
    {
        if (tenantId is null)
        {
            throw new ArgumentException("Tenant ID cannot be null.", nameof(tenantId));
        }

        var allTenants = await TenantService.GetTenants(cancellationToken);
        var tenantResource = allTenants.FirstOrDefault(t => t.Data.TenantId == tenantId.Value)
            ?? throw new InvalidOperationException($"No accessible tenant found for tenant ID '{tenantId}'.");
        return tenantResource;
    }
}
