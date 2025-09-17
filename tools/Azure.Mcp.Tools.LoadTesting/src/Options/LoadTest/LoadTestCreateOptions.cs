// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.LoadTesting.Options.LoadTest;

public class TestCreateOptions : BaseLoadTestingOptions
{
    /// <summary>
    /// The ID of the load test.
    /// </summary>
    public string? TestId { get; set; }

    /// <summary>
    /// The display name of the load test.
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// The display name of the load test.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// The display name of the load test.
    /// </summary>
    public string? Endpoint { get; set; }

    /// <summary>
    /// The display name of the load test.
    /// </summary>
    public int? VirtualUsers { get; set; }

    /// <summary>
    /// The duration of the load test.
    /// </summary>
    public int? Duration { get; set; }

    /// <summary>
    /// The ramp-up time for the load test.
    /// </summary>
    public int? RampUpTime { get; set; }

    /// <summary>
    /// The kind of load test. Default is "URL".
    /// JMX for jmeter based tests and is Locust for python based locust tests and URL for url tests.
    /// </summary>
    public string Kind { get; set; } = "URL";
}
