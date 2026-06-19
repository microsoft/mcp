// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.SreAgent.Options.Docs;

public sealed class MemoryRemoteOptions : BaseSreAgentOptions
{
    [Option(SreAgentOptionDefinitions.ContentDescription)]
    public required string Content { get; set; }
}
