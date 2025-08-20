// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Foundry.Options.Models;

public class ModelDeploymentOptions : SubscriptionOptions
{
    [JsonPropertyName(FoundryOptionDefinitions.DeploymentName)]
    public string? DeploymentName { get; set; }
    [JsonPropertyName(FoundryOptionDefinitions.ModelName)]
    public string? ModelName { get; set; }
    [JsonPropertyName(FoundryOptionDefinitions.ModelFormat)]
    public string? ModelFormat { get; set; }
    [JsonPropertyName(FoundryOptionDefinitions.AzureAiServicesName)]
    public string? AzureAiServicesName { get; set; }
    [JsonPropertyName(FoundryOptionDefinitions.ModelVersion)]
    public string? ModelVersion { get; set; }
    [JsonPropertyName(FoundryOptionDefinitions.ModelSource)]
    public string? ModelSource { get; set; }
    [JsonPropertyName(FoundryOptionDefinitions.SkuName)]
    public string? SkuName { get; set; }
    [JsonPropertyName(FoundryOptionDefinitions.SkuCapacity)]
    public int? SkuCapacity { get; set; }
    [JsonPropertyName(FoundryOptionDefinitions.ScaleType)]
    public string? ScaleType { get; set; }
    [JsonPropertyName(FoundryOptionDefinitions.ScaleCapacity)]
    public int? ScaleCapacity { get; set; }
}
