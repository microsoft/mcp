// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Foundry.Options;

public class AgentsEvaluateOptions : GlobalOptions
{

    [JsonPropertyName(FoundryOptionDefinitions.Query)]
    public string? Query { get; set; }

    [JsonPropertyName(FoundryOptionDefinitions.Endpoint)]
    public string? Endpoint { get; set; }

    [JsonPropertyName(FoundryOptionDefinitions.AzureOpenAIEndpoint)]
    public string? AzureOpenAIEndpoint { get; set; }

    [JsonPropertyName(FoundryOptionDefinitions.AzureOpenAIDeployment)]
    public string? AzureOpenAIDeployment { get; set; }

    [JsonPropertyName(FoundryOptionDefinitions.EvaluatorName)]
    public string? EvaluatorName { get; set; }

    [JsonPropertyName(FoundryOptionDefinitions.Response)]
    public string? Response { get; set; }

    [JsonPropertyName(FoundryOptionDefinitions.ToolDefinitions)]
    public string? ToolDefinitions { get; set; }
}
