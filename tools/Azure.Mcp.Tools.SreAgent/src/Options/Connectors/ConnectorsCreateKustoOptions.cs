// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.SreAgent.Options.Connectors;

public sealed class ConnectorsCreateKustoOptions : BaseSreAgentOptions
{
    [Option(Description = SreAgentOptionDefinitions.NameDescription)]
    public required string Name { get; set; }

    [Option(Description = "The Azure Data Explorer cluster URL.")]
    public required string ClusterUrl { get; set; }

    [Option(Description = SreAgentOptionDefinitions.DatabaseDescription)]
    public string? Database { get; set; }
}
