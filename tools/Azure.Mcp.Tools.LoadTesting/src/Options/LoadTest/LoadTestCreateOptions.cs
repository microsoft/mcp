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
}
