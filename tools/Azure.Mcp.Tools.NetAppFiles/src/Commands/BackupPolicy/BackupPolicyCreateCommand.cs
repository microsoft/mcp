// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json.Serialization;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.Mcp.Tools.NetAppFiles.Options;
using Azure.Mcp.Tools.NetAppFiles.Options.BackupPolicy;
using Azure.Mcp.Tools.NetAppFiles.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.NetAppFiles.Commands.BackupPolicy;

public sealed class BackupPolicyCreateCommand(ILogger<BackupPolicyCreateCommand> logger) : SubscriptionCommand<BackupPolicyCreateOptions>()
{
    private const string CommandTitle = "Create NetApp Files Backup Policy";

    private readonly ILogger<BackupPolicyCreateCommand> _logger = logger;

    public override string Id => "c9e5f3b7-7d4a-4c8e-a2f6-d1b3e4f5a6c8";

    public override string Name => "create";

    public override string Description =>
        """
        Creates an Azure NetApp Files backup policy in a specified account and resource group, and returns the created backup policy details including name, location, resource group, provisioning state, and backup retention settings. Requires account name, backup policy name, resource group, location, and subscription.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = true,
        Idempotent = false,
        OpenWorld = false,
        ReadOnly = false,
        LocalRequired = false,
        Secret = false
    };

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
    }

    protected override BackupPolicyCreateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Account = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.Account.Name);
        options.BackupPolicy = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.BackupPolicy.Name);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.Location = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.Location.Name);
        options.DailyBackupsToKeep = parseResult.GetValueOrDefault<int?>(NetAppFilesOptionDefinitions.DailyBackupsToKeep.Name);
        options.WeeklyBackupsToKeep = parseResult.GetValueOrDefault<int?>(NetAppFilesOptionDefinitions.WeeklyBackupsToKeep.Name);
        options.MonthlyBackupsToKeep = parseResult.GetValueOrDefault<int?>(NetAppFilesOptionDefinitions.MonthlyBackupsToKeep.Name);
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

            var backupPolicy = await netAppFilesService.CreateBackupPolicy(
                options.Account!,
                options.BackupPolicy!,
                options.ResourceGroup!,
                options.Location!,
                options.Subscription!,
                options.DailyBackupsToKeep,
                options.WeeklyBackupsToKeep,
                options.MonthlyBackupsToKeep,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new(backupPolicy),
                NetAppFilesJsonContext.Default.BackupPolicyCreateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error creating NetApp Files backup policy. Account: {Account}, BackupPolicy: {BackupPolicy}, ResourceGroup: {ResourceGroup}, Options: {@Options}",
                options.Account, options.BackupPolicy, options.ResourceGroup, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            "A backup policy with this name already exists. Choose a different name.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed creating the backup policy. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Account or resource group not found. Verify they exist and you have access.",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    internal record BackupPolicyCreateCommandResult([property: JsonPropertyName("backupPolicy")] BackupPolicyCreateResult BackupPolicy);
}
