// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Options.Operation;
using Azure.Mcp.Tools.AzureBackup.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.AzureBackup.Commands.Operation;

[CommandMetadata(
    Id = "29a73ac9-f8ea-45ed-ae6c-ddbd9d9ec713",
    Name = "get",
    Title = "Get RSV Backup Operation Status",
    Description = """
        Gets asynchronous operation status for a Recovery Services vault (RSV only; not DPP).
        Requires --operation, --vault, --resource-group and --subscription. Reads vault
        backupOperations/{id}, or protected-item operationsStatus/{id} when --container and
        --protected-item are supplied together (--fabric defaults to Azure). Returns operation
        identity, scope, status, timestamps, safe error codes and actual jobId/jobIds when Azure
        supplies them. An operation ID is not a job ID; no job is a valid result. Use job get only
        for returned job IDs. Accepts an opaque operation ID, never a URL. Does not poll or modify backup.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = false, Idempotent = true, OpenWorld = false,
    ReadOnly = true, Secret = false, LocalRequired = false)]
public sealed class OperationGetCommand(
    ILogger<OperationGetCommand> logger,
    IRsvBackupOperationService service,
    ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<OperationGetOptions, OperationGetCommand.OperationGetCommandResult>(subscriptionResolver)
{
    public override void ValidateOptions(OperationGetOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        validationResult.Errors.AddRange(RsvOperationInputValidator.Validate(
            options.Operation, options.Vault, options.ResourceGroup, options.Container, options.ProtectedItem, options.Fabric));
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, OperationGetOptions options, CancellationToken cancellationToken)
    {
        AzureBackupTelemetryTags.AddSubscriptionTag(context.Activity, options.Subscription);
        AzureBackupTelemetryTags.AddVaultTags(context.Activity, "rsv");
        context.Activity?.AddTag(AzureBackupTelemetryTags.OperationScope, options.Container is null ? "vault" : "protectedItem");
        try
        {
            var result = await service.GetOperationAsync(options.Operation, options.Vault, options.ResourceGroup,
                options.Subscription, options.Container, options.ProtectedItem, options.Fabric, options.Tenant, cancellationToken);
            context.Response.Results = ResponseResult.Create(new(result), AzureBackupJsonContext.Default.OperationGetCommandResult);
        }
        catch (Exception ex)
        {
            // Do not attach the exception: SDK/authentication exception messages may contain response bodies.
            logger.LogError("Error reading RSV operation status. Vault: {Vault}, ResourceGroup: {ResourceGroup}", options.Vault, options.ResourceGroup);
            HandleException(context, ex);
        }
        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException { Status: 404 } => "RSV operation not found. Verify the operation ID, vault and scope; operation records can expire.",
        RequestFailedException { Status: 401 or 403 } => "Access denied. Verify tenant, subscription and permission to read RSV operation status.",
        ArgumentException => "Invalid RSV operation parameters. Check the operation ID and paired container/protected-item scope.",
        OperationCanceledException => "RSV operation status request was canceled.",
        _ => "Unable to retrieve RSV operation status. Check Azure Backup diagnostics."
    };

    protected override void HandleException(CommandContext context, Exception ex)
    {
        // Framework validation exceptions already carry a safe message and the correct status
        // (for example 400 for missing required options); let the base handler process them.
        if (ex is CommandValidationException)
        {
            base.HandleException(context, ex);
            return;
        }

        // The base handler also serializes ex.Message into ExceptionResult, independently
        // of GetErrorMessage. Replace the exception before handing it to the framework.
        var status = GetStatusCode(ex);
        base.HandleException(context, new RequestFailedException((int)status, GetErrorMessage(ex)));
    }

    public sealed record OperationGetCommandResult(BackupOperationInfo Operation);
}
