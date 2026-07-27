// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.SreAgent.Options.ScheduledTasks;

public sealed class ScheduledTasksCreateOptions : BaseSreAgentOptions
{
    [Option(Description = SreAgentOptionDefinitions.NameDescription)]
    public required string Name { get; set; }

    [Option(Description = "The cron expression for the schedule.")]
    public required string CronExpression { get; set; }

    [Option(Description = SreAgentOptionDefinitions.MessageDescription)]
    public required string Message { get; set; }

    [Option(Description = SreAgentOptionDefinitions.DescriptionDescription)]
    public string? Description { get; set; }
}
