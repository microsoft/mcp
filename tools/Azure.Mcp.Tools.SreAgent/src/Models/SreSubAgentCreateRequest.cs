// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.SreAgent.Models;

public sealed class SreSubAgentCreateRequest
{
    public required string Name { get; set; }

    public string Type { get; set; } = "ExtendedAgent";

    public SreSubAgentProperties Properties { get; set; } = new();
}
