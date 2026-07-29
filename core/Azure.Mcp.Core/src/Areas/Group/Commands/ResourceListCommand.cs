// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas.Group.Options;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.ResourceGroup;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Resource;

namespace Azure.Mcp.Core.Areas.Group.Commands;

[CommandMetadata(
    Id = "b1c2d3e4-f5a6-7890-abcd-ef1234567890",
    Name = "list",
    Title = "List Resources in Resource Group",
    Description = """
        List all resources in a resource group. This command retrieves all resources available
        in the specified resource group within the given subscription. Results include resource
        names, IDs, types, and locations. The command returns a JSON object with a `resources`
        array containing these entries.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    LocalRequired = false,
    Secret = false)]
public sealed class ResourceListCommand(ILogger<ResourceListCommand> logger, IResourceGroupService resourceGroupService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<ResourceListOptions, ResourceListCommand.ResourceListCommandResult>(subscriptionResolver)
{
    private readonly ILogger<ResourceListCommand> _logger = logger;
    private readonly IResourceGroupService _resourceGroupService = resourceGroupService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ResourceListOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var resources = await _resourceGroupService.GetGenericResources(
                options.Subscription!,
                options.ResourceGroup,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken).ToListAsync(cancellationToken);

            context.Response.Results = ResponseResult.Create(new(resources), GroupJsonContext.Default.ResourceListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred listing resources in resource group.");
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record ResourceListCommandResult(List<GenericResourceInfo> Resources);
}
