// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Models;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Kusto.Options;

public class SampleOptions : ISubscriptionOption, ITableOption
{
    [Option("The maximum number of results to return. Must be a positive integer between 1 and 10000. Default is 10.")]
    public int Limit { get; set; } = 10;

    [Option("Kusto Table name.")]
    public required string Table { get; set; }

    [Option("Kusto Database name.")]
    public required string Database { get; set; }

    [Option("Kusto Cluster URI.", Name = "cluster-uri")]
    public string? ClusterUri { get; set; }

    [Option("Kusto Cluster name.", Name = "cluster")]
    public string? ClusterName { get; set; }

    [Option(OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [Option(OptionDescriptions.AuthMethod, Name = "auth-method")]
    public AuthMethod? AuthMethod { get; set; }

    [Option(Name = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
