// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.SreAgent.Options.Connectors;

public class ConnectorsCreateKustoOptions : BaseSreAgentOptions
{
    public string Name { get; set; } = string.Empty;

    public string? ClusterUrl { get; set; }

    public string? Database { get; set; }
}
