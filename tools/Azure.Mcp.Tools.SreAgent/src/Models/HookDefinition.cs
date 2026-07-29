// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.SreAgent.Models;

public sealed class HookDefinition
{
    public string? Type { get; set; }

    public string? Prompt { get; set; }

    public string? Command { get; set; }

    public string? Script { get; set; }

    public string? Matcher { get; set; }

    public int? Timeout { get; set; }

    public string? Model { get; set; }

    public string? FailMode { get; set; }

    public int? MaxRejections { get; set; }
}
