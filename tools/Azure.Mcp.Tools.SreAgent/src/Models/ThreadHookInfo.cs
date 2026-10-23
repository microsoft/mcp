// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.SreAgent.Models;

public sealed class ThreadHookInfo
{
    public string? Name { get; set; }

    public string? EventType { get; set; }

    public string? ActivationMode { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public bool IsSystemHook { get; set; }

    public bool CanModify { get; set; }
}
