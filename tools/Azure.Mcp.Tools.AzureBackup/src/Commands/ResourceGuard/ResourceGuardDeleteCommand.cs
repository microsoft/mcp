// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Options.ResourceGuard;
using Azure.Mcp.Tools.AzureBackup.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.AzureBackup.Commands.ResourceGuard;

[CommandMetadata(
    Id = "5b8e2d1a-4c7f-4a9b-8e2d-3f5a1c9b7d24",
    Name = "delete",
    Title = "Delete Resource Guard",
    Description = """
        Deletes a Resource Guard (Microsoft.DataProtection/resourceGuards). This is a destructive
        operation: deleting a Resource Guard removes MUA protection from every vault currently
        linked to it. Vaults will need to be re-linked to a different Resource Guard to restore
        MUA protection.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class ResourceGuardDeleteCommand(ILogger<ResourceGuardDeleteCommand> logger, IAzureBackupService azureBackupService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<ResourceGuardDeleteOptions, ResourceGuardDeleteCommand.ResourceGuardDeleteCommandResult>(subscriptionResolver)
{
    private readonly ILogger<ResourceGuardDeleteCommand> _logger = logger;
    private readonly IAzureBackupService _azureBackupService = azureBackupService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ResourceGuardDeleteOptions options, CancellationToken cancellationToken)
    {
        AzureBackupTelemetryTags.AddSubscriptionTag(context.Activity, options.Subscription);
        AzureBackupTelemetryTags.AddResourceGuardOperationTag(context.Activity, "delete");

        try
        {
            var result = await _azureBackupService.DeleteResourceGuardAsync(
                options.ResourceGuard,
                options.ResourceGroup,
                options.Subscription!,
                options.Tenant,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new(result),
                AzureBackupJsonContext.Default.ResourceGuardDeleteCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting Resource Guard. Name: {ResourceGuard}, ResourceGroup: {ResourceGroup}",
                options.ResourceGuard, options.ResourceGroup);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        ArgumentException argEx => argEx.Message,
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Resource Guard not found. Verify the name and resource group.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed. Deleting a Resource Guard requires Contributor role on the resource group. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            $"Cannot delete the Resource Guard while vaults are linked to it. First disable MUA on all linked vaults using 'security disable-mua --force'. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    public sealed record ResourceGuardDeleteCommandResult(OperationResult Result);
}
