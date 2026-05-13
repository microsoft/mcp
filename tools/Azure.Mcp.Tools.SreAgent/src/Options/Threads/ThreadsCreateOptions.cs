// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.SreAgent.Options.Threads;

public class ThreadsCreateOptions : BaseSreAgentOptions
{
    [JsonPropertyName("message")]
    public string? Message { get; set; }
}
