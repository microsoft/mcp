// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.SreAgent.Options.Docs;

public class MemoriesAddOptions : BaseSreAgentOptions
{
    public string Name { get; set; } = string.Empty;
    public string? Content { get; set; }
}
