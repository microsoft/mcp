// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Options;
using Azure.Mcp.Tools.AzureBackup.Options.Backup;
using Azure.Mcp.Tools.AzureBackup.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.AzureBackup.Commands.Backup;

public sealed class BackupStatusCommand(ILogger<BackupStatusCommand> logger) : SubscriptionCommand<BackupStatusOptions>()
{
    private const string CommandTitle = "Check Backup Status";
    private readonly ILogger<BackupStatusCommand> _logger = logger;

    public override string Id => "b1a2c3d4-e5f6-7890-abcd-ef12345678b5";
    public override string Name => "status";
    public override string Description => "Checks whether a datasource is protected and returns vault and policy details.";
    public override string Title => CommandTitle;
    public override ToolMetadata Metadata => new() { Destructive = false, Idempotent = true, OpenWorld = false, ReadOnly = true, LocalRequired = false, Secret = false };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(AzureBackupOptionDefinitions.DatasourceId);
        command.Options.Add(AzureBackupOptionDefinitions.Location);
    }

    protected override BackupStatusOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.DatasourceId = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.DatasourceId.Name);
        options.Location = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.Location.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid) return context.Response;
        var options = BindOptions(parseResult);
        try
        {
            var service = context.GetService<IAzureBackupService>();
            var result = await service.GetBackupStatusAsync(options.DatasourceId!, options.Subscription!, options.Location!, options.Tenant, options.RetryPolicy, cancellationToken);
            context.Response.Results = ResponseResult.Create(new BackupStatusCommandResult(result), AzureBackupJsonContext.Default.BackupStatusCommandResult);
        }
        catch (Exception ex) { _logger.LogError(ex, "Error checking backup status"); HandleException(context, ex); }
        return context.Response;
    }

    internal record BackupStatusCommandResult([property: JsonPropertyName("status")] BackupStatusResult Status);
}
