// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.NetAppFiles.Models;

namespace Azure.Mcp.Tools.NetAppFiles.Services;

public interface INetAppFilesAccountService
{
    Task<NetAppFilesAccount> GetAccountAsync(
        string account,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        CancellationToken cancellationToken = default);

    Task<NetAppFilesAccount> UpdateAccountAsync(
        string account,
        string resourceGroup,
        string subscription,
        IReadOnlyDictionary<string, string>? tags = null,
        string? nfsV4IdDomain = null,
        string? tenant = null,
        CancellationToken cancellationToken = default);

    Task<NetAppFilesAccount> CreateAccountAsync(
        string account,
        string location,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        CancellationToken cancellationToken = default);
}
