// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Options;
using Azure.Mcp.Tools.AzureBackup.Options.Vault;
using Azure.Mcp.Tools.AzureBackup.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.AzureBackup.Commands.Vault;

public sealed class VaultUpdateCommand(ILogger<VaultUpdateCommand> logger) : BaseAzureBackupCommand<VaultUpdateOptions>()
{
    private const string CommandTitle = "Update Backup Vault";
    private readonly ILogger<VaultUpdateCommand> _logger = logger;

    public override string Id => "b1a2c3d4-e5f6-7890-abcd-ef12345678a0";
    public override string Name => "update";
    public override string Description => "Updates vault-level settings including storage redundancy, soft delete, immutability, and managed identity.";
    public override string Title => CommandTitle;
    public override ToolMetadata Metadata => new() { Destructive = true, Idempotent = true, OpenWorld = false, ReadOnly = false, LocalRequired = false, Secret = false };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(AzureBackupOptionDefinitions.Redundancy);
        command.Options.Add(AzureBackupOptionDefinitions.SoftDelete);
        command.Options.Add(AzureBackupOptionDefinitions.SoftDeleteRetentionDays);
        command.Options.Add(AzureBackupOptionDefinitions.ImmutabilityState);
        command.Options.Add(AzureBackupOptionDefinitions.IdentityType);
        command.Options.Add(AzureBackupOptionDefinitions.Tags);
    }

    protected override VaultUpdateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Redundancy = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.Redundancy.Name);
        options.SoftDeleteState = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.SoftDelete.Name);
        options.SoftDeleteRetentionDays = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.SoftDeleteRetentionDays.Name);
        options.ImmutabilityState = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.ImmutabilityState.Name);
        options.IdentityType = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.IdentityType.Name);
        options.Tags = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.Tags.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            return context.Response;
        var options = BindOptions(parseResult);
        try
        {
            var service = context.GetService<IAzureBackupService>();
            var result = await service.UpdateVaultAsync(options.Vault!, options.ResourceGroup!, options.Subscription!, options.VaultType, options.Redundancy, options.SoftDeleteState, options.SoftDeleteRetentionDays, options.ImmutabilityState, options.IdentityType, options.Tags, options.Tenant, options.RetryPolicy, cancellationToken);
            context.Response.Results = ResponseResult.Create(new VaultUpdateCommandResult(result), AzureBackupJsonContext.Default.VaultUpdateCommandResult);
        }
        catch (Exception ex) { _logger.LogError(ex, "Error updating vault"); HandleException(context, ex); }
        return context.Response;
    }

    internal record VaultUpdateCommandResult([property: JsonPropertyName("result")] OperationResult Result);
}
