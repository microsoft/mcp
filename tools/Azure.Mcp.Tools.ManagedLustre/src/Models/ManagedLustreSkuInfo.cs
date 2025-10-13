// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.ManagedLustre.Models;

public sealed class ManagedLustreSkuInfo
{
    public ManagedLustreSkuInfo(
        string name,
        string location,
        bool supportsZones,
    List<ManagedLustreSkuCapability> capabilities)
    {
        Name = name;
        Location = location;
        SupportsZones = supportsZones;
        Capabilities = capabilities ?? [];
    }

    public string Name { get; }
    public string Location { get; }
    public bool SupportsZones { get; }
    public List<ManagedLustreSkuCapability> Capabilities { get; }
}

public sealed class ManagedLustreSkuCapability
{
    public ManagedLustreSkuCapability(string name, string value)
    {
        Name = name;
        Value = value;
    }

    public string Name { get; }
    public string Value { get; }
}
