// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.SreAgent.Options.ScheduledTasks;

public class ScheduledTasksListOptions : BaseSreAgentOptions;

public class ScheduledTasksGetOptions : BaseSreAgentOptions
{
    [JsonPropertyName("task-id")]
    public string? TaskId { get; set; }
}

public class ScheduledTasksCreateOptions : BaseSreAgentOptions
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("cron-expression")]
    public string? CronExpression { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }
}

public class ScheduledTasksDeleteOptions : BaseSreAgentOptions
{
    [JsonPropertyName("task-id")]
    public string? TaskId { get; set; }

    [JsonPropertyName("confirm")]
    public bool Confirm { get; set; }
}

public class ScheduledTasksPauseOptions : BaseSreAgentOptions
{
    [JsonPropertyName("task-id")]
    public string? TaskId { get; set; }
}

public class ScheduledTasksResumeOptions : BaseSreAgentOptions
{
    [JsonPropertyName("task-id")]
    public string? TaskId { get; set; }
}
