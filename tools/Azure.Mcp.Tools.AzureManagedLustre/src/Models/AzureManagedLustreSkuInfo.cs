// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureManagedLustre.Models;

public sealed class AzureManagedLustreSkuInfo
{
    public AzureManagedLustreSkuInfo(
        string name,
        string location,
        bool supportsZones,
        List<AzureManagedLustreSkuCapability> capabilities)
    {
        Name = name;
        Location = location;
        SupportsZones = supportsZones;
        Capabilities = capabilities ?? [];
    }

    public string Name { get; }
    public string Location { get; }
    public bool SupportsZones { get; }
    public List<AzureManagedLustreSkuCapability> Capabilities { get; }
}

public sealed class AzureManagedLustreSkuCapability
{
    public AzureManagedLustreSkuCapability(string name, string value)
    {
        Name = name;
        Value = value;
    }

    public string Name { get; }
    public string Value { get; }
}
