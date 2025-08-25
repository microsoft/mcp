// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Data.AppConfiguration;
using Azure.Mcp.Core.Models.Identity;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.AppConfig.Models;
using Azure.ResourceManager.AppConfiguration;
using Azure.ResourceManager.Resources;

namespace Azure.Mcp.Tools.AppConfig.Services;

using ETag = Azure.Mcp.Core.Models.ETag;

public class AppConfigService(ISubscriptionService subscriptionService, ITenantService tenantService)
    : BaseAzureService(tenantService), IAppConfigService
{
    private readonly ISubscriptionService _subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));

    public async Task<List<AppConfigurationAccount>> GetAppConfigAccounts(string subscription, string? tenant = null, RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(subscription);

        var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy);
        var accounts = new List<AppConfigurationAccount>();

        await foreach (var account in subscriptionResource.GetAppConfigurationStoresAsync())
        {
            ResourceIdentifier resourceId = account.Id;
            if (resourceId.ToString().Length == 0)
                continue;

            var acc = new AppConfigurationAccount
            {
                Name = account.Data.Name,
                Location = account.Data.Location.ToString(),
                Endpoint = account.Data.Endpoint,
                CreationDate = account.Data.CreatedOn?.DateTime ?? DateTime.MinValue,
                PublicNetworkAccess = account.Data.PublicNetworkAccess.HasValue &&
                    account.Data.PublicNetworkAccess.Value.ToString().Equals("Enabled", StringComparison.OrdinalIgnoreCase),
                Sku = account.Data.SkuName,
                Tags = account.Data.Tags ?? new Dictionary<string, string>(),
                DisableLocalAuth = account.Data.DisableLocalAuth,
                SoftDeleteRetentionInDays = account.Data.SoftDeleteRetentionInDays,
                EnablePurgeProtection = account.Data.EnablePurgeProtection,
                CreateMode = account.Data.CreateMode?.ToString(),

                // Map the new managed identity structure
                ManagedIdentity = account.Data.Identity == null ? null : new ManagedIdentityInfo
                {
                    SystemAssignedIdentity = new SystemAssignedIdentityInfo
                    {
                        Enabled = account.Data.Identity != null,
                        TenantId = account.Data.Identity?.TenantId?.ToString(),
                        PrincipalId = account.Data.Identity?.PrincipalId?.ToString()
                    },
                    UserAssignedIdentities = account.Data.Identity?.UserAssignedIdentities?
                        .Select(id => new UserAssignedIdentityInfo
                        {
                            ClientId = id.Value.ClientId?.ToString(),
                            PrincipalId = id.Value.PrincipalId?.ToString()
                        })
                        .ToArray()
                },

                // Full encryption properties from KeyVaultProperties
                Encryption = account.Data.EncryptionKeyVaultProperties == null ? null : new EncryptionProperties
                {
                    KeyIdentifier = account.Data.EncryptionKeyVaultProperties.KeyIdentifier,
                    IdentityClientId = account.Data.EncryptionKeyVaultProperties.IdentityClientId,
                }
            };

            accounts.Add(acc);
        }

        return accounts;
    }

    public async Task<List<KeyValueSetting>> ListKeyValues(
        string accountName,
        string subscription,
        string? key = null,
        string? label = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(accountName, subscription);

        var client = await GetConfigurationClient(accountName, subscription, tenant, retryPolicy);
        var settings = new List<KeyValueSetting>();

        var selector = new SettingSelector
        {
            KeyFilter = string.IsNullOrEmpty(key) ? null : key,
            LabelFilter = string.IsNullOrEmpty(label) ? null : label
        };

        await foreach (var setting in client.GetConfigurationSettingsAsync(selector))
        {
            settings.Add(new KeyValueSetting
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

        return settings;
    }

    public async Task<KeyValueSetting> GetKeyValue(string accountName, string key, string subscription, string? tenant = null, RetryPolicyOptions? retryPolicy = null, string? label = null, string? contentType = null)
    {
        ValidateRequiredParameters(accountName, key, subscription);
        var client = await GetConfigurationClient(accountName, subscription, tenant, retryPolicy);
        var response = await client.GetConfigurationSettingAsync(key, label, cancellationToken: default);
        var setting = response.Value;

        return new KeyValueSetting
        {
            Key = setting.Key,
            Value = setting.Value,
            Label = setting.Label ?? string.Empty,
            ContentType = setting.ContentType ?? string.Empty,
            ETag = new ETag { Value = setting.ETag.ToString() },
            LastModified = setting.LastModified,
            Locked = setting.IsReadOnly
        };
    }

    public async Task LockKeyValue(string accountName, string key, string subscription, string? tenant = null, RetryPolicyOptions? retryPolicy = null, string? label = null)
    {
        await SetKeyValueReadOnlyState(accountName, key, subscription, tenant, retryPolicy, label, true);
    }

    public async Task UnlockKeyValue(string accountName, string key, string subscription, string? tenant = null, RetryPolicyOptions? retryPolicy = null, string? label = null)
    {
        await SetKeyValueReadOnlyState(accountName, key, subscription, tenant, retryPolicy, label, false);
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

    private async Task SetKeyValueReadOnlyState(string accountName, string key, string subscription, string? tenant, RetryPolicyOptions? retryPolicy, string? label, bool isReadOnly)
    {
        ValidateRequiredParameters(accountName, key, subscription);
        var client = await GetConfigurationClient(accountName, subscription, tenant, retryPolicy);
        await client.SetReadOnlyAsync(key, label, isReadOnly, cancellationToken: default);
    }

    private async Task<ConfigurationClient> GetConfigurationClient(string accountName, string subscription, string? tenant, RetryPolicyOptions? retryPolicy)
    {
        var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy);
        var configStore = await FindAppConfigStore(subscriptionResource, accountName, subscription);
        var endpoint = configStore.Data.Endpoint;
        var credential = await GetCredential(tenant);
        var options = new ConfigurationClientOptions();
        AddDefaultPolicies(options);

        return new ConfigurationClient(new Uri(endpoint), credential, options);
    }

    private static async Task<AppConfigurationStoreResource> FindAppConfigStore(SubscriptionResource subscription, string accountName, string subscriptionIdentifier)
    {
        AppConfigurationStoreResource? configStore = null;
        await foreach (var store in subscription.GetAppConfigurationStoresAsync())
        {
            if (store.Data.Name == accountName)
            {
                configStore = store;
                break;
            }
        }

        if (configStore == null)
            throw new Exception($"App Configuration store '{accountName}' not found in subscription '{subscriptionIdentifier}'");

        return configStore;
    }
}
