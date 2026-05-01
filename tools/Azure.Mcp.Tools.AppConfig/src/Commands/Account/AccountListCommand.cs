// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Tools.AppConfig.Models;
using Azure.Mcp.Tools.AppConfig.Options.Account;
using Azure.Mcp.Tools.AppConfig.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.AppConfig.Commands.Account;

[CommandMetadata(
    Id = "e403c988-b57b-4ac1-afb7-25ba3fdd6e6a",
    Name = "list",
    Title = "List App Configuration Stores",
    Description = """
        List all App Configuration stores in a subscription. This command retrieves and displays all App Configuration
        stores available in the specified subscription. Results include store names returned as a JSON array.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class AccountListCommand(ILogger<AccountListCommand> logger, IAppConfigService appConfigService) : SubscriptionCommand<AccountListOptions>()
{
    private readonly ILogger<AccountListCommand> _logger = logger;
    private readonly IAppConfigService _appConfigService = appConfigService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            var accounts = await _appConfigService.GetAppConfigAccounts(
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(accounts?.Results ?? [], accounts?.AreResultsTruncated ?? false), AppConfigJsonContext.Default.AccountListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred listing accounts.");
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record AccountListCommandResult(List<AppConfigurationAccount> Accounts, bool AreResultsTruncated);
}
