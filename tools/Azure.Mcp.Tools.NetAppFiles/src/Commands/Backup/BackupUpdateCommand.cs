// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json.Serialization;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.Mcp.Tools.NetAppFiles.Options;
using Azure.Mcp.Tools.NetAppFiles.Options.Backup;
using Azure.Mcp.Tools.NetAppFiles.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.NetAppFiles.Commands.Backup;

public sealed class BackupUpdateCommand(ILogger<BackupUpdateCommand> logger) : SubscriptionCommand<BackupUpdateOptions>()
{
    private const string CommandTitle = "Update NetApp Files Backup";

    private readonly ILogger<BackupUpdateCommand> _logger = logger;

    public override string Id => "e7b3a1d5-9c4f-4e8a-b2d6-f1a5c3e7d9b4";

    public override string Name => "update";

    public override string Description =>
        """
        Updates an existing Azure NetApp Files backup in a specified backup vault under a NetApp account, and returns the updated backup details including name, location, resource group, provisioning state, volume resource ID, label, and backup type. Supports updating the backup label. Requires account name, backup vault name, backup name, resource group, location, and subscription. Optionally accepts a label.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = true,
        Idempotent = true,
        OpenWorld = false,
        ReadOnly = false,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(NetAppFilesOptionDefinitions.Account.AsRequired());
        command.Options.Add(NetAppFilesOptionDefinitions.BackupVault.AsRequired());
        command.Options.Add(NetAppFilesOptionDefinitions.Backup.AsRequired());
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsRequired());
        command.Options.Add(NetAppFilesOptionDefinitions.Location);
        command.Options.Add(NetAppFilesOptionDefinitions.Label.AsOptional());
    }

    protected override BackupUpdateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Account = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.Account.Name);
        options.BackupVault = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.BackupVault.Name);
        options.Backup = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.Backup.Name);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.Location = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.Location.Name);
        options.Label = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.Label.Name);
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

            var backup = await netAppFilesService.UpdateBackup(
                options.Account!,
                options.BackupVault!,
                options.Backup!,
                options.ResourceGroup!,
                options.Location!,
                options.Subscription!,
                options.Label,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new BackupUpdateCommandResult(backup),
                NetAppFilesJsonContext.Default.BackupUpdateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error updating NetApp Files backup. Account: {Account}, BackupVault: {BackupVault}, Backup: {Backup}, ResourceGroup: {ResourceGroup}, Options: {@Options}",
                options.Account, options.BackupVault, options.Backup, options.ResourceGroup, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            "A backup with this name already exists. Choose a different name.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed updating the backup. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Backup, account, backup vault, or resource group not found. Verify they exist and you have access.",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    internal record BackupUpdateCommandResult([property: JsonPropertyName("backup")] BackupCreateResult Backup);
}
