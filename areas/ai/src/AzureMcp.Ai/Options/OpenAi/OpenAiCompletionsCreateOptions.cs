// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace AzureMcp.Ai.Options.OpenAi;

public class OpenAiCompletionsCreateOptions : BaseAiOptions
{
    [JsonPropertyName(AiOptionDefinitions.DeploymentNameName)]
    public string? DeploymentName { get; set; }

    [JsonPropertyName(AiOptionDefinitions.PromptTextName)]
    public string? PromptText { get; set; }

    [JsonPropertyName(AiOptionDefinitions.MaxTokensName)]
    public int? MaxTokens { get; set; }

    [JsonPropertyName(AiOptionDefinitions.TemperatureName)]
    public double? Temperature { get; set; }
}
