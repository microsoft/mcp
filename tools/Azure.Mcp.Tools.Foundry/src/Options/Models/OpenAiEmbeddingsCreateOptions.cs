// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Foundry.Options.Models;

public class OpenAiEmbeddingsCreateOptions : SubscriptionOptions
{
    [JsonPropertyName(FoundryOptionDefinitions.ResourceName)]
    public string? ResourceName { get; set; }

    [JsonPropertyName(FoundryOptionDefinitions.DeploymentName)]
    public string? DeploymentName { get; set; }

    [JsonPropertyName(FoundryOptionDefinitions.InputText)]
    public string? InputText { get; set; }

    [JsonPropertyName(FoundryOptionDefinitions.User)]
    public string? User { get; set; }

    [JsonPropertyName(FoundryOptionDefinitions.EncodingFormat)]
    public string? EncodingFormat { get; set; } = "float";

    [JsonPropertyName(FoundryOptionDefinitions.Dimensions)]
    public int? Dimensions { get; set; }
}
