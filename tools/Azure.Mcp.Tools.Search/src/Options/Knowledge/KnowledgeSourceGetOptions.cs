// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Search.Options.Knowledge;

public sealed class KnowledgeSourceGetOptions
{
    [Option(Description = "The name of the knowledge source within the Azure AI Search service.")]
    public string? KnowledgeSource { get; set; }

    [Option(Description = SearchOptionDescriptions.Service)]
    public required string Service { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
