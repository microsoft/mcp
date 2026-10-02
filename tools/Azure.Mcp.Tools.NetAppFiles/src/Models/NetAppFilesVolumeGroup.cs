// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.NetAppFiles.Models;

public record NetAppFilesVolumeGroup(
    string Name,
    string Id,
    string Location,
    string? ProvisioningState,
    string? ApplicationType,
    string? ApplicationIdentifier,
    IReadOnlyList<string> Volumes);
