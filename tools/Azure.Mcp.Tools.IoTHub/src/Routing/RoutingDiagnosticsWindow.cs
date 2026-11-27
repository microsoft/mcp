// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Xml;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.IoTHub.Routing;

// Observation-window rules shared by routing diagnostics option validation and the routing service.
internal static class RoutingDiagnosticsWindow
{
    public static readonly TimeSpan DefaultObservationWindow = TimeSpan.FromHours(24);
    public static readonly TimeSpan MaxObservationWindow = TimeSpan.FromDays(30);
    public const int MaxMetricBuckets = 720;

    public const string IncompleteTimeRangeError =
        "--start-time and --end-time must be provided together.";

    public const string InvalidTimeRangeOrderError =
        "--start-time must be earlier than --end-time.";

    public const string TimeRangeTooLargeError =
        "The observation window cannot exceed 30 days.";

    public const string InvalidIntervalError =
        "--interval must be one of the Azure Monitor supported time grains: PT1M, PT5M, PT15M, PT30M, PT1H, PT6H, PT12H, or P1D.";

    public const string InvalidWindowTelemetryMessage =
        "Invalid IoT Hub routing diagnostics time window.";

    // Time grains Azure Monitor accepts for the metrics query; any other value is rejected by the service.
    private static readonly TimeSpan[] s_supportedIntervals =
    [
        TimeSpan.FromMinutes(1),
        TimeSpan.FromMinutes(5),
        TimeSpan.FromMinutes(15),
        TimeSpan.FromMinutes(30),
        TimeSpan.FromHours(1),
        TimeSpan.FromHours(6),
        TimeSpan.FromHours(12),
        TimeSpan.FromDays(1),
    ];

    public static void Validate(
        DateTimeOffset? startTime,
        DateTimeOffset? endTime,
        string? interval,
        ValidationResult validationResult)
    {
        try
        {
            Resolve(startTime, endTime, interval);
        }
        catch (ArgumentException ex)
        {
            validationResult.AddError(ex.Message, InvalidWindowTelemetryMessage);
        }
    }

    public static (DateTimeOffset StartTime, DateTimeOffset EndTime, TimeSpan Interval) Resolve(
        DateTimeOffset? startTime,
        DateTimeOffset? endTime,
        string? interval,
        DateTimeOffset? currentTime = null)
    {
        if (startTime.HasValue != endTime.HasValue)
        {
            throw new ArgumentException(IncompleteTimeRangeError);
        }

        var end = (endTime ?? currentTime ?? DateTimeOffset.UtcNow).ToUniversalTime();
        var start = (startTime ?? end.Subtract(DefaultObservationWindow)).ToUniversalTime();
        if (start >= end)
        {
            throw new ArgumentException(InvalidTimeRangeOrderError);
        }

        var duration = end - start;
        if (duration > MaxObservationWindow)
        {
            throw new ArgumentException(TimeRangeTooLargeError);
        }

        var resolvedInterval = ParseInterval(interval);
        var bucketCount = (int)Math.Ceiling(duration.Ticks / (double)resolvedInterval.Ticks);
        if (bucketCount > MaxMetricBuckets)
        {
            var minimumInterval = s_supportedIntervals.First(candidate =>
                Math.Ceiling(duration.Ticks / (double)candidate.Ticks) <= MaxMetricBuckets);
            throw new ArgumentException(
                $"The selected window and --interval produce {bucketCount} buckets; the maximum is {MaxMetricBuckets}. Use --interval {XmlConvert.ToString(minimumInterval)} or larger.");
        }

        return (start, end, resolvedInterval);
    }

    public static TimeSpan ParseInterval(string? interval)
    {
        if (string.IsNullOrWhiteSpace(interval))
        {
            return TimeSpan.FromHours(1);
        }

        TimeSpan value;
        try
        {
            value = XmlConvert.ToTimeSpan(interval);
        }
        catch (FormatException ex)
        {
            throw new ArgumentException(InvalidIntervalError, ex);
        }

        return s_supportedIntervals.Contains(value)
            ? value
            : throw new ArgumentException(InvalidIntervalError);
    }
}
