// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.LoadTesting.Options.LoadTest;

public sealed class TestCreateOptions : ISubscriptionOption
{
    /// <summary>
    /// The name of the test resource.
    /// </summary>
    [Option(Description = LoadTestingOptionDescriptions.TestResourceName)]
    public required string TestResourceName { get; set; }

    /// <summary>
    /// The ID of the load test.
    /// </summary>
    [Option(Description = LoadTestingOptionDescriptions.TestId)]
    public required string TestId { get; set; }

    /// <summary>
    /// The display name of the load test.
    /// </summary>
    [Option(Description = LoadTestingOptionDescriptions.DisplayName)]
    public string? DisplayName { get; set; }

    /// <summary>
    /// The description of the load test.
    /// </summary>
    [Option(Description = LoadTestingOptionDescriptions.Description)]
    public string? Description { get; set; }

    /// <summary>
    /// The endpoint of the load test.
    /// </summary>
    [Option(Description = "The endpoint URL to be tested. This is the URL of the HTTP endpoint that will be subjected to load testing.")]
    public string? Endpoint { get; set; }

    /// <summary>
    /// The number of virtual users to simulate.
    /// </summary>
    [Option(Description = "Virtual users is a measure of load that is simulated to test the HTTP endpoint. Default is 50.")]
    public int? VirtualUsers { get; set; }

    /// <summary>
    /// The duration of the load test.
    /// </summary>
    [Option(Description = "Number of minutes for which the load is simulated against the endpoint. Default is 20 minutes.")]
    public int? Duration { get; set; }

    /// <summary>
    /// The ramp-up time for the load test.
    /// </summary>
    [Option(Description = "Number of minutes it takes for the system to ramp-up to the total load specified. Default is 1 minute.")]
    public int? RampUpTime { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public string? ResourceGroup { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
