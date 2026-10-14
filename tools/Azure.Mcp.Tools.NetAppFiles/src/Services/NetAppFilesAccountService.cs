// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
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
        IReadOnlyDictionary<string, string>? tags = null,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        var resourceGroupResource = await GetResourceGroupOrThrowAsync(
            resourceGroup,
            subscription,
            tenant,
            cancellationToken: cancellationToken);

        var existingAccounts = resourceGroupResource.GetNetAppAccounts();
        var hasAccount = await existingAccounts.ExistsAsync(account, cancellationToken).ConfigureAwait(false);
        if (hasAccount)
        {
            throw new RequestFailedException((int)HttpStatusCode.Conflict, $"Conflict: NetApp account '{account}' already exists in resource group '{resourceGroup}'.");
        }

        var accountData = new NetAppAccountData(new AzureLocation(location));
        if (tags is not null)
        {
            foreach (var tag in tags)
            {
                accountData.Tags[tag.Key] = tag.Value;
            }
        }
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
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        var resourceGroupResource = await GetResourceGroupOrThrowAsync(
            resourceGroup,
            subscription,
            tenant,
            cancellationToken: cancellationToken);

        var accountResource = (await resourceGroupResource.GetNetAppAccountAsync(account, cancellationToken)).Value;
        var patch = new NetAppAccountPatch(accountResource.Data.Location);
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
