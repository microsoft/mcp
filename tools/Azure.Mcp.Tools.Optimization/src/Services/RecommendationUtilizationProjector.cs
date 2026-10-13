// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Optimization.Models;

namespace Azure.Mcp.Tools.Optimization.Services;

/// <summary>Projects current versus target-SKU utilization from Azure Monitor metrics.</summary>
public static class RecommendationUtilizationProjector
{
    private static readonly TimeSpan NetworkSampleInterval = TimeSpan.FromMinutes(1);

    public static RecommendationUtilization Build(
        RecommendationExplanation recommendation,
        AzureMonitorUtilizationData metrics,
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        TimeSpan interval)
    {
        ArgumentNullException.ThrowIfNull(recommendation);
        ArgumentNullException.ThrowIfNull(metrics);

        var currentInstances = Math.Max(recommendation.CurrentInstanceCount ?? 1, 1);
        var newInstances = Math.Max(recommendation.NewInstanceCount ?? currentInstances, 1);
        var instanceRatio = currentInstances / (double)newInstances;
        var isShutdown = string.Equals(
            recommendation.RecommendationSubType,
            "shutdown",
            StringComparison.OrdinalIgnoreCase);

        var result = new RecommendationUtilization
        {
            StartTime = startTime,
            EndTime = endTime,
            IntervalMinutes = checked((int)interval.TotalMinutes),
            MemoryUnavailableReason = metrics.MemoryUnavailableReason,
        };

        for (var timestamp = startTime; timestamp < endTime; timestamp += interval)
        {
            metrics.CpuMaximumPercent.TryGetValue(timestamp, out var cpu);
            metrics.UsedMemoryMaximumPercent.TryGetValue(timestamp, out var memory);
            var hasCpu = metrics.CpuMaximumPercent.ContainsKey(timestamp);
            var hasMemory = metrics.UsedMemoryMaximumPercent.ContainsKey(timestamp);
            var maximumNetworkBytes = GetMaximumNetworkBytes(metrics, timestamp, timestamp + interval);
            var hasNetwork = maximumNetworkBytes.HasValue;

            var currentNetwork = hasNetwork
                ? ToNetworkPercent(
                    maximumNetworkBytes!.Value,
                    NetworkSampleInterval,
                    recommendation.NetworkMbps,
                    currentInstances)
                : null;

            result.Points.Add(new RecommendationUtilizationPoint
            {
                Timestamp = timestamp,
                Current = new UtilizationValues
                {
                    CpuPercent = hasCpu ? cpu : null,
                    UsedMemoryPercent = hasMemory ? memory : null,
                    NetworkPercent = currentNetwork,
                },
                Projected = new UtilizationValues
                {
                    CpuPercent = Project(
                        hasCpu ? cpu : null,
                        recommendation.SkuCores,
                        recommendation.NewSkuCores,
                        instanceRatio,
                        isShutdown),
                    UsedMemoryPercent = Project(
                        hasMemory ? memory : null,
                        recommendation.MemoryGB,
                        recommendation.NewMemoryGB,
                        instanceRatio,
                        isShutdown),
                    NetworkPercent = hasNetwork
                        ? ProjectNetwork(
                            maximumNetworkBytes!.Value,
                            NetworkSampleInterval,
                            recommendation.NewNetworkMbps,
                            newInstances,
                            isShutdown)
                        : null,
                },
            });
        }

        return result;
    }

    private static double? GetMaximumNetworkBytes(
        AzureMonitorUtilizationData metrics,
        DateTimeOffset startTime,
        DateTimeOffset endTime)
    {
        var timestamps = metrics.NetworkInTotalBytes.Keys
            .Concat(metrics.NetworkOutTotalBytes.Keys)
            .Where(timestamp => timestamp >= startTime && timestamp < endTime)
            .Distinct()
            .ToList();
        if (timestamps.Count == 0)
        {
            return null;
        }

        return timestamps.Max(timestamp =>
        {
            metrics.NetworkInTotalBytes.TryGetValue(timestamp, out var networkIn);
            metrics.NetworkOutTotalBytes.TryGetValue(timestamp, out var networkOut);
            return networkIn + networkOut;
        });
    }

    private static double? Project(
        double? currentPercent,
        double? currentCapacity,
        double? newCapacity,
        double instanceRatio,
        bool isShutdown)
    {
        if (!currentPercent.HasValue)
        {
            return null;
        }

        if (isShutdown)
        {
            return 0;
        }

        return currentCapacity is > 0 && newCapacity is > 0
            ? currentPercent.Value * currentCapacity.Value / newCapacity.Value * instanceRatio
            : null;
    }

    private static double? ToNetworkPercent(
        double totalBytes,
        TimeSpan interval,
        double? networkMbps,
        int instanceCount)
    {
        if (networkMbps is not > 0 || interval.TotalSeconds <= 0)
        {
            return null;
        }

        var observedMbps = totalBytes * 8.0 / interval.TotalSeconds / 1_000_000.0;
        return observedMbps / (networkMbps.Value * instanceCount) * 100.0;
    }

    private static double? ProjectNetwork(
        double totalBytes,
        TimeSpan interval,
        double? newNetworkMbps,
        int newInstanceCount,
        bool isShutdown) =>
        isShutdown ? 0 : ToNetworkPercent(totalBytes, interval, newNetworkMbps, newInstanceCount);
}
