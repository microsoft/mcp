// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.Storage.Models;
using Azure.Mcp.Tools.Storage.Options.Account;
using Azure.Mcp.Tools.Storage.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Storage.Commands.Account;

[CommandMetadata(
    Id = "a2cf843a-57f2-45ea-8078-59b0be0805e6",
    Name = "create",
    Title = "Create Storage Account",
    Description = """
        Creates an Azure Storage account in the specified resource group and location and returns the created storage account
        information including name, location, SKU, access settings, and configuration details.
        """,
    Destructive = true,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class AccountCreateCommand(ILogger<AccountCreateCommand> logger, IStorageService storageService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<AccountCreateOptions, AccountCreateCommand.AccountCreateCommandResult>(subscriptionResolver)
{
    private readonly ILogger<AccountCreateCommand> _logger = logger;
    private readonly IStorageService _storageService = storageService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, AccountCreateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var account = await _storageService.CreateStorageAccount(
                options.Account,
                options.ResourceGroup,
                options.Location,
                options.Subscription!,
                options.Sku,
                options.AccessTier,
                options.EnableHierarchicalNamespace,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new AccountCreateCommandResult(account), StorageJsonContext.Default.AccountCreateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error creating storage account. Account: {Account}, ResourceGroup: {ResourceGroup}, Location: {Location}.",
                options.Account, options.ResourceGroup, options.Location);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        KeyNotFoundException => $"Storage account not found. Verify the account name, subscription, and that you have access.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            "Storage account name already exists. Choose a different name.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed creating the storage account. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Resource group not found. Verify the resource group exists and you have access.",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    public record AccountCreateCommandResult(StorageAccountResult Account);
}
