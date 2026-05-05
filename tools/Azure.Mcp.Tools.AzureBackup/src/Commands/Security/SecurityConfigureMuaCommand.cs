// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Options;
using Azure.Mcp.Tools.AzureBackup.Options.Security;
using Azure.Mcp.Tools.AzureBackup.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.AzureBackup.Commands.Security;

[CommandMetadata(
    Id = "c3a21f68-9b5e-4d1a-bf3c-7e2a0f8d4b19",
    Name = "configure-mua",
    Title = "Configure Multi-User Authorization",
    Description = """
        Configures Multi-User Authorization (MUA) on a vault by linking or unlinking a Resource Guard.
        Provide --resource-guard-id to enable MUA, protecting critical operations (disable soft delete,
        remove immutability, stop protection) so they require approval from a security admin with
        permissions on the Resource Guard. Omit --resource-guard-id to disable MUA (this itself is a
        protected operation requiring Backup MUA Operator role on the Resource Guard).
        """,
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class SecurityConfigureMuaCommand(ILogger<SecurityConfigureMuaCommand> logger, IAzureBackupService azureBackupService) : BaseAzureBackupCommand<SecurityConfigureMuaOptions>()
{
    private readonly ILogger<SecurityConfigureMuaCommand> _logger = logger;
    private readonly IAzureBackupService _azureBackupService = azureBackupService;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(AzureBackupOptionDefinitions.ResourceGuardId);
        command.Validators.Add(commandResult =>
        {
            if (commandResult.HasOptionResult(AzureBackupOptionDefinitions.ResourceGuardId.Name))
            {
                var value = commandResult.GetValue<string>(AzureBackupOptionDefinitions.ResourceGuardId.Name);
                if (!string.IsNullOrEmpty(value) &&
                    !value.StartsWith("/subscriptions/", StringComparison.OrdinalIgnoreCase))
                {
                    commandResult.AddError("--resource-guard-id must be a valid ARM resource ID starting with '/subscriptions/'.");
                }
            }
        });
    }

    protected override SecurityConfigureMuaOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGuardId = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.ResourceGuardId.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        AzureBackupTelemetryTags.AddVaultTags(context.Activity, options.VaultType);

        try
        {
            OperationResult result;

            if (!string.IsNullOrEmpty(options.ResourceGuardId))
            {
                result = await _azureBackupService.ConfigureMultiUserAuthorizationAsync(
                    options.Vault!,
                    options.ResourceGroup!,
                    options.Subscription!,
                    options.ResourceGuardId,
                    options.VaultType,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);
            }
            else
            {
                result = await _azureBackupService.DisableMultiUserAuthorizationAsync(
                    options.Vault!,
                    options.ResourceGroup!,
                    options.Subscription!,
                    options.VaultType,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);
            }

            context.Response.Results = ResponseResult.Create(
                new(result),
                AzureBackupJsonContext.Default.SecurityConfigureMuaCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error configuring MUA. Vault: {Vault}, ResourceGuardId: {ResourceGuardId}",
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
            "Vault or Resource Guard not found, or MUA is not enabled for this vault. Verify the vault name, resource group, and Resource Guard ID. If you are disabling MUA, ensure MUA is currently configured.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.BadRequest =>
            $"Bad request configuring MUA. Ensure the Resource Guard is in the same region as the vault. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed. To enable MUA, you need Reader role on the Resource Guard. To disable MUA, you need Backup MUA Operator role on the Resource Guard. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            "MUA configuration conflict. The vault may already have a Resource Guard linked, or the operation is blocked by the current MUA configuration.",
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

    internal record SecurityConfigureMuaCommandResult(OperationResult Result);
}
