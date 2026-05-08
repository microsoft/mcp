// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.SreAgent.Models;

public sealed class HookSpec
{
    public string? EventType { get; set; }

    public string? ActivationMode { get; set; }

    public string? Description { get; set; }

    public HookDefinition? Hook { get; set; }
}
