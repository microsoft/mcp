// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Microsoft.Mcp.Core.Extensions;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.Mcp.Tools.NetAppFiles.Options;
using Azure.Mcp.Tools.NetAppFiles.Options.Backup;
using Azure.Mcp.Tools.NetAppFiles.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.NetAppFiles.Commands.Backup;

[CommandMetadata(
    Id = "b2d4f6a8-0c1e-3b5d-7f9a-c4e6d8f0a2b4",
    Name = "get",
    Description =
        """
        Retrieves detailed information about Azure NetApp Files backups, including backup name, location, resource group, provisioning state, backup type, size, label, and creation date. If a specific backup name is not provided, the command will return details for all backups in a subscription. Optionally filter by account and backup vault.
        """,
    Title = "Get NetApp Files Backup Details",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    LocalRequired = false,
    Secret = false
)]
public sealed class BackupGetCommand(ILogger<BackupGetCommand> logger, INetAppFilesService netAppFilesService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<BackupGetOptions, BackupGetCommand.BackupGetCommandResult>(subscriptionResolver)
{
    private readonly ILogger<BackupGetCommand> _logger = logger;

    private readonly INetAppFilesService _netAppFilesService = netAppFilesService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, BackupGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var backups = await _netAppFilesService.GetBackupDetails(
                options.Account,
                options.BackupVault,
                options.Backup,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new BackupGetCommandResult(backups?.Results ?? [], backups?.AreResultsTruncated ?? false),
                NetAppFilesJsonContext.Default.BackupGetCommandResult);
        }
        catch (Exception ex)
        {
            if (options.Backup is null)
            {
                _logger.LogError(ex, "Error listing NetApp Files backup details. Subscription: {Subscription}, Options: {@Options}", options.Subscription, options);
            }
            else
            {
                _logger.LogError(ex, "Error getting NetApp Files backup details. Backup: {Backup}, Subscription: {Subscription}, Options: {@Options}",
                    options.Backup, options.Subscription, options);
            }

            HandleException(context, ex);
        }

        return context.Response;
    }

    public record BackupGetCommandResult(List<BackupInfo> Backups, bool AreResultsTruncated);
}
