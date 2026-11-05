// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Globalization;
using System.Text.Json;
using Azure.Mcp.Tools.IoTHub.Commands;
using Azure.Mcp.Tools.IoTHub.Models;

namespace Azure.Mcp.Tools.IoTHub.Routing;

internal sealed class RoutingMetricEvidenceBuilder(string metricNamespace)
{
    private readonly string _metricNamespace = metricNamespace;
    private readonly Dictionary<string, JsonElement> _windowAggregates = new(StringComparer.Ordinal);
    private readonly SortedDictionary<DateTimeOffset, Dictionary<string, JsonElement>> _buckets = [];
    private readonly Dictionary<string, string> _fieldIdentities = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> _metricAvailability = new(StringComparer.Ordinal);

    public bool HasValues => _windowAggregates.Count > 0 || _buckets.Count > 0;

    public bool ContainsMetric(string metricName) => _metricAvailability.ContainsKey(metricName);

    public void MarkSuccessful(string metricName, bool hasValues = false)
    {
        if (!_metricAvailability.TryGetValue(metricName, out var status) || status == "noValuesReturned")
        {
            _metricAvailability[metricName] = hasValues ? "valuesReturned" : "noValuesReturned";
        }
        else if (hasValues && status == "failed")
        {
            _metricAvailability[metricName] = "partial";
        }
    }

    public void MarkFailed(string metricName) =>
        _metricAvailability[metricName] =
            _metricAvailability.GetValueOrDefault(metricName) is "valuesReturned" or "partial"
                ? "partial"
                : "failed";

    public void MarkPartial(string metricName) => _metricAvailability[metricName] = "partial";

    public void Merge(RoutingMetricEvidence evidence)
    {
        foreach (var (metricName, status) in evidence.MetricAvailability)
        {
            _metricAvailability[metricName] = status;
        }

        foreach (var (fieldName, value) in evidence.WindowAggregates)
        {
            if (!_windowAggregates.TryAdd(fieldName, value))
            {
                throw new InvalidDataException($"Metric field '{fieldName}' was returned more than once.");
            }
        }

        foreach (var sourceBucket in evidence.Buckets)
        {
            var timestamp = DateTimeOffset.Parse(
                sourceBucket["timestamp"].GetString()!,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal);
            var targetBucket = GetBucket(timestamp);
            foreach (var (fieldName, value) in sourceBucket)
            {
                if (string.Equals(fieldName, "timestamp", StringComparison.Ordinal))
                {
                    continue;
                }
                if (!targetBucket.TryAdd(fieldName, value))
                {
                    throw new InvalidDataException(
                        $"Metric field '{fieldName}' was returned more than once for bucket '{sourceBucket["timestamp"].GetString()}'.");
                }
            }
        }
    }

    public void AddCountSeries(
        string metricName,
        string aggregation,
        IReadOnlyDictionary<string, string> dimensions,
        IEnumerable<(DateTimeOffset Timestamp, double? Value)> points)
    {
        const string unit = "Count";
        var fieldName = ResolveFieldName(metricName, aggregation, unit, dimensions);
        long sum = 0;
        var values = new Dictionary<DateTimeOffset, long>();
        foreach (var (timestamp, value) in points)
        {
            if (!value.HasValue)
            {
                continue;
            }

            var count = ToInt64Count(value.Value, fieldName);
            sum = checked(sum + count);
            var utcTimestamp = timestamp.ToUniversalTime();
            values[utcTimestamp] = checked(values.GetValueOrDefault(utcTimestamp) + count);
        }

        var hasValue = values.Count > 0;
        if (hasValue)
        {
            var current = _windowAggregates.TryGetValue(fieldName, out var existing)
                ? existing.GetInt64()
                : 0;
            var aggregate = checked(current + sum);
            foreach (var (timestamp, value) in values)
            {
                var currentBucketValue = _buckets.TryGetValue(timestamp, out var bucket) &&
                    bucket.TryGetValue(fieldName, out var existingBucketValue)
                        ? existingBucketValue.GetInt64() : 0;
                _ = checked(currentBucketValue + value);
            }
            foreach (var (timestamp, value) in values)
            {
                AddCountBucketValue(timestamp, fieldName, value);
            }
            _windowAggregates[fieldName] = ToJsonElement(aggregate);
        }
        MarkSuccessful(metricName, hasValue);
    }

    public void AddAverageSeries(
        string metricName,
        string unit,
        IReadOnlyDictionary<string, string> dimensions,
        IEnumerable<(DateTimeOffset Timestamp, double? Value)> points)
    {
        var fieldName = ResolveFieldName(metricName, "Average", unit, dimensions);
        var values = points.Where(point => point.Value.HasValue).ToArray();
        foreach (var (_, value) in values)
        {
            ValidateFinite(value!.Value, fieldName);
        }
        foreach (var (timestamp, value) in values)
        {
            AddDoubleBucketValue(timestamp, fieldName, value!.Value);
        }
        MarkSuccessful(metricName, values.Length > 0);
    }

