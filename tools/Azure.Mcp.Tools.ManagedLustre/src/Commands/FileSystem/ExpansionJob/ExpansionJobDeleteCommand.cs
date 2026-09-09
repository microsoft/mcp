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
    Id = "dbdda79a-6858-4b32-96f1-413d0078bff1",
    Name = "delete",
    Title = "Delete Azure Managed Lustre Expansion Job",
    Description = """
        Deletes an expansion job for an Azure Managed Lustre filesystem. This permanently removes the expansion job record from the filesystem. Use this to clean up completed, failed, or cancelled expansion jobs.
        """,
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class ExpansionJobDeleteCommand(IManagedLustreService service, ILogger<ExpansionJobDeleteCommand> logger, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<ExpansionJobDeleteOptions, ExpansionJobDeleteCommand.ExpansionJobDeleteResult>(subscriptionResolver)
{
    private readonly IManagedLustreService _service = service;
    private readonly ILogger<ExpansionJobDeleteCommand> _logger = logger;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ExpansionJobDeleteOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var deleted = await _service.DeleteExpansionJobAsync(
                options.Subscription!,
                options.ResourceGroup,
                options.FilesystemName,
                options.ExpansionJobName,
                options.Tenant,
                cancellationToken);

            var message = deleted
                ? $"Expansion job '{options.ExpansionJobName}' was successfully deleted."
                : $"Expansion job '{options.ExpansionJobName}' was not found. Nothing was deleted.";

            context.Response.Results = ResponseResult.Create(
                new(options.ExpansionJobName, deleted, message),
                ManagedLustreJsonContext.Default.ExpansionJobDeleteResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting expansion job {JobName} for AMLFS filesystem {FileSystem}.",
                options.ExpansionJobName, options.FilesystemName);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record ExpansionJobDeleteResult(string JobName, bool Deleted, string Message);
}
