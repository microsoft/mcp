// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Tools.Storage.Models;
using Azure.Mcp.Tools.Storage.Options;
using Azure.Mcp.Tools.Storage.Options.Account;
using Azure.Mcp.Tools.Storage.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.Storage.Commands.Account;

[CommandMetadata(
    Id = "eb2363f1-f21f-45fc-ad63-bacfbae8c45c",
    Name = "get",
    Title = "Get Storage Account Details",
    Description = "Retrieves detailed information about Azure Storage accounts, including account name, location, SKU, kind, hierarchical namespace status, HTTPS-only settings, and blob public access configuration. If a specific account name is not provided, the command will return details for all accounts in a subscription.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class AccountGetCommand(ILogger<AccountGetCommand> logger, IStorageService storageService) : SubscriptionCommand<AccountGetOptions>()
{
    private readonly ILogger<AccountGetCommand> _logger = logger;
    private readonly IStorageService _storageService = storageService;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(StorageOptionDefinitions.Account.AsOptional());
    }

    protected override AccountGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Account = parseResult.GetValueOrDefault<string>(StorageOptionDefinitions.Account.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            // Call service operation with required parameters
            var accounts = await _storageService.GetAccountDetails(
                options.Account,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            // Set results
            context.Response.Results = ResponseResult.Create(new(accounts?.Results ?? [], accounts?.AreResultsTruncated ?? false), StorageJsonContext.Default.AccountGetCommandResult);
        }
        catch (Exception ex)
        {
            if (options.Account is null)
            {
                _logger.LogError(ex, "Error listing account details. Subscription: {Subscription}.", options.Subscription);
            }
            else
            {
                _logger.LogError(ex, "Error getting storage account details. Account: {Account}, Subscription: {Subscription}.",
                    options.Account, options.Subscription);
            }
            HandleException(context, ex);
        }

        return context.Response;
    }

    // Strongly-typed result record
    internal record AccountGetCommandResult(List<StorageAccountInfo> Accounts, bool AreResultsTruncated);
}