    public void AddMaximumSeries(
        string metricName,
        string unit,
        IReadOnlyDictionary<string, string> dimensions,
        IEnumerable<(DateTimeOffset Timestamp, double? Value)> points)
    {
        var fieldName = ResolveFieldName(metricName, "Maximum", unit, dimensions);
        double? maximum = null;
        foreach (var (timestamp, value) in points)
        {
            if (!value.HasValue)
            {
                continue;
            }

            ValidateFinite(value.Value, fieldName);
            AddDoubleBucketValue(timestamp, fieldName, value.Value);
            maximum = !maximum.HasValue || value.Value > maximum.Value ? value.Value : maximum;
        }

        if (maximum.HasValue)
        {
            _windowAggregates[fieldName] = ToJsonElement(maximum.Value);
        }
        MarkSuccessful(metricName, maximum.HasValue);
    }

    public RoutingMetricEvidence Build() => new(
        _metricNamespace,
        new Dictionary<string, JsonElement>(_windowAggregates, StringComparer.Ordinal),
        [.. _buckets.Select(entry =>
        {
            var bucket = new Dictionary<string, JsonElement>(StringComparer.Ordinal)
            {
                ["timestamp"] = JsonSerializer.SerializeToElement(
                    ToIsoString(entry.Key),
                    IoTHubJsonContext.Default.String)
            };
            foreach (var (fieldName, value) in entry.Value)
            {
                bucket[fieldName] = value;
            }
            return bucket;
        })])
    {
        MetricAvailability = new Dictionary<string, string>(_metricAvailability, StringComparer.Ordinal)
    };

    private string ResolveFieldName(
        string metricName,
        string aggregation,
        string unit,
        IReadOnlyDictionary<string, string> dimensions)
    {
        var identity = MetricFieldNameBuilder.GetCanonicalIdentity(
            _metricNamespace,
            metricName,
            aggregation,
            unit,
            dimensions);
        var fieldName = MetricFieldNameBuilder.GetBaseFieldName(metricName, aggregation, unit, dimensions);
        if (_fieldIdentities.TryGetValue(fieldName, out var existingIdentity) &&
            !string.Equals(existingIdentity, identity, StringComparison.Ordinal))
        {
            fieldName = MetricFieldNameBuilder.AppendCollisionHash(fieldName, identity);
        }

        _fieldIdentities[fieldName] = identity;
        return fieldName;
    }

    private void AddCountBucketValue(DateTimeOffset timestamp, string fieldName, long value)
    {
        var bucket = GetBucket(timestamp);
        var current = bucket.TryGetValue(fieldName, out var existing) ? existing.GetInt64() : 0;
        bucket[fieldName] = ToJsonElement(checked(current + value));
    }

    private void AddDoubleBucketValue(DateTimeOffset timestamp, string fieldName, double value)
    {
        ValidateFinite(value, fieldName);
        GetBucket(timestamp)[fieldName] = ToJsonElement(value);
    }

    private Dictionary<string, JsonElement> GetBucket(DateTimeOffset timestamp)
    {
        var utcTimestamp = timestamp.ToUniversalTime();
        if (!_buckets.TryGetValue(utcTimestamp, out var bucket))
        {
            bucket = new Dictionary<string, JsonElement>(StringComparer.Ordinal);
            _buckets[utcTimestamp] = bucket;
        }
        return bucket;
    }

    private static long ToInt64Count(double value, string fieldName)
    {
        ValidateFinite(value, fieldName);
        if (value != Math.Truncate(value) || value < long.MinValue || value > long.MaxValue)
        {
            throw new InvalidDataException(
                $"Metric field '{fieldName}' returned a count that cannot be represented as a 64-bit integer.");
        }
        return checked((long)value);
    }

    private static void ValidateFinite(double value, string fieldName)
    {
        if (!double.IsFinite(value))
        {
            throw new InvalidDataException($"Metric field '{fieldName}' returned a non-finite value.");
        }
    }

    private static JsonElement ToJsonElement(long value) =>
        JsonSerializer.SerializeToElement(value, IoTHubJsonContext.Default.Int64);

    private static JsonElement ToJsonElement(double value) =>
        JsonSerializer.SerializeToElement(value, IoTHubJsonContext.Default.Double);

    private static string ToIsoString(DateTimeOffset value) =>
        value.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ", CultureInfo.InvariantCulture);
}
