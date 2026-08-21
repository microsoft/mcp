// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.ManagedLustre.Options.FileSystem.ExpansionJob;
using Azure.Mcp.Tools.ManagedLustre.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.ManagedLustre.Commands.FileSystem.ExpansionJob;

[CommandMetadata(
    Id = "e4f592ba-405a-492f-92dc-7fd8a7d4220b",
    Name = "create",
    Title = "Create Azure Managed Lustre Expansion Job",
    Description = """
        Creates an expansion job to increase the storage capacity of an Azure Managed Lustre (AMLFS) file system. The expansion job resizes the filesystem to the specified new capacity in TiB. The new size must be a multiple of the SKU step size and greater than the current storage capacity.
        Required options:
        - filesystem-name: The name of the AMLFS filesystem
        - new-size: The new storage capacity in TiB after expansion
        - resource-group: The resource group containing the filesystem
        - subscription: The subscription containing the filesystem
        """,
    Destructive = true,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class ExpansionJobCreateCommand(IManagedLustreService service, ILogger<ExpansionJobCreateCommand> logger, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<ExpansionJobCreateOptions, ExpansionJobCreateCommand.ExpansionJobCreateResult>(subscriptionResolver)
{
    private readonly IManagedLustreService _service = service;
    private readonly ILogger<ExpansionJobCreateCommand> _logger = logger;

    public override void ValidateOptions(ExpansionJobCreateOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (!float.IsFinite(options.NewSize) || options.NewSize <= 0)
        {
            validationResult.Errors.Add("The new-size option must be a finite value greater than zero.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ExpansionJobCreateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var jobName = await _service.CreateExpansionJobAsync(
                options.Subscription!,
                options.ResourceGroup,
                options.FilesystemName,
                options.NewSize,
                options.ExpansionJobName,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(jobName), ManagedLustreJsonContext.Default.ExpansionJobCreateResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating expansion job for AMLFS filesystem {FileSystem}.",
                options.FilesystemName);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record ExpansionJobCreateResult(string JobName);
}
