// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas.Group.Options;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.ResourceGroup;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.ResourceGroup;

namespace Azure.Mcp.Core.Areas.Group.Commands;

[CommandMetadata(
    Id = "a0049f31-9a32-4b5e-91ec-e7b074fc7246",
    Name = "list",
    Title = "List Resource Groups",
    Description = """
        List all resource groups in a subscription. This command retrieves all resource groups available
        in the specified subscription. Results include resource group names and IDs,
        returned as a JSON array.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    LocalRequired = false,
    Secret = false)]
public sealed class GroupListCommand(ILogger<GroupListCommand> logger) : SubscriptionCommand<BaseGroupOptions>()
{
    private readonly ILogger<GroupListCommand> _logger = logger;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            var resourceGroupService = context.GetService<IResourceGroupService>();
            var groups = await resourceGroupService.GetResourceGroups(
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(groups ?? []), GroupJsonContext.Default.Result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred listing resource groups.");
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record class Result(List<ResourceGroupInfo> Groups);
}
