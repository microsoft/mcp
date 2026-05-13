// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.LoadTesting.Models.LoadTest;

public class OptionalLoadTestConfig
{
    /// <summary>
    /// Gets or sets the test duration in seconds.
    /// </summary>
    public int? Duration { get; set; }

    /// <summary>
    /// Gets or sets the target endpoint URL for the load test.
    /// </summary>
    public string? EndpointUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the maximum acceptable response time in milliseconds.
    /// </summary>
    public int? MaxResponseTimeInMs { get; set; }

    /// <summary>
    /// Gets or sets the ramp-up time in seconds to reach target load.
    /// </summary>
    public int? RampUpTime { get; set; }

    /// <summary>
    /// Gets or sets the target requests per second rate.
    /// </summary>
    public int? RequestsPerSecond { get; set; }

    /// <summary>
    /// Gets or sets the number of virtual users to simulate.
    /// </summary>
    public int? VirtualUsers { get; set; }
}
