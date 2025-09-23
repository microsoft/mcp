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
using Azure.Mcp.Tools.AppConfig.Services.Models;

namespace Azure.Mcp.Tools.AppConfig.Services;

using ETag = Core.Models.ETag;

public class AppConfigService(ISubscriptionService subscriptionService, ITenantService tenantService)
    : BaseAzureResourceService(subscriptionService, tenantService), IAppConfigService
{
    private readonly ISubscriptionService _subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));

    public async Task<List<AppConfigurationAccount>> GetAppConfigAccounts(string subscription, string? tenant = null, RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(subscription);

        var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy);
        var accounts = new List<AppConfigurationAccount>();

        return await ExecuteResourceQueryAsync(
            "Microsoft.AppConfiguration/configurationStores",
            resourceGroup: null, // All resource groups
            subscription,
            retryPolicy,
            ConvertToAccount,
            cancellationToken: CancellationToken.None);
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

    private async Task SetKeyValueReadOnlyState(string accountName, string key, string subscription, string? tenant, RetryPolicyOptions? retryPolicy, string? label, bool isReadOnly)
    {
        ValidateRequiredParameters(accountName, key, subscription);
        var client = await GetConfigurationClient(accountName, subscription, tenant, retryPolicy);
        await client.SetReadOnlyAsync(key, label, isReadOnly, cancellationToken: default);
    }

    private async Task<ConfigurationClient> GetConfigurationClient(string accountName, string subscription, string? tenant, RetryPolicyOptions? retryPolicy)
    {
        var endpoint = await FindAppConfigStoreEndpoint(subscription, accountName, subscription, retryPolicy);
        var credential = await GetCredential(tenant);
        var options = new ConfigurationClientOptions();
        AddDefaultPolicies(options);

        return new ConfigurationClient(new Uri(endpoint), credential, options);
    }

    private async Task<string> FindAppConfigStoreEndpoint(string subscription, string accountName, string subscriptionIdentifier, RetryPolicyOptions? retryPolicy)
    {
        AppConfigurationAccount? configStore = await ExecuteSingleResourceQueryAsync(
            "Microsoft.AppConfiguration/configurationStores",
            resourceGroup: null, // All resource groups
            subscription,
            retryPolicy,
            ConvertToAccount,
            $"name ~= '{EscapeKqlString(accountName)}'");

        if (configStore == null)
            throw new Exception($"App Configuration store '{accountName}' not found in subscription '{subscriptionIdentifier}'");

        return configStore.Endpoint;
    }

    /// <summary>
    /// Converts a JsonElement from Azure Resource Graph query to an AppConfig account.
    /// </summary>
    /// <param name="item">The JsonElement containing store data</param>
    /// <returns>The account model</returns>
    private static AppConfigurationAccount ConvertToAccount(JsonElement item)
    {
        AppConfigStoreData? store = AppConfigStoreData.FromJson(item);
        if (store == null)
            throw new InvalidOperationException("Failed to parse AppConfig store data");

        if (string.IsNullOrEmpty(store.ResourceId))
            throw new InvalidOperationException("Resource ID is missing");
        var id = new ResourceIdentifier(store.ResourceId);

        return new AppConfigurationAccount
        {
            Name = store.ResourceName!,
            Location = store.Location!,
            Endpoint = store.Properties!.Endpoint!,
            CreationDate = store.Properties?.CreatedOn?.DateTime ?? DateTime.MinValue,
            PublicNetworkAccess = store.Properties?.PublicNetworkAccess?.Equals("Enabled", StringComparison.OrdinalIgnoreCase) == true,
            Sku = store.Sku?.Name,
            Tags = store.Tags ?? new Dictionary<string, string>(),
            DisableLocalAuth = store.Properties?.DisableLocalAuth,
            SoftDeleteRetentionInDays = store.Properties?.SoftDeleteRetentionInDays,
            EnablePurgeProtection = store.Properties?.EnablePurgeProtection,
            CreateMode = store.Properties?.CreateMode,

            // Map the new managed identity structure
            ManagedIdentity = store.Identity == null ? null : new ManagedIdentityInfo
            {
                SystemAssignedIdentity = new SystemAssignedIdentityInfo
                {
                    Enabled = store.Identity != null,
                    TenantId = store.Identity?.TenantId.ToString(),
                    PrincipalId = store.Identity?.PrincipalId.ToString()
                },
                UserAssignedIdentities = store.Identity?.UserAssignedIdentities?
                    .Select(id => new UserAssignedIdentityInfo
                    {
                        ClientId = id.Value.ClientId?.ToString(),
                        PrincipalId = id.Value.PrincipalId?.ToString()
                    })
                    .ToArray()
            },

            // Full encryption properties from KeyVaultProperties
            Encryption = store.Properties?.EncryptionProperties?.KeyVaultProperties == null ? null : new EncryptionProperties
            {
                KeyIdentifier = store.Properties.EncryptionProperties.KeyVaultProperties.KeyIdentifier,
                IdentityClientId = store.Properties.EncryptionProperties.KeyVaultProperties.IdentityClientId,
            }
        };
    }
}
