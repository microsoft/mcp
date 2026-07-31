// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.SreAgent.Options.CommonPrompts;

public sealed class CommonPromptsListOptions : BaseSreAgentOptions
{
    [Option(Description = "Optional search filter.")]
    public string? Search { get; set; }
}
