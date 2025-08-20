// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Foundry.Models;

public class ModelDeploymentInformation
{
    [JsonPropertyName("openai")] public bool IsOpenAI { get; set; }

    [JsonPropertyName("serverless_endpoint")]
    public bool IsServerlessEndpoint { get; set; }

    [JsonPropertyName("managed_compute")] public bool IsManagedCompute { get; set; }

    [JsonPropertyName("free_playground")] public bool IsFreePlayground { get; set; }
}
