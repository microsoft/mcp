// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.Mcp.Tools.NetAppFiles.Options;
using Azure.Mcp.Tools.NetAppFiles.Options.Backup;
using Azure.Mcp.Tools.NetAppFiles.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.NetAppFiles.Commands.Backup;

public sealed class BackupGetCommand(ILogger<BackupGetCommand> logger) : SubscriptionCommand<BackupGetOptions>()
{
    private const string CommandTitle = "Get NetApp Files Backup Details";

    private readonly ILogger<BackupGetCommand> _logger = logger;

    public override string Id => "b2d4f6a8-0c1e-3b5d-7f9a-c4e6d8f0a2b4";
    public override string Name => "get";
    public override string Description =>
        """
        Retrieves detailed information about Azure NetApp Files backups, including backup name, location, resource group, provisioning state, backup type, size, label, and creation date. If a specific backup name is not provided, the command will return details for all backups in a subscription. Optionally filter by account and backup vault.
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
        command.Options.Add(NetAppFilesOptionDefinitions.Backup.AsOptional());
    }

    protected override BackupGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Account = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.Account.Name);
        options.BackupVault = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.BackupVault.Name);
        options.Backup = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.Backup.Name);
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

            var backups = await netAppFilesService.GetBackupDetails(
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

    internal record BackupGetCommandResult(List<BackupInfo> Backups, bool AreResultsTruncated);
}
