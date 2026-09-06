// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.ResourceManager.AppService.Models;
using Azure.ResourceManager.Resources;
using Azure.ResourceManager.Storage;
using Azure.ResourceManager.Storage.Models;

namespace Azure.Mcp.Tools.FunctionApp.Services;

/// <summary>
/// Creates or reuses the storage account that backs a Function App.
/// </summary>
internal static class FunctionAppStorageProvisioner
{
    public const string DefaultStorageEndpointSuffix = "core.windows.net";

    private const int MaxGeneratedPrefixLength = 18;
    private const int GeneratedSuffixLength = 6;

    public static string CreateStorageAccountName(string functionAppName)
    {
        var baseName = new string(functionAppName.ToLowerInvariant().Where(char.IsAsciiLetterOrDigit).ToArray());
        if (baseName.Length == 0)
        {
            baseName = "func";
        }

        var prefix = baseName.Length > MaxGeneratedPrefixLength ? baseName[..MaxGeneratedPrefixLength] : baseName;
        var suffix = Guid.NewGuid().ToString("N")[..GeneratedSuffixLength];
        return $"{prefix}{suffix}";
    }

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
        ResourceGroupResource resourceGroup,
        string functionAppName,
        string location,
        string? storageAccountName,
        bool useManagedIdentity,
        string endpointSuffix,
        CancellationToken cancellationToken)
    {
        var accountName = storageAccountName ?? CreateStorageAccountName(functionAppName);
        var storageAccounts = resourceGroup.GetStorageAccounts();

        StorageAccountResource storage;
        if (await storageAccounts.ExistsAsync(accountName, cancellationToken: cancellationToken))
        {
            storage = (await storageAccounts.GetAsync(accountName, cancellationToken: cancellationToken)).Value;
        }
        else
        {
            var operation = await storageAccounts.CreateOrUpdateAsync(WaitUntil.Completed, accountName, CreateStorageAccountOptions(location), cancellationToken);
            storage = operation.Value;
        }

        if (useManagedIdentity)
        {
            return new StorageProvisioningResult(accountName, string.Empty);
        }

        var keys = await storage.GetKeysAsync(cancellationToken: cancellationToken);
        var primaryKey = keys.Value.Keys.FirstOrDefault()
            ?? throw new InvalidOperationException($"No keys found for storage account '{accountName}'.");

        return new StorageProvisioningResult(accountName, BuildConnectionString(accountName, primaryKey.Value, endpointSuffix));
    }

    /// <summary>
    /// Builds the Flex Consumption deployment storage configuration pointing at the host's blob container.
    /// </summary>
    public static FunctionAppStorage? BuildDeploymentStorage(string? accountName, bool useManagedIdentity, string endpointSuffix = DefaultStorageEndpointSuffix)
    {
        if (string.IsNullOrWhiteSpace(accountName))
        {
            return null;
        }

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
