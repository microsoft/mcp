// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.ManagedLustre.Options.FileSystem;
using Azure.Mcp.Tools.ManagedLustre.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.ManagedLustre.Commands.FileSystem;

[CommandMetadata(
    Id = "723d9b34-9022-486e-83a7-f72d83bdafd2",
    Name = "list",
    Title = "List Azure Managed Lustre File Systems",
    Description = "Lists Azure Managed Lustre (AMLFS) file systems in a subscription or optional resource group including provisioning state, MGS address, tier, capacity (TiB), blob integration container, and maintenance window details. Use to inventory Azure Managed Lustre filesystems and to check their properties.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class FileSystemListCommand(IManagedLustreService service, ILogger<FileSystemListCommand> logger, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<FileSystemListOptions, FileSystemListCommand.FileSystemListResult>(subscriptionResolver)
{
    private readonly IManagedLustreService _service = service;
    private readonly ILogger<FileSystemListCommand> _logger = logger;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, FileSystemListOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var fileSystems = await _service.ListFileSystemsAsync(
                options.Subscription!,
                options.ResourceGroup,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(fileSystems ?? []), ManagedLustreJsonContext.Default.FileSystemListResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error listing AMLFS file systems. ResourceGroup: {ResourceGroup}.",
                options.ResourceGroup);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record FileSystemListResult(List<Models.LustreFileSystem> FileSystems);
}
