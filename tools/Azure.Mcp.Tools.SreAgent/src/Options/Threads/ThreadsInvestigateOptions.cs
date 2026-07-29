// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.SreAgent.Options.Threads;

public sealed class ThreadsInvestigateOptions : BaseSreAgentOptions
{
    [Option(Description = SreAgentOptionDefinitions.MessageDescription)]
    public required string Message { get; set; }

    [Option(Description = SreAgentOptionDefinitions.MaxIterationsDescription)]
    public int? MaxIterations { get; set; }

    [Option(Description = SreAgentOptionDefinitions.TimeoutSecondsDescription)]
    public int? TimeoutSeconds { get; set; }
}
