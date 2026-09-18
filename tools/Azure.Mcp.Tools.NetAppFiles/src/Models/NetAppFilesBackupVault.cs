// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.NetAppFiles.Models;

public record NetAppFilesBackupVault(
    string Name,
    string Id,
    string Location,
    string? ProvisioningState);
