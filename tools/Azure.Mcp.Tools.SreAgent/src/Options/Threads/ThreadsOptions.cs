// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.SreAgent.Options.Threads;

public class ThreadsListOptions : BaseSreAgentOptions;

public class ThreadsGetOptions : BaseSreAgentOptions
{
    [JsonPropertyName("thread-id")]
    public string? ThreadId { get; set; }
}

public class ThreadsCreateOptions : BaseSreAgentOptions
{
    [JsonPropertyName("message")]
    public string? Message { get; set; }
}

public class ThreadsSendMessageOptions : BaseSreAgentOptions
{
    [JsonPropertyName("thread-id")]
    public string? ThreadId { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}

public class ThreadsDeleteOptions : BaseSreAgentOptions
{
    [JsonPropertyName("thread-id")]
    public string? ThreadId { get; set; }

    [JsonPropertyName("confirm")]
    public bool Confirm { get; set; }
}

public class ThreadsInvestigateOptions : BaseSreAgentOptions
{
    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("max-iterations")]
    public int MaxIterations { get; set; } = 20;

    [JsonPropertyName("timeout-seconds")]
    public int TimeoutSeconds { get; set; } = 600;
}

public class ThreadsInvestigateYoloOptions : ThreadsInvestigateOptions;
