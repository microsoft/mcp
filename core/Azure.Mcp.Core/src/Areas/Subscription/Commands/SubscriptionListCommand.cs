// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas.Subscription.Options;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.ResourceManager.Resources;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Core.Areas.Subscription.Commands;

public sealed class SubscriptionListCommand(ILogger<SubscriptionListCommand> logger) : GlobalCommand<SubscriptionListOptions>()
{
    private const string CommandTitle = "List Azure Subscriptions";
    private readonly ILogger<SubscriptionListCommand> _logger = logger;

    public override string Name => "list";

    public override string Description =>
    "List all or current subscriptions for an account in Azure; returns subscriptionId, displayName, state, tenantId, and isDefault. Use for scope selection in governance, policy, access, cost management, or deployment.";
    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        ReadOnly = true,
        LocalRequired = false,
        Secret = false
    };

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            var subscriptionService = context.GetService<ISubscriptionService>();
            var subscriptions = await subscriptionService.GetSubscriptions(options.Tenant, options.RetryPolicy);

            context.Response.Results = ResponseResult.Create(
                    new SubscriptionListCommandResult(subscriptions),
                    SubscriptionJsonContext.Default.SubscriptionListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing subscriptions.");
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record SubscriptionListCommandResult(List<SubscriptionData> Subscriptions);
}
