// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Monitor.Options;

public sealed class TableListOptions : ISubscriptionOption
{
    [Option(Description = MonitorOptionDescriptions.Workspace)]
    public required string Workspace { get; set; }

    [Option(Description = "The type of table to query. Options: 'CustomLog', 'AzureMetrics', etc. Defaults to 'CustomLog'.", DefaultValue = "CustomLog")]
    public required string TableType { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public required string ResourceGroup { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
