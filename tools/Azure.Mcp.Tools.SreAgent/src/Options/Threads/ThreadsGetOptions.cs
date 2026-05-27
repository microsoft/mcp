// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.SreAgent.Options.Threads;

public class ThreadsGetOptions : BaseSreAgentOptions
{
    [JsonPropertyName("thread-id")]
    public string? ThreadId { get; set; }
}
