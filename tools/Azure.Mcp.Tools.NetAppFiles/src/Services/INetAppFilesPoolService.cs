// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.NetAppFiles.Models;

namespace Azure.Mcp.Tools.NetAppFiles.Services;

public interface INetAppFilesPoolService
{
    Task<NetAppFilesPool> GetPoolAsync(
        string account,
        string pool,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        CancellationToken cancellationToken = default);

    Task<NetAppFilesPool> CreatePoolAsync(
        string account,
        string pool,
        long sizeInBytes,
        string serviceLevel,
        string resourceGroup,
        string subscription,
        string? location = null,
        string? qosType = null,
        bool? coolAccess = null,
        string? encryptionType = null,
        int? customThroughputMibps = null,
        IReadOnlyDictionary<string, string>? tags = null,
        string? tenant = null,
        CancellationToken cancellationToken = default);

    Task<NetAppFilesPool> UpdatePoolAsync(
        string account,
        string pool,
        string resourceGroup,
        string subscription,
        long? sizeInBytes = null,
        string? qosType = null,
        bool? coolAccess = null,
        int? customThroughputMibps = null,
        IReadOnlyDictionary<string, string>? tags = null,
        string? tenant = null,
        CancellationToken cancellationToken = default);
}
