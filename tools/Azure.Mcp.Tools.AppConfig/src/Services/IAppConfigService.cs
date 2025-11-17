// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.AppConfig.Models;

namespace Azure.Mcp.Tools.AppConfig.Services;

public interface IAppConfigService
{
    Task<List<AppConfigurationAccount>> GetAppConfigAccounts(
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);
    Task<List<KeyValueSetting>> GetKeyValues(
        string accountName,
        string subscription,
        string? key = null,
        string? label = null,
        string? keyFilter = null,
        string? labelFilter = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);
    Task SetKeyValueLockState(
        string accountName,
        string key,
        bool locked,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        string? label = null,
        CancellationToken cancellationToken = default);
    Task SetKeyValue(
        string accountName,
        string key,
        string value,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        string? label = null,
        string? contentType = null,
        string[]? tags = null,
        CancellationToken cancellationToken = default);
    Task DeleteKeyValue(
        string accountName,
        string key,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        string? label = null,
        CancellationToken cancellationToken = default);
}
