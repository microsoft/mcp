// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json.Serialization;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
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

    public override string Name => "create";

    public override string Description =>
        """
        Creates an Azure Storage account in the specified resource group and location and returns the created storage account
        information including name, location, SKU, access settings, and configuration details.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = true,
        Idempotent = false,
        OpenWorld = false,
        ReadOnly = false,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsRequired());
        command.Options.Add(StorageOptionDefinitions.AccountCreate);
        command.Options.Add(StorageOptionDefinitions.Location);
        command.Options.Add(StorageOptionDefinitions.Sku);
        command.Options.Add(StorageOptionDefinitions.AccessTier);
        command.Options.Add(StorageOptionDefinitions.EnableHierarchicalNamespace);
    }

    protected override AccountCreateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.Account = parseResult.GetValueOrDefault<string>(StorageOptionDefinitions.AccountCreate.Name);
        options.Location = parseResult.GetValueOrDefault<string>(StorageOptionDefinitions.Location.Name);
        options.Sku = parseResult.GetValueOrDefault<string>(StorageOptionDefinitions.Sku.Name);
        options.AccessTier = parseResult.GetValueOrDefault<string>(StorageOptionDefinitions.AccessTier.Name);
        options.EnableHierarchicalNamespace = parseResult.GetValueOrDefault<bool>(StorageOptionDefinitions.EnableHierarchicalNamespace.Name);
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
            context.Response.Results = ResponseResult.Create(new(account), StorageJsonContext.Default.AccountCreateCommandResult);
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

    // Strongly-typed result record
    internal record AccountCreateCommandResult([property: JsonPropertyName("account")] StorageAccountResult Account);
}
