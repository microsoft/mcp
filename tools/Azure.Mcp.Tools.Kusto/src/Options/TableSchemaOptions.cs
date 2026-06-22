// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Models;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Kusto.Options;

public class TableSchemaOptions : ISubscriptionOption, ITableOption
{
    [Option(Description = "Kusto Table name.")]
    public required string Table { get; set; }

    [Option(Description = "Kusto Database name.")]
    public required string Database { get; set; }

    [Option(Description = "Kusto Cluster URI.")]
    public string? ClusterUri { get; set; }

    [Option(Description = "Kusto Cluster name.")]
    public string? Cluster { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [Option(Description = OptionDescriptions.AuthMethod)]
    public AuthMethod? AuthMethod { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
