// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.SreAgent.Options.Hooks;

public class HooksThreadDeactivateOptions : BaseSreAgentOptions
{
    public string? ThreadId { get; set; }

    public string? HookName { get; set; }
}
