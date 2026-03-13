// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Deploy.Options.Pipeline;

public class GuidanceGetOptions : SubscriptionOptions
{
    [JsonPropertyName("isAZDProject")]
    public bool IsAZDProject { get; set; }

    [JsonPropertyName("pipelinePlatform")]
    public string? PipelinePlatform { get; set; }

    [JsonPropertyName("deployOption")]
    public string? DeployOption { get; set; }
}
