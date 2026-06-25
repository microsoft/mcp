// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Search.Options.Index;

public class IndexQueryOptions
{
    [Option(Description = "The search query to execute against the Azure AI Search index.")]
    public required string Query { get; set; }

    [Option(Description = SearchOptionDescriptions.Service)]
    public required string Service { get; set; }

    [Option(Description = SearchOptionDescriptions.Index)]
    public required string Index { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
