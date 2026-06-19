// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.SreAgent.Options.Threads;

public sealed class ThreadsDeleteOptions : BaseSreAgentOptions
{
    [Option(SreAgentOptionDefinitions.ThreadIdDescription)]
    public required string ThreadId { get; set; }

    [Option(SreAgentOptionDefinitions.ConfirmDescription)]
    public bool Confirm { get; set; }
}
