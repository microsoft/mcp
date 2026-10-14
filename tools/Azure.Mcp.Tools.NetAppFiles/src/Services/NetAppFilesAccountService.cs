// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.ResourceManager.NetApp;
using Azure.ResourceManager.NetApp.Models;
using Azure.ResourceManager.Resources;

namespace Azure.Mcp.Tools.NetAppFiles.Services;

public class NetAppFilesAccountService(IAzureService azureService) : BaseAzureService(azureService), INetAppFilesAccountService
{
    public async Task<NetAppFilesAccount> GetAccountAsync(
        string account,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        var resourceGroupResource = await GetResourceGroupOrThrowAsync(
            resourceGroup,
            subscription,
            tenant,
            cancellationToken: cancellationToken);

        var accountResource = await resourceGroupResource.GetNetAppAccountAsync(account, cancellationToken);

        return Map(accountResource.Value);
    }

    public async Task<NetAppFilesAccount> CreateAccountAsync(
        string account,
        string location,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        var resourceGroupResource = await GetResourceGroupOrThrowAsync(
            resourceGroup,
            subscription,
            tenant,
            cancellationToken: cancellationToken);

        try
        {
            var existingAccount = await resourceGroupResource.GetNetAppAccountAsync(account, cancellationToken);
            if (existingAccount.HasValue)
            {
                throw new InvalidOperationException($"NetApp account '{account}' already exists in resource group '{resourceGroup}'.");
            }
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            // Account does not exist, continue with creation
        }

        var accountData = new NetAppAccountData(new AzureLocation(location));
        var operation = await resourceGroupResource
            .GetNetAppAccounts()
            .CreateOrUpdateAsync(WaitUntil.Started, account, accountData, cancellationToken);

        await WaitForLroCompletionAsync(operation, cancellationToken);

        return Map(operation.Value);
    }

    public async Task<NetAppFilesAccount> UpdateAccountAsync(
        string account,
        string resourceGroup,
        string subscription,
        IReadOnlyDictionary<string, string>? tags = null,
        string? nfsV4IdDomain = null,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        var resourceGroupResource = await GetResourceGroupOrThrowAsync(
            resourceGroup,
            subscription,
            tenant,
            cancellationToken: cancellationToken);

        var accountResource = (await resourceGroupResource.GetNetAppAccountAsync(account, cancellationToken)).Value;
        var patch = new NetAppAccountPatch(accountResource.Data.Location)
        {
            NfsV4IdDomain = nfsV4IdDomain
        };

        if (tags is not null)
        {
            patch.Tags.Clear();
            foreach (var tag in tags)
            {
                patch.Tags[tag.Key] = tag.Value;
            }
        }

        var operation = await accountResource.UpdateAsync(WaitUntil.Started, patch, cancellationToken);
        await WaitForLroCompletionAsync(operation, cancellationToken);

        return Map(operation.Value);
    }

    private static NetAppFilesAccount Map(NetAppAccountResource account) => new(
        account.Data.Name,
        account.Data.Id.ToString(),
        account.Data.Location.ToString(),
        account.Data.ProvisioningState?.ToString());

    private async Task<ResourceGroupResource> GetResourceGroupOrThrowAsync(
        string resourceGroup,
        string subscription,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        var resourceGroupResource = await AzureService.GetResourceGroupResource(
            subscription,
            resourceGroup,
            tenant,
            cancellationToken: cancellationToken)
            ?? throw new KeyNotFoundException($"Resource group '{resourceGroup}' was not found.");

        return resourceGroupResource;
    }
}
