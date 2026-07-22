// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.SreAgent.Options.Threads;

public sealed class ThreadsCreateOptions : BaseSreAgentOptions
{
    [Option(Description = SreAgentOptionDefinitions.MessageDescription)]
    public required string Message { get; set; }
}
