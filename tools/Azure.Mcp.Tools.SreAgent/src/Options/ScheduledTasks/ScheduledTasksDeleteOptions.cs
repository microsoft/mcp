// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.SreAgent.Options.ScheduledTasks;

public sealed class ScheduledTasksDeleteOptions : BaseSreAgentOptions
{
    [Option(Description = SreAgentOptionDefinitions.TaskIdDescription)]
    public required string TaskId { get; set; }

    [Option(Description = SreAgentOptionDefinitions.ConfirmDescription)]
    public bool Confirm { get; set; }
}
