// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.WellArchitected.Models;

public sealed class PropertySignals
{
    public List<string> Encryption { get; set; } = [];
    public List<string> Identity { get; set; } = [];
    public List<string> Networking { get; set; } = [];
    public List<string> Redundancy { get; set; } = [];
    public List<string> Monitoring { get; set; } = [];
}
