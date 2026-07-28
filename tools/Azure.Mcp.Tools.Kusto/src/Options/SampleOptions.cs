// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Kusto.Options;

public sealed class SampleOptions : ISubscriptionOption, ITableOption
{
    [Option(Description = "The maximum number of results to return. Must be a positive integer between 1 and 10000. Default is 10.")]
    public int? Limit { get; set; }

    [Option(Description = KustOptionDescriptions.Table)]
    public required string Table { get; set; }

    [Option(Description = KustOptionDescriptions.Database)]
    public required string Database { get; set; }

    [Option(Description = KustOptionDescriptions.ClusterUri)]
    public string? ClusterUri { get; set; }

    [Option(Description = KustOptionDescriptions.Cluster)]
    public string? Cluster { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
