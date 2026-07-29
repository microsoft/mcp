// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.SreAgent.Options.CommonPrompts;

public sealed class CommonPromptsGetOptions : BaseSreAgentOptions
{
    [Option(Description = SreAgentOptionDefinitions.NameDescription)]
    public required string Name { get; set; }
}
