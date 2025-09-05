// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.Storage.Models;
using Azure.Mcp.Tools.Storage.Options;
using Azure.Mcp.Tools.Storage.Options.Account;
using Azure.Mcp.Tools.Storage.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Storage.Commands.Account;

public sealed class AccountCreateCommand(ILogger<AccountCreateCommand> logger) : SubscriptionCommand<AccountCreateOptions>()
{
    private const string CommandTitle = "Create Storage Account";
    private readonly ILogger<AccountCreateCommand> _logger = logger;

    // Define options from OptionDefinitions
    private readonly Option<string> _accountCreateOption = StorageOptionDefinitions.AccountCreate;
    private readonly Option<string> _locationOption = StorageOptionDefinitions.Location;
    private readonly Option<string> _skuOption = StorageOptionDefinitions.Sku;
    private readonly Option<string> _accessTierOption = StorageOptionDefinitions.AccessTier;
    private readonly Option<bool> _enableHierarchicalNamespaceOption = StorageOptionDefinitions.EnableHierarchicalNamespace;

    public override string Name => "create";

    public override string Description =>
        """
        Create a new Azure Storage account in the specified resource group and location.
        Creates a storage account with the specified configuration options. Returns the
        created storage account information including name, location, SKU, and other properties.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        ReadOnly = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(_accountCreateOption);
        RequireResourceGroup();
        command.Options.Add(_locationOption);
        command.Options.Add(_skuOption);
        command.Options.Add(_accessTierOption);
        command.Options.Add(_enableHierarchicalNamespaceOption);
    }

    protected override AccountCreateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Account = parseResult.GetValueOrDefault(_accountCreateOption);
        options.Location = parseResult.GetValueOrDefault(_locationOption);
        options.Sku = parseResult.GetValueOrDefault(_skuOption);
        options.AccessTier = parseResult.GetValueOrDefault(_accessTierOption);
        options.EnableHierarchicalNamespace = parseResult.GetValueOrDefault(_enableHierarchicalNamespaceOption);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {

        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            // Get the storage service from DI
            var storageService = context.GetService<IStorageService>();

            // Call service to create storage account
            var account = await storageService.CreateStorageAccount(
                options.Account!,
                options.ResourceGroup!,
                options.Location!,
                options.Subscription!,
                options.Sku,
                options.AccessTier,
                options.EnableHierarchicalNamespace,
                options.Tenant,
                options.RetryPolicy);

            // Set results
            context.Response.Results = ResponseResult.Create(
                new AccountCreateCommandResult(account),
                StorageJsonContext.Default.AccountCreateCommandResult);
        }
        catch (Exception ex)
        {
            // Log error with all relevant context
            _logger.LogError(ex,
                "Error creating storage account. Account: {Account}, ResourceGroup: {ResourceGroup}, Location: {Location}, Options: {@Options}",
                options.Account, options.ResourceGroup, options.Location, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    // Implementation-specific error handling
    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == 409 =>
            "Storage account name already exists. Choose a different name.",
        RequestFailedException reqEx when reqEx.Status == 403 =>
            $"Authorization failed creating the storage account. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == 404 =>
            "Resource group not found. Verify the resource group exists and you have access.",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    protected override int GetStatusCode(Exception ex) => ex switch
    {
        RequestFailedException reqEx => reqEx.Status,
        _ => base.GetStatusCode(ex)
    };

    // Strongly-typed result record
    internal record AccountCreateCommandResult(StorageAccountInfo Account);
}
