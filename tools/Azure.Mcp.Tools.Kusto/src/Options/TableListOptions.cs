// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Models;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Kusto.Options;

public class TableListOptions : ISubscriptionOption, IDatabaseOption
{
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
