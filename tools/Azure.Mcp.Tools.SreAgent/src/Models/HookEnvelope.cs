// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.SreAgent.Models;

public sealed class HookEnvelope
{
    public string? Name { get; set; }

    public string? Owner { get; set; }

    public List<string>? Tags { get; set; }

    public HookSpec? Properties { get; set; }
}
