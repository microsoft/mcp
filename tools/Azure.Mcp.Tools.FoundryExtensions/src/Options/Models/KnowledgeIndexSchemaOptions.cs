// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.FoundryExtensions.Options.Models;

public sealed class KnowledgeIndexSchemaOptions
{
    [Option(Description = "The name of the knowledge index.")]
    public required string Index { get; set; }

    [Option(Description = FoundryExtensionsOptionDescriptions.Endpoint)]
    public required string Endpoint { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
