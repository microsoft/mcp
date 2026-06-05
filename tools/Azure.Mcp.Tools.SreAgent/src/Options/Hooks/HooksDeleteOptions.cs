// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.SreAgent.Options.Hooks;

public class HooksDeleteOptions : BaseSreAgentOptions
{
    public string Name { get; set; } = string.Empty;

    public bool Confirm { get; set; }
}
