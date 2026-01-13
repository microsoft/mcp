// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Foundry.Models;

public class AgentsCreateResult
{
    [JsonPropertyName("agentId")]
    public string? AgentId { get; set; }

    [JsonPropertyName("agentName")]
    public string? AgentName { get; set; }

    [JsonPropertyName("projectEndpoint")]
    public string? ProjectEndpoint { get; set; }

    [JsonPropertyName("modelDeploymentName")]
    public string? ModelDeploymentName { get; set; }
}
