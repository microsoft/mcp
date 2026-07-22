// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Models;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.FoundryExtensions.Options.Models;

public sealed class OpenAiChatCompletionsCreateOptions : ISubscriptionOption
{
    [Option(Description = FoundryExtensionsOptionDescriptions.ResourceName)]
    public required string ResourceName { get; set; }

    [Option(Description = FoundryExtensionsOptionDescriptions.Deployment)]
    public required string Deployment { get; set; }

    [Option(Description = "JSON array of messages in the conversation. Each message should have 'role' and 'content' properties.")]
    public required string MessageArray { get; set; }

    [Option(Description = FoundryExtensionsOptionDescriptions.MaxTokens)]
    public int? MaxTokens { get; set; }

    [Option(Description = FoundryExtensionsOptionDescriptions.Temperature)]
    public double? Temperature { get; set; }

    [Option(Description = "Controls diversity via nucleus sampling (0.0 to 1.0). Default is 1.0.")]
    public double? TopP { get; set; }

    [Option(Description = "Penalizes new tokens based on their frequency (-2.0 to 2.0). Default is 0.")]
    public double? FrequencyPenalty { get; set; }

    [Option(Description = "Penalizes new tokens based on presence (-2.0 to 2.0). Default is 0.")]
    public double? PresencePenalty { get; set; }

    [Option(Description = "Up to 4 sequences where the API will stop generating further tokens.")]
    public string? Stop { get; set; }

    [Option(Description = "Whether to stream back partial progress. Default is false.")]
    public bool? Stream { get; set; }

    [Option(Description = "If specified, the system will make a best effort to sample deterministically.")]
    public int? Seed { get; set; }

    [Option(Description = FoundryExtensionsOptionDescriptions.User)]
    public string? User { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public required string ResourceGroup { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }

    [Option(Description = OptionDescriptions.AuthMethod)]
    public AuthMethod? AuthMethod { get; set; }
}
