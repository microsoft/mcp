// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Storage.Services;

public interface IAttachedDiskService
{
    Task<(string VmResourceId, string[]? DiskResourceIds)> ResolveFriendlySelectorAsync(
        string subscription,
        string resourceGroup,
        string vm,
        string[]? diskNames,
        CancellationToken cancellationToken);

    Task<string[]> ResolveDiskNamesAsync(
        string vmResourceId,
        string[] diskNames,
        CancellationToken cancellationToken);
}
