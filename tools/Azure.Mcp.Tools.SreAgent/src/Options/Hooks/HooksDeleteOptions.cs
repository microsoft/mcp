// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.SreAgent.Options.Hooks;

public sealed class HooksDeleteOptions : BaseSreAgentOptions
{
    [Option(Description = SreAgentOptionDefinitions.NameDescription)]
    public required string Name { get; set; }

    [Option(Description = SreAgentOptionDefinitions.ConfirmDescription)]
    public bool Confirm { get; set; }
}
