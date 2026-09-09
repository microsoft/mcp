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
    Id = "1c9a4b3e-6f8d-4a1c-9e2b-5d0f7c8a3b12",
    Name = "get",
    Title = "Get Resource Guard",
    Description = """
        Retrieves a specific Resource Guard by name, or lists Resource Guards (in a resource group
        if --resource-group is provided, otherwise across the subscription). Returns the Resource
        Guard's ARM ID, location, and the list of vault critical operations it protects and
        excludes.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class ResourceGuardGetCommand(ILogger<ResourceGuardGetCommand> logger, IAzureBackupService azureBackupService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<ResourceGuardGetOptions, ResourceGuardGetCommand.ResourceGuardGetCommandResult>(subscriptionResolver)
{
    private readonly ILogger<ResourceGuardGetCommand> _logger = logger;
    private readonly IAzureBackupService _azureBackupService = azureBackupService;

    public override void ValidateOptions(ResourceGuardGetOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (!string.IsNullOrEmpty(options.ResourceGuard) && string.IsNullOrEmpty(options.ResourceGroup))
        {
            validationResult.Errors.Add("--resource-group is required when --resource-guard is specified.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ResourceGuardGetOptions options, CancellationToken cancellationToken)
    {
        AzureBackupTelemetryTags.AddSubscriptionTag(context.Activity, options.Subscription);
        var operation = !string.IsNullOrEmpty(options.ResourceGuard) ? "get" : "list";
        AzureBackupTelemetryTags.AddResourceGuardOperationTag(context.Activity, operation);

        try
        {
            List<ResourceGuardInfo> guards;
            if (!string.IsNullOrEmpty(options.ResourceGuard))
            {
                var guard = await _azureBackupService.GetResourceGuardAsync(
                    options.ResourceGuard,
                    options.ResourceGroup!,
                    options.Subscription!,
                    options.Tenant,
                    cancellationToken);
                guards = [guard];
            }
            else
            {
                guards = await _azureBackupService.ListResourceGuardsAsync(
                    options.Subscription!,
                    options.ResourceGroup,
                    options.Tenant,
                    cancellationToken);
            }

            context.Response.Results = ResponseResult.Create(
                new(guards),
                AzureBackupJsonContext.Default.ResourceGuardGetCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Resource Guard(s). Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, ResourceGuard: {ResourceGuard}",
                options.Subscription, options.ResourceGroup, options.ResourceGuard);
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
            $"Authorization failed. Ensure you have Reader role on the resource group or subscription. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    public sealed record ResourceGuardGetCommandResult(List<ResourceGuardInfo> Guards);
}
