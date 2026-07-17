// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.LoadTesting.Options.LoadTestRun;

public sealed class TestRunGetOptions : ISubscriptionOption
{
    /// <summary>
    /// The name of the test resource.
    /// </summary>
    [Option(Description = LoadTestingOptionDescriptions.TestResourceName)]
    public required string TestResourceName { get; set; }

    /// <summary>
    /// The ID of the load test run resource. If provided, returns a single test run.
    /// </summary>
    [Option(Description = LoadTestingOptionDescriptions.TestrunId)]
    public string? TestrunId { get; set; }

    /// <summary>
    /// The ID of the load test resource. If provided (and TestRunId is not), returns all test runs for this test.
    /// </summary>
    [Option(Description = LoadTestingOptionDescriptions.TestId)]
    public string? TestId { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public string? ResourceGroup { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
