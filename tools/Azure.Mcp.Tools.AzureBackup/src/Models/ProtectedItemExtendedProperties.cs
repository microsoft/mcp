// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Models;

/// <summary>
/// Extended properties currently returned by RSV VM protected-item APIs.
/// </summary>
public sealed record ProtectedItemExtendedProperties(
    ProtectedItemDiskExclusionProperties? DiskExclusionProperties,
    string? LinuxVmApplicationName);
