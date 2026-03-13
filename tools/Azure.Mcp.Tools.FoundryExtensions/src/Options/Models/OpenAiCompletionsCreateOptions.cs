// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.FoundryExtensions.Options.Models;

public class OpenAiCompletionsCreateOptions : SubscriptionOptions
{
    [JsonPropertyName(FoundryExtensionsOptionDefinitions.DeploymentName)]
    public string? DeploymentName { get; set; }

    [JsonPropertyName(FoundryExtensionsOptionDefinitions.PromptText)]
    public string? PromptText { get; set; }

    [JsonPropertyName(FoundryExtensionsOptionDefinitions.MaxTokens)]
    public int? MaxTokens { get; set; }

    [JsonPropertyName(FoundryExtensionsOptionDefinitions.Temperature)]
    public double? Temperature { get; set; }

    [JsonPropertyName(FoundryExtensionsOptionDefinitions.ResourceName)]
    public string? ResourceName { get; set; }
}
