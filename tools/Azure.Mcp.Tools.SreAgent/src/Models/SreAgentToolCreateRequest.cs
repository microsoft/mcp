// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.SreAgent.Models;

public sealed class SreAgentToolCreateRequest
{
    public required string Name { get; set; }

    public string Type { get; set; } = "ExtendedAgentTool";

    public SreAgentToolProperties Properties { get; set; } = new();
}
