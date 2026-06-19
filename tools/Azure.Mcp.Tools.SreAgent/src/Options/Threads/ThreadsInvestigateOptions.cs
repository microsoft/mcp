// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.SreAgent.Options.Threads;

public sealed class ThreadsInvestigateOptions : BaseSreAgentOptions
{
    [Option(SreAgentOptionDefinitions.MessageDescription)]
    public required string Message { get; set; }

    [Option(SreAgentOptionDefinitions.MaxIterationsDescription)]
    public int? MaxIterations { get; set; }

    [Option(SreAgentOptionDefinitions.TimeoutSecondsDescription)]
    public int? TimeoutSeconds { get; set; }
}
