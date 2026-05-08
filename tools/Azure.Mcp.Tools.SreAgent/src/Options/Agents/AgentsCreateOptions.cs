// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.SreAgent.Options.Agents;

public class AgentsCreateOptions : BaseSreAgentOptions
{
    public string? Name { get; set; }

    public string? Description { get; set; }

    public string? Instructions { get; set; }

    public string[]? Tools { get; set; }

    public string[]? Handoffs { get; set; }
}
