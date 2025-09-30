// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Foundry.Options;

public class AgentsQueryAndEvaluateOptions : GlobalOptions
{
    [JsonPropertyName(FoundryOptionDefinitions.AgentId)]
    public string? AgentId { get; set; }

    [JsonPropertyName(FoundryOptionDefinitions.Query)]
    public string? Query { get; set; }

    [JsonPropertyName(FoundryOptionDefinitions.Endpoint)]
    public string? Endpoint { get; set; }

    [JsonPropertyName(FoundryOptionDefinitions.Evaluators)]
    public string? Evaluators { get; set; }

    [JsonPropertyName(FoundryOptionDefinitions.AzureOpenAIEndpoint)]
    public string? AzureOpenAIEndpoint { get; set; }

    [JsonPropertyName(FoundryOptionDefinitions.AzureOpenAIDeployment)]
    public string? AzureOpenAIDeployment { get; set; }
}
