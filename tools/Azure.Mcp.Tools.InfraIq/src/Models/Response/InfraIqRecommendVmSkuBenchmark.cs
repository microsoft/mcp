// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.InfraIq.Models.Response;

public sealed class InfraIqRecommendVmSkuBenchmark
{
    public required string Level { get; set; }

    public required string ModelClass { get; set; }

    public required string Scenario { get; set; }

    public required int SubmitterCount { get; set; }

    public string? SubmissionId { get; set; }

    public string? BenchmarkModel { get; set; }

    public string? Submitter { get; set; }

    public string? System { get; set; }

    public double? Throughput { get; set; }

    public string? ThroughputUnit { get; set; }

    public string? Software { get; set; }

    public string? OperatingSystem { get; set; }

    public string? WeightDataTypes { get; set; }

    public double? P99TtftMs { get; set; }

    public double? P99TpotMs { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}
