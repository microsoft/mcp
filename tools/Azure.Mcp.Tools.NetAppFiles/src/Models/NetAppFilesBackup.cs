// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.NetAppFiles.Models;

public record NetAppFilesBackup(
    string Name,
    string Id,
    string? ProvisioningState,
    string? BackupType,
    string? Label,
    long? Size,
    string? VolumeResourceId);
