// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.SreAgent.Options.Docs;

public sealed class DocsGetOptions
{
    [Option("Documentation topic.")]
    public required string Topic { get; set; }
}
