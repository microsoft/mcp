// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Models;

/// <summary>
/// Extended properties returned by the Recovery Services protected-item API.
/// </summary>
public sealed record ProtectedItemExtendedProperties(
    ProtectedItemDiskExclusionProperties? DiskExclusionProperties,
    string? LinuxVmApplicationName);

public sealed record ProtectedItemDiskExclusionProperties(
    IReadOnlyList<int>? DiskLunList,
    bool? IsInclusionList);