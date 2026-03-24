// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.Mcp.Tools.NetAppFiles.Options;
using Azure.Mcp.Tools.NetAppFiles.Options.Account;
using Azure.Mcp.Tools.NetAppFiles.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.NetAppFiles.Commands.Account;

public sealed class AccountGetCommand(ILogger<AccountGetCommand> logger) : SubscriptionCommand<AccountGetOptions>()
{
    private const string CommandTitle = "Get NetApp Files Account Details";

    private readonly ILogger<AccountGetCommand> _logger = logger;

    public override string Id => "a7c3e1b4-9d2f-4a8e-b5c6-d3e7f0a1b2c4";

    public override string Name => "get";

    public override string Description =>
        """
        Retrieves detailed information about Azure NetApp Files accounts, including account name, location, resource group, provisioning state, active directory configuration, and encryption settings. If a specific account name is not provided, the command will return details for all NetApp Files accounts in a subscription.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        ReadOnly = true,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(NetAppFilesOptionDefinitions.Account.AsOptional());
    }

    protected override AccountGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Account = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.Account.Name);
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
            var netAppFilesService = context.GetService<INetAppFilesService>();

            var accounts = await netAppFilesService.GetAccountDetails(
                options.Account,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new(accounts?.Results ?? [], accounts?.AreResultsTruncated ?? false),
                NetAppFilesJsonContext.Default.AccountGetCommandResult);
        }
        catch (Exception ex)
        {
            if (options.Account is null)
            {
                _logger.LogError(ex, "Error listing NetApp Files account details. Subscription: {Subscription}, Options: {@Options}", options.Subscription, options);
            }
            else
            {
                _logger.LogError(ex, "Error getting NetApp Files account details. Account: {Account}, Subscription: {Subscription}, Options: {@Options}",
                    options.Account, options.Subscription, options);
            }
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record AccountGetCommandResult(List<NetAppAccountInfo> Accounts, bool AreResultsTruncated);
}
