// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.SreAgent.Models;

public sealed class AgentConnectorEnvelope
{
    public string? Name { get; set; }

    public AgentConnector? Properties { get; set; }
}
