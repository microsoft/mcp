// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.SreAgent.Options.Threads;

public sealed class ThreadsGetOptions : BaseSreAgentOptions
{
    [Option(Description = SreAgentOptionDefinitions.ThreadIdDescription)]
    public required string ThreadId { get; set; }
}
