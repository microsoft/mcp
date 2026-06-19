// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.SreAgent.Options.Hooks;

public sealed class HooksThreadListOptions : BaseSreAgentOptions
{
    [Option(SreAgentOptionDefinitions.ThreadIdDescription)]
    public required string ThreadId { get; set; }
}
