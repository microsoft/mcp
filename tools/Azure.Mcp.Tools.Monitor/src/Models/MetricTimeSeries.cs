// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Monitor.Models
{
    /// <summary>
    /// Represents a compact time series optimized for minimal JSON payload
    /// </summary>
    public class MetricTimeSeries
    {
        /// <summary>
        /// The dimension metadata for this time series (omitted if empty)
        /// </summary>
        [JsonPropertyName("metadata")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Dictionary<string, string> Metadata { get; set; } = new();

        /// <summary>
        /// Start time of the time series
        /// </summary>
        [JsonPropertyName("start")]
        public DateTime Start { get; set; }

        /// <summary>
        /// End time of the time series
        /// </summary>
        [JsonPropertyName("end")]
        public DateTime End { get; set; }

        /// <summary>
        /// Time grain (interval) between data points (e.g., "PT1M" for 1 minute)
        /// </summary>
        [JsonPropertyName("interval")]
        public string Interval { get; set; } = string.Empty;

        /// <summary>
        /// Array of average values (omitted if no average values)
        /// </summary>
        [JsonPropertyName("avgBuckets")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonConverter(typeof(RoundedDoubleArrayConverter))]
        public double[]? AvgBuckets { get; set; }

        /// <summary>
        /// Array of minimum values (omitted if no minimum values)
        /// </summary>
        [JsonPropertyName("minBuckets")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonConverter(typeof(RoundedDoubleArrayConverter))]
        public double[]? MinBuckets { get; set; }

        /// <summary>
        /// Array of maximum values (omitted if no maximum values)
        /// </summary>
        [JsonPropertyName("maxBuckets")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonConverter(typeof(RoundedDoubleArrayConverter))]
        public double[]? MaxBuckets { get; set; }

        /// <summary>
        /// Array of total values (omitted if no total values)
        /// </summary>
        [JsonPropertyName("totalBuckets")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonConverter(typeof(RoundedDoubleArrayConverter))]
        public double[]? TotalBuckets { get; set; }

        /// <summary>
        /// Array of count values (omitted if no count values)
        /// </summary>
        [JsonPropertyName("countBuckets")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonConverter(typeof(RoundedDoubleArrayConverter))]
        public double[]? CountBuckets { get; set; }
    }
}
