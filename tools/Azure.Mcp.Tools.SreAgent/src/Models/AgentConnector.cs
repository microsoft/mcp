// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.SreAgent.Models;

public sealed class AgentConnector
{
    public string? Name { get; set; }

    public string? DataConnectorType { get; set; }

    public string? ConnectorType { get; set; }

    public string? DataSource { get; set; }

    public string? Identity { get; set; }

    public Dictionary<string, object>? ExtendedProperties { get; set; }
}
