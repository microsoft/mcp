// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.SreAgent.Options.Hooks;

public sealed class HooksThreadActivateOptions : BaseSreAgentOptions
{
    [Option(Description = SreAgentOptionDefinitions.ThreadIdDescription)]
    public required string ThreadId { get; set; }

    [Option(Description = SreAgentOptionDefinitions.HookNameDescription)]
    public required string HookName { get; set; }

}
