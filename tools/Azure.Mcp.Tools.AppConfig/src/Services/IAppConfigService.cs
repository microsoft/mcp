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
        RetryPolicyOptions? retryPolicy = null);
    Task<List<KeyValueSetting>> ListKeyValues(
        string accountName,
        string subscription,
        string? key = null, string? label = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null);
    Task<KeyValueSetting> GetKeyValue(
        string accountName,
        string key,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        string? label = null,
        string? contentType = null);
    Task LockKeyValue(
        string accountName,
        string key,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        string? label = null);
    Task UnlockKeyValue(
        string accountName,
        string key,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        string? label = null);
    Task SetKeyValue(
        string accountName,
        string key,
        string value,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        string? label = null,
        string? contentType = null,
        string[]? tags = null);
    Task DeleteKeyValue(
        string accountName,
        string key,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        string? label = null);
}
