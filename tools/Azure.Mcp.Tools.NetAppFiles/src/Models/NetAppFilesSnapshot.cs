// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.NetAppFiles.Models;

public record NetAppFilesSnapshot(
    string Name,
    string Id,
    string Location,
    string? ProvisioningState,
    string? SnapshotId,
    DateTimeOffset? Created);