// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Options.Governance;
using Azure.Mcp.Tools.AzureBackup.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.AzureBackup.Commands.Governance;

[CommandMetadata(
    Id = "b3f1ea2d-5535-4155-849c-61f2fc49f1d9",
    Name = "soft-delete",
    Title = "Configure Soft Delete",
    Description = """
        Configures the soft delete settings for a backup vault. Set the state to 'AlwaysOn', 'On',
        or 'Off', and optionally specify the retention period in days (14-180).
        """,
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class GovernanceSoftDeleteCommand(ILogger<GovernanceSoftDeleteCommand> logger, IAzureBackupService azureBackupService, ISubscriptionResolver subscriptionResolver)
    : BaseAzureBackupCommand<GovernanceSoftDeleteOptions, GovernanceSoftDeleteCommand.GovernanceSoftDeleteCommandResult>(subscriptionResolver)
{
    private readonly ILogger<GovernanceSoftDeleteCommand> _logger = logger;
    private readonly IAzureBackupService _azureBackupService = azureBackupService;

    public override void ValidateOptions(GovernanceSoftDeleteOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (!string.IsNullOrEmpty(options.SoftDelete) &&
            !options.SoftDelete.Equals("AlwaysOn", StringComparison.OrdinalIgnoreCase) &&
            !options.SoftDelete.Equals("On", StringComparison.OrdinalIgnoreCase) &&
            !options.SoftDelete.Equals("Off", StringComparison.OrdinalIgnoreCase))
        {
            validationResult.Errors.Add("--soft-delete must be 'AlwaysOn', 'On', or 'Off'.");
        }

        if (!string.IsNullOrEmpty(options.SoftDeleteRetentionDays) &&
            (!int.TryParse(options.SoftDeleteRetentionDays, out var retentionDays)
                || retentionDays < 14
                || retentionDays > 180))
        {
            validationResult.Errors.Add("--soft-delete-retention-days must be an integer between 14 and 180.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, GovernanceSoftDeleteOptions options, CancellationToken cancellationToken)
    {
        AzureBackupTelemetryTags.AddSubscriptionTag(context.Activity, options.Subscription);
        AzureBackupTelemetryTags.AddVaultTags(context.Activity, options.VaultType);

        try
        {
            var result = await _azureBackupService.ConfigureSoftDeleteAsync(
                options.Vault,
                options.ResourceGroup,
                options.Subscription!,
                options.SoftDelete,
                options.VaultType,
                options.SoftDeleteRetentionDays,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new(result),
                AzureBackupJsonContext.Default.GovernanceSoftDeleteCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error configuring soft delete. Vault: {Vault}, State: {SoftDeleteState}",
                options.Vault, options.SoftDelete);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record GovernanceSoftDeleteCommandResult(OperationResult Result);
}
