using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.ApplicationInsights.Models;

public class TraceIdEntry : IEquatable<TraceIdEntry>
{
    [JsonPropertyName("traceId")]
    public string TraceId { get; set; } = string.Empty;

    [JsonPropertyName("spanId")]
    public string SpanId { get; set; } = string.Empty;

    public bool Equals(TraceIdEntry? other)
    {
        return other is not null &&
               string.Equals(TraceId, other.TraceId, StringComparison.OrdinalIgnoreCase) &&
               string.Equals(SpanId, other.SpanId, StringComparison.OrdinalIgnoreCase);
    }

    public override bool Equals(object? obj)
    {
        if (obj is TraceIdEntry other)
        {
            return Equals(other);
        }
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(TraceId, SpanId);
    }
}
