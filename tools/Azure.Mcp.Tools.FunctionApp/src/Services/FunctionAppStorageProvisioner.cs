// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.ResourceManager.AppService.Models;
using Azure.ResourceManager.Resources;
using Azure.ResourceManager.Storage;
using Azure.ResourceManager.Storage.Models;

namespace Azure.Mcp.Tools.FunctionApp.Services;

internal readonly record struct StorageProvisioningResult(string AccountName, string ConnectionString);

internal static class FunctionAppStorageProvisioner
{
    public static string CreateStorageAccountName(string functionAppName)
    {
        var baseName = new string(functionAppName.ToLowerInvariant().Where(char.IsLetterOrDigit).ToArray());
        if (string.IsNullOrEmpty(baseName))
            baseName = "func";

        var trimmed = baseName.Length > 18 ? baseName[..18] : baseName;
        var suffix = Guid.NewGuid().ToString("N")[..6];
        return $"{trimmed}{suffix}";
    }

    public const string DefaultStorageEndpointSuffix = "core.windows.net";

    public static string BuildConnectionString(string accountName, string key, string endpointSuffix = DefaultStorageEndpointSuffix) =>
        $"DefaultEndpointsProtocol=https;AccountName={accountName};AccountKey={key};EndpointSuffix={endpointSuffix}";

    public static StorageAccountCreateOrUpdateContent CreateStorageAccountOptions(string location) =>
        new(new StorageSku(StorageSkuName.StandardLrs), StorageKind.StorageV2, location)
        {
            AccessTier = StorageAccountAccessTier.Hot,
            EnableHttpsTrafficOnly = true,
            AllowBlobPublicAccess = false,
            IsHnsEnabled = false
        };

    public static async Task<StorageProvisioningResult> EnsureStorageForFunctionApp(
        SubscriptionResource subscription,
        ResourceGroupResource rg,
        string functionAppName,
        string location,
        string? storageAccountName = null,
        bool useManagedIdentity = false,
        string endpointSuffix = DefaultStorageEndpointSuffix,
        CancellationToken cancellationToken = default)
    {
        var accountName = storageAccountName ?? CreateStorageAccountName(functionAppName);
        var storageAccounts = rg.GetStorageAccounts();

        StorageAccountResource storage;
        if (await storageAccounts.ExistsAsync(accountName, cancellationToken: cancellationToken))
        {
            storage = (await storageAccounts.GetAsync(accountName, cancellationToken: cancellationToken)).Value;
        }
        else
        {
            var createOptions = CreateStorageAccountOptions(location);
            var op = await storageAccounts.CreateOrUpdateAsync(WaitUntil.Completed, accountName, createOptions, cancellationToken);
            storage = op.Value;
        }

        if (useManagedIdentity)
            return new StorageProvisioningResult(accountName, string.Empty);

        StorageAccountKey? primaryKey = null;
        await foreach (var key in storage.GetKeysAsync(cancellationToken: cancellationToken))
        {
            primaryKey = key;
            break;
        }
        if (primaryKey is null)
            throw new InvalidOperationException($"No keys found for storage account '{accountName}'");
        return new StorageProvisioningResult(accountName, BuildConnectionString(accountName, primaryKey.Value, endpointSuffix));
    }

    public static string? ExtractStorageAccountName(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            return null;
        var accountNamePart = connectionString
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .FirstOrDefault(p => p.StartsWith("AccountName=", StringComparison.OrdinalIgnoreCase));
        return accountNamePart?[12..];
    }

    public static FunctionAppStorage? BuildDeploymentStorage(string storageConnectionString, string endpointSuffix = DefaultStorageEndpointSuffix)
    {
        var accountName = ExtractStorageAccountName(storageConnectionString);
        return BuildDeploymentStorageForAccount(accountName, useManagedIdentity: false, endpointSuffix);
    }

    public static FunctionAppStorage? BuildDeploymentStorageForAccount(string? accountName, bool useManagedIdentity, string endpointSuffix = DefaultStorageEndpointSuffix)
    {
        if (string.IsNullOrWhiteSpace(accountName))
            return null;
        var authentication = useManagedIdentity
            ? new FunctionAppStorageAuthentication
            {
                AuthenticationType = FunctionAppStorageAccountAuthenticationType.SystemAssignedIdentity
            }
            : new FunctionAppStorageAuthentication
            {
                AuthenticationType = FunctionAppStorageAccountAuthenticationType.StorageAccountConnectionString,
                StorageAccountConnectionStringName = "AzureWebJobsStorage"
            };
        return new FunctionAppStorage
        {
            StorageType = FunctionAppStorageType.BlobContainer,
            Value = new Uri($"https://{accountName}.blob.{endpointSuffix}/azure-webjobs-hosts"),
            Authentication = authentication
        };
    }
}
