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
    Id = "b5f9c1a2-4d3e-4a7b-91e2-8a6c0d3f5e91",
    Name = "disable-mua",
    Title = "Disable Multi-User Authorization",
    Description = """
        Disables Multi-User Authorization (MUA) on a vault by unlinking its Resource Guard.
        This is a destructive operation: it removes protection from critical operations (disable
        soft delete, remove immutability, stop protection). The caller needs the Backup MUA Operator
        role on the linked Resource Guard because the unlink itself is a MUA-protected operation.
        """,
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class SecurityDisableMuaCommand(ILogger<SecurityDisableMuaCommand> logger, IAzureBackupService azureBackupService, ISubscriptionResolver subscriptionResolver)
    : BaseAzureBackupCommand<SecurityDisableMuaOptions, SecurityDisableMuaCommand.SecurityDisableMuaCommandResult>(subscriptionResolver)
{
    private readonly ILogger<SecurityDisableMuaCommand> _logger = logger;
    private readonly IAzureBackupService _azureBackupService = azureBackupService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, SecurityDisableMuaOptions options, CancellationToken cancellationToken)
    {
        AzureBackupTelemetryTags.AddSubscriptionTag(context.Activity, options.Subscription);
        AzureBackupTelemetryTags.AddVaultTags(context.Activity, options.VaultType);
        AzureBackupTelemetryTags.AddMuaActionTag(context.Activity, "disable");

        try
        {
            var result = await _azureBackupService.DisableMultiUserAuthorizationAsync(
                options.Vault,
                options.ResourceGroup,
                options.Subscription!,
                options.VaultType,
                options.Tenant,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new(result),
                AzureBackupJsonContext.Default.SecurityDisableMuaCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disabling MUA. Vault: {Vault}", options.Vault);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        ArgumentException argEx => argEx.Message,
        UnauthorizedAccessException => "Authorization failed. Verify your RBAC permissions on the vault, or specify --vault-type to skip auto-detection.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Vault not found, or MUA is not enabled for this vault. Verify the vault name and resource group. If MUA is not configured on this vault there is nothing to disable.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed. Disabling MUA requires the Backup MUA Operator role on the linked Resource Guard. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    public sealed record SecurityDisableMuaCommandResult(OperationResult Result);
}
