// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.SreAgent.Options.ScheduledTasks;

public class ScheduledTasksPauseOptions : BaseSreAgentOptions
{
    [JsonPropertyName("task-id")]
    public string? TaskId { get; set; }
}
