// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.SreAgent.Options.CommonPrompts;

public class CommonPromptsDeleteOptions : BaseSreAgentOptions
{
    public string? Name { get; set; }
    public bool Confirm { get; set; }
}
