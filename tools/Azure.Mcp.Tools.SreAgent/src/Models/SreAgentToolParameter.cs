// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.SreAgent.Models;

public sealed class SreAgentToolParameter
{
    public string? Name { get; set; }

    public string? Type { get; set; }

    public string? Description { get; set; }

    public bool? Required { get; set; }
}
