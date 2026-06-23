// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.SreAgent.Options.Agents;

public sealed class AgentsToolsGetOptions : BaseSreAgentOptions
{
    [Option(Description = SreAgentOptionDefinitions.NameDescription)]
    public required string Name { get; set; }
}
