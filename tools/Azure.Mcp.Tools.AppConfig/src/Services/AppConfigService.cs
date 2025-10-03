// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Core;
using Azure.Data.AppConfiguration;
using Azure.Mcp.Core.Models.Identity;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.AppConfig.Models;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.AppConfig.Services;

using ETag = Core.Models.ETag;

public sealed class AppConfigService(ISubscriptionService subscriptionService, ITenantService tenantService, ILogger<AppConfigService> logger)
    : BaseAzureResourceService(subscriptionService, tenantService), IAppConfigService
{
    private readonly ISubscriptionService _subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));
    private readonly ILogger<AppConfigService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<List<AppConfigurationAccount>> GetAppConfigAccounts(string subscription, string? tenant = null, RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(subscription);

        try
        {
            var accounts = await ExecuteResourceQueryAsync(
                "Microsoft.AppConfiguration/configurationStores",
                resourceGroup: null, // all resource groups
                subscription,
                retryPolicy,
                ConvertToAppConfigurationAccountModel,
                cancellationToken: CancellationToken.None);

            return accounts;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error retrieving App Configuration stores: {ex.Message}", ex);
        }
    }

    public async Task<List<KeyValueSetting>> GetKeyValues(
        string accountName,
        string subscription,
        string? key = null,
        string? label = null,
        string? keyFilter = null,
        string? labelFilter = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(accountName, subscription);

        var client = await GetConfigurationClient(accountName, subscription, tenant, retryPolicy);
        var settings = new List<KeyValueSetting>();
        if (!string.IsNullOrEmpty(key))
        {
            var response = await client.GetConfigurationSettingAsync(key, label, cancellationToken: default);
            AddSetting(response.Value, settings);
        }
        else
        {
            var selector = new SettingSelector
            {
                KeyFilter = string.IsNullOrEmpty(keyFilter) ? null : keyFilter,
                LabelFilter = string.IsNullOrEmpty(labelFilter) ? null : labelFilter
            };

            await foreach (var setting in client.GetConfigurationSettingsAsync(selector))
            {
                AddSetting(setting, settings);
            }
        }

        return settings;
    }

    private static void AddSetting(ConfigurationSetting setting, List<KeyValueSetting> settings)
    {
        settings.Add(new()
        {
            Key = setting.Key,
            Value = setting.Value,
            Label = setting.Label ?? string.Empty,
            ContentType = setting.ContentType ?? string.Empty,
            ETag = new ETag { Value = setting.ETag.ToString() },
            LastModified = setting.LastModified,
            Locked = setting.IsReadOnly
        });
    }

    public async Task SetKeyValueLockState(string accountName, string key, bool locked, string subscription, string? tenant = null, RetryPolicyOptions? retryPolicy = null, string? label = null)
    {
        ValidateRequiredParameters(accountName, key, subscription);
        var client = await GetConfigurationClient(accountName, subscription, tenant, retryPolicy);
        await client.SetReadOnlyAsync(key, label, locked, cancellationToken: default);
    }

    public async Task SetKeyValue(string accountName, string key, string value, string subscription, string? tenant = null, RetryPolicyOptions? retryPolicy = null, string? label = null, string? contentType = null, string[]? tags = null)
    {
        ValidateRequiredParameters(accountName, key, value, subscription);
        var client = await GetConfigurationClient(accountName, subscription, tenant, retryPolicy);

        // Create a ConfigurationSetting object to include contentType if provided
        var setting = new ConfigurationSetting(key, value, label)
        {
            ContentType = contentType
        };

        // Parse and add tags if provided
        if (tags != null && tags.Length > 0)
        {
            foreach (var tagPair in tags)
            {
                var parts = tagPair.Split('=', 2);
                if (parts.Length == 2)
                {
                    var tagKey = parts[0].Trim();
                    if (!string.IsNullOrEmpty(tagKey))
                    {
                        setting.Tags[tagKey] = parts[1];
                    }
                }
                else if (parts.Length == 1 && !string.IsNullOrEmpty(parts[0]))
                {
                    // Handle tags that don't follow key=value format
                    setting.Tags[parts[0]] = string.Empty;
                }
            }
        }

        await client.SetConfigurationSettingAsync(setting, cancellationToken: default);
    }
    public async Task DeleteKeyValue(string accountName, string key, string subscription, string? tenant = null, RetryPolicyOptions? retryPolicy = null, string? label = null)
    {
        ValidateRequiredParameters(accountName, key, subscription);
        var client = await GetConfigurationClient(accountName, subscription, tenant, retryPolicy);
        await client.DeleteConfigurationSettingAsync(key, label, cancellationToken: default);
    }

    private async Task<ConfigurationClient> GetConfigurationClient(string accountName, string subscription, string? tenant, RetryPolicyOptions? retryPolicy)
    {
        var configStore = await FindAppConfigStore(subscription, accountName, subscription, retryPolicy);
        var endpoint = configStore.Endpoint;
        if (string.IsNullOrEmpty(endpoint))
        {
            throw new InvalidOperationException($"The App Configuration store '{accountName}' does not have a valid endpoint.");
        }
        var credential = await GetCredential(tenant);
        var options = new ConfigurationClientOptions();
        AddDefaultPolicies(options);

        return new ConfigurationClient(new Uri(endpoint), credential, options);
    }

    private async Task<AppConfigurationAccount> FindAppConfigStore(string subscription, string accountName, string subscriptionIdentifier, RetryPolicyOptions? retryPolicy)
    {
        try
        {
            var account = await ExecuteSingleResourceQueryAsync(
                        "Microsoft.AppConfiguration/configurationStores",
                        resourceGroup: null, // all resource groups
                        subscription,
                        retryPolicy,
                        ConvertToAppConfigurationAccountModel,
                        additionalFilter: $"name =~ '{EscapeKqlString(accountName)}'");

            if (account == null)
            {
                throw new KeyNotFoundException($"App Configuration store '{accountName}' not found for subscription '{subscriptionIdentifier}'.");
            }
            return account;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error retrieving App Configuration store '{StoreName}' for subscription '{Subscription}'",
                accountName, subscriptionIdentifier);
            throw;
        }
    }

    /// <summary>
    /// Converts a JsonElement from Azure Resource Graph query to an App Configuration account model.
    /// </summary>
    /// <param name="item">The JsonElement containing App Configuration account data</param>
    /// <returns>The App Configuration account model</returns>
    private static AppConfigurationAccount ConvertToAppConfigurationAccountModel(JsonElement item)
    {
        Models.AppConfigurationStoreData? appConfigAccount = Models.AppConfigurationStoreData.FromJson(item);
        if (appConfigAccount == null)
            throw new InvalidOperationException("Failed to parse App Configuration account data");

        bool publicNetworkAccess = false;
        if (appConfigAccount.Properties?.PublicNetworkAccess != null)
        {
            publicNetworkAccess = appConfigAccount.Properties.PublicNetworkAccess.Equals("Enabled", StringComparison.OrdinalIgnoreCase);
        }

        return new AppConfigurationAccount
        {
            Name = appConfigAccount.ResourceName ?? "Unknown",
            Location = appConfigAccount.Location,
            Endpoint = appConfigAccount.Properties?.Endpoint,
            CreationDate = appConfigAccount.Properties?.CreatedOn,
            PublicNetworkAccess = publicNetworkAccess,
            Sku = appConfigAccount.Sku?.Name,
            Tags = appConfigAccount.Tags ?? new Dictionary<string, string>(),
            DisableLocalAuth = appConfigAccount.Properties?.DisableLocalAuth,
            SoftDeleteRetentionInDays = appConfigAccount.Properties?.SoftDeleteRetentionInDays,
            EnablePurgeProtection = appConfigAccount.Properties?.EnablePurgeProtection,
            CreateMode = appConfigAccount.Properties?.CreateMode,
            // Map the new managed identity structure
            ManagedIdentity = appConfigAccount.Identity == null ? null : new ManagedIdentityInfo
            {
                SystemAssignedIdentity = new SystemAssignedIdentityInfo
                {
                    Enabled = appConfigAccount.Identity != null,
                    TenantId = appConfigAccount.Identity?.TenantId?.ToString(),
                    PrincipalId = appConfigAccount.Identity?.PrincipalId?.ToString()
                },
                UserAssignedIdentities = appConfigAccount.Identity?.UserAssignedIdentities?
                    .Select(id => new UserAssignedIdentityInfo
                    {
                        ClientId = id.Value.ClientId?.ToString(),
                        PrincipalId = id.Value.PrincipalId?.ToString()
                    })
                    .ToArray()
            },
            // Full encryption properties from KeyVaultProperties
            Encryption = appConfigAccount.Properties?.Encryption?.KeyVaultProperties == null ? null : new EncryptionProperties
            {
                KeyIdentifier = appConfigAccount.Properties?.Encryption?.KeyVaultProperties?.KeyIdentifier,
                IdentityClientId = appConfigAccount.Properties?.Encryption?.KeyVaultProperties?.IdentityClientId,
            }
        };
    }
}
