// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.ManagedLustre.Options.FileSystem.AutoimportJob;
using Azure.Mcp.Tools.ManagedLustre.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.ManagedLustre.Commands.FileSystem.AutoimportJob;

[CommandMetadata(
    Id = "a1b2c3d4-5e6f-7a8b-9c0d-1e2f3a4b5c6d",
    Name = "create",
    Title = "Create Azure Managed Lustre Autoimport Job",
    Description = """
        Creates a continuous autoimport job that syncs new or modified blobs from the linked hierarchical
        storage management (HSM) container to an Azure Managed Lustre filesystem. Configure conflict handling,
        imported prefixes, and deletions through the parameters. Returns the job name.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = true,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class AutoimportJobCreateCommand(IManagedLustreService service, ILogger<AutoimportJobCreateCommand> logger, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<AutoimportJobCreateOptions, AutoimportJobCreateCommand.AutoimportJobCreateResult>(subscriptionResolver)
{
    private readonly IManagedLustreService _service = service;
    private readonly ILogger<AutoimportJobCreateCommand> _logger = logger;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, AutoimportJobCreateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            // Log the prefixes for debugging
            if (options.AutoimportPrefixes != null && options.AutoimportPrefixes.Length > 0)
            {
                _logger.LogInformation("Autoimport prefixes received: {Prefixes}", string.Join(", ", options.AutoimportPrefixes));
            }
            else
            {
                _logger.LogInformation("No autoimport prefixes received, will use default");
            }

            var job = await _service.CreateAutoimportJobAsync(
                options.Subscription!,
                options.ResourceGroup,
                options.FilesystemName,
                options.JobName,
                options.ConflictResolutionMode?.ToString(),
                options.AutoimportPrefixes,
                options.AdminStatus?.ToString(),
                options.EnableDeletions,
                options.MaximumErrors,
                options.Tenant,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(job), ManagedLustreJsonContext.Default.AutoimportJobCreateResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating autoimport job for AMLFS filesystem {FileSystem}.",
                options.FilesystemName);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record AutoimportJobCreateResult(string JobName);
}
