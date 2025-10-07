// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Foundry.Models;

public sealed record OpenAiModelsListResult(
    [property: JsonPropertyName("models")] List<OpenAiModelDeployment> Models,
    [property: JsonPropertyName("resourceName")] string ResourceName);

public sealed record OpenAiModelDeployment(
    [property: JsonPropertyName("deploymentName")] string DeploymentName,
    [property: JsonPropertyName("modelName")] string ModelName,
    [property: JsonPropertyName("modelVersion")] string? ModelVersion,
    [property: JsonPropertyName("scaleType")] string? ScaleType,
    [property: JsonPropertyName("capacity")] int? Capacity,
    [property: JsonPropertyName("provisioningState")] string? ProvisioningState,
    [property: JsonPropertyName("createdAt")] DateTime? CreatedAt,
    [property: JsonPropertyName("updatedAt")] DateTime? UpdatedAt,
    [property: JsonPropertyName("capabilities")] OpenAiModelCapabilities? Capabilities);

public sealed record OpenAiModelCapabilities(
    [property: JsonPropertyName("completions")] bool Completions,
    [property: JsonPropertyName("embeddings")] bool Embeddings,
    [property: JsonPropertyName("chatCompletions")] bool ChatCompletions,
    [property: JsonPropertyName("fineTuning")] bool FineTuning);
