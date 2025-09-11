// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.Storage.Models;
using Azure.Mcp.Tools.Storage.Options;
using Azure.Mcp.Tools.Storage.Options.Account;
using Azure.Mcp.Tools.Storage.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Storage.Commands.Account;

public sealed class AccountGetCommand(ILogger<AccountGetCommand> logger) : SubscriptionCommand<AccountGetOptions>()
{
    private const string CommandTitle = "Get Storage Account Details";
    private readonly ILogger<AccountGetCommand> _logger = logger;

    private readonly Option<string> _optionalAccountOption = StorageOptionDefinitions.OptionalAccount;

    public override string Name => "get";

    public override string Description =>
        """
        Gets detailed information about Azure Storage accounts, including account name, location, SKU, access settings,
        and configuration details. If a specific account name is not provided, the command will return details for all
        accounts in a subscription.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = true,
        ReadOnly = true,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(_optionalAccountOption);
    }

    protected override AccountGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Account = parseResult.GetValueOrDefault(_optionalAccountOption);
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

            // Call service operation with required parameters
            var accounts = await storageService.GetAccountDetails(
                options.Account,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy);

            // Set results
            context.Response.Results = accounts is { Count: > 0 }
                ? ResponseResult.Create(new AccountGetCommandResult(accounts), StorageJsonContext.Default.AccountGetCommandResult)
                : null;
        }
        catch (Exception ex)
        {
            if (options.Account is null)
            {
                _logger.LogError(ex, "Error listing account details. Subscription: {Subscription}, Options: {@Options}", options.Subscription, options);
            }
            else
            {
                _logger.LogError(ex, "Error getting storage account details. Account: {Account}, Subscription: {Subscription}, Options: {@Options}",
                    options.Account, options.Subscription, options);
            }
            HandleException(context, ex);
        }

        return context.Response;
    }

    // Strongly-typed result record
    internal record AccountGetCommandResult([property: JsonPropertyName("accounts")] List<AccountInfo> Accounts);
}
