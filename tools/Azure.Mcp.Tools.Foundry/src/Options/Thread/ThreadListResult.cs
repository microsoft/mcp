// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Foundry.Options.Thread;

public class ThreadListResult
{
    [JsonPropertyName("threads")]
    public ThreadItem[] Threads { get; set; } = [];
}

public class ThreadItem
{
    [JsonPropertyName("threadId")]
    public string? ThreadId;
}
