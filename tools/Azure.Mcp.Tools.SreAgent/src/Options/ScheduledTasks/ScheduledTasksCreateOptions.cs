// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.SreAgent.Options.ScheduledTasks;

public sealed class ScheduledTasksCreateOptions : BaseSreAgentOptions
{
    [Option(SreAgentOptionDefinitions.NameDescription)]
    public required string Name { get; set; }

    [Option("The cron expression for the schedule.")]
    public required string CronExpression { get; set; }

    [Option(SreAgentOptionDefinitions.MessageDescription)]
    public required string Message { get; set; }

    [Option(SreAgentOptionDefinitions.DescriptionDescription)]
    public string? Description { get; set; }
}
