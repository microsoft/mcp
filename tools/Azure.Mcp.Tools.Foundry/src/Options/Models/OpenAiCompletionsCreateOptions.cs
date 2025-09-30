// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Foundry.Options.Models;

public class OpenAiCompletionsCreateOptions : SubscriptionOptions
{
    [JsonPropertyName(FoundryOptionDefinitions.DeploymentName)]
    public string? DeploymentName { get; set; }

    [JsonPropertyName(FoundryOptionDefinitions.PromptText)]
    public string? PromptText { get; set; }

    [JsonPropertyName(FoundryOptionDefinitions.MaxTokens)]
    public int? MaxTokens { get; set; }

    [JsonPropertyName(FoundryOptionDefinitions.Temperature)]
    public double? Temperature { get; set; }

    [JsonPropertyName(FoundryOptionDefinitions.ResourceName)]
    public string? ResourceName { get; set; }
}
