// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.SreAgent.Options.ScheduledTasks;

public sealed class ScheduledTasksPauseOptions : BaseSreAgentOptions
{
    [Option(SreAgentOptionDefinitions.TaskIdDescription)]
    public required string TaskId { get; set; }
}
