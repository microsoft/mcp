// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.ComponentModel;
using System.Text.Json.Serialization;
using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Foundry.Options.Models;

public class OpenAiChatCompletionsCreateOptions : SubscriptionOptions
{
    [JsonPropertyName("resource-name")]
    [Description("The name of the Azure OpenAI resource")]
    public string? ResourceName { get; set; }

    [JsonPropertyName("deployment-name")]
    [Description("The name of the deployment for the chat model")]
    public string? DeploymentName { get; set; }

    [JsonPropertyName("message-array")]
    [Description("Array of messages in the conversation. Each message should have 'role' (system/user/assistant) and 'content' properties")]
    public string? MessageArray { get; set; }

    [JsonPropertyName("max-tokens")]
    [Description("Maximum number of tokens to generate in the completion")]
    public int? MaxTokens { get; set; }

    [JsonPropertyName("temperature")]
    [Description("Controls randomness in the model's output (0.0 to 2.0)")]
    public double? Temperature { get; set; }

    [JsonPropertyName("top-p")]
    [Description("Controls diversity of the model's output using nucleus sampling (0.0 to 1.0)")]
    public double? TopP { get; set; }

    [JsonPropertyName("frequency-penalty")]
    [Description("Penalizes new tokens based on their frequency in the text so far (-2.0 to 2.0)")]
    public double? FrequencyPenalty { get; set; }

    [JsonPropertyName("presence-penalty")]
    [Description("Penalizes new tokens based on whether they appear in the text so far (-2.0 to 2.0)")]
    public double? PresencePenalty { get; set; }

    [JsonPropertyName("stop")]
    [Description("Up to 4 sequences where the API will stop generating further tokens")]
    public string? Stop { get; set; }

    [JsonPropertyName("stream")]
    [Description("Whether to stream back partial progress")]
    public bool? Stream { get; set; }

    [JsonPropertyName("seed")]
    [Description("If specified, the system will make a best effort to sample deterministically")]
    public int? Seed { get; set; }

    [JsonPropertyName("user")]
    [Description("A unique identifier representing your end-user")]
    public string? User { get; set; }
}
