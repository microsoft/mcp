// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Microsoft.Mcp.Core.Extensions;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.Mcp.Tools.NetAppFiles.Options;
using Azure.Mcp.Tools.NetAppFiles.Options.BackupPolicy;
using Azure.Mcp.Tools.NetAppFiles.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.NetAppFiles.Commands.BackupPolicy;

[CommandMetadata(
    Id = "b8d4f2c5-6e3a-4b9f-c7d8-e0f1a2b3c4d5",
    Name = "get",
    Description =
        """
        Retrieves detailed information about Azure NetApp Files backup policies, including policy name, location, resource group, provisioning state, daily/weekly/monthly backups to keep, volume backups count, and enabled state. If a specific backup policy name is not provided, the command will return details for all backup policies in a subscription. Optionally filter by account, resource group, or resource IDs.
        """,
    Title = "Get NetApp Files Backup Policy Details",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    LocalRequired = false,
    Secret = false
)]
public sealed class BackupPolicyGetCommand(ILogger<BackupPolicyGetCommand> logger, INetAppFilesService netAppFilesService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<BackupPolicyGetOptions, BackupPolicyGetCommand.BackupPolicyGetCommandResult>(subscriptionResolver)
{
    private readonly ILogger<BackupPolicyGetCommand> _logger = logger;

    private readonly INetAppFilesService _netAppFilesService = netAppFilesService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, BackupPolicyGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var backupPolicies = await _netAppFilesService.GetBackupPolicyDetails(
                options.Account,
                options.BackupPolicy,
                options.ResourceGroup,
                options.Ids,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new BackupPolicyGetCommandResult(backupPolicies?.Results ?? [], backupPolicies?.AreResultsTruncated ?? false),
                NetAppFilesJsonContext.Default.BackupPolicyGetCommandResult);
        }
        catch (Exception ex)
        {
            if (options.BackupPolicy is null)
            {
                _logger.LogError(ex, "Error listing NetApp Files backup policy details. Subscription: {Subscription}, Options: {@Options}", options.Subscription, options);
            }
            else
            {
                _logger.LogError(ex, "Error getting NetApp Files backup policy details. BackupPolicy: {BackupPolicy}, Subscription: {Subscription}, Options: {@Options}",
                    options.BackupPolicy, options.Subscription, options);
            }
            HandleException(context, ex);
        }

        return context.Response;
    }

    public record BackupPolicyGetCommandResult(List<BackupPolicyInfo> BackupPolicies, bool AreResultsTruncated);
}
