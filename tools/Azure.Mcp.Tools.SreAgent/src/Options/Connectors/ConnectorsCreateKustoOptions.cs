// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.SreAgent.Options.Connectors;

public sealed class ConnectorsCreateKustoOptions : BaseSreAgentOptions
{
    [Option(SreAgentOptionDefinitions.NameDescription)]
    public required string Name { get; set; }

    [Option("The Azure Data Explorer cluster URL.")]
    public string? ClusterUrl { get; set; }

    [Option(SreAgentOptionDefinitions.DatabaseDescription)]
    public string? Database { get; set; }
}
