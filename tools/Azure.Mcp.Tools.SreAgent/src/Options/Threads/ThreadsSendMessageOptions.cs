// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.SreAgent.Options.Threads;

public sealed class ThreadsSendMessageOptions : BaseSreAgentOptions
{
    [Option(Description = SreAgentOptionDefinitions.ThreadIdDescription)]
    public required string ThreadId { get; set; }

    [Option(Description = SreAgentOptionDefinitions.MessageDescription)]
    public required string Message { get; set; }
}
