// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.Mcp.Tools.NetAppFiles.Options;
using Azure.Mcp.Tools.NetAppFiles.Options.BackupVault;
using Azure.Mcp.Tools.NetAppFiles.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.NetAppFiles.Commands.BackupVault;

public sealed class BackupVaultGetCommand(ILogger<BackupVaultGetCommand> logger) : SubscriptionCommand<BackupVaultGetOptions>()
{
    private const string CommandTitle = "Get NetApp Files Backup Vault Details";

    private readonly ILogger<BackupVaultGetCommand> _logger = logger;

    public override string Id => "a1c3e5f7-9b2d-4f6a-8e0c-d2b4a6c8e0f2";

    public override string Name => "get";

    public override string Description =>
        """
        Retrieves detailed information about Azure NetApp Files backup vaults, including vault name, location, resource group, and provisioning state. If a specific backup vault name is not provided, the command will return details for all backup vaults in a subscription. Optionally filter by account.
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
        command.Options.Add(NetAppFilesOptionDefinitions.BackupVault.AsOptional());
    }

    protected override BackupVaultGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Account = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.Account.Name);
        options.BackupVault = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.BackupVault.Name);
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

            var backupVaults = await netAppFilesService.GetBackupVaultDetails(
                options.Account,
                options.BackupVault,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new BackupVaultGetCommandResult(backupVaults?.Results ?? [], backupVaults?.AreResultsTruncated ?? false),
                NetAppFilesJsonContext.Default.BackupVaultGetCommandResult);
        }
        catch (Exception ex)
        {
            if (options.BackupVault is null)
            {
                _logger.LogError(ex, "Error listing NetApp Files backup vault details. Subscription: {Subscription}, Options: {@Options}", options.Subscription, options);
            }
            else
            {
                _logger.LogError(ex, "Error getting NetApp Files backup vault details. BackupVault: {BackupVault}, Subscription: {Subscription}, Options: {@Options}",
                    options.BackupVault, options.Subscription, options);
            }
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record BackupVaultGetCommandResult(List<BackupVaultInfo> BackupVaults, bool AreResultsTruncated);
}
