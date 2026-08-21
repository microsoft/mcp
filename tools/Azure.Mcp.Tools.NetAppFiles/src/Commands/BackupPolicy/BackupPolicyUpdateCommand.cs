// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Core.Commands.Subscription;
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
    Id = "d8f4a2c6-5e3b-4d7a-b1f9-e2c6d3a8f5b7",
    Name = "update",
    Description =
        """
        Updates an existing Azure NetApp Files backup policy in a specified account and resource group, and returns the updated backup policy details including name, location, resource group, provisioning state, and backup retention settings. Supports updating daily, weekly, and monthly backup retention counts, enabled state, and tags. Requires account name, backup policy name, resource group, location, and subscription.
        """,
    Title = "Update NetApp Files Backup Policy",
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    LocalRequired = false,
    Secret = false
)]
public sealed class BackupPolicyUpdateCommand(ILogger<BackupPolicyUpdateCommand> logger, INetAppFilesService netAppFilesService) : SubscriptionCommand<BackupPolicyUpdateOptions>()
{
    private readonly ILogger<BackupPolicyUpdateCommand> _logger = logger;

    private readonly INetAppFilesService _netAppFilesService = netAppFilesService;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(NetAppFilesOptionDefinitions.Account.AsRequired());
        command.Options.Add(NetAppFilesOptionDefinitions.BackupPolicy.AsRequired());
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsRequired());
        command.Options.Add(NetAppFilesOptionDefinitions.Location);
        command.Options.Add(NetAppFilesOptionDefinitions.DailyBackupsToKeep);
        command.Options.Add(NetAppFilesOptionDefinitions.WeeklyBackupsToKeep);
        command.Options.Add(NetAppFilesOptionDefinitions.MonthlyBackupsToKeep);
        command.Options.Add(NetAppFilesOptionDefinitions.Enabled.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.Tags.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.Ids.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.NoWait.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.Add.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.Set.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.Remove.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.ForceString.AsOptional());
    }

    protected override BackupPolicyUpdateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Account = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.Account.Name);
        options.BackupPolicy = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.BackupPolicy.Name);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.Location = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.Location.Name);
        options.DailyBackupsToKeep = parseResult.GetValueOrDefault<int?>(NetAppFilesOptionDefinitions.DailyBackupsToKeep.Name);
        options.WeeklyBackupsToKeep = parseResult.GetValueOrDefault<int?>(NetAppFilesOptionDefinitions.WeeklyBackupsToKeep.Name);
        options.MonthlyBackupsToKeep = parseResult.GetValueOrDefault<int?>(NetAppFilesOptionDefinitions.MonthlyBackupsToKeep.Name);
        options.Enabled = parseResult.GetValueOrDefault<bool?>(NetAppFilesOptionDefinitions.Enabled.Name);
        options.Tags = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.Tags.Name);
        options.Ids = parseResult.GetValueOrDefault<string[]>(NetAppFilesOptionDefinitions.Ids.Name);
        options.NoWait = parseResult.GetValueOrDefault<bool>(NetAppFilesOptionDefinitions.NoWait.Name);
        options.Add = parseResult.GetValueOrDefault<string[]>(NetAppFilesOptionDefinitions.Add.Name);
        options.Set = parseResult.GetValueOrDefault<string[]>(NetAppFilesOptionDefinitions.Set.Name);
        options.Remove = parseResult.GetValueOrDefault<string[]>(NetAppFilesOptionDefinitions.Remove.Name);
        options.ForceString = parseResult.GetValueOrDefault<bool>(NetAppFilesOptionDefinitions.ForceString.Name);
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
            ValidateUnsupportedUpdateArguments(options);

            Dictionary<string, string>? tags = null;
            if (!string.IsNullOrEmpty(options.Tags))
            {
                try
                {
                    tags = JsonSerializer.Deserialize(options.Tags, NetAppFilesJsonContext.Default.DictionaryStringString);
                }
                catch (JsonException ex)
                {
                    throw new ArgumentException($"Invalid tags JSON format: {ex.Message}", nameof(options.Tags));
                }
            }

            var backupPolicy = await _netAppFilesService.UpdateBackupPolicy(
                options.Account!,
                options.BackupPolicy!,
                options.ResourceGroup!,
                options.Location!,
                options.Subscription!,
                options.DailyBackupsToKeep,
                options.WeeklyBackupsToKeep,
                options.MonthlyBackupsToKeep,
                options.Enabled,
                tags,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new BackupPolicyUpdateCommandResult(backupPolicy),
                NetAppFilesJsonContext.Default.BackupPolicyUpdateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error updating NetApp Files backup policy. Account: {Account}, BackupPolicy: {BackupPolicy}, ResourceGroup: {ResourceGroup}, Options: {@Options}",
                options.Account, options.BackupPolicy, options.ResourceGroup, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        ArgumentException argEx => argEx.Message,
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            "A backup policy with this name already exists. Choose a different name.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed updating the backup policy. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Backup policy, account, or resource group not found. Verify they exist and you have access.",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    private static void ValidateUnsupportedUpdateArguments(BackupPolicyUpdateOptions options)
    {
        if (options.Ids is { Length: > 0 })
        {
            throw new ArgumentException("The --ids argument is not supported by this command yet.");
        }

        if (options.NoWait)
        {
            throw new ArgumentException("The --no-wait argument is not supported by this command yet.");
        }

        if (options.Add is { Length: > 0 })
        {
            throw new ArgumentException("The --add argument is not supported by this command yet.");
        }

        if (options.Set is { Length: > 0 })
        {
            throw new ArgumentException("The --set argument is not supported by this command yet.");
        }

        if (options.Remove is { Length: > 0 })
        {
            throw new ArgumentException("The --remove argument is not supported by this command yet.");
        }

        if (options.ForceString)
        {
            throw new ArgumentException("The --force-string argument is not supported by this command yet.");
        }
    }

    internal record BackupPolicyUpdateCommandResult([property: JsonPropertyName("backupPolicy")] BackupPolicyCreateResult BackupPolicy);
}
