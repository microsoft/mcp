// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.SreAgent.Options.Connectors;

public class ConnectorsCreateKustoOptions : ISreAgentOption
{
    [Option("The name of the Azure SRE Agent resource to target.")]
    public required string? Agent { get; set; }

    [Option(OptionDescriptions.ResourceGroup)]
    public string? ResourceGroup { get; set; }

    [Option(OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [Option(Name = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }

    [Option("The name of the SRE Agent item.")]
    public required string Name { get; set; }

    [Option("The Azure Data Explorer cluster URL.")]
    public required string ClusterUrl { get; set; }

    [Option("The Kusto database for Kusto tools.")]
    public string? Database { get; set; }
}
