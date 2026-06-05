// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.SreAgent.Options.Threads;

public class ThreadsInvestigateOptions : BaseSreAgentOptions
{
    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("max-iterations")]
    public int MaxIterations { get; set; } = 20;

    [JsonPropertyName("timeout-seconds")]
    public int TimeoutSeconds { get; set; } = 600;
}
