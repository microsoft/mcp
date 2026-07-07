// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Models;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.FoundryExtensions.Options.Models;

public sealed class OpenAiEmbeddingsCreateOptions : ISubscriptionOption
{
    [Option(Description = FoundryExtensionsOptionDescriptions.ResourceName)]
    public required string ResourceName { get; set; }

    [Option(Description = FoundryExtensionsOptionDescriptions.Deployment)]
    public required string Deployment { get; set; }

    [Option(Description = "The input text to generate embeddings for.")]
    public required string InputText { get; set; }

    [Option(Description = FoundryExtensionsOptionDescriptions.User)]
    public string? User { get; set; }

    [Option(Description = "The format to return embeddings in (float or base64).", DefaultValue = "float")]
    public string? EncodingFormat { get; set; }

    [Option(Description = "The number of dimensions for the embedding output. Only supported in some models.")]
    public int? Dimensions { get; set; }

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
