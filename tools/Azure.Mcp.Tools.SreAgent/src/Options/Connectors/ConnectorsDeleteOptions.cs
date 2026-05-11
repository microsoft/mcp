// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.SreAgent.Options.Connectors;

public class ConnectorsDeleteOptions : BaseSreAgentOptions
{
    public string? Name { get; set; }

    public bool Confirm { get; set; }
}
