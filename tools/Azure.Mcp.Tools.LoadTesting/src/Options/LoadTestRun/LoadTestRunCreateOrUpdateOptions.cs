// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.LoadTesting.Options.LoadTestRun;

public sealed class TestRunCreateOrUpdateOptions : ISubscriptionOption
{
    /// <summary>
    /// The name of the test resource.
    /// </summary>
    [Option(Description = LoadTestingOptionDescriptions.TestResourceName)]
    public required string TestResourceName { get; set; }

    /// <summary>
    /// The ID of the load test run resource.
    /// </summary>
    [Option(Description = LoadTestingOptionDescriptions.TestrunId)]
    public required string TestrunId { get; set; }

    /// <summary>
    /// The ID of the load test.
    /// </summary>
    [Option(Description = LoadTestingOptionDescriptions.TestId)]
    public required string TestId { get; set; }

    /// <summary>
    /// The display name for the load test run.
    /// </summary>
    [Option(Description = LoadTestingOptionDescriptions.DisplayName)]
    public string? DisplayName { get; set; }

    /// <summary>
    /// The description for the load test run.
    /// </summary>
    [Option(Description = LoadTestingOptionDescriptions.Description)]
    public string? Description { get; set; }

    /// <summary>
    /// The ID of an existing test run to update. If provided, the command will trigger a rerun of the given test run id.
    /// </summary>
    [Option(Description = "The ID of an existing test run to update. If provided, the command will trigger a rerun of the given test run id.")]
    public string? OldTestrunId { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public string? ResourceGroup { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
