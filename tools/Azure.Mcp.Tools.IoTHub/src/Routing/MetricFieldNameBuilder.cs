// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Azure.Mcp.Tools.IoTHub.Routing;

internal static class MetricFieldNameBuilder
{
    public static string GetCanonicalIdentity(
        string metricNamespace,
        string metricName,
        string aggregation,
        string unit,
        IReadOnlyDictionary<string, string> dimensions)
    {
        var dimensionIdentity = string.Join(
            "|",
            dimensions
                .OrderBy(entry => entry.Key, StringComparer.Ordinal)
                .ThenBy(entry => entry.Value, StringComparer.Ordinal)
                .Select(entry => $"{entry.Key}={entry.Value}"));
        return $"{metricNamespace}|{metricName}|{aggregation}|{unit}|{dimensionIdentity}";
    }

    public static string GetBaseFieldName(
        string metricName,
        string aggregation,
        string unit,
        IReadOnlyDictionary<string, string> dimensions)
    {
        var segments = new List<string> { EscapeSegment(metricName) };
        foreach (var (name, value) in dimensions
            .OrderBy(entry => GetDimensionOrder(entry.Key))
            .ThenBy(entry => entry.Key, StringComparer.Ordinal))
        {
            segments.Add(EscapeSegment(name));
            segments.Add(EscapeSegment(value));
        }

        var aggregationSegment = aggregation switch
        {
            "Average" => "avg",
            "Maximum" => "max",
            "Minimum" => "min",
            _ => EscapeSegment(aggregation)
        };
        var unitSegment = unit switch
        {
            "Milliseconds" => "ms",
            "Percent" => "pct",
            _ => EscapeSegment(unit)
        };

        segments.Add(aggregationSegment);
        if (!string.Equals(aggregationSegment, unitSegment, StringComparison.Ordinal))
        {
            segments.Add(unitSegment);
        }

        return string.Join(".", segments);
    }

    public static string AppendCollisionHash(string fieldName, string canonicalIdentity)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(canonicalIdentity));
        return $"{fieldName}~{Convert.ToHexString(hash.AsSpan(0, 4)).ToLowerInvariant()}";
    }

    private static int GetDimensionOrder(string name) => name switch
    {
        "Result" => 0,
        "FailureReasonCategory" => 1,
        _ => 2
    };

    private static string EscapeSegment(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        var lowerCamel = char.ToLowerInvariant(value[0]) + value[1..];
        var bytes = Encoding.UTF8.GetBytes(lowerCamel);
        var result = new StringBuilder(bytes.Length);
        foreach (var valueByte in bytes)
        {
            var isAllowed = valueByte is >= (byte)'a' and <= (byte)'z'
                or >= (byte)'A' and <= (byte)'Z'
                or >= (byte)'0' and <= (byte)'9'
                or (byte)'_'
                or (byte)'-';
            if (isAllowed)
            {
                result.Append((char)valueByte);
            }
            else
            {
                result.Append('%');
                result.Append(valueByte.ToString("X2", CultureInfo.InvariantCulture));
            }
        }
        return result.ToString();
    }
}
