// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.ManagedLustre.Models;

public sealed class ManagedLustreSkuInfo(
    string name,
    string location,
    bool supportsZones,
    List<ManagedLustreSkuCapability> capabilities)
{
    public string Name { get; } = name;
    public string Location { get; } = location;
    public bool SupportsZones { get; } = supportsZones;
    public List<ManagedLustreSkuCapability> Capabilities { get; } = capabilities ?? [];
}

public sealed class ManagedLustreSkuCapability(string name, string value)
{
    public string Name { get; } = name;
    public string Value { get; } = value;
}
