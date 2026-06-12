// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.SreAgent.Options.ScheduledTasks;

public class ScheduledTasksDeleteOptions : BaseSreAgentOptions
{
    [JsonPropertyName("task-id")]
    public string? TaskId { get; set; }

    [JsonPropertyName("confirm")]
    public bool Confirm { get; set; }
}
