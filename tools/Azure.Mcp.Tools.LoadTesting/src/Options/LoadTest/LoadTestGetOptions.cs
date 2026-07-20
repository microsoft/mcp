// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.LoadTesting.Options.LoadTest;

public sealed class TestGetOptions : ISubscriptionOption
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

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public string? ResourceGroup { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
