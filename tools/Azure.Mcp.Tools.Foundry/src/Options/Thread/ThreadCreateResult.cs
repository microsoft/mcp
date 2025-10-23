// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Foundry.Options.Thread;

public class ThreadCreateResult
{
    [JsonPropertyName("threadId")]
    public string? ThreadId;

    [JsonPropertyName("projectEndpoint")]
    public string? ProjectEndpoint;
}
