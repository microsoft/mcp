// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Options;
using Azure.Mcp.Tools.AzureBackup.Options.Vault;
using Azure.Mcp.Tools.AzureBackup.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.AzureBackup.Commands.Vault;

public sealed class VaultUpdateCommand(ILogger<VaultUpdateCommand> logger, IAzureBackupService azureBackupService) : BaseAzureBackupCommand<VaultUpdateOptions>()
{
    private const string CommandTitle = "Update Backup Vault";
    private readonly ILogger<VaultUpdateCommand> _logger = logger;
    private readonly IAzureBackupService _azureBackupService = azureBackupService;

    public override string Id => "da7f163e-471c-4d7d-ae00-d41f5f4b939e";
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
        command.Validators.Add(commandResult =>
        {
            bool hasUpdate =
                commandResult.HasOptionResult(AzureBackupOptionDefinitions.Redundancy.Name) ||
                commandResult.HasOptionResult(AzureBackupOptionDefinitions.SoftDelete.Name) ||
                commandResult.HasOptionResult(AzureBackupOptionDefinitions.SoftDeleteRetentionDays.Name) ||
                commandResult.HasOptionResult(AzureBackupOptionDefinitions.ImmutabilityState.Name) ||
                commandResult.HasOptionResult(AzureBackupOptionDefinitions.IdentityType.Name) ||
                commandResult.HasOptionResult(AzureBackupOptionDefinitions.Tags.Name);

            if (!hasUpdate)
            {
                commandResult.AddError(
                    "At least one update option must be provided: --redundancy, --soft-delete, --soft-delete-retention-days, --immutability-state, --identity-type, or --tags.");
            }

            if (commandResult.HasOptionResult(AzureBackupOptionDefinitions.SoftDeleteRetentionDays.Name))
            {
                var retentionValue = commandResult.GetValue<string>(AzureBackupOptionDefinitions.SoftDeleteRetentionDays.Name);
                if (!string.IsNullOrEmpty(retentionValue))
                {
                    if (!int.TryParse(retentionValue, out var retentionDays) || retentionDays < 14 || retentionDays > 180)
                    {
                        commandResult.AddError("--soft-delete-retention-days must be an integer between 14 and 180.");
                    }
                }
            }

            if (commandResult.HasOptionResult(AzureBackupOptionDefinitions.IdentityType.Name))
            {
                var identityValue = commandResult.GetValue<string>(AzureBackupOptionDefinitions.IdentityType.Name);
                if (!string.IsNullOrEmpty(identityValue) &&
                    !identityValue.Equals("SystemAssigned", StringComparison.OrdinalIgnoreCase) &&
                    !identityValue.Equals("UserAssigned", StringComparison.OrdinalIgnoreCase) &&
                    !identityValue.Equals("None", StringComparison.OrdinalIgnoreCase) &&
                    !identityValue.Equals("SystemAssigned,UserAssigned", StringComparison.OrdinalIgnoreCase))
                {
                    commandResult.AddError("--identity-type must be 'SystemAssigned', 'UserAssigned', 'SystemAssigned,UserAssigned', or 'None'.");
                }
            }
        });
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
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            var result = await _azureBackupService.UpdateVaultAsync(
                options.Vault!,
                options.ResourceGroup!,
                options.Subscription!,
                options.VaultType,
                options.Redundancy,
                options.SoftDeleteState,
                options.SoftDeleteRetentionDays,
                options.ImmutabilityState,
                options.IdentityType,
                options.Tags,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new(result),
                AzureBackupJsonContext.Default.VaultUpdateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating vault. Vault: {Vault}, ResourceGroup: {ResourceGroup}",
                options.Vault, options.ResourceGroup);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        ArgumentException argEx => argEx.Message,
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Vault not found. Verify the vault name and resource group.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            "Update conflict. The vault settings may have been modified concurrently.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed updating the vault. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    internal record VaultUpdateCommandResult(OperationResult Result);
}
