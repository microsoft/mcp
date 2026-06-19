// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.SreAgent.Options.Agents;

public sealed class AgentsDeleteOptions : BaseSreAgentOptions
{
    [Option(SreAgentOptionDefinitions.NameDescription)]
    public required string Name { get; set; }

    [Option(SreAgentOptionDefinitions.ConfirmDescription)]
    public bool Confirm { get; set; }
}
