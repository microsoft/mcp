// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.NetAppFiles.Models;

namespace Azure.Mcp.Tools.NetAppFiles.Services;

public interface INetAppFilesSnapshotPolicyService
{
    Task<NetAppFilesSnapshotPolicy> CreateSnapshotPolicyAsync(
        SnapshotPolicyCreateRequest request,
        string subscription,
        string? tenant = null,
        CancellationToken cancellationToken = default);

    Task<NetAppFilesSnapshotPolicy> GetSnapshotPolicyAsync(
        string account,
        string snapshotPolicy,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        CancellationToken cancellationToken = default);

    Task<NetAppFilesSnapshotPolicy> UpdateSnapshotPolicyAsync(
        SnapshotPolicyUpdateRequest request,
        string subscription,
        string? tenant = null,
        CancellationToken cancellationToken = default);
}
