// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.SreAgent.Models;

public sealed class ThreadHooksResponse
{
    public List<ThreadHookInfo>? Hooks { get; set; }
}
