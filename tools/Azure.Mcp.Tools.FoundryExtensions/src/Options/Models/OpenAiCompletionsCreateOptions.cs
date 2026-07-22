// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Models;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.FoundryExtensions.Options.Models;

public sealed class OpenAiCompletionsCreateOptions : ISubscriptionOption
{
    [Option(Description = FoundryExtensionsOptionDescriptions.Deployment)]
    public required string Deployment { get; set; }

    [Option(Description = "The prompt text to send to the completion model.")]
    public required string PromptText { get; set; }

    [Option(Description = FoundryExtensionsOptionDescriptions.MaxTokens)]
    public int? MaxTokens { get; set; }

    [Option(Description = FoundryExtensionsOptionDescriptions.Temperature)]
    public double? Temperature { get; set; }

    [Option(Description = FoundryExtensionsOptionDescriptions.ResourceName)]
    public required string ResourceName { get; set; }

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
