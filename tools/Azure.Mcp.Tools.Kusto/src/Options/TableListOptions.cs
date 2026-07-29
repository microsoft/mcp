// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Kusto.Options;

public sealed class TableListOptions : ISubscriptionOption, IDatabaseOption
{
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
