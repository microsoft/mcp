// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.Optimization.Models;
using Azure.ResourceManager.ResourceGraph;
using Azure.ResourceManager.ResourceGraph.Models;
using Azure.ResourceManager.Resources;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Optimization.Services;

public class OptimizationService(IAzureService azureService, ILogger<OptimizationService> logger)
    : BaseAzureResourceService(azureService), IOptimizationService
{
    private const int AlternativesLimit = 100;
    private const int ExplanationLimit = 100;
    private const double DefaultThreshold = 80;
    private static readonly TimeSpan RecentObservationWindow = TimeSpan.FromDays(7);
    private static readonly TimeSpan RecentInterval = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan LongTermObservationWindow = TimeSpan.FromDays(7);
    private static readonly TimeSpan LongTermInterval = TimeSpan.FromHours(6);

    private readonly ILogger<OptimizationService> _logger = logger;

    public async Task<ResourceQueryResults<CostSavingsRecommendation>> ListCostSavingsAsync(
        string subscription,
        int top,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subscription);

        var query = $"{OptimizationKqlQueries.TopCostSavingsQuery}\n| limit {top}";
        var (rows, truncated) = await QueryResourceGraphAsync(
            query, subscription, tenant, cancellationToken);

        var recommendations = rows.Select(ConvertToCostSavings).ToList();
        return new ResourceQueryResults<CostSavingsRecommendation>(recommendations, truncated);
    }

    public async Task<IReadOnlyList<AlternativeRecommendation>> GetAlternativesAsync(
        string resourceId,
        string subscription,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceId);
        ArgumentException.ThrowIfNullOrWhiteSpace(subscription);

        // Accept either the Advisor recommendation id or the impacted resource id.
        resourceId = ArmResourceId.StripAdvisorRecommendationSuffix(resourceId);

        var query = $"{OptimizationKqlQueries.BuildAlternativesQuery(resourceId)}\n| limit {AlternativesLimit}";
        var (rows, _) = await QueryResourceGraphAsync(query, subscription, tenant, cancellationToken);

        return AlternativeRecommendationsArgParser.Parse(rows, resourceId);
    }

    public async Task<RecommendationExplanationResult> GetRecommendationExplanationAsync(
        string resourceId,
        string? targetSku,
        UtilizationView view,
        string subscription,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceId);
        ArgumentException.ThrowIfNullOrWhiteSpace(subscription);

        // Accept either the Advisor recommendation id or the impacted resource id.
        resourceId = ArmResourceId.StripAdvisorRecommendationSuffix(resourceId);

        var query = $"{OptimizationKqlQueries.BuildAdvisorRecommendationQuery(resourceId)}\n| limit {ExplanationLimit}";
        var (rows, _) = await QueryResourceGraphAsync(query, subscription, tenant, cancellationToken);

        if (rows.Count == 0)
        {
            return new RecommendationExplanationResult(
                OptimizationStrings.ExplanationRenderingInstructions,
                0, resourceId, null, null, null, null, null, null, null);
        }

        // When no target SKU is supplied, project only the current utilization (no target comparison).
        var credential = await GetCredential(tenant, cancellationToken);
        var armHost = AzureService.CloudConfiguration.ArmEnvironment.Endpoint.ToString();
        var armScope = AzureService.CloudConfiguration.ArmEnvironment.DefaultScope;
        var httpClient = AzureService.GetClient();

        var computeClient = new OptimizationComputeSkuClient(httpClient, credential, armHost, armScope);

        string location;
        string resourceKind;
        SkuConfiguration currentConfiguration;
        SkuConfiguration? targetConfiguration;
        RecommendationExplanation projection;

        if (string.IsNullOrWhiteSpace(targetSku))
        {
            var current = await computeClient.GetCurrentAsync(resourceId, cancellationToken);
            location = current.Location;
            resourceKind = current.ResourceKind;
            currentConfiguration = new SkuConfiguration(
                current.Current.Name, current.CurrentInstanceCount, current.Current.AvailableVcpus, current.Current.MemoryGB, null);
            targetConfiguration = null;
            projection = new RecommendationExplanation
            {
                ResourceId = resourceId,
                RecommendationMessage = $"Current utilization for {current.Current.Name}",
                SKU = current.Current.Name,
                SkuCores = current.Current.AvailableVcpus,
                MemoryGB = current.Current.MemoryGB,
                CurrentInstanceCount = current.CurrentInstanceCount,
                MaxCpuThreshold = DefaultThreshold,
                MaxMemoryThreshold = DefaultThreshold,
                MaxNetworkThreshold = DefaultThreshold,
            };
        }
        else
        {
            var comparison = await computeClient.GetComparisonAsync(resourceId, targetSku, cancellationToken);
            var resolvedTargetInstances = comparison.CurrentInstanceCount;
            location = comparison.Location;
            resourceKind = comparison.ResourceKind;
            currentConfiguration = new SkuConfiguration(
                comparison.Current.Name, comparison.CurrentInstanceCount, comparison.Current.AvailableVcpus, comparison.Current.MemoryGB, null);
            targetConfiguration = new SkuConfiguration(
                comparison.Target.Name, resolvedTargetInstances, comparison.Target.AvailableVcpus, comparison.Target.MemoryGB, null);
            projection = new RecommendationExplanation
            {
                ResourceId = resourceId,
                RecommendationMessage = $"Project {comparison.Current.Name} to {comparison.Target.Name}",
                SKU = comparison.Current.Name,
                NewSKU = comparison.Target.Name,
                SkuCores = comparison.Current.AvailableVcpus,
                NewSkuCores = comparison.Target.AvailableVcpus,
                MemoryGB = comparison.Current.MemoryGB,
                NewMemoryGB = comparison.Target.MemoryGB,
                CurrentInstanceCount = comparison.CurrentInstanceCount,
                NewInstanceCount = resolvedTargetInstances,
                MaxCpuThreshold = DefaultThreshold,
                MaxMemoryThreshold = DefaultThreshold,
                MaxNetworkThreshold = DefaultThreshold,
            };
        }

        var includeDetail = view is UtilizationView.Detail or UtilizationView.Both;
        var includeTrend = view is UtilizationView.Trend or UtilizationView.Both;

        var monitorClient = new OptimizationMonitorClient(httpClient, credential, armHost, armScope, _logger);
        var endTime = FloorToInterval(DateTimeOffset.UtcNow, LongTermInterval);
        var recentStartTime = endTime - RecentObservationWindow;
        var longTermStartTime = endTime - LongTermObservationWindow;

        var recentMetricsTask = includeDetail
            ? monitorClient.GetUtilizationAsync(resourceId, recentStartTime, endTime, RecentInterval, cancellationToken)
            : null;
        var longTermMetricsTask = includeTrend
            ? monitorClient.GetUtilizationAsync(resourceId, longTermStartTime, endTime, LongTermInterval, cancellationToken)
            : null;
        await Task.WhenAll(
                new[] { recentMetricsTask, longTermMetricsTask }
                    .Where(task => task is not null)
                    .Select(task => task!))
            .ConfigureAwait(false);

        var recentUtilization = recentMetricsTask is null
            ? null
            : RecommendationUtilizationProjector.Build(
                projection, recentMetricsTask.Result, recentStartTime, endTime, RecentInterval);
        var longTermUtilization = longTermMetricsTask is null
            ? null
            : RecommendationUtilizationProjector.Build(
                projection, longTermMetricsTask.Result, longTermStartTime, endTime, LongTermInterval);

        return new RecommendationExplanationResult(
            OptimizationStrings.ExplanationRenderingInstructions,
            rows.Count,
            resourceId,
            location,
            resourceKind,
            currentConfiguration,
            targetConfiguration,
            new UtilizationThresholds(DefaultThreshold, DefaultThreshold, DefaultThreshold),
            recentUtilization,
            longTermUtilization);
    }

    private static DateTimeOffset FloorToInterval(DateTimeOffset value, TimeSpan interval)
    {
        var utcTicks = value.UtcTicks - (value.UtcTicks % interval.Ticks);
        return new DateTimeOffset(utcTicks, TimeSpan.Zero);
    }

    /// <summary>
    /// Runs a raw Azure Resource Graph query scoped to a single subscription and returns the
    /// cloned data rows plus the truncation flag.
    /// </summary>
    private async Task<(List<JsonElement> Rows, bool Truncated)> QueryResourceGraphAsync(
        string query,
        string subscription,
        string? tenant,
        CancellationToken cancellationToken)
    {
        var (subscriptionId, subscriptionTenantId) = await ResolveSubscriptionAsync(
            subscription, tenant, cancellationToken);
        var tenantResource = await GetTenantResourceAsync(subscriptionTenantId, cancellationToken);

        var queryContent = new ResourceQueryContent(query)
        {
            Subscriptions = { subscriptionId },
        };

        ResourceQueryResult result = await tenantResource.GetResourcesAsync(queryContent, cancellationToken);

        var rows = new List<JsonElement>();
        if (result != null && result.Count > 0)
        {
            using var jsonDocument = JsonDocument.Parse(result.Data);
            if (jsonDocument.RootElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in jsonDocument.RootElement.EnumerateArray())
                {
                    rows.Add(item.Clone());
                }
            }
        }

        return (rows, result?.ResultTruncated == ResultTruncated.True);
    }

    /// <summary>
    /// Resolves the subscription id and owning tenant. When the caller passes a subscription name,
    /// the id is looked up through an Azure Resource Graph query over resourcecontainers rather than
    /// enumerating every subscription.
    /// </summary>
    private async Task<(string SubscriptionId, Guid? TenantId)> ResolveSubscriptionAsync(
        string subscription,
        string? tenant,
        CancellationToken cancellationToken)
    {
        var tenantId = Guid.TryParse(tenant, out var parsedTenant) ? parsedTenant : (Guid?)null;

        if (Guid.TryParse(subscription, out _))
        {
            return (subscription, tenantId);
        }

        var tenantResource = await GetTenantResourceAsync(tenantId, cancellationToken);
        var queryContent = new ResourceQueryContent(OptimizationKqlQueries.BuildSubscriptionIdByNameQuery(subscription));
        ResourceQueryResult result = await tenantResource.GetResourcesAsync(queryContent, cancellationToken);

        List<JsonElement> matches = [];
        if (result != null && result.Count > 0)
        {
            using var jsonDocument = JsonDocument.Parse(result.Data);
            if (jsonDocument.RootElement.ValueKind == JsonValueKind.Array)
            {
                matches.AddRange(jsonDocument.RootElement.EnumerateArray().Select(item => item.Clone()));
            }
        }

        if (matches.Count == 0)
        {
            throw new KeyNotFoundException($"Could not find subscription with name '{subscription}'.");
        }

        if (matches.Count > 1)
        {
            throw new InvalidOperationException($"Multiple subscriptions found with name '{subscription}'.");
        }

        var subscriptionId = GetString(matches[0], "subscriptionId")
            ?? throw new KeyNotFoundException($"Could not find subscription with name '{subscription}'.");
        var resolvedTenantId = Guid.TryParse(GetString(matches[0], "tenantId"), out var matchTenant)
            ? matchTenant
            : tenantId;

        return (subscriptionId, resolvedTenantId);
    }

    private async Task<TenantResource> GetTenantResourceAsync(Guid? tenantId, CancellationToken cancellationToken)
    {
        var tenants = await AzureService.GetTenants(cancellationToken);
        if (tenants.Count == 0)
        {
            throw new InvalidOperationException("No accessible Azure tenants were found.");
        }

        if (tenantId is { } id)
        {
            var match = tenants.FirstOrDefault(t => t.Data.TenantId == id);
            if (match is not null)
            {
                return match;
            }
        }

        return tenants[0];
    }

    private static CostSavingsRecommendation ConvertToCostSavings(JsonElement item) => new(
        GetString(item, "id"),
        GetString(item, "name"),
        GetString(item, "tenantId"),
        GetString(item, "resourceGroup"),
        GetString(item, "subscriptionId"),
        GetString(item, "recommendationTypeId"),
        GetString(item, "savingsCurrency"),
        GetDouble(item, "annualSavingsAmount"),
        GetDouble(item, "savingsAmount"),
        GetDouble(item, "monthlyCarbonSavings"),
        GetString(item, "recommendationMessage"),
        GetString(item, "recommendationMessageDetailed"),
        GetString(item, "recommendationTypeSubCategory"),
        GetString(item, "solution"),
        GetString(item, "impactedField"),
        GetString(item, "impactedValue"),
        GetString(item, "impact"),
        GetString(item, "resourceId"));

    private static string? GetString(JsonElement item, string property)
    {
        if (!item.TryGetProperty(property, out var value) || value.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        return value.ValueKind == JsonValueKind.String ? value.GetString() : value.GetRawText();
    }

    private static double? GetDouble(JsonElement item, string property)
    {
        if (!item.TryGetProperty(property, out var value))
        {
            return null;
        }

        return value.ValueKind == JsonValueKind.Number && value.TryGetDouble(out var number) ? number : null;
    }
}
