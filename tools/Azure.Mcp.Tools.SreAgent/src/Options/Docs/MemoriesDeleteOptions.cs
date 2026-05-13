// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.SreAgent.Options.Docs;

public class MemoriesDeleteOptions : BaseSreAgentOptions
{
    public string? Name { get; set; }
    public bool Confirm { get; set; }
}
