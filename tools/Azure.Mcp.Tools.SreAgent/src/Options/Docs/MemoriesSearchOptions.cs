// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.SreAgent.Options.Docs;

public sealed class MemoriesSearchOptions : BaseSreAgentOptions
{
    [Option(Description = SreAgentOptionDefinitions.QueryDescription)]
    public required string Query { get; set; }
}
