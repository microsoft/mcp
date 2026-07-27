// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.ManagedLustre.Options.FileSystem.AutoexportJob;
using Azure.Mcp.Tools.ManagedLustre.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.ManagedLustre.Commands.FileSystem.AutoexportJob;

[CommandMetadata(
    Id = "9f3e7c2a-4b8d-4e5f-a1c6-8d9e2f3b4a5c",
    Name = "create",
    Title = "Create Azure Managed Lustre Autoexport Job",
    Description = """
        Creates an auto export job for an Azure Managed Lustre filesystem to continuously export modified files to the linked blob storage container. The auto export job syncs changes from the Lustre filesystem to the configured HSM blob container. Use this to keep blob storage updated with changes in the filesystem.
        Required options:
        - filesystem-name: The name of the AMLFS filesystem
        - resource-group: The resource group containing the filesystem
        - subscription: The subscription containing the filesystem
        """,
    Destructive = true,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class AutoexportJobCreateCommand(IManagedLustreService service, ILogger<AutoexportJobCreateCommand> logger, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<AutoexportJobCreateOptions, AutoexportJobCreateCommand.AutoexportJobCreateResult>(subscriptionResolver)
{
    private readonly IManagedLustreService _service = service;
    private readonly ILogger<AutoexportJobCreateCommand> _logger = logger;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, AutoexportJobCreateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var job = await _service.CreateAutoexportJobAsync(
                options.Subscription!,
                options.ResourceGroup,
                options.FilesystemName,
                options.JobName,
                options.AutoexportPrefix,
                options.AdminStatus,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(job), ManagedLustreJsonContext.Default.AutoexportJobCreateResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating autoexport job for AMLFS filesystem {FileSystem}.",
                options.FilesystemName);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record AutoexportJobCreateResult(string JobName);
}
