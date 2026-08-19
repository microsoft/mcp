// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Options.Security;
using Azure.Mcp.Tools.AzureBackup.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.AzureBackup.Commands.Security;

[CommandMetadata(
    Id = "c3a21f68-9b5e-4d1a-bf3c-7e2a0f8d4b19",
    Name = "configure-mua",
    Title = "Configure Multi-User Authorization",
    Description = """
        Enables Multi-User Authorization (MUA) on a vault by linking a Resource Guard. --resource-guard-id
        is required. Once enabled, critical operations (disable soft delete, remove immutability, stop
        protection) require approval from a security admin with permissions on the Resource Guard.
        To disable MUA, use the 'security disable-mua' command (requires --force).
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class SecurityConfigureMuaCommand(ILogger<SecurityConfigureMuaCommand> logger, IAzureBackupService azureBackupService, ISubscriptionResolver subscriptionResolver)
    : BaseAzureBackupCommand<SecurityConfigureMuaOptions, SecurityConfigureMuaCommand.SecurityConfigureMuaCommandResult>(subscriptionResolver)
{
    private readonly ILogger<SecurityConfigureMuaCommand> _logger = logger;
    private readonly IAzureBackupService _azureBackupService = azureBackupService;

    public override void ValidateOptions(SecurityConfigureMuaOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (string.IsNullOrWhiteSpace(options.ResourceGuardId))
        {
            validationResult.Errors.Add("--resource-guard-id is required to enable Multi-User Authorization. To disable MUA on a vault, use the 'security disable-mua' command (which requires --force).");
            return;
        }

        if (!options.ResourceGuardId.StartsWith("/subscriptions/", StringComparison.OrdinalIgnoreCase))
        {
            validationResult.Errors.Add("--resource-guard-id must be a valid ARM resource ID starting with '/subscriptions/'.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, SecurityConfigureMuaOptions options, CancellationToken cancellationToken)
    {
        AzureBackupTelemetryTags.AddSubscriptionTag(context.Activity, options.Subscription);
        AzureBackupTelemetryTags.AddVaultTags(context.Activity, options.VaultType);
        AzureBackupTelemetryTags.AddMuaActionTag(context.Activity, "enable");

        try
        {
            var result = await _azureBackupService.ConfigureMultiUserAuthorizationAsync(
                options.Vault,
                options.ResourceGroup,
                options.Subscription!,
                options.ResourceGuardId!,
                options.VaultType,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new(result),
                AzureBackupJsonContext.Default.SecurityConfigureMuaCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enabling MUA. Vault: {Vault}, ResourceGuardId: {ResourceGuardId}",
                options.Vault, options.ResourceGuardId);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        ArgumentException argEx => argEx.Message,
        UnauthorizedAccessException => "Authorization failed. Verify your RBAC permissions on the vault, or specify --vault-type to skip auto-detection.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Vault or Resource Guard not found. Verify the vault name, resource group, and Resource Guard ID.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.BadRequest =>
            $"Bad request configuring MUA. Ensure the Resource Guard is in the same region as the vault. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed. Enabling MUA requires Reader role on the Resource Guard and Backup Contributor role on the vault. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            "MUA configuration conflict. The vault may already have a different Resource Guard linked. Disable the existing link with 'security disable-mua --force' before configuring a new one.",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        UnauthorizedAccessException => HttpStatusCode.Forbidden,
        ArgumentException or FormatException => HttpStatusCode.BadRequest,
        RequestFailedException reqEx => (HttpStatusCode)reqEx.Status,
        _ => base.GetStatusCode(ex)
    };

    public sealed record SecurityConfigureMuaCommandResult(OperationResult Result);
}
