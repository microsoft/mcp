// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Foundry.Models;

public class ModelInformation
{
    [JsonPropertyName("id")] public string? Id { get; set; }

    [JsonPropertyName("name")] public string? Name { get; set; }

    [JsonPropertyName("publisher")] public string? Publisher { get; set; }

    [JsonPropertyName("description")] public string? Description { get; set; }

    [JsonPropertyName("azureOffers")] public List<string>? AzureOffers { get; set; } = [];

    [JsonPropertyName("playgroundLimits")] public object? PlaygroundLimits { get; set; }

    [JsonPropertyName("deployment_options")]
    public ModelDeploymentInformation DeploymentInformation { get; set; } = new();
}
