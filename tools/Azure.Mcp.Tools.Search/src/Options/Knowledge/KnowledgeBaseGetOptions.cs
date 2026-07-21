// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Search.Options.Knowledge;

public sealed class KnowledgeBaseGetOptions
{
    [Option(Description = SearchOptionDescriptions.KnowledgeBase)]
    public string? KnowledgeBase { get; set; }

    [Option(Description = SearchOptionDescriptions.Service)]
    public required string Service { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
