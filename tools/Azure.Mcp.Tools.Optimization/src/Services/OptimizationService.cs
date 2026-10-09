// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

// cSpell:ignore Vcpus resourcecontainers

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

    public async Task<CostSavingsResult> ListCostSavingsAsync(
        string? subscription,
        int top,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        // Normalize the multi-line raw string query to CRLF so the request body is byte-identical
        // across platforms; raw string literal line endings follow the checkout EOL (CRLF on
        // Windows, LF on Linux), which otherwise breaks recorded-test playback matching.
        var query = $"{OptimizationKqlQueries.TopCostSavingsQuery.ReplaceLineEndings("\r\n")}\n| limit {top}";

        string? subscriptionId = null;
        string? queryTenantId;
        if (!string.IsNullOrWhiteSpace(subscription))
        {
            (subscriptionId, queryTenantId, var candidates) = await ResolveSubscriptionAsync(
                subscription.Trim('"', '\''), tenant, returnCandidatesOnMultipleMatch: true, cancellationToken);

            if (candidates is not null)
            {
                return new CostSavingsResult([], false, candidates);
            }
        }
        else
        {
            // Without a subscription, the queries are not scoped and cover every accessible subscription in the
            // --tenant tenant, or in the signed-in session's default tenant when none is given (null tenant id).
            queryTenantId = string.IsNullOrWhiteSpace(tenant)
                ? null
                : await AzureService.GetTenantId(tenant, cancellationToken);
        }

        var tenantResource = await GetTenantResourceAsync(queryTenantId, cancellationToken);

        var recommendationsTask = ExecuteResourceGraphQueryAsync(tenantResource, query, subscriptionId, cancellationToken);
        var diskSummaryTask = GetDiskRecommendationSummaryAsync(tenantResource, subscriptionId, cancellationToken);
        await Task.WhenAll(recommendationsTask, diskSummaryTask);

        var (rows, truncated) = recommendationsTask.Result;
        var recommendations = rows.Select(ConvertToCostSavings).ToList();
        return new CostSavingsResult(recommendations, truncated, DiskRecommendationSummary: diskSummaryTask.Result);
    }

    /// <summary>
    /// Runs the unattached-disk summary query. Returns null when no unattached-disk recommendations
    /// exist, or when the summary query fails so the main recommendation list is still returned.
    /// A null <paramref name="subscriptionId"/> summarizes across all accessible subscriptions.
    /// </summary>
    private async Task<DiskRecommendationSummary?> GetDiskRecommendationSummaryAsync(
        TenantResource tenantResource,
        string? subscriptionId,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = OptimizationKqlQueries.UnattachedDiskSummaryQuery.ReplaceLineEndings("\r\n");
            var (rows, _) = await ExecuteResourceGraphQueryAsync(tenantResource, query, subscriptionId, cancellationToken);
            return BuildDiskRecommendationSummary(rows, subscriptionId);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Failed to summarize unattached-disk recommendations. Subscription: {Subscription}.",
                subscriptionId ?? "all accessible subscriptions");
            return null;
        }
    }

    internal static DiskRecommendationSummary? BuildDiskRecommendationSummary(IReadOnlyList<JsonElement> rows, string? subscriptionId)
    {
        if (rows.Count == 0
            || !rows[0].TryGetProperty("unattachedDiskCount", out var countElement)
            || !countElement.TryGetInt32(out var count)
            || count <= 0)
        {
            return null;
        }

        List<string> subscriptionIds = [];
        if (rows[0].TryGetProperty("subscriptionIds", out var idsElement) && idsElement.ValueKind == JsonValueKind.Array)
        {
            subscriptionIds.AddRange(idsElement.EnumerateArray()
                .Where(e => e.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(e.GetString()))
                .Select(e => e.GetString()!));
        }

        if (subscriptionIds.Count == 0 && !string.IsNullOrWhiteSpace(subscriptionId))
        {
            subscriptionIds.Add(subscriptionId.ToLowerInvariant());
        }

        var encodedIds = string.Join("%2C", subscriptionIds.Select(id => $"%22{Uri.EscapeDataString(id)}%22"));
        var actionUrl =
            "https://ms.portal.azure.com/#view/Microsoft_Azure_Expert/RecommendationList.ReactView/recommendationTypeId/" +
            $"{OptimizationKqlQueries.UnattachedDiskRecommendationTypeId}/recommendationStatus~/0/subscriptionIds~/%5B{encodedIds}%5D";

        return new DiskRecommendationSummary(
            $"Review disks that are not attached to a VM and evaluate if you still need the disks: {count} found.",
            count,
            subscriptionIds,
            actionUrl);
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
        var (subscriptionId, subscriptionTenantId, _) = await ResolveSubscriptionAsync(
            subscription, tenant, returnCandidatesOnMultipleMatch: false, cancellationToken);

        var tenantResource = await GetTenantResourceAsync(subscriptionTenantId, cancellationToken);

        return await ExecuteResourceGraphQueryAsync(tenantResource, query, subscriptionId!, cancellationToken);
    }

    /// <summary>
    /// Executes an Azure Resource Graph query. When <paramref name="subscriptionId"/> is null the
    /// query is not scoped and covers every subscription the caller can access.
    /// </summary>
    private static async Task<(List<JsonElement> Rows, bool Truncated)> ExecuteResourceGraphQueryAsync(
        TenantResource tenantResource,
        string query,
        string? subscriptionId,
        CancellationToken cancellationToken)
    {
        var queryContent = new ResourceQueryContent(query);
        if (!string.IsNullOrWhiteSpace(subscriptionId))
        {
            queryContent.Subscriptions.Add(subscriptionId);
        }

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
    /// enumerating every subscription. When <paramref name="returnCandidatesOnMultipleMatch"/> is
    /// true and the name matches more than one subscription, the candidates are returned so the
    /// caller can ask the user to select the correct one; otherwise an exception is thrown.
    /// </summary>
    private async Task<(string? SubscriptionId, string? TenantId, IReadOnlyList<SubscriptionOption>? Candidates)> ResolveSubscriptionAsync(
        string subscription,
        string? tenant,
        bool returnCandidatesOnMultipleMatch,
        CancellationToken cancellationToken)
    {
        var tenantId = string.IsNullOrWhiteSpace(tenant)
            ? null
            : await AzureService.GetTenantId(tenant, cancellationToken);

        if (Guid.TryParse(subscription, out _))
        {
            return (subscription, tenantId, null);
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
            if (returnCandidatesOnMultipleMatch)
            {
                var candidates = matches
                    .Select(m => new SubscriptionOption(
                        GetString(m, "subscriptionId"),
                        GetString(m, "name"),
                        GetString(m, "tenantId")))
                    .ToList();
                return (null, null, candidates);
            }

            var options = string.Join(
                "; ",
                matches.Select(m => $"'{GetString(m, "name")}' ({GetString(m, "subscriptionId")})"));
            throw new InvalidOperationException(
                $"Multiple subscriptions match '{subscription}'. Please select the correct one by specifying its exact name or subscription id: {options}.");
        }

        var subscriptionId = GetString(matches[0], "subscriptionId")
            ?? throw new KeyNotFoundException($"Could not find subscription with name '{subscription}'.");
        var resolvedTenantId = GetString(matches[0], "tenantId") ?? tenantId;

        return (subscriptionId, resolvedTenantId, null);
    }

    /// <summary>
    /// Returns a <see cref="TenantResource"/> whose client authenticates against <paramref name="tenantId"/>
    /// (or the default tenant when null). Resource Graph queries use the client's credential, so the client
    /// must be created for the target tenant rather than selecting a tenant from the default client.
    /// </summary>
    private async Task<TenantResource> GetTenantResourceAsync(string? tenantId, CancellationToken cancellationToken)
    {
        var armClient = await CreateArmClientAsync(tenantId, cancellationToken: cancellationToken);
        await foreach (var tenant in armClient.GetTenants().GetAllAsync(cancellationToken))
        {
            return tenant;
        }

        throw new InvalidOperationException("No accessible Azure tenants were found for the current credential.");
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
