// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.SreAgent.Options.Threads;

public class ThreadsDeleteOptions : BaseSreAgentOptions
{
    [JsonPropertyName("thread-id")]
    public string? ThreadId { get; set; }

    [JsonPropertyName("confirm")]
    public bool Confirm { get; set; }
}
