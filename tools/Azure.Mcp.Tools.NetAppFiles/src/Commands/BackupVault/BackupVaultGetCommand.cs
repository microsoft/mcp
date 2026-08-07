// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Microsoft.Mcp.Core.Extensions;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.Mcp.Tools.NetAppFiles.Options;
using Azure.Mcp.Tools.NetAppFiles.Options.BackupVault;
using Azure.Mcp.Tools.NetAppFiles.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.NetAppFiles.Commands.BackupVault;

[CommandMetadata(
    Id = "a1c3e5f7-9b2d-4f6a-8e0c-d2b4a6c8e0f2",
    Name = "get",
    Description =
        """
        Retrieves detailed information about Azure NetApp Files backup vaults, including vault name, location, resource group, and provisioning state. If a specific backup vault name is not provided, the command will return details for all backup vaults in a subscription. Optionally filter by account, resource group, or resource IDs.
        """,
    Title = "Get NetApp Files Backup Vault Details",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    LocalRequired = false,
    Secret = false
)]
public sealed class BackupVaultGetCommand(ILogger<BackupVaultGetCommand> logger, INetAppFilesService netAppFilesService) : SubscriptionCommand<BackupVaultGetOptions>()
{
    private readonly ILogger<BackupVaultGetCommand> _logger = logger;

    private readonly INetAppFilesService _netAppFilesService = netAppFilesService;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(NetAppFilesOptionDefinitions.Account.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.BackupVault.AsOptional());
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.Ids.AsOptional());
    }

    protected override BackupVaultGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Account = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.Account.Name);
        options.BackupVault = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.BackupVault.Name);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.Ids = parseResult.GetValueOrDefault<string[]>(NetAppFilesOptionDefinitions.Ids.Name);
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
            var backupVaults = await _netAppFilesService.GetBackupVaultDetails(
                options.Account,
                options.BackupVault,
                options.ResourceGroup,
                options.Ids,
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
