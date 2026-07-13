// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Globalization;
using System.Text.Json;
using Azure.Core;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.IoTHub.Models;
using Azure.ResourceManager;
using Azure.ResourceManager.IotHub;
using Azure.ResourceManager.IotHub.Models;
using Azure.Monitor.Query;
using Azure.Monitor.Query.Models;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.IoTHub.Services;

public class IoTHubService(
    ISubscriptionService subscriptionService,
    ITenantService tenantService,
    ILogger<IoTHubService> logger)
    : BaseAzureResourceService(subscriptionService, tenantService), IIoTHubService
{
    private readonly ISubscriptionService _subscriptionService = subscriptionService;
    private readonly ILogger<IoTHubService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    private const string IoTHubMetricNamespace = "Microsoft.Devices/IotHubs";

    private static readonly string[] IoTHubUsageMetricNames =
    [
        "connectedDeviceCount",
        "totalDeviceCount",
        "dailyMessageQuotaUsed",
        "d2c.telemetry.ingress.success",
        "d2c.telemetry.ingress.sendThrottle",
    ];

    public async Task<List<IoTHubDescription>> GetIoTHub(
        string? name,
        string? resourceGroup,
        string subscription,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(subscription), subscription));

        string? additionalFilter = null;
        if (!string.IsNullOrEmpty(name))
        {
            additionalFilter = $"name =~ '{name}'";
        }

        return await ExecuteResourceQueryAsync(
            "Microsoft.Devices/IotHubs",
            resourceGroup,
            subscription,
            retryPolicy,
            ConvertToIoTHubDescription,
            additionalFilter: additionalFilter,
            cancellationToken: cancellationToken);
    }

    public async Task<IoTHubDescription> CreateIoTHub(
        string name,
        string resourceGroup,
        string location,
        string sku,
        long capacity,
        string subscription,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(name), name),
            (nameof(resourceGroup), resourceGroup),
            (nameof(location), location),
            (nameof(sku), sku),
            (nameof(subscription), subscription));

        var subResource = await _subscriptionService.GetSubscription(subscription, null, retryPolicy, cancellationToken);
        var rg = await subResource.GetResourceGroups().GetAsync(resourceGroup, cancellationToken);

        var data = new IotHubDescriptionData(new AzureLocation(location), new IotHubSkuInfo(new IotHubSku(sku)) { Capacity = capacity });
        var collection = rg.Value.GetIotHubDescriptions();
        var operation = await collection.CreateOrUpdateAsync(WaitUntil.Completed, name, data, null, cancellationToken);

        return ConvertToIoTHubDescription(operation.Value);
    }

    public async Task<IoTHubDescription> UpdateIoTHub(
        string name,
        string resourceGroup,
        string? sku,
        long? capacity,
        string subscription,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(name), name),
            (nameof(resourceGroup), resourceGroup),
            (nameof(subscription), subscription));

        var subResource = await _subscriptionService.GetSubscription(subscription, null, retryPolicy, cancellationToken);
        var rg = await subResource.GetResourceGroups().GetAsync(resourceGroup, cancellationToken);
        var hub = await rg.Value.GetIotHubDescriptionAsync(name, cancellationToken);

        var data = hub.Value.Data;
        if (sku != null)
        {
            data.Sku = new IotHubSkuInfo(new IotHubSku(sku))
            {
                Capacity = capacity ?? data.Sku.Capacity
            };
        }
        else if (capacity.HasValue)
        {
            data.Sku.Capacity = capacity.Value;
        }

        var operation = await rg.Value.GetIotHubDescriptions().CreateOrUpdateAsync(WaitUntil.Completed, name, data, null, cancellationToken);
        return ConvertToIoTHubDescription(operation.Value);
    }

    public async Task DeleteIoTHub(
        string name,
        string resourceGroup,
        string subscription,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(name), name),
            (nameof(resourceGroup), resourceGroup),
            (nameof(subscription), subscription));

        var subResource = await _subscriptionService.GetSubscription(subscription, null, retryPolicy, cancellationToken);
        var rg = await subResource.GetResourceGroups().GetAsync(resourceGroup, cancellationToken);
        var hub = await rg.Value.GetIotHubDescriptionAsync(name, cancellationToken);
        await hub.Value.DeleteAsync(WaitUntil.Started, cancellationToken);
    }

    public async Task<List<IoTHubKey>> GetIoTHubKeys(
        string name,
        string resourceGroup,
        string subscription,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(name), name),
            (nameof(resourceGroup), resourceGroup),
            (nameof(subscription), subscription));

        var subResource = await _subscriptionService.GetSubscription(subscription, null, retryPolicy, cancellationToken);
        var rg = await subResource.GetResourceGroups().GetAsync(resourceGroup, cancellationToken);
        var hub = await rg.Value.GetIotHubDescriptionAsync(name, cancellationToken);

        var keys = new List<IoTHubKey>();
        await foreach (var key in hub.Value.GetKeysAsync(cancellationToken))
        {
            keys.Add(new IoTHubKey(key.KeyName, key.PrimaryKey, key.SecondaryKey, key.Rights.ToString()));
        }
        return keys;
    }

    public async Task<IoTHubUsageSnapshot> GetUsageSnapshot(
        string name,
        string resourceGroup,
        string subscription,
        string? startTime = null,
        string? endTime = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(name), name),
            (nameof(resourceGroup), resourceGroup),
            (nameof(subscription), subscription));

        // The subscription argument may be a name or an ID; Azure Monitor requires the
        // subscription GUID in the resource URI, so resolve it first.
        var subResource = await _subscriptionService.GetSubscription(subscription, null, retryPolicy, cancellationToken);
        var subscriptionId = subResource.Id.SubscriptionId;

        var resourceId =
            $"/subscriptions/{subscriptionId}/resourceGroups/{resourceGroup}" +
            $"/providers/Microsoft.Devices/IotHubs/{name}";

        // Resolve the query window. Defaults to the last 24 hours when not provided.
        // Normalize to UTC so the query and the reported window are unambiguous; offset-aware
        // inputs (e.g. ...-07:00) denote the same instant and are converted to UTC here.
        var startOffset = ParseTime(startTime, nameof(startTime));
        var endOffset = ParseTime(endTime, nameof(endTime));
        var resolvedEnd = (endOffset ?? DateTimeOffset.UtcNow).ToUniversalTime();
        var resolvedStart = (startOffset ?? resolvedEnd - TimeSpan.FromDays(1)).ToUniversalTime();

        if (resolvedStart >= resolvedEnd)
        {
            throw new ArgumentException($"The start time ('{resolvedStart:o}') must be earlier than the end time ('{resolvedEnd:o}').");
        }

        var credential = await GetCredential(cancellationToken);
        var clientOptions = ConfigureRetryPolicy(AddDefaultPolicies(new MetricsQueryClientOptions()), retryPolicy);
        var client = new MetricsQueryClient(credential, clientOptions);

        var queryOptions = new MetricsQueryOptions
        {
            TimeRange = new QueryTimeRange(resolvedStart, resolvedEnd),
            MetricNamespace = IoTHubMetricNamespace,
        };
        queryOptions.Aggregations.Add(MetricAggregationType.Maximum);
        queryOptions.Aggregations.Add(MetricAggregationType.Average);
        queryOptions.Aggregations.Add(MetricAggregationType.Total);

        var response = await client.QueryResourceAsync(
            resourceId,
            IoTHubUsageMetricNames,
            queryOptions,
            cancellationToken);

        var metrics = response.Value.Metrics;

        var (quotaSingle, quotaByDay, quotaTotal) = BuildQuotaUsage(metrics, "dailyMessageQuotaUsed", resolvedStart, resolvedEnd);

        return new IoTHubUsageSnapshot(
            HubName: name,
            SnapshotTime: DateTimeOffset.UtcNow,
            StartTime: resolvedStart,
            EndTime: resolvedEnd,
            ConnectedDeviceCount: BuildDeviceCountStats(metrics, "connectedDeviceCount"),
            TotalDeviceCount: BuildDeviceCountStats(metrics, "totalDeviceCount"),
            DailyMessageQuotaUsed: quotaSingle,
            DailyMessageQuotaUsedByDay: quotaByDay,
            TotalMessagesUsed: quotaTotal,
            D2CMessageCount: SumTotal(metrics, "d2c.telemetry.ingress.success"),
            ThrottlingErrors: SumTotal(metrics, "d2c.telemetry.ingress.sendThrottle"));
    }

    private static DateTimeOffset? ParseTime(string? value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (!DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var parsed))
        {
            throw new ArgumentException($"Invalid {parameterName} format: '{value}'. Use ISO 8601, for example 2026-07-07T00:00:00Z");
        }

        return parsed;
    }

    private static IoTHubDeviceCountStats BuildDeviceCountStats(IReadOnlyList<MetricResult> metrics, string metricName)
    {
        var values = metrics
            .FirstOrDefault(m => string.Equals(m.Name, metricName, StringComparison.OrdinalIgnoreCase))?
            .TimeSeries
            .SelectMany(ts => ts.Values)
            .ToList();

        if (values is null || values.Count == 0)
        {
            return new IoTHubDeviceCountStats(null, null, null);
        }

        // Snapshot: the most recent point-in-time value in the window.
        var snapshot = values
            .Where(v => v.Maximum.HasValue)
            .OrderBy(v => v.TimeStamp)
            .LastOrDefault()?
            .Maximum;

        // Peak: the maximum value observed across the window.
        var maximums = values
            .Where(v => v.Maximum.HasValue)
            .Select(v => v.Maximum!.Value)
            .ToList();
        double? peak = maximums.Count > 0 ? maximums.Max() : null;

        // Average: the mean value across the window.
        var averages = values
            .Where(v => v.Average.HasValue)
            .Select(v => v.Average!.Value)
            .ToList();
        double? average = averages.Count > 0 ? averages.Average() : null;

        return new IoTHubDeviceCountStats(snapshot, peak, average);
    }

    private static double? SumTotal(IReadOnlyList<MetricResult> metrics, string metricName)
    {
        var values = metrics
            .FirstOrDefault(m => string.Equals(m.Name, metricName, StringComparison.OrdinalIgnoreCase))?
            .TimeSeries
            .SelectMany(ts => ts.Values)
            .Where(v => v.Total.HasValue)
            .Select(v => v.Total!.Value)
            .ToList();

        return values is { Count: > 0 } ? values.Sum() : null;
    }

    /// <summary>
    /// Resolves the daily message quota usage for the window.
    /// dailyMessageQuotaUsed is a cumulative counter that resets at 00:00 UTC, so each UTC day's
    /// usage is its closing value: the last sample reported that day. Using the closing value
    /// (rather than the daily maximum) avoids double-counting when the counter's reset lags past
    /// 00:00 UTC and a high previous-day peak lingers into the early hours of an otherwise idle day.
    /// A single-day window returns a scalar; a multi-day window returns a per-day breakdown plus the
    /// total across days.
    /// </summary>
    private static (double? Single, IReadOnlyList<IoTHubDailyMessageUsage>? ByDay, double? Total) BuildQuotaUsage(
        IReadOnlyList<MetricResult> metrics,
        string metricName,
        DateTimeOffset start,
        DateTimeOffset end)
    {
        var samples = metrics
            .FirstOrDefault(m => string.Equals(m.Name, metricName, StringComparison.OrdinalIgnoreCase))?
            .TimeSeries
            .SelectMany(ts => ts.Values)
            .Where(v => v.Maximum.HasValue)
            .Select(v => (v.TimeStamp, Value: v.Maximum!.Value));

        var byDate = samples is null ? null : ComputeDailyClosingValues(samples);

        var dates = EnumerateUtcDates(start, end);

        if (dates.Count <= 1)
        {
            var day = dates.Count == 1 ? dates[0] : DateOnly.FromDateTime(start.UtcDateTime);
            double? single = byDate is not null && byDate.TryGetValue(day, out var value) ? value : null;
            return (single, null, null);
        }

        var breakdown = new List<IoTHubDailyMessageUsage>(dates.Count);
        double total = 0;
        var hasAny = false;
        foreach (var day in dates)
        {
            double? value = byDate is not null && byDate.TryGetValue(day, out var v) ? v : null;
            if (value.HasValue)
            {
                total += value.Value;
                hasAny = true;
            }

            breakdown.Add(new IoTHubDailyMessageUsage(day.ToString("yyyy-MM-dd"), value));
        }

        return (null, breakdown, hasAny ? total : null);
    }

    /// <summary>
    /// Groups cumulative-counter samples by UTC day and returns each day's closing value: the
    /// value of the latest sample reported that day. For a counter that resets at 00:00 UTC this
    /// is the correct daily usage, and-unlike a daily maximum-it is not corrupted when the reset
    /// lags past midnight and the prior day's peak lingers into the early hours of the next day.
    /// </summary>
    internal static Dictionary<DateOnly, double> ComputeDailyClosingValues(
        IEnumerable<(DateTimeOffset TimeStamp, double Value)> samples)
    {
        return samples
            .GroupBy(s => DateOnly.FromDateTime(s.TimeStamp.UtcDateTime))
            .ToDictionary(g => g.Key, g => g.MaxBy(s => s.TimeStamp).Value);
    }

    /// <summary>
    /// Enumerates the UTC calendar dates covered by the window. The end is treated as exclusive,
    /// so a window ending exactly at midnight does not include that final day.
    /// </summary>
    private static List<DateOnly> EnumerateUtcDates(DateTimeOffset start, DateTimeOffset end)
    {
        var startDate = DateOnly.FromDateTime(start.UtcDateTime);
        var endUtc = end.UtcDateTime;
        var lastDate = DateOnly.FromDateTime(endUtc);
        if (endUtc.TimeOfDay == TimeSpan.Zero)
        {
            lastDate = lastDate.AddDays(-1);
        }

        if (lastDate < startDate)
        {
            lastDate = startDate;
        }

        var result = new List<DateOnly>();
        for (var day = startDate; day <= lastDate; day = day.AddDays(1))
        {
            result.Add(day);
        }

        return result;
    }

    private IoTHubDescription ConvertToIoTHubDescription(IotHubDescriptionResource resource)
    {
        return new IoTHubDescription(
            resource.Id.ToString(),
            resource.Data.Name,
            resource.Data.Location.Name,
            resource.Id.ResourceGroupName ?? "",
            resource.Id.SubscriptionId ?? "",
            resource.Data.Sku.Name.ToString(),
            resource.Data.Sku.Capacity ?? 0,
            "Unknown",
            resource.Data.Properties.HostName);
    }

    private IoTHubDescription ConvertToIoTHubDescription(JsonElement element)
    {
        var properties = element.GetProperty("properties");
        var sku = element.GetProperty("sku");

        return new IoTHubDescription(
            element.GetProperty("id").GetString()!,
            element.GetProperty("name").GetString()!,
            element.GetProperty("location").GetString()!,
            element.GetProperty("resourceGroup").GetString()!,
            element.GetProperty("subscriptionId").GetString()!,
            sku.GetProperty("name").GetString()!,
            sku.TryGetProperty("capacity", out var cap) ? cap.GetInt64() : 0,
            properties.TryGetProperty("state", out var state) ? state.GetString()! : "Unknown",
            properties.TryGetProperty("hostName", out var hostName) ? hostName.GetString()! : ""
        );
    }
}
