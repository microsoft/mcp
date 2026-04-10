// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.FoundryExtensions.Options.Models;

public class OpenAiEmbeddingsCreateOptions : SubscriptionOptions
{
    [JsonPropertyName(FoundryExtensionsOptionDefinitions.ResourceName)]
    public string? ResourceName { get; set; }

    [JsonPropertyName(FoundryExtensionsOptionDefinitions.DeploymentName)]
    public string? DeploymentName { get; set; }

    [JsonPropertyName(FoundryExtensionsOptionDefinitions.InputText)]
    public string? InputText { get; set; }

    [JsonPropertyName(FoundryExtensionsOptionDefinitions.User)]
    public string? User { get; set; }

    [JsonPropertyName(FoundryExtensionsOptionDefinitions.EncodingFormat)]
    public string? EncodingFormat { get; set; } = "float";

    [JsonPropertyName(FoundryExtensionsOptionDefinitions.Dimensions)]
    public int? Dimensions { get; set; }
}
