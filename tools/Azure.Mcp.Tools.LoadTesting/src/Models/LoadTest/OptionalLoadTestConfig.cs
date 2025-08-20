// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.LoadTesting.Models.LoadTest;
public class OptionalLoadTestConfig
{
    /// <summary>
    /// Gets or sets the test duration in seconds.
    /// </summary>
    [JsonPropertyName("duration")]
    public int? Duration { get; set; }

    /// <summary>
    /// Gets or sets the target endpoint URL for the load test.
    /// </summary>
    [JsonPropertyName("endpointUrl")]
    public string? EndpointUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the maximum acceptable response time in milliseconds.
    /// </summary>
    [JsonPropertyName("maxResponseTimeInMs")]
    public int? MaxResponseTimeInMs { get; set; }

    /// <summary>
    /// Gets or sets the ramp-up time in seconds to reach target load.
    /// </summary>
    [JsonPropertyName("rampUpTime")]
    public int? RampUpTime { get; set; }

    /// <summary>
    /// Gets or sets the target requests per second rate.
    /// </summary>
    [JsonPropertyName("requestsPerSecond")]
    public int? RequestsPerSecond { get; set; }

    /// <summary>
    /// Gets or sets the number of virtual users to simulate.
    /// </summary>
    [JsonPropertyName("virtualUsers")]
    public int? VirtualUsers { get; set; }
}
