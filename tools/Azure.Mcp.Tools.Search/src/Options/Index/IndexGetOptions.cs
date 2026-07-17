// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Search.Options.Index;

public sealed class IndexGetOptions
{
    [Option(Description = SearchOptionDescriptions.Service)]
    public required string Service { get; set; }

    [Option(Description = SearchOptionDescriptions.Index)]
    public string? Index { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
