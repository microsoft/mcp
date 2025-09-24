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

public override string Description => """
List all subscriptions for my account (your account); inspect subscription metadata/status; enumerate scopes for governance, policy, access, or cost management; or discover further subscription-level commands (policies, access, settings) in multi-subscription scenarios. Azure subscription operations router. A subscription is the billing and administrative boundary above resource groups and resources. Invocation: set "command" plus its arguments inside "parameters"; set "learn=true" to list available sub-commands and their required parameters. Returns only subscriptions you are permitted to view. Not for: resource group or individual resource CRUD, tenant-wide AAD tasks, billing account administration, or directory policy management. Shows subscriptionId, displayName, state, tenantId, isDefault (current/default) to help choose a working scope.
""";

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

            context.Response.Results = subscriptions?.Count > 0
                ? ResponseResult.Create(
                    new SubscriptionListCommandResult(subscriptions),
                    SubscriptionJsonContext.Default.SubscriptionListCommandResult)
                : null;
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
